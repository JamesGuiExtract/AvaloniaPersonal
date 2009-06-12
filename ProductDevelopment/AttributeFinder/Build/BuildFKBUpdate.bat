@ECHO OFF
CALL InitUserEnv.Bat
CALL InitBuildEnv.Bat

REM Set internal variables
SET EncryptEXE=D:\temp_6.0.0.77\Flex\Engineering\Binaries\Release\EncryptFile.exe
SET TargetFolder=I:\Common\Engineering\ProductDevelopment\AttributeFinder\FKBUpdateInstallation\Files\ComponentData
SET EXEBuilder=C:\Program Files\InstallShield\PackageForTheWeb 4\Pftwwiz.exe

ECHO Getting RulePack Labeled "%1"
if not exist %TargetFolder% md %TargetFolder%
cd /d %TargetFolder%
ss get $/Engineering/ProductDevelopment/AttributeFinder/ComponentData -R -I- -W -V%1

ECHO Encrypting files...
SendFilesAsArgumentToApplication *.dat 1 1 "%EncryptEXE%"
SendFilesAsArgumentToApplication *.dcc 1 1 "%EncryptEXE%"
SendFilesAsArgumentToApplication *.rsd 1 1 "%EncryptEXE%"

REM Create version file
ECHO %~1 > %TargetFolder%\FKBVersion.txt

ECHO Deleting unnecessary files
del vssver.scc /s
del GrantorGranteeFinder\*.* /q
del *.dat /s
del *.dcc /s
del *.rsd /s
del LegalDescSplitter\SUB-BLO\NTChangeLog.txt

ECHO Building the EXE package...
rem copy I:\Common\Engineering\ProductDevelopment\AttributeFinder\FKBUpdateInstallation\Project\FlexIndexKBUpdate.pfw I:\Common\Engineering\ProductDevelopment\AttributeFinder\FKBUpdateInstallation\Project\temp.pfw
rem replacestring I:\Common\Engineering\ProductDevelopment\AttributeFinder\FKBUpdateInstallation\Project\temp.pfw {UCLID_FKB_Ver} "[%1]"
"%EXEBuilder%" I:\Common\Engineering\ProductDevelopment\AttributeFinder\FKBUpdateInstallation\Project\FlexIndexKBUpdate.pfw -a -s

REM Copy FlexIndexKBUpdate.exe to Product Releases\Bleeding Edge
cd I:\Common\Engineering\ProductDevelopment\AttributeFinder\FKBUpdateInstallation\Project
md I:\Common\Engineering\ProductReleases\FlexIndex\Internal\BleedingEdge\%1
copy FlexIndexKBUpdate.exe I:\Common\Engineering\ProductReleases\FlexIndex\Internal\BleedingEdge\%1

REM Clear out ComponentData folder
rd %TargetFolder% /s /q

ECHO Build complete!
