@echo off

pushd %~dp0

md artifacts
cd workaround/LibLmdb
call dotnet restore
call dotnet pack --configuration Release --output ..\..\artifacts
cd ..\..\src\LightningDB.Tests
call dotnet restore
if %errorlevel% neq 0 exit /b %errorlevel%
call dotnet test
if %errorlevel% neq 0 exit /b %errorlevel%
cd ..\..\

echo Packing LightningDb Nuget
cd src/LightningDB
call dotnet pack --configuration Release --output ..\..\artifacts
cd ..\..\
del artifacts\LibLmdb.0.0.1.nupkg
del artifacts\LibLmdb.0.0.1.symbols.nupkg

popd