:: -----------------------------------------------------------------------------
:: Variables
:: -----------------------------------------------------------------------------

set td=%workdir%\testdetails.txt

:: Get OS
if defined ProgramFiles(x86) (
set bits=64
) else (
set bits=32
)

systeminfo | find "OS Name" > "%cd%\os.txt"
for /F "usebackq delims=: tokens=2" %%i in ("%cd%\os.txt") do set vers=%%i
for /f "tokens=* delims= " %%a in ("%vers%") do set vers=%%a
del "%cd%\os.txt"

:: Get build number
set vbs="%temp%\filever.vbs"
set file="%ccdir%\AFCoreTest.dll"

echo Set oFSO = CreateObject("Scripting.FileSystemObject") >%vbs%
echo WScript.Echo oFSO.GetFileVersion(WScript.Arguments.Item(0)) >>%vbs%

for /f "tokens=*" %%a in (
'cscript.exe //Nologo %vbs% %file%') do set build=%%a

del %vbs%

:: -----------------------------------------------------------------------------
:: Write testdetails.txt file
:: -----------------------------------------------------------------------------

echo Memory Leak Test Details > %td%
echo.>>%td%
echo Test Started:       %DATE% >>%td%
echo Machine Name:       %computername%>>%td%
echo Operating System:   %bits%-bit %vers%>>%td%
echo Build Number:       %build%>>%td%
echo Tester:             %recipients%>>%td%
echo.>>%td%
echo Test Object:        %testobject%>>%td%
echo.>>%td%
echo Test Number:        %testname%>>%td%
echo.>>%td%
echo Notes:>>%td%