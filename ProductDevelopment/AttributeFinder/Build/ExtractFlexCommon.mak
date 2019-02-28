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
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Imaging.Forms.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Imaging.Forms.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Utilities.Parsers.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Utilities.Parsers.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Imaging.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Imaging.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated"  $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Imaging.Utilities.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Imaging.Utilities.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated"  $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Rules.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Rules.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	
CopyExtractFlexCommonFiles: CleanupExtractFlexCommonFiles ObfuscateFiles
    @ECHO Copying the ExtractFlexCommon files to installation directory...
	@COPY /v  "$(BinariesFolder)\Obfuscated\*.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"
	@COPY /V "$(ReusableComponentsRootDirectory)\APIs\LeadTools_17\Dotnet\Leadtools*.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"
	@COPY /V "$(ReusableComponentsRootDirectory)\APIs\LeadTools_17\Dotnet\Leadtools*.dll" "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V "$(RCNETDir)\APIs\EnterpriseDT\Bin\*.*" "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles"
	@XCOPY "$(LEADTOOLS_API_DIR)\*.*" "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles" /v /s /e /y
    @XCOPY "$(NUANCE_API_DIR)\*.*" "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles" /v /s /e /y
	@COPY /V "$(BinariesFolder)\Interop.UCLID_EXCEPTIONMGMTLib.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"
	@COPY /V "$(BinariesFolder)\Interop.UCLID_COMUTILSLib.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"
	@COPY /V "$(BinariesFolder)\Interop.UCLID_RASTERANDOCRMGMTLib.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"
	@COPY /V "$(BinariesFolder)\Interop.UCLID_SSOCRLib.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"	
	@COPY /V "$(BinariesFolder)\Interop.UCLID_COMLMLib.dll" "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles"	
	@COPY /V "$(BinariesFolder)\ZLibUtils.dll" "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V "$(BinariesFolder)\TopoUtils.dll" "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V "$(BinariesFolder)\leadutils.dll" "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V "$(PDCommonDir)\dcomperm\*.*" "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V "$(BinariesFolder)\UserLicense.exe" "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V "$(BinariesFolder)\UEXViewer.exe" "$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles"	
	@COPY /V "$(BinariesFolder)\SSOCR2.Exe" "$(ExtractFlexCommonInstallFilesRootDir)\SelfRegFiles"
    @COPY /V "$(BinariesFolder)\SSOCR.dll" "$(ExtractFlexCommonInstallFilesRootDir)\SelfRegFiles"
    @COPY /V "$(BinariesFolder)\UCLIDImageUtils.dll" "$(ExtractFlexCommonInstallFilesRootDir)\SelfRegFiles"
    @COPY /V "$(BinariesFolder)\UCLIDRasterAndOCRMgmt.dll" "$(ExtractFlexCommonInstallFilesRootDir)\SelfRegFiles"
	

# Create .rl files for registration
	@DIR "$(ExtractFlexCommonInstallFilesRootDir)\SelfRegFiles\*.*" /b >"$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles\ExtractFlexCommon.rl"

# Add .net com objects to the .nl file
	@DIR "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles\Extract.Utilities.Parsers.dll" /b >"$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles\ExtractFlexCommon.nl
	@DIR "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles\Extract.Imaging.dll" /b >>"$(ExtractFlexCommonInstallFilesRootDir)\NonSelfRegFiles\ExtractFlexCommon.nl

CreateExtractFlexCommonMergeModule: CopyExtractFlexCommonFiles
	@ECHO Creating ExtractFlexCommon merge module...
	$(SetProductVerScript) "$(ExtractFlexCommonInstallDir)\ExtractFlexCommonMM.ism" "$(FlexIndexVersion)"
    @"$(DEV_STUDIO_DIR)\System\IsCmdBld.exe" -p "$(ExtractFlexCommonInstallDir)\ExtractFlexCommonMM.ism"
