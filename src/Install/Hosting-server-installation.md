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
	* Create a 2 GiB swap file if `free -h` reports zero swap and `cat /proc/swaps` prints only its header:
		* Create the file: `dd if=/dev/zero of=/swapfile bs=1M count=2048`
		* Allow only root to access it: `chmod 600 /swapfile`
		* Format and enable it: `mkswap /swapfile && swapon /swapfile`
		* Run `grep -qxF '/swapfile none swap defaults 0 0' /etc/fstab || printf '%s\n' '/swapfile none swap defaults 0 0' >> /etc/fstab`
		* Enable swap during boot: `rc-update add swap boot`
		* Verify the result: `free -h` and `cat /proc/swaps`
	* Collect historical memory and swap statistics with `sysstat` and retain them for 31 days:
		* Install `sysstat`: `apk add sysstat`
		* Set the retention period: `sed -i 's/^HISTORY=.*/HISTORY=31/' /etc/sysconfig/sysstat`
		* Enable and start `crond`; `sysstat` uses cron rather than a persistent service: `rc-update add crond default && rc-service crond start`
		* Collect statistics every 10 minutes: `grep -qxF '*/10 * * * * /usr/lib/sa/sa1 1 1' /etc/crontabs/root || echo '*/10 * * * * /usr/lib/sa/sa1 1 1' >> /etc/crontabs/root`
		* Generate the daily report and remove expired data shortly after midnight: `grep -qxF '5 0 * * * /usr/lib/sa/sa2 -A' /etc/crontabs/root || echo '5 0 * * * /usr/lib/sa/sa2 -A' >> /etc/crontabs/root`
		* Apply the new schedule and create the first sample: `rc-service crond restart && /usr/lib/sa/sa1 1 1`
		* Verify collection: `rc-service crond status`, `sar -S`, `sar -W`, and `du -sh /var/log/sa`
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
	* Synchronize time via NTP
	  	* `apk add openntpd`
        * `rc-update add openntpd`
        * `service openntpd start`
	* Install docker: 
		* `nano /etc/apk/repositories` - uncomment line http://dl-cdn.alpinelinux.org/alpine/v3.22/community
		* `apk add docker`
		* `rc-update add docker default` - use the `default` runlevel rather than `boot`. The Docker OpenRC service depends on networking, which starts in the `default` runlevel. Registering Docker in `boot` can race network initialization and leave Docker stopped after a reboot.
		* `service docker start`
		* If you manage docker via `alpine` user: `addgroup alpine docker`
	* Deploy the app via Kamal - see [Deployment](Deployment.md)
	* Install Caddy reverse proxy on the host:
		* `apk update`
		* `apk add --no-cache curl`
		* `curl -L "https://raw.githubusercontent.com/xhafan/MrWatchdog/refs/heads/main/src/Install/install-caddy-to-kamal-proxy.sh" -o ./install-caddy-to-kamal-proxy.sh`
		* `chmod +x install-caddy-to-kamal-proxy.sh`
		* `./install-caddy-to-kamal-proxy.sh --domains "<domain one> <domain two>"`
