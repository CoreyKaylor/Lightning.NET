@echo off

pushd %~dp0

SETLOCAL
where dnvm
if %ERRORLEVEL% neq 0 (
    @powershell -NoProfile -ExecutionPolicy unrestricted -Command "&{$Branch='dev';iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1'))}"
    set PATH=!PATH!;!userprofile!\.dnx\bin
    set DNX_HOME=!USERPROFILE!\.dnx
    goto install
)

:install
call dnvm install 1.0.0-rc1-final
call dnvm use 1.0.0-rc1-final

call dnu restore
if %errorlevel% neq 0 exit /b %errorlevel%
cd tests\LightningDB.Tests
call dnx test
if %errorlevel% neq 0 exit /b %errorlevel%
cd ..\..\
md artifacts

echo Packing LightningDb Nuget
cd src/LightningDB
call dnu pack --configuration Release --out ..\..\artifacts

popd