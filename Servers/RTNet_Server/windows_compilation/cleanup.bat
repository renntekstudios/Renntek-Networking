@echo off

echo Removing "bin"
rd /q /s bin
echo Removing "include"
rd /q /s include
echo Removing "lib" and "libexec"
rd /q /s lib libexec 
echo Removing "mingw32"
rd /q /s mingw32
echo Removing "msys"
rd /q /s msys
echo Removing "share", "var" and "tmp"
rd /q /s share var tmp
echo Removing "mingw-get-setup.exe" 
rm mingw-get-setup.exe

del cleanup.bat

echo "Done!"
pause