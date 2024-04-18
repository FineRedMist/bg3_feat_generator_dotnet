@echo off

rmdir /s /q %~dp0\lslib

call :GetDependency https://github.com/Norbyte/lslib/releases/download/v1.19.5/ExportTool-v1.19.5.zip %CD%/lslib
call :MoveUpOneDir %CD%\lslib

rem call :GetDependency https://s3.eu-central-1.amazonaws.com/nb-stor/dos/ExportTool/gplex-distro-1_2_2.zip %CD%/lslib/External/gplex
rem call :MoveUpOneDir %CD%\lslib\External\gplex
rem call :GetDependency https://s3.eu-central-1.amazonaws.com/nb-stor/dos/ExportTool/gppg-distro-1_5_2.zip %CD%/lslib/External/gppg
rem call :MoveUpOneDir %CD%\lslib\External\gppg
rem call :GetDependency https://github.com/protocolbuffers/protobuf/releases/download/v3.6.1/protoc-3.6.1-win32.zip %CD%/lslib/External/protoc

goto :eof

:GetDependency %1 %2
if "%1"=="" goto :eof

echo Downloading %1 to %2
powershell -command "Start-BitsTransfer -Source %1 -Destination temp.zip"
powershell -command "Expand-Archive temp.zip %2"
del temp.zip

goto :eof

REM Moves the one subdirectory in the folder up one directory to place it correctly.
:MoveUpOneDir %1

pushd %1
for /d %%I in (*) do call :MoveFiles %%I
popd

goto :eof

:MoveFiles %1

pushd %1

for /d %%I in (*) do move %%I ..
for %%I in (*) do move %%I ..

popd

rmdir %1
