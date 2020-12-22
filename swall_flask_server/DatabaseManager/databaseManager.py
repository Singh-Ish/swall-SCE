import mysql.connector
from mysql.connector import errorcode
from mysql.connector import pooling
import logging
import sys
import threading
import traceback

from Logger.Logger import Logger
from ConfigurationManager.configurationManager import ConfigurationManager
from EnumManager.enumManager import ActivityType, SynchronizationType, ActivityTagCategory, SyncStatus
from Helper import helper

"""
Ensure this class only has one instance, and provide a global point of
access to it.
"""

class DatabaseManager:
    __className = 'Database Manager'
    __instance = None
    __DB_LOCK = threading.RLock()

    @staticmethod 
    def getInstance():
        """ Static access method. """
        with DatabaseManager.__DB_LOCK:
            if DatabaseManager.__instance == None:
                DatabaseManager()
            return DatabaseManager.__instance
    
    def __init__(self):
        """ Virtually private constructor. """
        if DatabaseManager.__instance != None:
            raise Exception("This class is a singleton!")
        else:
            DatabaseManager.__instance = self
            self.__connectToDatabase()
    
    def __connectToDatabase(self):
        configuration = ConfigurationManager.getInstance().configuration
        try:
            Logger.getInstance().log(DatabaseManager.__className, 'Attempting to connect to MySQL database.', logging.INFO)
            self.connection = mysql.connector.connect(host = configuration['DatabaseHost'], port = int(configuration['DatabasePort']), user = configuration['DatabaseUser'], password = configuration['DatabasePassword'], database = configuration['DatabaseName'])
            Logger.getInstance().log(DatabaseManager.__className, 'MySQL database connection successful.', logging.INFO)
        except Exception as error:
            Logger.getInstance().log(DatabaseManager.__className, 'DATABASE CONNECTION ERROR: {}'.format(str(error)), logging.ERROR)
            raise(error)
    
    def __getCursor(self):
        return self.connection.cursor(dictionary=True, buffered=True)

    def __insertActivity(self, activityType, activityJSON, cursor):
        command = "INSERT into activity(activityTypeId, name, filename, ownerId, creationTime, lastUpdateTime, logoPath, description) VALUES ({}, '{}', '{}', '{}', '{}', '{}', '{}', '{}');".format(
            activityType.value,
            activityJSON["name"],
            activityJSON['file'].filename,
            activityJSON["ownerId"],
            activityJSON["creationTime"],
            activityJSON["lastUpdateTime"],
            activityJSON["logoPath"],
            activityJSON["description"]
        )
        try:
            Logger.getInstance().log(DatabaseManager.__className, "Creating Activity with command {}: ".format(command), logging.INFO)
            cursor.execute(command)
            Logger.getInstance().log(DatabaseManager.__className, "Success", logging.INFO)
        except Exception as error:
            Logger.getInstance().log(DatabaseManager.__className, "Error Inserting Activity: {}".format(str(error)), logging.ERROR)
            raise(error)
        else:
            Logger.getInstance().log(DatabaseManager.__className, "Success", logging.INFO)
    
    def __getFilesDictionaryForSyncKey(self, syncKey, synchronizationType):
        selectFilesCommand = ""
        if synchronizationType == SynchronizationType.master:
            selectFilesCommand = ("SELECT files.path, sync_files.fileModificationTime, sync_files.fileSize"
                                  "   FROM ("
                                  "       SELECT * from masterSynchronization_files WHERE syncKey = {}"
                                  "   ) AS sync_files"
                                  "   INNER JOIN files ON files.fileId = sync_files.fileId;"
                                  ).format(syncKey)
        elif synchronizationType == SynchronizationType.slave:
            selectFilesCommand = ("SELECT files.path, sync_files.fileModificationTime, sync_files.fileSize"
                                  "   FROM ("
                                  "       SELECT * from slaveSynchronization_files WHERE syncKey = {}"
                                  "   ) AS sync_files"
                                  "   INNER JOIN files ON files.fileId = sync_files.fileId;"
                                  ).format(syncKey)
        else:
            raise (Exception('Unimplemented Synchronization Type'))
        with DatabaseManager.__DB_LOCK:
            try:
                cursor = self.__getCursor()
                cursor.execute(selectFilesCommand)
                filesArray = cursor.fetchall()

                filesDictionary = {}
                if filesArray is not None:
                    for file in filesArray:
                        filesDictionary[file["path"]] = {
                            "FileModificationTime": file["fileModificationTime"], "FileSize": file["fileSize"]}
                return filesDictionary
            except Exception as error:
                Logger.getInstance().log(DatabaseManager.__className,
                                         'Error Fetching Files For Command {} : {}'.format(selectFilesCommand, str(error)), logging.ERROR)
                raise(error)

    def insertBrowser(self, browserJSON): # TODO: Refactor
        with DatabaseManager.__DB_LOCK:
            try:
                cursor = self.__getCursor()
                self.__insertActivity(ActivityType.browser, browserJSON, cursor)
                activityId = cursor.lastrowid
                command = "INSERT into browser(activityId, executablePath, url) VALUES ({}, '{}', '{}');".format(cursor.lastrowid, browserJSON["executablePath"], browserJSON["url"])
                Logger.getInstance().log(DatabaseManager.__className, "Inserting Browser activity with command {}: ".format(command), logging.INFO)
                cursor.execute(command)
                self.insertTag(browserJSON, activityId, cursor)
                Logger.getInstance().log(DatabaseManager.__className, 'Database Browser Activity Creation Successful', logging.INFO)
                self.uploadActivityFile(ActivityType.browser, browserJSON['file'])
                self.connection.commit()
                cursor.close()
                Logger.getInstance().log(DatabaseManager.__className, 'Browser activity successfully created in the database and uploaded to the FTP server: {}'.format(browserJSON), logging.INFO)
            except Exception as error:
                Logger.getInstance().log(DatabaseManager.__className, 'Error Inserting Browser Activity: {}. Reverting Changes'.format(str(error)), logging.ERROR)
                self.connection.rollback()
                raise(error)
    
    def insertGame(self, gameJSON): # TODO: Refactor
        with DatabaseManager.__DB_LOCK:
            try:
                cursor = self.__getCursor()
                self.__insertActivity(ActivityType.game, gameJSON, cursor)
                activityId = cursor.lastrowid
                command = "INSERT into game(activityId, executablePath) VALUES ({}, '{}');".format(cursor.lastrowid, gameJSON["executablePath"])
                Logger.getInstance().log(DatabaseManager.__className, 'Inserting Game activity with command {}: '.format(command), logging.INFO)
                cursor.execute(command)
                self.insertTag(gameJSON, activityId, cursor)
                Logger.getInstance().log(DatabaseManager.__className, 'Database Game Activity Creation Successful', logging.INFO)
                self.uploadActivityFile(ActivityType.game, gameJSON['file'])
                self.connection.commit()
                cursor.close()
                Logger.getInstance().log(DatabaseManager.__className, 'Game activity successfully created in the database and uploaded to the FTP server: {}'.format(gameJSON), logging.INFO)
            except Exception as error:
                Logger.getInstance().log(DatabaseManager.__className, 'Error Inserting Game Activity: {}. Reverting Changes'.format(str(error)), logging.ERROR)
                self.connection.rollback()
                raise(error)
    
    def insertMedia(self, mediaJSON): # TODO: Refactor
        with DatabaseManager.__DB_LOCK:
            try:
                cursor = self.__getCursor()
                self.__insertActivity(ActivityType.media, mediaJSON, cursor)
                activityId = cursor.lastrowid
                command = "INSERT into media(activityId, width, height) VALUES ({}, '{}', {});".format(activityId, mediaJSON["width"], mediaJSON["height"])
                Logger.getInstance().log(DatabaseManager.__className, 'Inserting Media activity with command {}: '.format(command), logging.INFO)
                cursor.execute(command)
                self.insertTag(mediaJSON, activityId, cursor)
                Logger.getInstance().log(DatabaseManager.__className, 'Database Media Activity Creation Successful', logging.INFO)
                self.uploadActivityFile(ActivityType.media, mediaJSON['file'])
                self.connection.commit()
                cursor.close()
                Logger.getInstance().log(DatabaseManager.__className, 'Media activity successfully created in the database and uploaded to the FTP server: {}'.format(mediaJSON), logging.INFO)
            except Exception as error:
                Logger.getInstance().log(DatabaseManager.__className, 'Error Inserting Media : {}. Reverting Changes'.format(str(error)), logging.ERROR)
                self.connection.rollback()
                raise(error)
    
    def insertTag(self, activityJSON, activityId, cursor):
        if 'tags' in activityJSON:
            tags = activityJSON['tags']
            if not helper.isEmptyString(tags):
                for tag in tags:
                    try:
                        tagName = tag['tagName']
                        tagCategory = ActivityTagCategory(tag['tagCategory'])
                        command = "INSERT INTO activityTag(activityId, tagId, tagName) VALUES ({}, {}, '{}');".format(activityId, tagCategory.value, tagName)
                        Logger.getInstance().log(DatabaseManager.__className, 'Inserting Tag with command {}: '.format(command), logging.INFO)
                        cursor.execute(command)
                        Logger.getInstance().log(DatabaseManager.__className, 'Database tag creation successful', logging.INFO)
                    except Exception as error:
                        Logger.getInstance().log(DatabaseManager.__className, 'Error Creating Tag : {}'.format(str(error)), logging.ERROR)
                        raise(error)
    
    def uploadActivityFile(self, activityType, file):
        from FTPServerManager.ftpServerManager import FTPServerManager  ## Importation is delayed as a result of circular imports. TODO:- Fix this! 
        FTPServerManager.getInstance().uploadActivityFile(activityType, file)
    
    def didObserveFTPChanges(self, filesDictionary, synchronizationType):
        with DatabaseManager.__DB_LOCK:
            try:
                cursor = self.connection.cursor(dictionary=True, buffered=True)
                syncKey = self.createNewSynchronizationEntry(cursor, synchronizationType)
                for filePath in filesDictionary:
                    fileId = self.createOrFetchFileEntry(cursor, filePath)
                    self.createSynchronizationFileEntry(
                        synchronizationType, cursor, syncKey, fileId, filesDictionary[filePath]["FileModificationTime"], filesDictionary[filePath]["FileSize"])
                self.connection.commit()
                cursor.close()
                Logger.getInstance().log(DatabaseManager.__className, 'Successfully registered changes for syncKey: {}'.format(syncKey), logging.INFO)
            except Exception as error:
                Logger.getInstance().log(DatabaseManager.__className, 'Error registering FTP changes : {}'.format(str(error)), logging.ERROR)
                self.connection.rollback()
                raise(error)

    def createNewSynchronizationEntry(self, cursor, synchronizationType):
        insertSynchronizationCommand = "INSERT into synchronizations(synchronizationTypeId) VALUES ({});".format(synchronizationType.value)
        Logger.getInstance().log(DatabaseManager.__className,
                                 'Creating synchronization entry with command: {}'.format(insertSynchronizationCommand), logging.INFO)
        try:
            cursor.execute(insertSynchronizationCommand)
            Logger.getInstance().log(DatabaseManager.__className, 'Synchronization entry creation successful. SyncKey is {}'.format(cursor.lastrowid), logging.INFO)
            syncKey = cursor.lastrowid
            insertMasterSlaveSynchronizationCommand = ""
            if (synchronizationType == SynchronizationType.master):
                insertMasterSlaveSynchronizationCommand = "INSERT into masterSynchronizations(syncKey) VALUES ({});".format(
                    syncKey)
            elif (synchronizationType == SynchronizationType.slave):
                insertMasterSlaveSynchronizationCommand = "INSERT into slaveSynchronizations(syncKey) VALUES ({});".format(
                    syncKey)
            else:
                raise (Exception('Unimplemented Synchronization Type'))

            cursor.execute(insertMasterSlaveSynchronizationCommand)
            Logger.getInstance().log(DatabaseManager.__className,
                                     'Master/Slave Synchronization entry creation successful. SyncKey is {}'.format(cursor.lastrowid), logging.INFO)
            
            return syncKey
        except Exception as error:
            Logger.getInstance().log(DatabaseManager.__className, 'Error creating synchronization entry : {}'.format(str(error)), logging.ERROR)
            raise(error)
            
    def createOrFetchFileEntry(self, cursor, filePath):
        fetchCommand = "SELECT * from files WHERE path = '{}'".format(filePath)
        Logger.getInstance().log(DatabaseManager.__className, 'Fetching file entry with command {}: '.format(fetchCommand), logging.INFO)
        try:
            cursor.execute(fetchCommand)
            existingFile = cursor.fetchone()
            if existingFile:
                return existingFile["fileId"]
            else:
                createFileCommand = "INSERT INTO files(path) VALUES('{}');".format(filePath)
                Logger.getInstance().log(DatabaseManager.__className, 'File entry with path {} does not exist. Creating entry with command {}'.format(filePath, createFileCommand), logging.INFO)
                cursor.execute(createFileCommand)
                Logger.getInstance().log(DatabaseManager.__className, 'File entry creation successful. FileId is {} and path is {}'.format(cursor.lastrowid, filePath), logging.INFO)
            return cursor.lastrowid
        except Exception as error:
            Logger.getInstance().log(DatabaseManager.__className, 'Error creating synchronization entry : {}'.format(str(error)), logging.ERROR)
            raise(error)

    def createSynchronizationFileEntry(self, synchronizationType, cursor, syncKey, fileId, fileModificationTime, fileSize):
        command = ""
        if synchronizationType == SynchronizationType.master:
            command = "INSERT into masterSynchronization_files(syncKey, fileId, fileModificationTime, fileSize) VALUES ({}, {}, '{}', {});".format(syncKey, fileId, fileModificationTime, fileSize)
        elif synchronizationType == SynchronizationType.slave:
            command = "INSERT into slaveSynchronization_files(syncKey, fileId, fileModificationTime, fileSize) VALUES ({}, {}, '{}', {});".format(
            syncKey, fileId, fileModificationTime, fileSize)
        else:
            raise (Exception('Unimplemented Synchronization Type'))

        Logger.getInstance().log(DatabaseManager.__className, 'Creating synchronization_file entry with command: {}'.format(command), logging.INFO)
        try:
            cursor.execute(command)
            Logger.getInstance().log(DatabaseManager.__className, 'Synchronization_file entry creation successful. entry ID is {}'.format(cursor.lastrowid), logging.INFO)
            return cursor.lastrowid
        except Exception as error:
            Logger.getInstance().log(DatabaseManager.__className, 'Error creating synchronization_file entry : {}'.format(str(error)), logging.ERROR)
            raise(error)
    
    def getCurrentFilesDictionaryTuple(self, synchronizationType):
        getCurrentSyncKeyCommand = ""
        if synchronizationType == SynchronizationType.master:
            getCurrentSyncKeyCommand = "select * from masterSynchronizations ORDER BY syncKey DESC LIMIT 1;"
        elif synchronizationType == SynchronizationType.slave:
            getCurrentSyncKeyCommand = "select * from slaveSynchronizations ORDER BY syncKey DESC LIMIT 1;"
        else:
            raise (Exception('Unimplemented Synchronization Type'))

        with DatabaseManager.__DB_LOCK:
            try:
                cursor = self.__getCursor()
                cursor.execute(getCurrentSyncKeyCommand)
                currentSyncronization = cursor.fetchone()
                if currentSyncronization is not None:
                    currentSyncKey = currentSyncronization["syncKey"]
                    cursor.close()
                    return {"SyncKey": currentSyncKey, "FilesDictionary": self.__getFilesDictionaryForSyncKey(currentSyncKey, synchronizationType)}
                else:
                    return {"SyncKey": None, "FilesDictionary": {}}
            except Exception as error:
                Logger.getInstance().log(DatabaseManager.__className, 'Error Processing Sync Request : {}'.format(str(error)), logging.ERROR)
                raise(error)
    
    def processSyncRequest(self, requestJSON, synchronizationType):
        response = {
            "Status": SyncStatus.serverError.value,
            "SyncKey": 0
        }
        if (
                requestJSON is not None and
                "SyncKey" in requestJSON and
                isinstance(requestJSON["SyncKey"], int) is True and
                requestJSON["SyncKey"] >= 0
            ):
            clientSyncKey = requestJSON["SyncKey"]
            print(clientSyncKey)
            clientFilesDictionary = {}
            adds = {}
            updates ={}
            deletes = {}
            with DatabaseManager.__DB_LOCK:
                try:
                    currentSyncTuple = self.getCurrentFilesDictionaryTuple(synchronizationType)
                    currentSyncKey = currentSyncTuple["SyncKey"]
                    currentFilesDictionary = currentSyncTuple["FilesDictionary"]
                    if currentSyncKey is not None:
                        if (clientSyncKey > currentSyncKey):
                            response["Status"] = SyncStatus.invalidSyncKey.value
                            response["SyncKey"] = 0
                            return response
                        
                        clientFilesDictionary = self.__getFilesDictionaryForSyncKey(clientSyncKey, synchronizationType)
                        for key in currentFilesDictionary: 
                            if not key in clientFilesDictionary:
                                adds[key] = currentFilesDictionary[key]
                            else:
                                for subKey in currentFilesDictionary[key].keys(): 
                                    if (
                                            not subKey in clientFilesDictionary[key] or
                                            currentFilesDictionary[key][subKey] != clientFilesDictionary[key][subKey]
                                        ):
                                        updates[key] = currentFilesDictionary[key]
                                        break
                        for key in clientFilesDictionary.keys(): 
                            if not key in currentFilesDictionary:
                                deletes[key] = clientFilesDictionary[key]
                            else:
                                for subKey in clientFilesDictionary[key].keys(): 
                                    if (not subKey in currentFilesDictionary[key]):
                                        updates[key] = currentFilesDictionary[key]
                                        break
                        response.update( {'Status': SyncStatus.success.value, 'SyncKey': currentSyncKey, 'Changes': {'Adds': adds, 'Updates': updates, 'Deletes': deletes}} )
                except Exception as error:
                    Logger.getInstance().log(DatabaseManager.__className, 'Error Processing Sync Request : {}'.format(str(error)), logging.ERROR)
                    traceback.print_exc()   ## TODO :- Log traceback ##
                    response["SyncKey"] = clientSyncKey
                    response["Status"] = SyncStatus.databaseError.value
        else:
            response["Status"] = SyncStatus.protocolError.value
        print("{} adds".format(len(adds)))
        print("{} updates".format(len(updates)))
        print("{} deletes".format(len(deletes)))
        return response
