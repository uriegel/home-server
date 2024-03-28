# home-server
a home server for my Raspberry Pi 3

## Setup
### Ubuntu 23.10 Server
```
sudo apt update
sudo apt upgrade
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
sudo nano /lib/systemd/system/home-server.service
```

```
[Unit]
Description=Home Server for serving videos to AMAZON FirePlayer
Documentation=https://github.com/uriegel/home-server/blob/master/README.md
After=network.target

[Service]
Environment=PATH=$PATH:/home/uwe/.dotnet
Environment=export DOTNET_ROOT=/home/uwe/.dotnet
Environment=SERVER_PORT=8080
Environment=SERVER_TLS_PORT=4433
Environment=FRITZ_HOST=fritz.domain.de
Environment=INTRANET_HOST=roxy
Environment=VIDEO_PATH=/media/video/videos
Environment=MUSIC_PATH=/media/video/Musik
Environment=PICTURE_PATH=/media/video/Fotos
Environment=MEDIA_MOUNT_PATH=/media/video
Environment=USB_MEDIA_PORT=5
Type=simple
ExecStart=/home/uwe/home-server/bin/Release/net8.0/server
User=uwe
Group=uwe
Restart=on-failure
WorkingDirectory=/home/uwe/home-server

[Install]
WantedBy=multi-user.target

```

```
sudo systemctl daemon-reload
sudo systemctl enable home-server.service
sudo systemctl start home-server
sudo systemctl status home-server
```

## Logging

``` sudo journalctl -u homeserver ```

Remove old logs:

```
journalctl --disk-usage
sudo journalctl --rotate      
sudo journalctl --vacuum-time=2weeks
```

## Switching usb DRIVE ON and off

Check environment variable USB_MEDIA_PORT in home-server.service.

Create file: ```sudo nano /etc/udev/rules.d/52-usb.rules```

with the following content:

```
SUBSYSTEM=="usb", DRIVER=="usb", MODE="0664", GROUP="uwe", ATTR{idVendor}=="0424"
# or for all users: SUBSYSTEM=="usb", DRIVER=="usb", MODE="0666", ATTR{idVendor}=="1234"
# Linux 6.0 or later (its ok to have this block present for older Linux kernels):
SUBSYSTEM=="usb", DRIVER=="usb", \
  RUN="/bin/sh -c \"chmod -f 666 $sys$devpath/*-port*/disable || true\""
```

replace vendor id (1234) with the correct value retrieved by calling:

```sudo uhubctl```

From the result ```Port 1: 0503 power highspeed enable connect [4321:ec00]``` you get the vendor id of the USB hub from [vid:pid], in this example ```4321```

Reboot or run 

```sudo udevadm trigger --attr-match=subsystem=usb```


## HTTP 3
```https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/http3?view=aspnetcore-6.0```

```
curl -sSL https://packages.microsoft.com/keys/microsoft.asc | sudo tee /etc/apt/trusted.gpg.d/microsoft.asc

sudo apt-add-repository https://packages.microsoft.com/ubuntu/20.04/prod

sudo apt-get update

sudo apt install libmsquic=1.9*
```

## Deprecated


Port 80 and port 443 on Linux:

```sudo setcap CAP_NET_BIND_SERVICE=+eip /home/uwe/.dotnet/dotnet```

Now the program is not debuggable any more. To remove:

```setcap -r /home/uwe/.dotnet/dotnet```

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
