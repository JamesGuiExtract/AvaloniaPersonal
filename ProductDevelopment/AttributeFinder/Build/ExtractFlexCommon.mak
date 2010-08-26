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
StrongNameKeyDir=P:\StrongNameKey

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

ObfuscateFiles: 
	@ECHO Obfuscating .Net files...
#	Copy the strong naming key to location dotfuscator xml file expects
	@IF NOT EXIST "$(StrongNameKeyDir)" @MKDIR "$(StrongNameKeyDir)"
	@COPY /V "$(RCNETDir)\Core\Code\ExtractInternalKey.snk" "$(StrongNameKeyDir)"
	@IF NOT EXIST "$(BinariesFolder)\Obfuscated" @MKDIR "$(BinariesFolder)\Obfuscated"
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(ReusableComponentsRootDirectory)\APIs\Nuance_16.3\bin;$(ReusableComponentsRootDirectory)\APIs\LeadTools_16.5\bin;;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(WINDOWS_SDK)\BIN;C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319;$(VCPP_DIR)\VCPackages;C:\Program Files\PreEmptive Solutions\Dotfuscator Professional Edition 4.7
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Utilities.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Utilities.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Imaging.Forms.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Imaging.Forms.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Licensing.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Licensing.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Utilities.Forms.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Utilities.Forms.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Drawing.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Drawing.Forms.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Encryption.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Encryption.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Interop.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Interop.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Utilities.Parsers.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Utilities.Parsers.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Imaging.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Imaging.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated"  $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Imaging.Utilities.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Imaging.Utilities.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated"  $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Rules.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Rules.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	
CopyExtractFlexCommonFiles: CleanupExtractFlexCommonFiles ObfuscateFiles
    @ECHO Copying the ExtractFlexCommon files to installation directory...
	@COPY /v  "$(BinariesFolder)\Obfuscated\*.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"
	@COPY /V "$(ReusableComponentsRootDirectory)\APIs\LeadTools_16.5\Dotnet\Leadtools*.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"
	@COPY "$(RCNETDir)\APIs\Divelements\SandDock\bin\SandDock.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles" 
	@XCOPY "$(ReusableComponentsRootDirectory)\APIs\LeadTools_16.5\Bin\*.*" "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles" /v /s /e /y
	@XCOPY "$(ReusableComponentsRootDirectory)\APIs\LeadTools_16.5\pdf\*.*" "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles\pdf" /v /s /e /y
    @XCOPY "$(ReusableComponentsRootDirectory)\APIs\Nuance_16.3\bin\*.*" "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles" /v /s /e /y
	@COPY /V "$(BinariesFolder)\Interop.UCLID_EXCEPTIONMGMTLib.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"
	@COPY /V "$(BinariesFolder)\Interop.UCLID_COMUTILSLib.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"
	@COPY /V "$(BinariesFolder)\Interop.UCLID_RASTERANDOCRMGMTLib.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"
	@COPY /V "$(BinariesFolder)\Interop.UCLID_SSOCRLib.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"	
	@COPY /V "$(BinariesFolder)\Interop.UCLID_COMLMLib.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"	
	@COPY /V "$(BinariesFolder)\ZLibUtils.dll" "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V "$(BinariesFolder)\TopoUtils.dll" "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V "$(BinariesFolder)\leadutils.dll" "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V "$(BinariesFolder)\UserLicense.exe" "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V "$(BinariesFolder)\UEXViewer.exe" "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles"	
	@COPY /V "$(BinariesFolder)\UCLIDExceptionMgmt.dll" "$(ExtractFlexCommonInstallFilesRootDir)\SelfRegFiles"
	@COPY /V "$(BinariesFolder)\SSOCR2.Exe" "$(ExtractFlexCommonInstallFilesRootDir)\SelfRegFiles"
    @COPY /V "$(BinariesFolder)\SSOCR.dll" "$(ExtractFlexCommonInstallFilesRootDir)\SelfRegFiles"
    @COPY /V "$(BinariesFolder)\UCLIDImageUtils.dll" "$(ExtractFlexCommonInstallFilesRootDir)\SelfRegFiles"
    @COPY /V "$(BinariesFolder)\UCLIDRasterAndOCRMgmt.dll" "$(ExtractFlexCommonInstallFilesRootDir)\SelfRegFiles"
	

# Create .rl files for registration
	@DIR "$(ExtractFlexCommonInstallFilesRootDir)\SelfRegFiles\*.*" /b >"$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles\ExtractFlexCommon.rl"

# Add .net com objects to the .nl file
	@DIR "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles\Extract.Utilities.Parsers.dll" /b >"$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles\ExtractFlexCommon.nl
	@DIR "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles\Extract.Imaging.dll" /b >>"$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles\ExtractFlexCommon.nl
	@DIR "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles\Extract.Utilities.Forms.dll" /b >>"$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles\ExtractFlexCommon.nl

CreateExtractFlexCommonMergeModule: CopyExtractFlexCommonFiles
	@ECHO Creating ExtractFlexCommon merge module...
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(ReusableComponentsRootDirectory)\APIs\Nuance_16.3\bin;$(ReusableComponentsRootDirectory)\APIs\LeadTools_16.5\bin;;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(WINDOWS_SDK)\BIN;C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319;$(VCPP_DIR)\VCPackages
	$(SetProductVerScript) "$(ExtractFlexCommonInstallDir)\ExtractFlexCommonMM.ism" "$(ReusableComponentsVersion)"
    @"$(DEV_STUDIO_DIR)\System\IsCmdBld.exe" -p "$(ExtractFlexCommonInstallDir)\ExtractFlexCommonMM.ism"
