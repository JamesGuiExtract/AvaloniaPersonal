#############################################################################
# I N C L U D E S
#
# the common.mak file being included here counts on $(ProductRootDirName)
# ProductRootDirName defined as a non-null string
#

!include ComponentVersions.mak
!include ..\..\Common\Common.mak

#############################################################################
# E N S U R E   P R E - C O N D I T I O N S   A R E   M E T
#
# The user must tell which configuration to build (e.g. "Win32 Release")
# Otherwise, we cannot continue.
#
!IF "$(BuildConfig)" == ""
!ERROR Build variable 'BuildConfig' must be defined (e.g. "Release")
!ENDIF

# determine the name of the release output directory based upon the build
# configuration that is being built
!IF "$(BuildConfig)" == "Release"
BuildOutputDir=Release
!ELSEIF "$(BuildConfig)" == "Debug"
BuildOutputDir=Debug
!ELSE
!ERROR Internal error - invalid value for BuildConfig variable!
!ENDIF

#############################################################################
# M A K E F I L E   V A R I A B L E S
#
PDRootDir=$(EngineeringRootDirectory)\ProductDevelopment
AFRootDirectory=$(PDRootDir)\AttributeFinder
RulesDir=$(EngineeringRootDirectory)\Rules
FKBUpdateInstallRoot=$(PDRootDir)\AttributeFinder\Installation\FKBInstall
AFCoreInstallFilesRootDir=P:\AttributeFinder\CoreInstallation\Files

AFInstallPDRootDir=P:\AttributeFinder
AFInstallFilesRootDir=$(AFInstallPDRootDir)\SDKInstallation\Files

FKBUpdateReleaseDir=$(BleedingEdgeVersionDir)\$(FKBVersion)
FKBInstallMediaDir=$(FKBUpdateInstallRoot)\Media\CD-ROM\DiskImages\DISK1

BinariesFolder=$(EngineeringRootDirectory)\Binaries\$(BuildOutputDir)

	
EncryptAndCopyComponentDataFiles: 
    @ECHO Copying the ComponentData subdirectories and files to installation directory...
	@IF EXIST "$(AFCoreInstallFilesRootDir)\ComponentData" @DeleteFiles "$(AFCoreInstallFilesRootDir)\ComponentData\*.*"
    @IF EXIST "$(AFCoreInstallFilesRootDir)\ComponentData" @rmdir "$(AFCoreInstallFilesRootDir)\ComponentData" /s /q
    @IF NOT EXIST "$(AFCoreInstallFilesRootDir)\ComponentData" @MKDIR "$(AFCoreInstallFilesRootDir)\ComponentData"
    @XCOPY "$(RulesDir)\ComponentData\*.*" "$(AFCoreInstallFilesRootDir)\ComponentData" /v /s /e /y
    $(VerifyDir) "$(RulesDir)\ComponentData" "$(AFCoreInstallFilesRootDir)\ComponentData"
    @ECHO Encrypting ComponentData pattern files...
    @SendFilesAsArgumentToApplication "$(AFCoreInstallFilesRootDir)\ComponentData\*.dat" 1 1 "$(BinariesFolder)\EncryptFile.exe"
    @SendFilesAsArgumentToApplication "$(AFCoreInstallFilesRootDir)\ComponentData\*.dcc" 1 1 "$(BinariesFolder)\EncryptFile.exe"
    @SendFilesAsArgumentToApplication "$(AFCoreInstallFilesRootDir)\ComponentData\*.rsd" 1 1 "$(BinariesFolder)\EncryptFile.exe"
    @SendFilesAsArgumentToApplication "$(AFCoreInstallFilesRootDir)\ComponentData\*.spm" 1 1 "$(BinariesFolder)\EncryptFile.exe"
    @SendFilesAsArgumentToApplication "$(AFCoreInstallFilesRootDir)\ComponentData\*.nlp" 1 1 "$(BinariesFolder)\EncryptFile.exe"
    @DeleteFiles "$(AFCoreInstallFilesRootDir)\ComponentData\*.dcc"
    @DeleteFiles "$(AFCoreInstallFilesRootDir)\ComponentData\*.dat"
    @DeleteFiles "$(AFCoreInstallFilesRootDir)\ComponentData\*.rsd"
    @DeleteFiles "$(AFCoreInstallFilesRootDir)\ComponentData\*.txt"
		@DeleteFiles "$(AFCoreInstallFilesRootDir)\ComponentData\*.spm"
    @DeleteFiles "$(AFCoreInstallFilesRootDir)\ComponentData\*.nlp"
    @DeleteFiles "$(AFCoreInstallFilesRootDir)\vssver.scc"
    @ECHO $(FKBVersion) > "$(AFCoreInstallFilesRootDir)\ComponentData\FKBVersion.txt"
		
BuildFKBUpdate: EncryptAndCopyComponentDataFiles
    @ECHO Building the FKBUpdate installation...
	CScript "$(CommonDirectory)\SetupFKBInstallBuild.vbs" "$(FKBUpdateInstallRoot)\FKBInstall.ism" "$(FKBVersion)"
    @"$(DEV_STUDIO_DIR)\System\IsCmdBld.exe" -p "$(FKBUpdateInstallRoot)\FKBInstall.ism" 

CreateFKBInstall: BuildFKBUpdate
	@ECHO Copying file to FKBUpdate directory...
	@IF NOT EXIST "$(FKBUpdateReleaseDir)" @MKDIR "$(FKBUpdateReleaseDir)"
	@XCOPY "$(FKBInstallMediaDir)\*.*" "$(FKBUpdateReleaseDir)" /v /s /e /y
