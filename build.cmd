@echo off

pushd %~dp0

call dotnet restore
if %errorlevel% neq 0 exit /b %errorlevel%
cd src\LightningDB.Tests
call dotnet test
if %errorlevel% neq 0 exit /b %errorlevel%
cd ..\..\
md artifacts

echo Packing LightningDb Nuget
cd src/LightningDB
call dotnet pack --configuration Release --out ..\..\artifacts

popd