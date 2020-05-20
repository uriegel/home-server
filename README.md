# WebServer
Implementierung des "UwebServers" für meine Homepage
## Vorbereitung des Dienstes 
```
sudo cp WebServer.service /etc/systemd/system/WebServer.service
sudo systemctl daemon-reload
sudo mkdir /srv/WebServer 
```
## Buildvorgang
```
sudo systemctl stop WebServer
dotnet publish -c Release -o /srv/WebServer
```
## Betrieb
```
sudo systemctl start WebServer
sudo journalctl -u WebServer -f -p 6
```