d:
cd D:\src\machaqueso\rambler
dotnet clean
dotnet clean -r linux-x64
cd D:\src\machaqueso\rambler\Rambler.Web
dotnet publish -c Release -r linux-x64 -o ../publish/linux-x64
cd D:\src\machaqueso\rambler
dotnet test

