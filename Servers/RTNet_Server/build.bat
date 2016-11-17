@echo off

set current_dir = %~dp0
set mingw=%current_dir%..\..\windows_compilation\
set PATH=%PATH%;%mingw%bin;%mingw%bin;%mingw%msys\1.0\bin

make -j6
pause