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
PDCommonDir=$(PDRootDir)\Common
ExtractBaseInstallFilesRootDir=P:\ExtractBase

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

CleanupExtractBaseFiles:
	@ECHO Removing files from previous ExtractBase build...
	@DeleteFiles "$(MergeModuleDir)\ExtractBaseMM.msm"
	@IF NOT EXIST "$(ExtractBaseInstallFilesRootDir)\NonSelfRegFiles" @MKDIR "$(ExtractBaseInstallFilesRootDir)\NonSelfRegFiles"
	@IF NOT EXIST "$(ExtractBaseInstallFilesRootDir)\SelfRegFiles" @MKDIR "$(ExtractBaseInstallFilesRootDir)\SelfRegFiles"
	@Deletefiles "$(ExtractBaseInstallFilesRootDir)\NonSelfRegFiles\*.*"
	@Deletefiles "$(ExtractBaseInstallFilesRootDir)\SelfRegFiles\*.*"	

CopyExtractBaseFiles : CleanupExtractBaseFiles
	@ECHO Copying the ExtractBase files to installation directory...
	@COPY /v  "$(BinariesFolder)\COMLM.dll" "$(ExtractBaseInstallFilesRootDir)\SelfRegFiles"
	@COPY /v  "$(BinariesFolder)\UCLIDCOMUtils.dll" "$(ExtractBaseInstallFilesRootDir)\SelfRegFiles"
	@COPY /v  "$(BinariesFolder)\BaseUtils.dll" "$(ExtractBaseInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v  "$(BinariesFolder)\COMLMCore.dll" "$(ExtractBaseInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v  "$(BinariesFolder)\ExtractTRP2.exe" "$(ExtractBaseInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V "$(PDCommonDir)\RegisterAll.bat" "$(ExtractBaseInstallFilesRootDir)\NonSelfRegFiles"
	
	@DIR "$(ExtractBaseInstallFilesRootDir)\SelfRegFiles\*.*" /b >"$(ExtractBaseInstallFilesRootDir)\NonSelfRegFiles\ExtractBase.rl"
	
CreateExtractBaseMergeModule: CopyExtractBaseFiles
	@ECHO Creating ExtractBase merge module...
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(ReusableComponentsRootDirectory)\APIs\Nuance_16.3\bin;$(ReusableComponentsRootDirectory)\APIs\LeadTools_16.5\bin;;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(WINDOWS_SDK)\BIN;C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319;$(VCPP_DIR)\VCPackages
	$(SetProductVerScript) "$(PDCommonDir)\ExtractBase\ExtractBaseMM.ism" "$(ReusableComponentsVersion)"
    @"$(DEV_STUDIO_DIR)\System\IsCmdBld.exe" -p "$(PDCommonDir)\ExtractBase\ExtractBaseMM.ism"
