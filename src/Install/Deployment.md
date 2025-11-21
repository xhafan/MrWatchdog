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
	* Setup `<destination>` (e.g. `staging`, `production`): `kamal setup -d <destination>` (run only once)
	* Deploy `<destination>` again after initial setup: `kamal deploy -d <destination>`

* After initial deployment, change Kamal proxy to listen on port 8080, HTTP only (no HTTPS).
  The incoming traffic to the host is handled by Traefik installed on the host, which forwards 
  requests to Kamal proxy over HTTP on port 8080:
	* `kamal proxy boot_config reset -d <destination>`
	* `kamal proxy boot_config set --no-publish --docker-options="publish 127.0.0.1:8080:80" -d <destination>`
	* `kamal proxy reboot -d <destination>`

