import enum


class ActivityType(enum.Enum):
    browser = 1
    game = 2
    media = 3


class ActivityTagCategory(enum.Enum):
    activityType = 1
    careers = 2
    studyArea = 3
    applications = 4
    theDepartment = 5


class FTPBASEDIRECTORIES(enum.Enum):
    CurrentDirectory = '.'
    Upload = 'upload'
    sWall = 'upload/sWall'
    slaveFiles = 'upload/sWall/Slave_Files'
    Activities = 'upload/sWall/Slave_Files/Activities'
    Resources = 'upload/sWall/Resources'
    Browser = 'upload/sWall/Slave_Files/Activities/Browser'
    Game = 'upload/sWall/Slave_Files/Activities/Game'
    Media = 'upload/sWall/Slave_Files/Activities/Media'


class SynchronizationType(enum.Enum):
    master = 1
    slave = 2


class SyncStatus(enum.Enum):
    success = 1
    invalidSyncKey = 2
    protocolError = 3
    serverError = 4
    databaseError = 5


class FetchStatus(enum.Enum):
    success = 1
    protocolError = 2
    serverError = 3
    fileNotFound = 4
    fileIsDirectory = 5
    fileEmpty = 6


class GetConfigurationStatus(enum.Enum):
    success = 1
    protocolError = 2
    serverError = 3
