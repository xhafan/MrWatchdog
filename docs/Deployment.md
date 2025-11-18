**Deployment**
* Install Kamal (https://kamal-deploy.org/docs/installation/); On Windows, install Kamal into WSL Ubuntu:
	* `sudo apt update`
	* `sudo apt install -y ruby ruby-dev build-essential git`
	* `sudo gem install kamal`
	* `kamal version`
 	* Install `jq` to parse JSON: `sudo apt install jq -y`
	* Optionally link existing Windows user's `.kamal` directory: `ln -s /mnt/c/Users/<your windows user name>/.kamal ~/.kamal`

* Install private SSH key:	
	* `mkdir -p ~/.ssh`
	* `chmod 700 ~/.ssh`
	* `cp /mnt/c/Users/<your windows user name>/.ssh/<private SSH key> ~/.ssh/`
	* `chmod 600 ~/.ssh/<private SSH key>`
	* `ssh root@<hosting server name>` - check the connection to the hosting server
	
* Deployment:
	* Setup staging: `kamal setup -d staging` (run only once)
	* Deploy staging: `kamal deploy -d staging`
	* Setup production: `kamal setup -d production` (run only once)
	* Deploy production: `kamal deploy -d production`

