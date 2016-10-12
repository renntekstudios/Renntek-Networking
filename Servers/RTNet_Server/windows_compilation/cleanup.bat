@echo off

echo Removing "bin"
del /f /s /q bin
rmdir /s /q bin

echo Removing "include"
del /f /s /q include
rmdir /s /q include

echo Removing "lib"
del /f /s /q lib
rmdir /s /q lib 

echo Removing "libexec"
del /f /s /q libexec 
rmdir /s /q libexec

echo Removing "mingw32"
del /f /s /q mingw32
rmdir /s /q mingw32

echo Removing "msys"
del /f /s /q msys
rmdir /s /q msys

echo Removing "share"
del /f /s /q share
rmdir /s /q share

echo Removing "var"
del /f /s /q var
rmdir /s /q var

rmdir /s /q tmp

echo Removing "tools.7z" and mingw-get-setup.exe" 
del tools.7z
del mingw-get-setup.exe

cd ..
del Makefile 
del build.bat
cd windows_compilation

pause
echo "Done!"