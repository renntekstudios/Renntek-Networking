@echo off

set mingw=%~dp0windows_compilation\
set PATH=%PATH%;%mingw%bin;%mingw%bin;%mingw%msys\1.0\bin

make -j6
pause