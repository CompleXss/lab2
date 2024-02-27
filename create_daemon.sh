cd /etc/systemd/system/

file="lab2-host.service"

sudo rm $file
sudo touch $file
sudo cat $file

echo "[Unit]
Description=lab2 file receiver daemon

[Service]
Type=notify
ExecStart=$HOME/Documents/lab2/lab2-host/build/lab2-host

[Install]
WantedBy=multi-user.target" | sudo tee -a $file

sudo cat $file

sudo systemctl daemon-reload
sudo systemctl start lab2-host
