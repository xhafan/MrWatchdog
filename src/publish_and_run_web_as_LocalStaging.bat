set CONFIGURATION=Debug
cd Web
dotnet publish -c %CONFIGURATION%
cd bin/%CONFIGURATION%/net9.0/publish

set ASPNETCORE_ENVIRONMENT=LocalStaging
set ASPNETCORE_URLS=https://localstaging.mrwatchdog.localhost:5030;http://localstaging.mrwatchdog.localhost:5029;https://0.0.0.0:5030
MrWatchdog.Web.exe
