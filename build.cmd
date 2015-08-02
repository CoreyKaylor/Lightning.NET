@echo off

pushd %~dp0

SETLOCAL
SET CACHED_NUGET=%LocalAppData%\NuGet\NuGet.exe

IF EXIST %CACHED_NUGET% goto checkdnx
echo Downloading latest version of NuGet.exe...
IF NOT EXIST %LocalAppData%\NuGet md %LocalAppData%\NuGet
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest 'https://www.nuget.org/nuget.exe' -OutFile '%CACHED_NUGET%'"

:checkdnx
set "DNX_NUGET_API_URL=https://www.nuget.org/api/v2"
setlocal EnableDelayedExpansion 
where dnvm
if %ERRORLEVEL% neq 0 (
    @powershell -NoProfile -ExecutionPolicy unrestricted -Command "&{$Branch='dev';iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1'))}"
    set PATH=!PATH!;!userprofile!\.dnx\bin
    set DNX_HOME=!USERPROFILE!\.dnx
    goto install
)

:install
call dnvm install 1.0.0-beta6
call dnvm use 1.0.0-beta6
rem set the runtime path because the above commands set \.dnx<space>\runtimes
set PATH=!USERPROFILE!\.dnx\runtimes\dnx-clr-win-x86.1.0.0-beta6\bin;!PATH!

call dnu restore
if %errorlevel% neq 0 exit /b %errorlevel%
cd tests\LightningDB.Tests
call dnx . test -parallel none
if %errorlevel% neq 0 exit /b %errorlevel%
cd ..\..\src\LightningDB
call dnu build
cd ..\..\
md artifacts

if "%LIGHTNING_NUGET_VERSION%"=="" set /p LIGHTNING_NUGET_VERSION=<VERSION.txt

echo Packing LightningDb Nuget Version %LIGHTNING_NUGET_VERSION%
call %CACHED_NUGET% pack packaging/nuget/lightningdb.nuspec -Version %LIGHTNING_NUGET_VERSION% -OutputDirectory artifacts -Symbols

popd