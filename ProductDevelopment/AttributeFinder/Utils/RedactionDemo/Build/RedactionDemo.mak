#############################################################################
# I N C L U D E S
#
# the common.mak file being included here counts on $(ProductRootDirName)
# ProductRootDirName defined as a non-null string
#

!include ..\..\..\..\Common\LatestComponentVersions.mak
!include ..\..\..\..\Common\Common.mak

#############################################################################
# E N S U R E   P R E - C O N D I T I O N S   A R E   M E T
#
# The user must tell which configuration to build (e.g. "Win32 Release")
# Otherwise, we cannot continue.
#
!IF "$(BuildConfig)" == ""
!ERROR Build variable 'BuildConfig' must be defined (e.g. "Win32 Release")
!ENDIF

#############################################################################
# M A K E F I L E   V A R I A B L E S
#
PDRootDir=$(EngineeringRootDirectory)\ProductDevelopment
AFRootDirectory=$(PDRootDir)\AttributeFinder
AFComponentData=$(AFRootDirectory)\ComponentData
AFBleedingEdgeDir=I:\Common\Engineering\ProductReleases\FlexIndex\Internal\BleedingEdge

RedactionImageDir=I:\Common\Engineering\ProductDevelopment\AttributeFinder\Demo_IDShield\Sanitized
RedactionInstallDir=$(AFBleedingEdgeDir)\$(FlexIndexVersion)\Demo_IDShield

# determine the name of the release output directory based upon the build
# configuration that is being built
!IF "$(BuildConfig)" == "Release"
BuildOutputDir=Release
!ELSEIF "$(BuildConfig)" == "Debug"
BuildOutputDir=Debug
!ELSE
!ERROR Internal error - invalid value for BuildConfig variable!
!ENDIF

BinariesFolder=$(EngineeringRootDirectory)\Binaries\$(BuildOutputDir)

#############################################################################
# B U I L D    T A R G E T S
#
CopyDemoFiles:
    @ECHO Copying the Demo files to installation directory...
    @IF NOT EXIST "$(RedactionInstallDir)"  @MKDIR "$(RedactionInstallDir)" 
    @XCOPY "$(AFRootDirectory)\Utils\RedactionDemo\Files\*.*" "$(RedactionInstallDir)" /V /s /e /y
    $(VerifyDir) "$(AFRootDirectory)\Utils\RedactionDemo\Files" "$(RedactionInstallDir)"
    @ECHO Encrypting Rules files...
    @SendFilesAsArgumentToApplication "$(RedactionInstallDir)\Rules\*.dat" 1 1 "$(BinariesFolder)\EncryptFile.exe"
    @SendFilesAsArgumentToApplication "$(RedactionInstallDir)\Rules\*.rsd" 1 1 "$(BinariesFolder)\EncryptFile.exe"
    @DeleteFiles "$(RedactionInstallDir)\Rules\*.dat"
    @DeleteFiles "$(RedactionInstallDir)\Rules\*.rsd"
    @DeleteFiles "$(RedactionInstallDir)\vssver.scc"

CopyImageFilesToInstallFolder: 
    @ECHO Copying the RedactionDemo Image files to installation directory...
    @IF NOT EXIST "$(RedactionInstallDir)\DemoFiles\Installs\HybridDemo\Input" @MKDIR "$(RedactionInstallDir)\DemoFiles\Installs\HybridDemo\Input"
    @XCOPY "$(RedactionImageDir)\*.*" "$(RedactionInstallDir)\DemoFiles\Installs\HybridDemo\Input" /V /s /e /y
    $(VerifyDir) "$(RedactionImageDir)" "$(RedactionInstallDir)\DemoFiles\Installs\HybridDemo\Input"
    @DeleteFiles "$(RedactionInstallDir)\vssver.scc"

DoEverything: CopyDemoFiles CopyImageFilesToInstallFolder
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @ECHO Redaction Demo Build process completed.
    @ECHO.
