from flask import Flask, render_template, request, send_file, Response, make_response
import mysql.connector
import os
import requests
import json
from mysql.connector import errorcode
from mysql.connector import pooling
import logging
from datetime import datetime
import sys
import zipfile
import io
import magic
import socket

from Logger.Logger import Logger
from DatabaseManager.databaseManager import DatabaseManager
from FTPServerManager.ftpServerManager import FTPServerManager
from EnumManager.enumManager import SynchronizationType, ActivityTagCategory, SyncStatus, FetchStatus, GetConfigurationStatus
from ConfigurationManager.configurationManager import ConfigurationManager
from FTPObserver.ftpObserver import FTPObserver

app = Flask(__name__)
__className = 'APP'
mime = magic.Magic(mime=True)
serverIPAddress = socket.gethostbyname(socket.gethostname())
serverPort = int(
    ConfigurationManager.getInstance().configuration["ServerPort"])


def list_databases(connection):
    cursor = connection.cursor()
    cursor.execute("SHOW DATABASES")
    dbs = cursor.fetchall()

    for db in dbs:
        print(db)

    return dbs


@app.route('/')
def databases():

    res = {}
    return render_template('databases.html', result=res, content_type='application/json')


@app.route('/newBrowserActivity', methods=['POST'])
def newBrowserActivity():
    response = 'Error Adding Browser Activity!'
    requestFile = request.files['file']
    requestJSON = json.load(request.files['data'])
    if requestFile is not None and requestJSON is not None:
        requestJSON['file'] = requestFile
        Logger.getInstance().log(__className,
                                 'Attempting to create new browser activity: {}'.format(requestJSON), logging.INFO)
        try:
            DatabaseManager.getInstance().insertBrowser(requestJSON)
            response = 'Browser Activity Added!'
        except Exception as error:
            response = str(error)
    return response


@app.route('/newGameActivity', methods=['POST'])
def newGameActivity():
    response = 'Error Adding Game Activity!'
    requestFile = request.files['file']
    requestJSON = json.load(request.files['data'])
    if requestFile is not None and requestJSON is not None:
        requestJSON['file'] = requestFile
        Logger.getInstance().log(__className,
                                 'Attempting to create new game activity: {}'.format(requestJSON), logging.INFO)
        try:
            DatabaseManager.getInstance().insertGame(requestJSON)
            response = 'Game Activity Added!'
        except Exception as error:
            response = str(error)
    return response


@app.route('/newMediaActivity', methods=['POST'])
def newMediaActivity():
    response = 'Error Adding Media Activity!'
    requestFile = request.files['file']
    requestJSON = json.load(request.files['data'])
    if requestFile is not None and requestJSON is not None:
        requestJSON['file'] = requestFile
        Logger.getInstance().log(__className,
                                 'Attempting to create new media activity. Data: {}'.format(requestJSON), logging.INFO)
        try:
            DatabaseManager.getInstance().insertMedia(requestJSON)
            response = 'Media Activity Added!'
        except Exception as error:
            response = str(error)
    return response


@app.route('/master-sync', methods=['POST'])
def masterSync():
    # This is done to ensure the FTPObserver is initiated by the FTPServerManager (in case FTPServerManager hasn't been initiated when Sync starts)
    FTPServerManager.getInstance()
    response = {
        "Status": SyncStatus.serverError.value,
        "SyncKey": 0
    }
    try:
        requestJSON = request.get_json()
        Logger.getInstance().log(
            __className, 'Processing Sync Request: {}'.format(requestJSON), logging.INFO)
        response = DatabaseManager.getInstance().processSyncRequest(
            requestJSON, SynchronizationType.master)
    except Exception as error:
        Logger.getInstance().log(__className,
                                 'Error extracting sync request JSON: {}'.format(error), logging.ERROR)
        response["Status"] = SyncStatus.protocolError.value
    return response


@app.route('/slave-sync', methods=['POST'])
def slaveSync():
    # This is done to ensure the FTPObserver is initiated by the FTPServerManager (in case FTPServerManager hasn't been initiated when Sync starts)
    FTPServerManager.getInstance()
    response = {
        "Status": SyncStatus.serverError.value,
        "SyncKey": 0
    }
    try:
        requestJSON = request.get_json()
        Logger.getInstance().log(
            __className, 'Processing Sync Request: {}'.format(requestJSON), logging.INFO)
        response = DatabaseManager.getInstance().processSyncRequest(
            requestJSON, SynchronizationType.slave)
    except Exception as error:
        Logger.getInstance().log(__className,
                                 'Error extracting sync request JSON: {}'.format(error), logging.ERROR)
        response["Status"] = SyncStatus.protocolError.value
    return response


@app.route('/fetch-directory', methods=['GET'])
def fetchDirectory():
    directoryName = request.args.get('directoryName')
    print(directoryName)
    response = 'Error Adding Media Activity!'
    try:
        configuration = ConfigurationManager.getInstance().configuration
        downloadsInProgressFolderName = configuration["Downloads_in_progress_folder"]
        activitiesFolderPath = os.path.join(
            os.getcwd(), downloadsInProgressFolderName, '{}.zip'.format(directoryName))
        if(os.path.exists(activitiesFolderPath) is False):
            FTPServerManager.getInstance().download_ftp_tree(directoryName)
        return send_file(activitiesFolderPath, mimetype='application/zip', as_attachment=True, attachment_filename='{}.zip'.format(directoryName))
    except Exception as error:
        response = str(error)
    return response


@app.route('/get-configuration', methods=['POST'])
def getConfiguration():
    ipAddress = ""
    response = {
        "Status": GetConfigurationStatus.serverError.value,
        "Configuration": {}
    }
    try:
        print(request.get_data())
        requestJSON = request.get_json()
        ipAddress = requestJSON['IPAddress']
    except Exception as error:
        Logger.getInstance().log(__className,
                                 'Error extracting get-configuration request: {}'.format(error), logging.ERROR)
        response['Status'] = GetConfigurationStatus.protocolError.value
    try:
        response["Configuration"] = ConfigurationManager.getInstance(
        ).getMasterSlaveConfigForIP(ipAddress)
        response['Status'] = GetConfigurationStatus.success.value
    except Exception as error:
        Logger.getInstance().log(__className,
                                 'Error getting configuration for ipAddress {}: {}'.format(ipAddress, error), logging.ERROR)
        response['Status'] = GetConfigurationStatus.serverError.value
    return response


@app.route('/fetch-file', methods=['POST'])
def fetchFile():
    responseToSend = {
        "responseJSON": {
            "Status": SyncStatus.serverError.value
        }
    }
    response = make_response()
    try:
        requestJSON = request.get_json()
        Logger.getInstance().log(
            __className, 'Processing Fetch Request: {}'.format(requestJSON), logging.INFO)
        responseToSend = FTPServerManager.getInstance().processFetchFileRequest(requestJSON)
        if (
            "responseFilePath" in responseToSend and
            isinstance(responseToSend["responseFilePath"], str) is True
        ):
            responseFilePath = responseToSend["responseFilePath"]
            with open(responseFilePath, 'rb') as fileToSend:
                response = make_response(fileToSend.read())
                response.headers["Content-Type"] = mime.from_file(
                    responseFilePath)
                response.headers["Content-Disposition"] = "attachment; filename={}".format(
                    fileToSend.name)
    except Exception as error:
        Logger.getInstance().log(__className,
                                 'Error extracting fetch request JSON: {}'.format(error), logging.ERROR)
        responseToSend.update(
            {'responseJSON': {'Status': FetchStatus.protocolError.value}})

    response.headers['X-Response-JSON'] = responseToSend["responseJSON"]
    return response


@app.route('/simulate-new-browser-activity')
def simulate_new_browser_activity():
    _url = 'http://{}:{}/newBrowserActivity'.format(
        serverIPAddress, serverPort)
    requestJSON = {
        "name": 'Test Browser Activity',
        "ownerId": 'sWall-01',
        "creationTime": '2019-10-25',
        "lastUpdateTime": '2019-10-25',
        "logoPath": 'logo.png',
        "description": 'This is a sample browser activity for development',
        "executablePath": 'testBrowserPath',
        "url": 'testBrowserUrl'
    }
    requestJSONString = json.dumps(requestJSON)
    Logger.getInstance().log(__className, 'Simulating the creation of a browser activity: {}'.format(
        str(requestJSONString)), logging.INFO)
    req = requests.post(url=_url, json=requestJSONString)
    Logger.getInstance().log(__className,
                             'Response received for creation of a browser activity: {}'.format(str(req.text)), logging.INFO)
    return req.text


@app.route('/simulate-new-game-activity')
def simulate_new_game_activity():
    _url = 'http://{}:{}/newGameActivity'.format(serverIPAddress, serverPort)
    requestJSON = {
        "name": 'Test Game Activity',
        "ownerId": 'sWall-01',
        "creationTime": '2019-10-25',
        "lastUpdateTime": '2019-10-25',
        "logoPath": 'logo.png',
        "description": 'This is a sample game activity for development',
        "executablePath": 'testGamePath'
    }
    requestJSONString = json.dumps(requestJSON)
    Logger.getInstance().log(__className, 'Simulating the creation of a game activity: {}'.format(
        str(requestJSONString)), logging.INFO)
    with open('testGame.txt', 'rb') as f:
        files = [
            ('file', (f.name, f, 'application/octet')),
            ('data', ('data', requestJSONString, 'application/json')),
        ]
        req = requests.post(url=_url, files=files)
        Logger.getInstance().log(__className,
                                 'Response received for creation of a game activity: {}'.format(str(req.text)), logging.INFO)
        return req.text


@app.route('/simulate-new-media-activity')
def simulate_new_media_activity():
    _url = 'http://{}:{}/newMediaActivity'.format(serverIPAddress, serverPort)
    requestJSON = {
        "name": 'Test Media Activity 2',
        "ownerId": 'sWall-01',
        "creationTime": '2019-10-25',
        "lastUpdateTime": '2019-10-25',
        "logoPath": 'logo.png',
        "description": 'This is a sample media activity for development',
        "width": 1920,
        "height": 1080,
        "tags": [
            {"tagName": 'Media', "tagCategory": ActivityTagCategory.activityType.value},
            {"tagName": 'Engineering',
                "tagCategory": ActivityTagCategory.studyArea.value},
            {"tagName": 'Engineering', "tagCategory": ActivityTagCategory.careers.value}
        ]
    }
    requestJSONString = json.dumps(requestJSON)
    Logger.getInstance().log(__className, 'Simulating the creation of a media activity: {}'.format(
        str(requestJSONString)), logging.INFO)
    with open('requirements.txt', 'rb') as f:
        files = [
            ('file', (f.name, f, 'application/octet')),
            ('data', ('data', requestJSONString, 'application/json')),
        ]
        req = requests.post(url=_url, files=files)
        Logger.getInstance().log(__className,
                                 'Response received for creation of a media activity: {}'.format(str(req.text)), logging.INFO)
        return req.text


@app.route('/simulate-fetch-directory')
def simulate_fetch_directory():
    requestParameters = {'directoryName': 'Resources'}
    _url = 'http://{}:{}/fetch-directory'.format(serverIPAddress, serverPort)
    req = requests.get(_url, params=requestParameters, stream=True)
    zipBytes = io.BytesIO(req.content)
    zipfile.ZipFile(zipBytes).extractall()
    return 'Simulation Successful'


@app.route('/simulate-get-configuration')
def simulate_get_configuration():
    requestJSON = {
        "IPAddress": "134.117.60.180"
    }
    _url = 'http://{}:{}/get-configuration'.format(serverIPAddress, serverPort)
    req = requests.post(url=_url, json=requestJSON)
    return req.text


@app.route('/simulate-master-sync')
def simulate_master_sync():
    requestJSON = {
        "SyncKey": 0
    }
    _url = 'http://{}:{}/master-sync'.format(serverIPAddress, serverPort)
    req = requests.post(url=_url, json=requestJSON)
    return req.text


@app.route('/simulate-slave-sync')
def simulate_slave_sync():
    requestJSON = {
        "SyncKey": 0
    }
    _url = 'http://{}:{}/slave-sync'.format(serverIPAddress, serverPort)
    req = requests.post(url=_url, json=requestJSON)
    return req.text


@app.route('/simulate-fetch-file')
def simulate_fetch_file():
    requestJSON = {
        "FilePath": "Activities/Game/CampusNavigation/NavigationApp_Data/level1.resS"
    }
    _url = 'http://{}:{}/fetch-file'.format(serverIPAddress, serverPort)
    req = requests.post(url=_url, json=requestJSON)
    print(req.headers)
    return req.text


if __name__ == "__main__":
    Logger.getInstance().log(__className, 'Starting Local Development Server.', logging.INFO)
    # Do not use the run function in production!!! See run's description for more information!
    app.run(debug=True, host=serverIPAddress, port=serverPort)
