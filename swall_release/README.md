# sWall_Release

## How to Initialize the system Instructions 

**Initial software Requirement**

* Windows OS  
* Git 
* Administrative rights on the system 
* Add the ip address's of Master and slave to the swall server 

### Running the master
1. Install the swall_release from git hub 

	>git clone https://swall_SCE@bitbucket.org/swall_SCE/swall_release.git


2. Go into the Master folder 

	>Run setup.exe as administrator
	
	
3. The swall master wil install with the name master -> Run the Master application as administrator 

4. The system wil Run and a white screen will appear on the master Screen. Your master is up and Running. 


### Running Slave 
1. Make sure Master is running before you run slaves 

2. Install the swall_release from git hub 

	>git clone https://swall_SCE@bitbucket.org/swall_SCE/swall_release.git

3. The swall Slave wil install with the name sWall_Slave -> Run the sWall_Slave application as administrator 

4. The system wil Run and a background image will appear, after that the screen will beblack as it waits for other slaves to connect

5. Once all the slave are connected the system will run and will start displaying images as in the ftp serve. 