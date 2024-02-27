
# Install
Запусти **setup .sh**, этот скрипт установит .net 8 sdk, забилдит проект и создаст демона **lab2-host**

Если хочется сделать всё по шагам самому:
```bash
sudo apt-get update && sudo apt-get install -y dotnet-sdk-8.0
build.sh
create_daemon.sh
```


## Run client
Запусти **run_client.sh**

## Run host in terminal
Если хочется запустить сервер из терминала, запусти **run_host.sh**


## Extra
Посмотреть состояние демона можно командой
```bash
sudo systemctl status lab2-host
```

Журнал доступен по команде
```bash
sudo systemctl -u lab2-host
```

Запуск, перезагрузка и остановка демона
```bash
sudo systemctl start lab2-host
sudo systemctl restart lab2-host
sudo systemctl stop lab2-host
```