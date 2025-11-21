**Hosting server installation**
* Install Alpine Linux into your server, setup SSH key for root password-less access.
	* If Installing into Hyper-V VM:
		* Disable secure boot
		* Match the number of CPUs with the production server (e.g. 2 CPUs)
	* Mount Apline Linux [iso](https://alpinelinux.org/downloads) (for virtual machines there is a smaller virtual iso)
	* Login as root (no password)
	* Run `setup-alpine` (install the system into a disk)
	* `Reboot`
	* Login as root with a password
	* Install nano editor: `apk add nano`
	* Temporarily allow root password login if disabled: `nano /etc/ssh/sshd_config` and set:
		* `PasswordAuthentication` to `yes`
		* `PermitRootLogin` to `yes`
	* Restart SSH server: `rc-service sshd restart`
	* Generate SSH keys on your working machine: `ssh-keygen -t ed25519`
	* Create .ssh directory: `mkdir -p .ssh`
	* Set permissions: `chmod 700 .ssh`
	* Create authorized_keys file: `nano .ssh/authorized_keys`
	* Copy the content of the public key (`id_ed25519.pub`) from your working machine into `authorized_keys` file; save and exit (in nano editor: Ctrl+O, CTRL+X)
	* Remove password login: ``nano /etc/ssh/sshd_config`` and set
		* `PasswordAuthentication` to `no`
		* `PermitRootLogin` to `prohibit-password`
		* `AllowTcpForwarding` to `yes` (to be able to get to the postgres DB via SHH tunnel)
	* Restart SSH server: `rc-service sshd restart`
	* Optionally change the IP address to static: `nano /etc/network/interfaces`, example:
	        
		```													
        iface eth0 inet static
            address 192.168.0.123
            netmask 255.255.255.0
            gateway 192.168.0.1
	        dns-nameservers 8.8.8.8 8.8.4.4
	 
        iface eth0 inet6 static
            address 2001:0db8:85a3:0000:0000:8a2e:0370:7334
            netmask 64
            gateway 2001:0db8:85a3:0000::1 
	        dns-nameservers 2001:4860:4860::8888 2001:4860:4860::8844
        ```
	    restart networking: `rc-service networking restart`, and configure SFP1 TXT `ip4` and `ip6` record at your domain registrar.
	* Install docker: 
		* `nano /etc/apk/repositories` - uncomment line http://dl-cdn.alpinelinux.org/alpine/v3.22/community
		* `apk add docker`
		* `rc-update add docker boot`
		* `service docker start`
	* Deploy the app via Kamal - see [Deployment](Deployment.md)
	* Install Traefik on the host:
		* `./install-traefik-to-kamal-proxy.sh --domain <domain one> [--domain <domain two>] --email email@for-lets-encrypt-certificate`


