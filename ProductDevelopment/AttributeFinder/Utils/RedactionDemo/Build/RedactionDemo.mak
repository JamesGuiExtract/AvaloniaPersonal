#############################################################################
# I N C L U D E S
#
# the common.mak file being included here counts on $(ProductRootDirName)
# ProductRootDirName defined as a non-null string
#

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
DemoRulesDir=$(EngineeringRootDirectory)\Rules\IDShield\Demo_IDShield\Rules

RedactionImageDir=P:\AttributeFinder\Demo_IDShield\Sanitized

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
    @IF NOT EXIST "$(IDShieldDemo)\Rules"  @MKDIR "$(IDShieldDemo)\Rules" 
	@ECHO Updating IDShield demo rules for FKB Version...
	$(CScriptProgram) "$(PDRootDir)\Utils\Scripts\UpdateFKB.js" -silent "$(DemoRulesDir)" "$(FKBVersion)"
	@XCOPY "$(AFRootDirectory)\Utils\RedactionDemo\Files\*.*" "$(IDShieldDemo)" /V /s /e /y
    $(VerifyDir) "$(AFRootDirectory)\Utils\RedactionDemo\Files" "$(IDShieldDemo)"
	@XCOPY "$(DemoRulesDir)\*.*" "$(IDShieldDemo)\Rules" /V /s /e /y
    $(VerifyDir) "$(DemoRulesDir)" "$(IDShieldDemo)\Rules"
    @ECHO Encrypting Rules files...
    @SendFilesAsArgumentToApplication "$(IDShieldDemo)\Rules\*.dat" 1 1 "$(BinariesFolder)\EncryptFile.exe"
    @SendFilesAsArgumentToApplication "$(IDShieldDemo)\Rules\*.rsd" 1 1 "$(BinariesFolder)\EncryptFile.exe"
    @SendFilesAsArgumentToApplication "$(IDShieldDemo)\Rules\*.nlp" 1 1 "$(BinariesFolder)\EncryptFile.exe"
    @DeleteFiles "$(IDShieldDemo)\Rules\*.dat"
    @DeleteFiles "$(IDShieldDemo)\Rules\*.rsd"
    @DeleteFiles "$(IDShieldDemo)\Rules\*.nlp"
    @DeleteFiles "$(IDShieldDemo)\vssver.scc"

CopyImageFilesToInstallFolder: 
    @ECHO Copying the RedactionDemo Image files to installation directory...
    @IF NOT EXIST "$(IDShieldDemo)\DemoFiles\Installs\HybridDemo\Input" @MKDIR "$(IDShieldDemo)\DemoFiles\Installs\HybridDemo\Input"
    @XCOPY "$(RedactionImageDir)\*.*" "$(IDShieldDemo)\DemoFiles\Installs\HybridDemo\Input\" /V /s /e /y
    $(VerifyDir) "$(RedactionImageDir)" "$(IDShieldDemo)\DemoFiles\Installs\HybridDemo\Input"
    @DeleteFiles "$(IDShieldDemo)\vssver.scc"

DoEverything: CopyDemoFiles CopyImageFilesToInstallFolder
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @ECHO Redaction Demo Build process completed.
    @ECHO.
