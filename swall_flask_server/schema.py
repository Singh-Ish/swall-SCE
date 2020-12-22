# creates the schema at a mySQL endpoint.
# Current endpoint details in config file.
# Schema class contains a 'create' method to create the defined tables,
# and a 'delete' method to delete existing tables before recreating the schema.
# Other schema related methods should be added in this file.

import mysql.connector  # pip install mysql-connector
from mysql.connector import errorcode
from mysql.connector import Error

from collections import OrderedDict

from ConfigurationManager.configurationManager import ConfigurationManager


class Schema:
    # returns a tupple of tupples of the attribute names and data types from each table defined below
    getAttributesFromTable = staticmethod(lambda table: tuple(tuple(attribute.lstrip().split(
        ' ')[:2]) for attribute in table.split('(', 1)[-1].rsplit(',  PRIMARY KEY', 1)[0].split(',')))
    # dependent on the format of tables below - so ensure strict format is followed

    databaseCreate = (
        "CREATE DATABASE IF NOT EXISTS {}".format(
            ConfigurationManager.getInstance().configuration['DatabaseName'])
    )

    databaseSelect = (
        "USE {}".format(
            ConfigurationManager.getInstance().configuration['DatabaseName'])
    )

    databaseDelete = (
        "DROP DATABASE {}".format(
            ConfigurationManager.getInstance().configuration['DatabaseName'])
    )

    synchronizationType = (
        "CREATE TABLE synchronizationType ("
        "  synchronizationTypeId INT PRIMARY KEY,"
        "  synchronizationType VARCHAR(255) NOT NULL"
        ")"
    )

    synchronizationTypeInserts = (
        "INSERT INTO synchronizationType"
        "   select 1,'Master' union all"
        "   select 2,'Slave';"
    )

    synchronizations = (
        "CREATE TABLE synchronizations ("
        "   syncKey BIGINT UNSIGNED PRIMARY KEY auto_increment,"
        "   synchronizationTypeId INT REFERENCES synchronizationType(synchronizationTypeId),"
        "   constraint synchronizations_AltPK unique (syncKey, synchronizationTypeId)"
        ");"
    )

    masterSynchronizations = (
        "CREATE TABLE masterSynchronizations ("
        "   syncKey BIGINT UNSIGNED PRIMARY KEY,"
        "   synchronizationTypeId INT NOT NULL DEFAULT 1 CHECK (synchronizationTypeId = 1),"
        "   FOREIGN KEY(syncKey, synchronizationTypeId) REFERENCES synchronizations(syncKey, synchronizationTypeId)"
        ");"
    )

    slaveSynchronizations = (
        "CREATE TABLE slaveSynchronizations ("
        "   syncKey BIGINT UNSIGNED PRIMARY KEY,"
        "   synchronizationTypeId INT NOT NULL DEFAULT 2 CHECK (synchronizationTypeId = 2),"
        "   FOREIGN KEY(syncKey, synchronizationTypeId) REFERENCES synchronizations(syncKey, synchronizationTypeId)"
        ");"
    )

    files = (
        "CREATE TABLE files ("
        "   fileId BIGINT UNSIGNED PRIMARY KEY auto_increment,"
        "   path VARCHAR(255) NOT NULL,"
        "   UNIQUE KEY unique_filePath (path)"
        ");"
    )

    masterSynchronization_files = (
        "CREATE TABLE masterSynchronization_files ("
        "   syncKey BIGINT UNSIGNED,"
        "   fileId BIGINT UNSIGNED,"
        "   fileModificationTime VARCHAR(255) NOT NULL,"
        "   fileSize BIGINT NOT NULL,"
        "   FOREIGN KEY(syncKey) REFERENCES masterSynchronizations(syncKey),"
        "   FOREIGN KEY(fileId) REFERENCES files(fileId),"
        "   UNIQUE KEY unique_masterSyncFile (syncKey, fileId)"
        ");"
    )

    slaveSynchronization_files = (
        "CREATE TABLE slaveSynchronization_files ("
        "   syncKey BIGINT UNSIGNED,"
        "   fileId BIGINT UNSIGNED,"
        "   fileModificationTime VARCHAR(255) NOT NULL,"
        "   fileSize BIGINT NOT NULL,"
        "   FOREIGN KEY(syncKey) REFERENCES slaveSynchronizations(syncKey),"
        "   FOREIGN KEY(fileId) REFERENCES files(fileId),"
        "   UNIQUE KEY unique_slaveSyncFile (syncKey, fileId)"
        ");"
    )

    activityType = (
        "CREATE TABLE activityType ("
        "  activityTypeId INT PRIMARY KEY,"
        "  activityType VARCHAR(255) NOT NULL"
        ")"
    )

    activityTypesInserts = (
        "INSERT INTO activityType"
        "   select 1,'Browser' union all"
        "   select 2,'Game' union all"
        "   select 3,'Media';"
    )

    activities = (
        "CREATE TABLE activities ("
        "   activityId INT PRIMARY KEY auto_increment,"
        "   activityTypeId INT REFERENCES activityType(activityTypeId),"
        "   name VARCHAR(255) UNIQUE NOT NULL,"
        "   filename VARCHAR(255) NOT NULL,"
        "   ownerId VARCHAR(255) NOT NULL,"
        "   creationTime VARCHAR(255) NOT NULL,"
        "   lastUpdateTime VARCHAR(255) NOT NULL,"
        "   logoPath VARCHAR(255) NOT NULL,"
        "   description VARCHAR(255) NOT NULL,"
        "   constraint activities_AltPK unique (activityId, activityTypeId)"
        ");"
    )

    browser = (
        "CREATE TABLE browser ("
        "   activityId INT PRIMARY KEY,"
        "   activityTypeId int not null default 1 check (activityTypeId = 1),"
        "   executablePath VARCHAR(255) NOT NULL,"
        "   url VARCHAR(255) NOT NULL,"
        "   FOREIGN KEY(activityId, activityTypeId) REFERENCES activities(activityId, activityTypeId)"
        ");"
    )

    game = (
        "CREATE TABLE game ("
        "   activityId INT PRIMARY KEY,"
        "   activityTypeId int not null default 2 check (activityTypeId = 2),"
        "   executablePath VARCHAR(255) NOT NULL,"
        "   FOREIGN KEY(activityId, activityTypeId) REFERENCES activities(activityId, activityTypeId)"
        ");"
    )

    media = (
        "CREATE TABLE media ("
        "   activityId INT PRIMARY KEY,"
        "   activityTypeId int not null default 3 check (activityTypeId = 3),"
        "   width INT NOT NULL,"
        "   height INT NOT NULL,"
        "   FOREIGN KEY(activityId, activityTypeId) REFERENCES activities(activityId, activityTypeId)"
        ");"
    )

    tag = (
        "CREATE TABLE tag ("
        "  tagId INT PRIMARY KEY,"
        "  tagCategory VARCHAR(255) NOT NULL"
        ")"
    )

    tagInserts = (
        "INSERT INTO tag"
        "   select 1,'Activity Type' union all"
        "   select 2,'Careers' union all"
        "   select 3,'Study Area' union all"
        "   select 4,'Applications' union all"
        "   select 5,'The Department';"
    )

    activityTag = (
        "CREATE TABLE activityTag ("
        "   activityId INT NOT NULL,"
        "   tagId INT NOT NULL,"
        "   tagName VARCHAR(255),"
        "   PRIMARY KEY (activityId, tagId, tagName),"
        "   FOREIGN KEY(activityId) REFERENCES activities(activityId),"
        "   FOREIGN KEY(tagId) REFERENCES tag(tagId)"
        ");"
    )

    def __init__(self):
        # dictionary of all commands (creates, inserts, etc.) to be executed in the database, in execution order.
        self.commands = OrderedDict()
        self.commands['databaseCreate'] = Schema.databaseCreate
        self.commands['databaseSelect'] = Schema.databaseSelect
        self.commands['synchronizationType'] = Schema.synchronizationType
        self.commands['synchronizationTypeInserts'] = Schema.synchronizationTypeInserts
        self.commands['synchronizations'] = Schema.synchronizations
        self.commands['masterSynchronizations'] = Schema.masterSynchronizations
        self.commands['slaveSynchronizations'] = Schema.slaveSynchronizations
        self.commands['files'] = Schema.files
        self.commands['masterSynchronization_files'] = Schema.masterSynchronization_files
        self.commands['slaveSynchronization_files'] = Schema.slaveSynchronization_files
        self.commands['activityType'] = Schema.activityType
        self.commands['activityTypesInserts'] = Schema.activityTypesInserts
        self.commands['activities'] = Schema.activities
        self.commands['browser'] = Schema.browser
        self.commands['game'] = Schema.game
        self.commands['media'] = Schema.media
        self.commands['tag'] = Schema.tag
        self.commands['tagInserts'] = Schema.tagInserts
        self.commands['activityTag'] = Schema.activityTag

    def delete(self, connection):

        cursor = connection.cursor()
        try:
            print("Deleting Database using command {}".format(
                Schema.databaseDelete))
            cursor.execute(Schema.databaseDelete)
        except mysql.connector.Error as err:
            print(str(err))
        else:
            print("Success")
        connection.commit()

    def create(self, connection):

        cursor = connection.cursor()
        for command_name in self.commands:
            command = self.commands[command_name]
            try:
                print("Creating table {}: ".format(command_name))
                cursor.execute(command)
            except mysql.connector.Error as err:
                connection.rollback()
                print(str(err))
                return
            else:
                print("Success")
        connection.commit()


def DbConnectAndExecute(connection):
    s = Schema()
    # execute any statements
    # be careful not to delete tables in production environment. Consider checking dev flag in config.
    s.delete(connection)
    s.create(connection)
    # done executing


if __name__ == "__main__":
    config = ConfigurationManager.getInstance().configuration
    try:
        print('Attempting to connect to MySQL database.')
        connection = mysql.connector.connect(host=config['DatabaseHost'], port=int(
            config['DatabasePort']), user=config['DatabaseUser'], password=config['DatabasePassword'], auth_plugin='mysql_native_password')
        connection.autocommit = False
        DbConnectAndExecute(connection)
        connection.close()
    except mysql.connector.Error as err:
        print('DATABASE CONNECTION ERROR: {}'.format(str(err)))
