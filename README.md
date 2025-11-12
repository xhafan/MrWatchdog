A watchdog platform for any publicly accessible web page on the internet.

Use it to watch any publicly accessible web page for changes and get notified by email when it changes or your search term is found.

View existing public watchdogs and set up email notifications about new watchdog search results.

----
To run the solution locally, you need to setup user secrets first. See <a href="src/user_secrets_init.bat.example">user_secrets_init.bat.example</a>.

Deployment:
* Install Kamal (https://kamal-deploy.org/docs/installation/); On Windows, install Kamal into WSL Ubuntu:
	* `sudo apt update`
	* `sudo apt install -y ruby ruby-dev build-essential git`
	* `sudo gem install kamal`
	* `kamal version`
	* `mkdir -p ~/.ssh`
	* `chmod 700 ~/.ssh`
	* `cp /mnt/c/Users/<your windows user name>/.ssh/<private SSH key> ~/.ssh/`
	* `chmod 600 ~/.ssh/<private SSH key>`
	* `ssh <your user name>@<server name>` - check the connection
	* `ln -s /mnt/c/Users/<your windows user name>/.kamal ~/.kamal`
	* `sudo apt install jq -y`
	* `nano /etc/ssh/sshd_config`
		* set `PasswordAuthentication no`		
		* set `AllowTcpForwarding yes`
	* `rc-service sshd restart`
* Install Alpine Linux into your server, setup SSH key for root password-less access.

