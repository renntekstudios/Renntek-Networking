@echo off
echo Unzipping
"7zip\7za.exe" x ".\compilation.7z"

echo Extracting Required tools
cd ..
"windows_compilation\7zip\7za.exe" x "windows_compilation\tools.7z"

exit