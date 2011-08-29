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

AFBleedingEdgeDir=R:\FlexIndex\Internal\BleedingEdge
FKBUpdateReleaseDir=$(AFBleedingEdgeDir)\$(FKBVersion)
FKBInstallMediaDir=$(FKBUpdateInstallRoot)\Media\CD-ROM\DiskImages\DISK1
	
EncryptAndCopyComponentDataFiles: GetComponentDataFiles
    @ECHO Copying the ComponentData subdirectories and files to installation directory...
    @rmdir "$(AFCoreInstallFilesRootDir)\ComponentData" /s /q
    @IF NOT EXIST "$(AFCoreInstallFilesRootDir)\ComponentData" @MKDIR "$(AFCoreInstallFilesRootDir)\ComponentData"
    @XCOPY "$(RulesDir)\ComponentData\*.*" "$(AFCoreInstallFilesRootDir)\ComponentData" /v /s /e /y
    $(VerifyDir) "$(RulesDir)\ComponentData" "$(AFCoreInstallFilesRootDir)\ComponentData"
    @ECHO Encrypting ComponentData pattern files...
    @SendFilesAsArgumentToApplication "$(AFCoreInstallFilesRootDir)\ComponentData\*.dat" 1 1 "$(BinariesFolder)\EncryptFile.exe"
    @SendFilesAsArgumentToApplication "$(AFCoreInstallFilesRootDir)\ComponentData\*.dcc" 1 1 "$(BinariesFolder)\EncryptFile.exe"
    @SendFilesAsArgumentToApplication "$(AFCoreInstallFilesRootDir)\ComponentData\*.rsd" 1 1 "$(BinariesFolder)\EncryptFile.exe"
    @SendFilesAsArgumentToApplication "$(AFCoreInstallFilesRootDir)\ComponentData\*.spm" 1 1 "$(BinariesFolder)\EncryptFile.exe"
    @DeleteFiles "$(AFCoreInstallFilesRootDir)\ComponentData\*.dcc"
    @DeleteFiles "$(AFCoreInstallFilesRootDir)\ComponentData\*.dat"
    @DeleteFiles "$(AFCoreInstallFilesRootDir)\ComponentData\*.rsd"
    @DeleteFiles "$(AFCoreInstallFilesRootDir)\ComponentData\*.txt"
	@DeleteFiles "$(AFCoreInstallFilesRootDir)\ComponentData\*.spm"
    @DeleteFiles "$(AFCoreInstallFilesRootDir)\vssver.scc"
    @ECHO $(FKBVersion) > "$(AFCoreInstallFilesRootDir)\ComponentData\FKBVersion.txt"

		
BuildFKBUpdate: EncryptAndCopyComponentDataFiles
    @ECHO Building the FKBUpdate installation...
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(ReusableComponentsRootDirectory)\APIs\Nuance_16.3\bin;$(ReusableComponentsRootDirectory)\APIs\LeadTools_17\bin;;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(WINDOWS_SDK)\BIN;C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319;$(VCPP_DIR)\VCPackages
	CScript "(CommonDirectory)\SetupFKBInstallBuild.vbs" "$(FKBUpdateInstallRoot)\FKBInstall.ism" "$(FKBVersion)"
    @"$(DEV_STUDIO_DIR)\System\IsCmdBld.exe" -p "$(FKBUpdateInstallRoot)\FKBInstall.ism" 

CreateFKBIntall: BuildFKBUpdate
	@ECHO Copying file to FKBUpdate directory...
	@IF NOT EXIST "$(FKBUpdateReleaseDir)" @MKDIR "$(FKBUpdateReleaseDir)"
	@XCOPY "$(FKBInstallMediaDir)\*.*" "$(FKBUpdateReleaseDir)" /v /s /e /y