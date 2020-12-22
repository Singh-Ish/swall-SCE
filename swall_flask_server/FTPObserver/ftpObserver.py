import time
from time import sleep
import logging
import threading
import weakref

from Logger.Logger import Logger
from DatabaseManager.databaseManager import DatabaseManager
from EnumManager.enumManager import SynchronizationType


class FTPObserver():
    __instance = None
    __className = 'FTP Observer'

    patterns = "*"

    @staticmethod
    def getInstance():
        """ Static access method. """
        if FTPObserver.__instance == None:
            FTPObserver()
        return FTPObserver.__instance

    def __init__(self):
        """ Virtually private constructor. """
        if FTPObserver.__instance != None:
            raise Exception("This class is a singleton!")
        else:
            FTPObserver.__instance = self

    def observe(self, delegate, dir='.'):
        thread = threading.Thread(target=self.__observe, args=(delegate, dir))
        thread.daemon = True
        thread.start()
        return

    def __observe(self, delegate, dir):
        previousMasterDirectoryDictionary = delegate.getCurrentFilesDictionary(
            SynchronizationType.master)
        previousSlaveDirectoryDictionary = delegate.getCurrentFilesDictionary(
            SynchronizationType.slave)

        while True:
            print("Observing...")
            currentMasterDirectoryDictionary = delegate.getFTPListRecursively(
                dir, SynchronizationType.master)
            currentSlaveDirectoryDictionary = delegate.getFTPListRecursively(
                dir, SynchronizationType.slave)

            if self.__checkForChanges(previousMasterDirectoryDictionary, currentMasterDirectoryDictionary):
                print("Master Change Detected")
                delegate.didObserveFTPChanges(
                    currentMasterDirectoryDictionary, SynchronizationType.master)
            if self.__checkForChanges(previousSlaveDirectoryDictionary, currentSlaveDirectoryDictionary):
                print("Slave Change Detected")
                delegate.didObserveFTPChanges(
                    currentSlaveDirectoryDictionary, SynchronizationType.slave)

            previousMasterDirectoryDictionary = currentMasterDirectoryDictionary
            previousSlaveDirectoryDictionary = currentSlaveDirectoryDictionary
            print("Done observing...")
            sleep(20)

    def __checkForChanges(self, previousDirectoryDictionary, currentDirectoryDictionary):
        changeDetected = False
        for key in currentDirectoryDictionary.keys():
            if (key in previousDirectoryDictionary):
                for subKey in currentDirectoryDictionary[key].keys():
                    if (
                        not subKey in previousDirectoryDictionary[key] or
                        currentDirectoryDictionary[key][subKey] != previousDirectoryDictionary[key][subKey]
                    ):
                        print("Update Detected. {}".format(key))
                        changeDetected = True
                        break
            else:
                print("Add Detected. {}".format(key))
                changeDetected = True
                break
            for key in previousDirectoryDictionary.keys():
                if changeDetected is True:
                    break
                elif not key in currentDirectoryDictionary:
                    print("Delete Detected. {}".format(key))
                    changeDetected = True
                else:
                    for subKey in previousDirectoryDictionary[key].keys():
                        if (not subKey in currentDirectoryDictionary[key]):
                            print("Update Detected. {}".format(key))
                            changeDetected = True
                            break
        return changeDetected
