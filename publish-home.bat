d:
cd D:\src\machaqueso\rambler\Rambler.Web
dotnet clean -r linux-x64
dotnet publish -c Release -r linux-x64 -o ../publish/linux-x64
cd D:\src\machaqueso\rambler
winscp /privatekey=D:\src\machaqueso\keys\machaqueso.ppk /script=deploy-home.txt
