**Edit a file, create a new file, and clone from Bitbucket in under 2 minutes**

When you're done, you can delete the content in this README and update the file with details for others getting started with your repository.

*We recommend that you open this README in another tab as you perform the tasks below. You can [watch our video](https://youtu.be/0ocf7u76WSo) for a full demo of all the steps in this tutorial. Open the video in a new tab to avoid leaving Bitbucket.*

---

## Edit a file

You’ll start by editing this README file to learn how to edit a file in Bitbucket.

1. Click **Source** on the left side.
2. Click the README.md link from the list of files.
3. Click the **Edit** button.
4. Delete the following text: *Delete this line to make a change to the README from Bitbucket.*
5. After making your change, click **Commit** and then **Commit** again in the dialog. The commit page will open and you’ll see the change you just made.
6. Go back to the **Source** page.

---

## Create a file

Next, you’ll add a new file to this repository.

1. Click the **New file** button at the top of the **Source** page.
2. Give the file a filename of **contributors.txt**.
3. Enter your name in the empty file space.
4. Click **Commit** and then **Commit** again in the dialog.
5. Go back to the **Source** page.

Before you move on, go ahead and explore the repository. You've already seen the **Source** page, but check out the **Commits**, **Branches**, and **Settings** pages.

---

## Clone a repository

Use these steps to clone from SourceTree, our client for using the repository command-line free. Cloning allows you to work on your files locally. If you don't yet have SourceTree, [download and install first](https://www.sourcetreeapp.com/). If you prefer to clone from the command line, see [Clone a repository](https://confluence.atlassian.com/x/4whODQ).

1. You’ll see the clone button under the **Source** heading. Click that button.
2. Now click **Check out in SourceTree**. You may need to create a SourceTree account or log in.
3. When you see the **Clone New** dialog in SourceTree, update the destination path and name if you’d like to and then click **Clone**.
4. Open the directory you just created to see your repository’s files.

Now that you're more familiar with your Bitbucket repository, go ahead and add a new file locally. You can [push your change back to Bitbucket with SourceTree](https://confluence.atlassian.com/x/iqyBMg), or you can [add, commit,](https://confluence.atlassian.com/x/8QhODQ) and [push from the command line](https://confluence.atlassian.com/x/NQ0zDQ).

---

##	Running the sWall Flask Server

1. Ensure that you have python 3 running on your system. To check what version of python you are running, run the following command:
	- python -- version
2. Clone the repository into any local directory of your choice
3. Once the cloning is done, locate the 'config.txt' file(For security reasons, this file is not pushed to the git repository because it contains user accounts and passwords for logging into the FTP Server and the MYSQL database)
4. In the base directory of the cloned project, create a new folder named 'config' and put the located 'config.txt' file in the created 'config' folder.
5. Start a new instance of 'Command Prompt' and cd into the base directory of the cloned project.
6. After changing the working directory of the command prompt to the cloned project's base directory, run the following command
	- pip install -r requirements.txt
7. Download the community edition of MSQL and set up the MYSQL server configuration.
8. Edit the config.txt file to match the credentials of your local MYSQL database and the credentials of the FTP server you're communicating with.
9. Edit the master-slave assignments by editing 'Master_Slave_Assignments/master_slave.json'. You can have multiple master-slave assignments within the JSON file. Each assignment must contain keys "Master", whose value is the master's ipaddress, and key "Slaves", whose value is an array of strings for each slave within that assignment.
10. Create the database schema by running the following command
	- python ./schema.py
11. Run the server using the following commands
	- python ./app.py