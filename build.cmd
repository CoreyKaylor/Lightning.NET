@echo off

pushd %~dp0

md artifacts
call dotnet --info
call dotnet restore src/Lightning.NET.sln
if %errorlevel% neq 0 exit /b %errorlevel%
call dotnet test src/LightningDB.Tests/LightningDB.Tests.csproj
if %errorlevel% neq 0 exit /b %errorlevel%

echo Packing LightningDb Nuget
call dotnet pack src/LightningDB --configuration Release --output ..\..\artifacts

popd
