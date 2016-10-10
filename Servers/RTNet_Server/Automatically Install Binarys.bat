echo Unzipping
"C:\Program Files\WinRAR\WinRAR.exe" x "%~dp0\complier*.zip" "%~dp0\"

echo Extracting Required tools
"C:\Program Files\WinRAR\WinRAR.exe" x "%~dp0\windows_compilation\tools*.zip" "%~dp0\"

//Delete

//Delete the zip file
@REM File to be deleted
SET DeleteComplierZip="%~dp0\complier*.zip"
 
@Try to delete the file only if it exists
IF EXIST %DeleteComplierZip% del /F %DeleteComplierZip%
 
@REM If the file wasn't deleted for some reason, stop and error
IF EXIST %DeleteComplierZip% exit 1

//Delete the zip file
@REM File to be deleted
SET DeleteToolsZip="%~dp0\windows_compilation\tools*.zip"
 
@Try to delete the file only if it exists
IF EXIST %DeleteToolsZip% del /F %DeleteToolsZip%
 
@REM If the file wasn't deleted for some reason, stop and error
IF EXIST %DeleteToolsZip% exit 1


//Finally we delete the bat
@REM Delete the batch
SET DeleteBatch="%~dp0\Automatically Install Binarys.bat"

@Try to delete the file only if it exists
IF EXIST %DeleteBatch% del /F %DeleteBatch%
 
@REM If the file wasn't deleted for some reason, stop and error
IF EXIST %DeleteBatch% exit 1
