# Home Server
a home server for my Raspberry Pi 3

## Setup
### Ubuntu 20.10 Server
```
sudo apt update
sudo apt upgrade
```

### Cross compiling on Ubuntu 21.10 box

``` 
# Make sure GCC's linker for the target platform is installed on your
# system
apt install gcc-arm-linux-gnueabihf
# Install the standard library for the target platform
rustup target add armv7-unknown-linux-gnueabihf
# Create a hello-world program
cargo new helloworld-rust && cd helloworld-rust
# Tell cargo to use the linker you just installed rather than the default
export CARGO_TARGET_ARMV7_UNKNOWN_LINUX_GNUEABIHF_LINKER=/usr/bin/arm-linux-gnueabihf-gcc
# Build!
cargo build --target=armv7-unknown-linux-gnueabihf
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

### Install as service

```
sudo nano /lib/systemd/system/homeserver.service
```

```
[Unit]
Description=Home Server for serving videos to AMAZON FirePlayer
Documentation=https://github.com/uriegel/HomeServer/blob/master/README.md
After=network.target

[Service]
Environment=MOUNT_PATH=/media/video
Environment=USB_MEDIA_PORT=5
Environment=VIDEO_PATH=/media/video/videos
Environment=MUSIC_PATH=/media/video/Musik
Environment=UPLOAD_PATH=/home/uwe/upload
Environment=UPLOAD_VIDEO_PATH=/media/video/videos
Environment=SERVER_PORT=8080
Environment=SERVER_TLS_PORT=4433
Environment=AUTH_NAME=nnnnn
Environment=AUTH_PW=**********
Environment=FRITZ_HOST=fritz.domain.de
Environment=INTRANET_HOST=roxy
Environment=LETS_ENCRYPT_DIR=/home/uwe/.config/letsencrypt-uweb
WorkingDirectory=/home/uwe/HomeServer
Type=simple
ExecStart=/home/uwe/.dotnet6/dotnet /home/uwe/HomeServer/HomeServer.dll
Restart=on-failure

[Install]
WantedBy=multi-user.target

```

```
sudo systemctl daemon-reload
sudo systemctl enable homeserver.service
sudo systemctl start homeserver
sudo systemctl status homeserver
```

Port 80 and port 443 on Linux:

```sudo setcap CAP_NET_BIND_SERVICE=+eip /home/uwe/.dotnet/dotnet```

Now the program is not debuggable any more. To remove:

```setcap -r /home/uwe/.dotnet/dotnet```

## Logging

``` sudo journalctl -u homeserver ```

Remove old logs:

```
journalctl --disk-usage
sudo journalctl --rotate      
sudo journalctl --vacuum-time=2weeks
```

## uhubctl

Completely shutdown usb (with ethernet):
```
echo '1-1' |sudo tee /sys/bus/usb/drivers/usb/unbind
```

Shutdown specific usb port:

```
sudo uhubctl -l 1-1 -p 2 -a 0
sudo apt-get install libusb-1.0-0-dev
git clone https://github.com/mvp/uhubctl
cd uhubctl
make
sudo apt install make
make
cc
sudo apt install gcc
cc
make
ls
sudo make install
```

## Deprecated

Send external disk to sleep after some time (20s):

```
sudo nano /etc/hdparm.conf
```

then edit this conf file:

```
/dev/disk/by-label/Videos {
        apm = 3
        spindown_time = 60
}
```

Send external disk to sleep after 10 min (deprecated, because tlp prevents booting):

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
sudo systemctl enable tlp
sudo systemctl start tlp
```

Not mandatory:

```
cd /media
ll
sudo chmod 777 /media/video
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

// TODO: /etc/nginx/sites-available/default:
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