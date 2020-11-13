# Home Server
a home server for my Raspberry Pi 3

This server woks together with FirePlayer, a Media Player for AMAZON Firestick:
```https://github.com/uriegel/FirePlayer```
## Setup
### Ubuntu 20.10 Server
```
sudo apt update
sudo apt upgrade
```

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
sudo snap install --classic certbot
sudo ln -s /snap/bin/certbot /usr/bin/certbot
sudo certbot --nginx
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

Get the UUID of the disk:

```
sudo blkid
```

Enter this in 

```
sudo nano /etc/fstab
```

```
UUID=04F20EEDF20EE332   /media/video    ext4    defaults,nofail 0       1
```

Then enter

```
sudo mkdir /media/video
```

mount drive:

```
sudo mount -a
```

```
cd /media
ll
sudo chmod 777 /media/video
```


sdfsdfsd

```
Send external disk to sleep after 10 min:

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

// TODO:
npm i in directory HomeServer
export MEDIA_PATH=/media/video/videos
node .

http://roxy:9865/videos

// TODO: /etc/nginx/sites-available/default:
// Items redirecting to nodejs
// Install node.js as service

location /video {
		proxy_pass http://localhost:9865/video;
	}

	location / {