#############################################################################
# I N C L U D E S
#
# the common.mak file being included here counts on $(ProductRootDirName)
# ProductRootDirName defined as a non-null string
#

!include ComponentVersions.mak
!include ..\..\Common\Common.mak

#############################################################################
# M A K E F I L E   V A R I A B L E S
#
PDRootDir=$(EngineeringRootDirectory)\ProductDevelopment
ExtractFlexCommonInstallFilesRootDir=P:\ExtractFlexCommon
PDCommonDir=$(PDRootDir)\Common
RCNETDir=$(EngineeringRootDirectory)\RC.Net

ExtractFlexCommonInstallDir=$(PDRootDir)\AttributeFinder\Installation\ExtractFlexCommon

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

CleanupExtractFlexCommonFiles:
	@ECHO Removing files from previous ExtractFlexCommon build...
	@DeleteFiles "$(MergeModuleDir)\ExtractFlexCommonMM.msm"
	@IF NOT EXIST "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles" @MKDIR "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles"
	@IF NOT EXIST "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles\pdf" @MKDIR "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles\pdf"
	@IF NOT EXIST "$(ExtractFlexCommonInstallFilesRootDir)\SelfRegFiles" @MKDIR "$(ExtractFlexCommonInstallFilesRootDir)\SelfRegFiles"
	@IF NOT EXIST "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles" @MKDIR "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"
	@Deletefiles "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles\*.*"
	@Deletefiles "$(ExtractFlexCommonInstallFilesRootDir)\SelfRegFiles\*.*"	
	@Deletefiles "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles\*.*"	

CopyExtractFlexCommonFiles: CleanupExtractFlexCommonFiles
    @ECHO Copying the ExtractFlexCommon files to installation directory...
	@XCOPY "$(ReusableComponentsRootDirectory)\APIs\LeadTools_16.5\Bin\*.*" "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles" /v /s /e /y
	@XCOPY "$(ReusableComponentsRootDirectory)\APIs\LeadTools_16.5\pdf\*.*" "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles\pdf" /v /s /e /y
    @XCOPY "$(ReusableComponentsRootDirectory)\APIs\Nuance_16.3\bin\*.*" "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles" /v /s /e /y
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.Utilities.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.Imaging.Forms.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.Licensing.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.Utilities.Forms.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.Drawing.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.Encryption.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.Imaging.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.Imaging.Utilities.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.Interop.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.Utilities.Parsers.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.Rules.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"
	@COPY /V "$(BinariesFolder)\Interop.UCLID_EXCEPTIONMGMTLib.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"
	@COPY /V "$(BinariesFolder)\Interop.UCLID_COMUTILSLib.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"
	@COPY /V "$(BinariesFolder)\Interop.UCLID_RASTERANDOCRMGMTLib.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"
	@COPY /V "$(BinariesFolder)\Interop.UCLID_SSOCRLib.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"	
	@COPY /V "$(BinariesFolder)\Interop.UCLID_COMLMLib.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"	
	@COPY /V "$(ReusableComponentsRootDirectory)\APIs\LeadTools_16.5\Dotnet\Leadtools*.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"
	@COPY "$(RCNETDir)\APIs\Divelements\SandDock\bin\SandDock.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles" 
    @COPY /V "$(BinariesFolder)\UCLIDRasterAndOCRMgmt.dll" "$(ExtractFlexCommonInstallFilesRootDir)\SelfRegFiles"
    @COPY /V "$(BinariesFolder)\UCLIDExceptionMgmt.dll" "$(ExtractFlexCommonInstallFilesRootDir)\SelfRegFiles"
    @COPY /V "$(BinariesFolder)\UCLIDImageUtils.dll" "$(ExtractFlexCommonInstallFilesRootDir)\SelfRegFiles"
    @COPY /V "$(BinariesFolder)\SSOCR.dll" "$(ExtractFlexCommonInstallFilesRootDir)\SelfRegFiles"
    @COPY /V "$(BinariesFolder)\ssocr2.Exe" "$(ExtractFlexCommonInstallFilesRootDir)\SelfRegFiles"
    @COPY /V "$(BinariesFolder)\LeadUtils.dll" "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /V "$(BinariesFolder)\TopoUtils.dll" "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /V "$(BinariesFolder)\ZLibUtils.dll" "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /V "$(BinariesFolder)\UserLicense.Exe" "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v "$(BinariesFolder)\UEXViewer.exe" "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles"

# Create .rl files for registration
	@DIR "$(ExtractFlexCommonInstallFilesRootDir)\SelfRegFiles\*.*" /b >"$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles\ExtractFlexCommon.rl"

# Add .net com objects to the .nl file
	@DIR "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles\Extract.Utilities.Parsers.dll" /b >"$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles\ExtractFlexCommon.nl
	@DIR "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles\Extract.Imaging.dll" /b >>"$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles\ExtractFlexCommon.nl

CreateExtractFlexCommonMergeModule: CopyExtractFlexCommonFiles
	@ECHO Creating ExtractFlexCommon merge module...
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(ReusableComponentsRootDirectory)\APIs\Nuance_16.3\bin;$(ReusableComponentsRootDirectory)\APIs\LeadTools_16.5\bin;$(ReusableComponentsRootDirectory)\APIs\RogueWave\bin;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(VCPP_DIR)\PlatformSDK\bin;$(VISUAL_STUDIO)\SDK\v2.0\bin;C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727;$(VCPP_DIR)\VCPackages
	$(SetProductVerScript) "$(ExtractFlexCommonInstallDir)\\ExtractFlexCommonMM.ism" "$(ReusableComponentsVersion)"
    @"$(DEV_STUDIO_DIR)\System\IsCmdBld.exe" -p "$(ExtractFlexCommonInstallDir)\\ExtractFlexCommonMM.ism"
