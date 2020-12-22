import logging
import logging.handlers
import os
from datetime import datetime

from ConfigurationManager.configurationManager import ConfigurationManager

"""
Ensure this class only has one instance, and provide a global point of
access to it.
"""


class Logger:
    __instance = None
    __className = 'LOGGER'

    @staticmethod
    def getInstance():
        """ Static access method. """
        if Logger.__instance == None:
            Logger()
        return Logger.__instance

    def __init__(self):
        """ Virtually private constructor. """
        if Logger.__instance != None:
            raise Exception("This class is a singleton!")
        else:
            Logger.__instance = self
            self.__configure()

    def __configure(self):
        configuration = ConfigurationManager.getInstance().configuration
        self.logger = logging.getLogger('sWall_database_Logger')
        self.logger.setLevel(logging.DEBUG)
        # Create directory for logging
        try:
            if os.path.exists('logs') is False:
                os.mkdir('logs')
        except Exception as error:
            raise(error)

        # File handler with logging capabilities
        fileHandler = logging.FileHandler(
            'logs/swall-database-logger-{}.log'.format(datetime.today().strftime('%Y-%m-%d')))
        fileHandler.setLevel(logging.DEBUG)
        # Set up the SMTP handler for sending error emails to service
        smtp_handler = logging.handlers.SMTPHandler(mailhost=(configuration["SMTPHost"], configuration["SMTPPort"]),
                                                    fromaddr=configuration["SMTPFrom"],
                                                    toaddrs=configuration["SMTPTo"],
                                                    credentials=(
                                                        configuration["SMTPUsername"], configuration["SMTPPassword"]),
                                                    secure=(),
                                                    timeout=30,
                                                    subject=u"sWall Error!")
        smtp_handler.setLevel(logging.ERROR)
        # Console handler with higher log level
        consoleHandler = logging.StreamHandler()
        consoleHandler.setLevel(logging.ERROR)
        # Create and add formatter to handlers
        formatter = logging.Formatter(
            '%(asctime)s - %(name)s - %(levelname)s - %(message)s')
        fileHandler.setFormatter(formatter)
        consoleHandler.setFormatter(formatter)
        # Add handlers to logger
        self.logger.addHandler(fileHandler)
        self.logger.addHandler(consoleHandler)
        self.logger.addHandler(smtp_handler)
        self.log(Logger.__className, 'Logger active!', logging.INFO)

    def log(self, className, message, level):
        classMessage = className + ": " + message
        if (level == logging.CRITICAL or level == logging.FATAL):
            self.logger.critical(classMessage)
        elif (level == logging.ERROR):
            self.logger.error(classMessage)
        elif (level == logging.WARNING or level == logging.WARN):
            self.logger.warning(classMessage)
        elif (level == logging.INFO):
            self.logger.info(classMessage)
        elif (level == logging.DEBUG):
            self.logger.debug(classMessage)
        else:
            self.logger.error(
                "UNIMPLEMENTED LOGGING LEVEL '{}': {}".format(level, classMessage))
