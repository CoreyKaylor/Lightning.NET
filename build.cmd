@echo off

pushd %~dp0

md artifacts
cd workaround/LibLmdb
call dotnet restore
call dotnet pack --configuration Release --output ..\..\artifacts
cd ..\..\
call dotnet restore
if %errorlevel% neq 0 exit /b %errorlevel%
call dotnet test src/LightningDB.Tests
if %errorlevel% neq 0 exit /b %errorlevel%

echo Packing LightningDb Nuget
call dotnet pack src/LightningDB --configuration Release --output artifacts
del artifacts\LibLmdb.0.0.1.nupkg
del artifacts\LibLmdb.0.0.1.symbols.nupkg

popd