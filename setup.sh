echo ">> Installing .NET 8 sdk
"
sudo apt-get update && sudo apt-get install -y dotnet-sdk-8.0

chmod +x ./build.sh
chmod +x ./create_daemon.sh
chmod +x ./run_client.sh
chmod +x ./run_host.sh
chmod +x ./uninstall_daemon.sh

echo "
>> Building app
"
./build.sh

echo "
>> Creating daemon
"
./create_daemon.sh

echo "
Daemon server should be working already
Run run_client.sh to run client app"
$SHELL