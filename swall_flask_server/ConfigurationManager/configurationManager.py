import os
import json

"""
Ensure this class only has one instance, and provide a global point of
access to it.
"""


class ConfigurationManager:
    __instance = None

    @staticmethod
    def getInstance():
        """ Static access method. """
        if ConfigurationManager.__instance == None:
            ConfigurationManager()
        return ConfigurationManager.__instance

    def __init__(self):
        """ Virtually private constructor. """
        if ConfigurationManager.__instance != None:
            raise Exception("This class is a singleton!")
        else:
            ConfigurationManager.__instance = self
            self.configPath = 'config/config.txt'
            self.masterSlaveConfigPath = 'Master_Slave_Assignments/master_slave.json'
            self.devTrigger = 'Dev'
            self.configuration = self.__parseConfig()

    def isDevMode(self):
        parsedConfig = self.__parseConfig()
        if 'True' in parsedConfig[self.devTrigger]:
            return True
        return False

    def getMasterSlaveConfigForIP(self, ipAddressString):
        with open(self.masterSlaveConfigPath, 'r') as f:
            masterSlaveAssignment = json.load(f)
            for masterSlaveConfiguration in masterSlaveAssignment['MSConfiguration']:
                if (masterSlaveConfiguration['Master'] == ipAddressString or ipAddressString in masterSlaveConfiguration['Slaves']):
                    return masterSlaveConfiguration
        return {}

    def __parseConfig(self):
        pairs = {}
        with open(self.configPath, 'r') as configFile:
            for line in configFile:
                if len(line) > 1 and '#' not in line:
                    stringArray = line.strip().split(',')
                    pairs[stringArray[0]] = stringArray[1]
        return pairs
