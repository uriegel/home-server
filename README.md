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
sudo apt install certbot python3-certbot-nginx
sudo certbot --nginx -d uriegel.de

systemctl status nginx
```
