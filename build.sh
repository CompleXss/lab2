cd "lab2-host"
dotnet publish -p:PublishSingleFile=true --self-contained --output build

cd ../"lab2-client"
dotnet publish -p:PublishSingleFile=true --self-contained --output build
