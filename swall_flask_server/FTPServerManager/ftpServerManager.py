import ftplib
import logging
import os
import re
import enum
import time
import shutil
import threading
import zipfile
from os.path import join
import traceback
import base64
import json

from ConfigurationManager.configurationManager import ConfigurationManager
from DatabaseManager.databaseManager import DatabaseManager
from Logger.Logger import Logger
from EnumManager.enumManager import ActivityType, SynchronizationType, FTPBASEDIRECTORIES, FetchStatus
from Helper import helper
from FTPObserver.ftpObserver import FTPObserver

"""
Ensure this class only has one instance, and provide a global point of
access to it.
"""


class FTPServerManager:
    __instance = None
    __className = 'FTP SERVER MANAGER'
    __FTP_LOCK = threading.RLock()
    __FTPBASEDIRECTORIESDICTIONARYARRAY = [
        {
            'directory': FTPBASEDIRECTORIES.Upload.value,
            'parent': FTPBASEDIRECTORIES.CurrentDirectory.value
        },
        {
            'directory': FTPBASEDIRECTORIES.sWall.value,
            'parent': FTPBASEDIRECTORIES.Upload.value
        },
        {
            'directory': FTPBASEDIRECTORIES.slaveFiles.value,
            'parent': FTPBASEDIRECTORIES.sWall.value
        },
        {
            'directory': FTPBASEDIRECTORIES.Activities.value,
            'parent':  FTPBASEDIRECTORIES.slaveFiles.value
        },
        {
            'directory': FTPBASEDIRECTORIES.Resources.value,
            'parent':  FTPBASEDIRECTORIES.sWall.value
        },
        {
            'directory': FTPBASEDIRECTORIES.Browser.value,
            'parent': FTPBASEDIRECTORIES.Activities.value
        },
        {
            'directory': FTPBASEDIRECTORIES.Game.value,
            'parent': FTPBASEDIRECTORIES.Activities.value
        },
        {
            'directory': FTPBASEDIRECTORIES.Media.value,
            'parent': FTPBASEDIRECTORIES.Activities.value
        }
    ]

    @staticmethod
    def getInstance():
        """ Static access method. """
        if FTPServerManager.__instance == None:
            FTPServerManager()
        return FTPServerManager.__instance

    def __init__(self):
        """ Virtually private constructor. """
        if FTPServerManager.__instance != None:
            raise Exception("This class is a singleton!")
        else:
            FTPServerManager.__instance = self
            self.ftp = ftplib.FTP(
                ConfigurationManager.getInstance().configuration["FTPServerURL"])
            self.__connectToFTPServer()
            self.__initializeBaseDirectories()
            FTPObserver.getInstance().observe(self)

    def uploadActivityFile(self, activityType, file):
        with FTPServerManager.__FTP_LOCK:
            if activityType is ActivityType.browser:
                self.ftp.cwd(FTPBASEDIRECTORIES.Browser.value.split(
                    FTPBASEDIRECTORIES.sWall.value + '/')[1])
            elif activityType is ActivityType.game:
                self.ftp.cwd(FTPBASEDIRECTORIES.Game.value.split(
                    FTPBASEDIRECTORIES.sWall.value + '/')[1])
            elif activityType is ActivityType.media:
                self.ftp.cwd(FTPBASEDIRECTORIES.Media.value.split(
                    FTPBASEDIRECTORIES.sWall.value + '/')[1])
            else:
                raise(Exception('Unimplemented Activity Type'))
            self.__uploadFile(file)
            self.ftp.cwd('/' + FTPBASEDIRECTORIES.sWall.value)

    # TODO: Create Folders with unique ids and timestamps for servicing multiple requests
    # TODO: Try/Catch
    def download_ftp_tree(self, path):
        """
        Downloads an entire directory tree from an ftp server to the local destination
        :param path: the folder on the ftp server to download
        """
        with FTPServerManager.__FTP_LOCK:
            path = path.lstrip("/")
            self.__mirror_ftp_dir(path)

            downloadedZipFileTuple = self.__get_zipped_directory_name(path)
            unarchivedDirectoryName = self.downloadsInProgressFolderPath + '/' + path
            self.__deleteDirectory(unarchivedDirectoryName)

            return downloadedZipFileTuple

    def deleteFile(self, filePath):
        """ Deletes the file whose path corresponds to the specified filePath """
        with FTPServerManager.__FTP_LOCK:
            try:
                Logger.getInstance().log(FTPServerManager.__className,
                                         'Attempting to delete file {}'.format(filePath), logging.INFO)
                if os.path.exists(filePath):
                    os.remove(filePath)
                    Logger.getInstance().log(FTPServerManager.__className,
                                             'Deleted file {}'.format(filePath), logging.INFO)
                else:
                    Logger.getInstance().log(FTPServerManager.__className,
                                             'The file {} does not exist'.format(filePath), logging.INFO)
            except OSError as error:
                Logger.getInstance().log(FTPServerManager.__className,
                                         'Error deleting file {}'.format(filePath), logging.ERROR)
                raise(error)

    def processFetchFileRequest(self, requestJSON):
        response = {
            "responseJSON": {
                "Status": FetchStatus.serverError.value
            }
        }
        if (
            requestJSON is not None and
            "FilePath" in requestJSON and
            isinstance(requestJSON["FilePath"], str) is True
        ):
            filePath = requestJSON["FilePath"]

            with FTPServerManager.__FTP_LOCK:
                try:
                    if self.__is_ftp_dir(filePath):
                        response.update(
                            {'responseJSON': {'Status': FetchStatus.fileIsDirectory.value}})
                    else:
                        response.update({'responseJSON': {'Status': FetchStatus.success.value},
                                         "responseFilePath": self.__download_ftp_file(filePath)})
                except Exception as error:
                    Logger.getInstance().log(FTPServerManager.__className,
                                             'Error Processing Sync Request : {}'.format(str(error)), logging.ERROR)
                    traceback.print_exc()  # TODO :- Log traceback ##
                    response.update(
                        {'responseJSON': {'Status': FetchStatus.serverError.value}})
        else:
            response.update(
                {'responseJSON': {'Status': FetchStatus.protocolError.value}})
        return response

    ### Delegate Functions ###

    def getFTPListRecursively(self, directoryName, synchronizationType):
        """ Returns a dictionary containing the directories and files on the FTP server in conjuction with their modification times."""
        configuration = ConfigurationManager.getInstance().configuration
        masterDirectory = configuration["MasterDirectory"]
        slaveDirectory = configuration["SlaveDirectory"]
        with FTPServerManager.__FTP_LOCK:
            fileDictionary = {}
            try:
                for item in self.ftp.nlst(directoryName):
                    if self.__is_ftp_dir(item):
                        # Add directories to the list so the client can create the directories if they are empty. Also, when all the files in the directory are deleted, the client can delete the empty directory and not just the files in them.
                        if (
                            (synchronizationType == SynchronizationType.master and item != slaveDirectory) or
                            (synchronizationType ==
                             SynchronizationType.slave and item != masterDirectory)
                        ):
                            fileDictionary[item] = {
                                "FileModificationTime": "", "FileSize": -1}
                            fileDictionary.update(
                                self.getFTPListRecursively(item, synchronizationType))
                    else:
                        try:
                            lastModificationTime = self.ftp.voidcmd(
                                "MDTM {}".format(item))[4:].strip()
                            fileDictionary[item] = {
                                "FileModificationTime": lastModificationTime, "FileSize": self.ftp.size(item)}
                        # This error could occur if the file is deleted on the FTP server before it's modification time is accessed. refetch the list and check if the item is in the new list. If so, raise exception else, ignore the file. Other changes made during parsing (i.e adds and updates) will be picked up during the next pass.
                        except Exception as error:
                            if item not in self.ftp.nlst(directoryName):
                                print("{} deleted".format(item))
                                Logger.getInstance().log(FTPServerManager.__className,
                                                         'File {} was deleted while creating FTP list. Ignoring file.'.format(item), logging.INFO)
                                continue
                            else:
                                Logger.getInstance().log(FTPServerManager.__className,
                                                         'ERROR Fetching Modification Time for File {}: {}'.format(item, str(error)), logging.ERROR)
                                raise(error)
                return fileDictionary
            except Exception as error:
                Logger.getInstance().log(FTPServerManager.__className,
                                         'ERROR Fetching FTP List: {}'.format(str(error)), logging.ERROR)
                raise(error)

    def getCurrentFilesDictionary(self, synchronizationType):
        return DatabaseManager.getInstance().getCurrentFilesDictionaryTuple(synchronizationType)["FilesDictionary"]

    def didObserveFTPChanges(self, filesDictionary, synchronizationType):
        DatabaseManager.getInstance().didObserveFTPChanges(
            filesDictionary, synchronizationType)

    ### Private Methods ###

    def __connectToFTPServer(self):
        configuration = ConfigurationManager.getInstance().configuration
        try:
            Logger.getInstance().log(FTPServerManager.__className,
                                     'Attempting to connect to FTP Server.', logging.INFO)
            response = self.ftp.login(
                configuration['FTPServerUser'], configuration['FTPServerPassword'])
            Logger.getInstance().log(FTPServerManager.__className,
                                     'FTP Server Connection Response: {}'.format(str(response)), logging.INFO)
            Logger.getInstance().log(FTPServerManager.__className,
                                     'FTP Server Welcome Message: {}'.format(self.ftp.getwelcome()), logging.INFO)
        except ftplib.Error as error:
            Logger.getInstance().log(FTPServerManager.__className,
                                     'FTP SERVER CONNECTION ERROR: {}'.format(str(error)), logging.ERROR)
            raise(error)

    def __initializeBaseDirectories(self):
        configuration = ConfigurationManager.getInstance().configuration
        downloadsInProgressFolderName = configuration["Downloads_in_progress_folder"]
        self.downloadsInProgressFolderPath = os.path.join(
            os.getcwd(), downloadsInProgressFolderName)
        self.__makeParentDirectoryIfAbsent('/')
        Logger.getInstance().log(FTPServerManager.__className,
                                 'Initializing base FTP directories', logging.INFO)
        for directoryDictionary in FTPServerManager.__FTPBASEDIRECTORIESDICTIONARYARRAY:
            directory = directoryDictionary['directory']
            parent = directoryDictionary['parent']
            fullDirectoryPath = directory + '/'
            if parent != '.':
                parent = parent + '/'
            Logger.getInstance().log(FTPServerManager.__className,
                                     'Attempting to create directory {} on the FTP server'.format(fullDirectoryPath), logging.INFO)
            if not directory in self.ftp.nlst(parent):
                try:
                    Logger.getInstance().log(FTPServerManager.__className,
                                             'Creating directory {} on the FTP server'.format(fullDirectoryPath), logging.INFO)
                    self.ftp.mkd(fullDirectoryPath)
                    Logger.getInstance().log(FTPServerManager.__className,
                                             'Directory {} created'.format(fullDirectoryPath), logging.INFO)
                except Exception as error:
                    Logger.getInstance().log(FTPServerManager.__className,
                                             'Unable to create directory {} Error: {}'.format(fullDirectoryPath, str(error)), logging.ERROR)
                    raise(error)
            else:
                Logger.getInstance().log(FTPServerManager.__className,
                                         'Directory {} already exists on the FTP server'.format(fullDirectoryPath), logging.INFO)
        try:
            Logger.getInstance().log(FTPServerManager.__className,
                                     'Switching to the base sWall directory', logging.INFO)
            self.ftp.cwd(FTPBASEDIRECTORIES.sWall.value)
            Logger.getInstance().log(FTPServerManager.__className,
                                     'FTP server working directory: {}'.format(self.ftp.pwd()), logging.INFO)
        except Exception as error:
            Logger.getInstance().log(FTPServerManager.__className,
                                     'Unable to switch to the base sWall directory. Error: {}'.format(str(error)), logging.ERROR)
            raise(error)

    def __uploadFile(self,  file):
        Logger.getInstance().log(FTPServerManager.__className,
                                 'Attempting to upload file {} to the FTP server'.format(file), logging.INFO)
        try:
            # TODO: Use the callback for progress updates.
            self.ftp.storbinary('STOR {}'.format(file.filename), file)
            Logger.getInstance().log(FTPServerManager.__className,
                                     'FTP server file upload successful: {}'.format(file), logging.INFO)
        except ftplib.Error as error:
            Logger.getInstance().log(FTPServerManager.__className,
                                     'FTP UPLOAD ERROR: {}'.format(str(error)), logging.ERROR)
            raise(error)

    def __mirror_ftp_dir(self, name):
        """ replicates a directory on an ftp server recursively """
        for item in self.ftp.nlst(name):
            if self.__is_ftp_dir(item):
                self.__makeParentDirectoryIfAbsent(item + '/')
                self.__mirror_ftp_dir(item)
            else:
                self.__download_ftp_file(item)

    def __download_ftp_file(self, itemPath):
        """ downloads a single file from an ftp server """
        try:
            self.__makeParentDirectoryIfAbsent(itemPath)
            destinationPath = os.path.join(
                self.downloadsInProgressFolderPath, itemPath)
            with open(destinationPath, 'wb') as f:
                self.ftp.retrbinary("RETR {}".format(itemPath), f.write)
            Logger.getInstance().log(FTPServerManager.__className,
                                     "downloaded: {}".format(destinationPath), logging.INFO)
            return destinationPath
        except IOError as error:
            Logger.getInstance().log(FTPServerManager.__className,
                                     "FAILED: {}, {}".format(destinationPath, error), logging.ERROR)
            raise(error)

    def __get_zipped_directory_name(self, baseDirectoryName):
        try:
            Logger.getInstance().log(FTPServerManager.__className,
                                     "Archiving Directory: {}".format(baseDirectoryName), logging.INFO)
            empty_dirs = []
            filename = "{}.zip".format(baseDirectoryName)
            zippedFilePath = join(self.downloadsInProgressFolderPath, filename)
            originalWorkingDirectory = os.getcwd()

            os.chdir(self.downloadsInProgressFolderPath)

            zip = zipfile.ZipFile(zippedFilePath, 'w',
                                  zipfile.ZIP_DEFLATED, allowZip64=True)
            for root, dirs, files in os.walk(baseDirectoryName):
                empty_dirs.extend(
                    [dir for dir in dirs if os.listdir(join(root, dir)) == []])
                for name in files:
                    zip.write(join(root, name))
                for dir in empty_dirs:
                    zipinfo = zipfile.ZipInfo(join(root, dir) + "/")
                    zip.writestr(zipinfo, "")
                empty_dirs = []
            zip.close()

            os.chdir(originalWorkingDirectory)
        except Exception as error:
            Logger.getInstance().log(FTPServerManager.__className,
                                     "Error Archiving Directory: {} Error: {}".format(baseDirectoryName, error), logging.ERROR)
            raise(error)

        return {'name': filename, 'path': zippedFilePath}

    def __is_ftp_dir(self, name):
        """ simply determines if an item listed on the ftp server is a valid directory or not """
        original_cwd = self.ftp.pwd()  # remember the current working directory
        try:
            self.ftp.cwd(name)  # try to set directory to new name
            self.ftp.cwd(original_cwd)  # set it back to what it was
            return True

        except ftplib.error_perm:
            return False

        except Exception:
            return False

    def __makeParentDirectoryIfAbsent(self, fpath):
        """ ensures the parent directory of a filepath exists """
        dirname = os.path.dirname(os.path.join(
            self.downloadsInProgressFolderPath, fpath))
        if not helper.isEmptyString(dirname) and not os.path.exists(dirname):
            try:
                os.makedirs(dirname)
                Logger.getInstance().log(FTPServerManager.__className,
                                         "Created directory {}".format(dirname), logging.INFO)
            except OSError as error:
                Logger.getInstance().log(FTPServerManager.__className,
                                         "Error creating directory {}".format(dirname), logging.ERROR)
                raise(error)
        else:
            Logger.getInstance().log(FTPServerManager.__className,
                                     "Directory {} already exists".format(dirname), logging.INFO)

    def __deleteDirectory(self, directoryPath):
        """ Deletes the directory whose path corresponds to the specified directoryPath """
        try:
            Logger.getInstance().log(FTPServerManager.__className,
                                     'Attempting to delete directory {}'.format(directoryPath), logging.INFO)
            shutil.rmtree(directoryPath)
            Logger.getInstance().log(FTPServerManager.__className,
                                     'Deleted directory {}'.format(directoryPath), logging.INFO)
        except OSError as error:
            Logger.getInstance().log(FTPServerManager.__className,
                                     'Error deleting directory {}'.format(directoryPath), logging.ERROR)
            raise(error)
