# Home Server
a home server for my Raspberry Pi 3

This server woks together with FirePlayer, a Media Player for AMAZON Firestick:
```https://github.com/uriegel/FirePlayer```
## Setup
### Ubuntu 20.10 Server
### NginX

```
sudo apt install nginx
sudo ufw app list
sudo ufw status
systemctl status nginx
```
### Lets Encrypt-Certificats
call http://raspberrypi call http://domain
```
sudo snap install core
sudo snap refresh core
sudo snap install --classic certbot
sudo ln -s /snap/bin/certbot /usr/bin/certbot
sudo certbot –nginx
sudo certbot renew --dry-run
```
### NginX configuration
```
cd /etc/nginx/sites-available
$ sudo nano default
sudo systemctl stop nginx
sudo systemctl start nginx
sudo systemctl restart nginx
```
If you want to reload configuration:
```
sudo systemctl reload nginx
```
### External USB disk
```
sudo nano /etc/fstab
```
```
UUID=04F20EEDF20EE332   /media/video    ntfs    defaults,nofail 0       0
```
Send external disk to sleep after 10 min:

Install ```tlp```:

```
sudo apt install tlp
```

Get disk ID for tlp:

```
sudo tlp diskid
```

```
sudo nano /etc/tlp.conf
```
```
# Disk devices; separate multiple devices with spaces (default: sda).
# Devices can be specified by disk ID also (lookup with: tlp diskid).
DISK_DEVICES="ata-TOSHIBA_MQ01ABD100_238MSIE4S"

# Disk advanced power management level: 1..254, 255 (max saving, min, off).
# Levels 1..127 may spin down the disk; 255 allowable on most drives.
# Separate values for multiple disks with spaces. Use the special value 'keep'
# to keep the hardware default for the particular disk.
DISK_APM_LEVEL_ON_AC="127"
DISK_APM_LEVEL_ON_BAT="127"

# Hard disk spin down timeout:
#   0:        spin down disabled
#   1..240:   timeouts from 5s to 20min (in units of 5s)
#   241..251: timeouts from 30min to 5.5 hours (in units of 30min)
# See 'man hdparm' for details.
# Separate values for multiple disks with spaces. Use the special value 'keep'
# to keep the hardware default for the particular disk.
DISK_SPINDOWN_TIMEOUT_ON_AC="120"
DISK_SPINDOWN_TIMEOUT_ON_BAT="120"
```
```
systemctl enable tlp
systemctl start tlp
```
## Install Home Server
### Node.js
Update your system package list:

```
pi@w3demopi:~ $ sudo apt-get update
```
Upgrade all your installed packages to their latest version:

```
pi@w3demopi:~ $ sudo apt-get dist-upgrade
```

To download and install newest version of Node.js, use the following command:
```
pi@w3demopi:~ $ curl -sL https://deb.nodesource.com/setup_15.x | sudo -E bash -
```

Now install it by running:

```
pi@w3demopi:~ $ sudo apt-get install -y nodejs
```

// TODO: /etc/nginx/sites-available/default:
// Items redirecting to nodejs
// Install node.js as service