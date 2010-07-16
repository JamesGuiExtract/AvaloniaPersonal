#############################################################################
# I N C L U D E S
#
# the common.mak file being included here counts on $(ProductRootDirName)
# ProductRootDirName defined as a non-null string
#

!include LatestComponentVersions.mak
!include Common.mak

#############################################################################
# M A K E F I L E   V A R I A B L E S
#
PDRootDir=$(EngineeringRootDirectory)\ProductDevelopment
ExtractCommonInstallFilesRootDir=P:\ExtractCommon
PDCommonDir=$(PDRootDir)\Common

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

CleanupExtractCommonFiles:
	@ECHO Removing files from previous ExtractCommon build...
	@DeleteFiles "$(MergeModuleDir)\ExtractCommonMM.msm"
	@IF NOT EXIST "$(ExtractCommonInstallFilesRootDir)\NonSelfRegFiles" @MKDIR "$(ExtractCommonInstallFilesRootDir)\NonSelfRegFiles"
	@IF NOT EXIST "$(ExtractCommonInstallFilesRootDir)\SelfRegFiles" @MKDIR "$(ExtractCommonInstallFilesRootDir)\SelfRegFiles"
	@Deletefiles "$(ExtractCommonInstallFilesRootDir)\NonSelfRegFiles\*.*"
	@Deletefiles "$(ExtractCommonInstallFilesRootDir)\SelfRegFiles\*.*"	

CopyExtractCommonFiles: CleanupExtractCommonFiles
    @ECHO Copying the ExtractCommon files to installation directory...
	@COPY /v  "$(BinariesFolder)\COMLM.dll" "$(ExtractCommonInstallFilesRootDir)\SelfRegFiles"
	@COPY /v  "$(BinariesFolder)\ESMessageUtils.dll" "$(ExtractCommonInstallFilesRootDir)\SelfRegFiles"
	@COPY /v  "$(BinariesFolder)\UCLIDCOMUtils.dll" "$(ExtractCommonInstallFilesRootDir)\SelfRegFiles"
	@COPY /V "$(ReusableComponentsRootDirectory)\APIs\LeadTools_13\bin\LTCML13n.dll" "$(ExtractCommonInstallFilesRootDir)\SelfRegFiles"
	@COPY /v  "$(BinariesFolder)\BaseUtils.dll" "$(ExtractCommonInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v  "$(BinariesFolder)\COMLMCore.dll" "$(ExtractCommonInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v  "$(BinariesFolder)\ExtractTRP2.exe" "$(ExtractCommonInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v  "$(BinariesFolder)\RWUtils.dll" "$(ExtractCommonInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /v  "$(BinariesFolder)\USBLicenseKeyManager.exe" "$(ExtractCommonInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v  "$(BinariesFolder)\SafeNetUtils.dll" "$(ExtractCommonInstallFilesRootDir)\NonSelfRegFiles"
	@XCOPY "$(ReusableComponentsRootDirectory)\APIs\RogueWave\bin\*.*" "$(ExtractCommonInstallFilesRootDir)\NonSelfRegFiles" /v /s /e /y	
	@COPY /V "$(PDCommonDir)\RegisterAll.bat" "$(ExtractCommonInstallFilesRootDir)\NonSelfRegFiles"
	
	@DIR "$(ExtractCommonInstallFilesRootDir)\SelfRegFiles\*.*" /b >"$(ExtractCommonInstallFilesRootDir)\NonSelfRegFiles\ExtractCommon.rl"

CreateExtractCommonMergeModule: CopyExtractCommonFiles
	@ECHO Creating ExtractCommon merge module...
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(ReusableComponentsRootDirectory)\APIs\Nuance_16.3\bin;$(ReusableComponentsRootDirectory)\APIs\LeadTools_16.5\bin;$(ReusableComponentsRootDirectory)\APIs\RogueWave\bin;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(VCPP_DIR)\PlatformSDK\bin;$(VISUAL_STUDIO)\SDK\v2.0\bin;C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727;$(VCPP_DIR)\VCPackages
	$(SetProductVerScript) "$(PDCommonDir)\ExtractCommon\ExtractCommonMM.ism" "$(ReusableComponentsVersion)"
    @"$(DEV_STUDIO_DIR)\System\IsCmdBld.exe" -p "$(PDCommonDir)\ExtractCommon\ExtractCommonMM.ism"
