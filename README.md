# home-server
a home server for my Raspberry Pi 3

## Setup on Raspberry

### install node

```
curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.40.3/install.sh | bash
nvm install 24

```
### Install as service
```
sudo nano /lib/systemd/system/homeserver.service
```
```
[Unit]
Description=Home Server for serving videos to AMAZON FirePlayer
Documentation=https://github.com/uriegel/home-server/blob/master/README.md
After=network.target

[Service]
Environment=PORT=9865
Environment=INTRANET_HOST=roxy
Environment=VIDEO_PATH=/media/video/videos
Environment=MUSIC_PATH=/media/video/Musik
Environment=PICTURE_PATH=/media/video/Fotos
Environment=MEDIA_MOUNT_PATH=/media/video
Environment=USB_MEDIA_PORT=5
Type=simple
ExecStart=/home/uwe/.nvm/versions/node/v24.11.0/bin/node /home/uwe/home-server
User=uwe
Group=uwe
Restart=on-failure
WorkingDirectory=/home/uwe/home-server

[Install]
WantedBy=multi-user.target

```

```
sudo systemctl daemon-reload
sudo systemctl enable homeserver.service
sudo systemctl start homeserver
sudo systemctl status homeserver
```

## Logging

``` sudo journalctl -u homeserver ```

Remove old logs:

```
journalctl --disk-usage
sudo journalctl --rotate      
sudo journalctl --vacuum-time=2weeks
```


### External USB disk


```
LABEL=Videos   /media/video    ext4    defaults,nofail 0       1
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
sudo apt install hd-idle
sudo nano /etc/default/hd-idle
```

```
# defaults file for hd-idle

# start hd-idle automatically?
START_HD_IDLE=true
HD_IDLE_OPTS="-i 600 -l /var/log/hd-idle.log"
```

```
sudo systemctl enable hd-idle
sudo systemctl restart hd-idle
```
To check disk status
```
sudo hdparm -C /dev/sda
```

### NginX

```
sudo apt install nginx
sudo ufw app list
sudo ufw status
systemctl status nginx
```
### Lets Encrypt-Certificates
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

## Install Home Server
### Compile on raspberry

cargo build --target armv7-unknown-linux-gnueabihf --release


export VIDEO_PATH=/media/video/videos

### NginX redirection

// ## /etc/nginx/sites-available/default:
// Items redirecting to nodejs
// Install node.js as service

```
location <path>; {
	proxy_pass http://localhost:9865/<path>;
}

location / {
```

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

or use disk label:


### Cross compiling on Ubuntu/Linux Mint for 64bit

Install Mint in Boxes

Install rust

On Ubuntu/Mint:

```
sudo apt install libssl-dev
```
On Fedora 41:

```
sudo dnf install pkg-config openssl-devel
```

``` 
# Make sure GCC's linker for the target platform is installed on your
# system
sudo apt install gcc-aarch64-linux-gnu
# Install the standard library for the target platform
rustup target add aarch64-unknown-linux-gnu
# Tell cargo to use the linker you just installed rather than the default
export CARGO_TARGET_AARCH64_UNKNOWN_LINUX_GNU_LINKER=/usr/bin/aarch64-linux-gnu-gcc
# Build!
cargo build --release --target=aarch64-unknown-linux-gnu
``` 
