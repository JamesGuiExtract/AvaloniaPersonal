#############################################################################
# I N C L U D E S
#
# the common.mak file being included here counts on $(ProductRootDirName)
# ProductRootDirName defined as a non-null string
#

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
	@IF NOT EXIST "$(ExtractCommonInstallFilesRootDir)\DotNetFiles" @MKDIR "$(ExtractCommonInstallFilesRootDir)\DotNetFiles"
	@Deletefiles "$(ExtractCommonInstallFilesRootDir)\NonSelfRegFiles\*.*"
	@Deletefiles "$(ExtractCommonInstallFilesRootDir)\SelfRegFiles\*.*"	
	@Deletefiles "$(ExtractCommonInstallFilesRootDir)\DotNetFiles\*.*"	

ObfuscateFiles: 
	@ECHO Obfuscating .Net files...
#	Copy the strong naming key to location dotfuscator xml file expects
	@IF NOT EXIST "$(StrongNameKeyDir)" @MKDIR "$(StrongNameKeyDir)"
	@COPY /V "$(RCNETDir)\Core\Code\ExtractInternalKey.snk" "$(StrongNameKeyDir)"
	@IF NOT EXIST "$(BinariesFolder)\Obfuscated" @MKDIR "$(BinariesFolder)\Obfuscated"
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Utilities.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Utilities.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Utilities.Email.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Utilities.Email.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Licensing.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Licensing.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Utilities.Forms.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Utilities.Forms.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Drawing.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Drawing.Forms.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Encryption.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Encryption.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Interop.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Interop.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Interfaces.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Interfaces.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Utilities.SecureFileDeleters.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Utilities.SecureFileDeleters.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	
CopyExtractCommonFiles: CleanupExtractCommonFiles ObfuscateFiles
    @ECHO Copying the ExtractCommon files to installation directory...
	@COPY /v  "$(BinariesFolder)\Obfuscated\*.dll" "$(ExtractCommonInstallFilesRootDir)\DotNetFiles"
	@COPY /v  "$(BinariesFolder)\UGMFC.dll" "$(ExtractCommonInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V "$(BinariesFolder)\Interop.UCLID_EXCEPTIONMGMTLib.dll" "$(ExtractCommonInstallFilesRootDir)\DotNetFiles"
	@COPY /V "$(BinariesFolder)\Interop.UCLID_COMUTILSLib.dll" "$(ExtractCommonInstallFilesRootDir)\DotNetFiles"
	@COPY /V "$(BinariesFolder)\Interop.UCLID_COMLMLib.dll" "$(ExtractCommonInstallFilesRootDir)\DotNetFiles"	
	@COPY /V "$(BinariesFolder)\UCLIDExceptionMgmt.dll" "$(ExtractCommonInstallFilesRootDir)\SelfRegFiles"
	@COPY "$(RCNETDir)\APIs\Divelements\SandDock\bin\SandDock.dll" "$(ExtractCommonInstallFilesRootDir)\DotNetFiles" 

	@DIR "$(ExtractCommonInstallFilesRootDir)\SelfRegFiles\*.*" /b >"$(ExtractCommonInstallFilesRootDir)\NonSelfRegFiles\ExtractCommon.rl"
	@DIR "$(ExtractCommonInstallFilesRootDir)\DotNetFiles\Extract.Utilities.Forms.dll" /b >>"$(ExtractCommonInstallFilesRootDir)\NonSelfRegFiles\ExtractCommon.nl
	@DIR "$(ExtractCommonInstallFilesRootDir)\DotNetFiles\Extract.Utilities.Email.dll" /b >>"$(ExtractCommonInstallFilesRootDir)\NonSelfRegFiles\ExtractCommon.nl
	@DIR "$(ExtractCommonInstallFilesRootDir)\DotNetFiles\Extract.Interfaces.dll" /b >>"$(ExtractCommonInstallFilesRootDir)\NonSelfRegFiles\ExtractCommon.nl
	@DIR "$(ExtractCommonInstallFilesRootDir)\DotNetFiles\Extract.Utilities.SecureFileDeleters.dll" /b >>"$(ExtractCommonInstallFilesRootDir)\NonSelfRegFiles\ExtractCommon.nl
	@DIR "$(ExtractCommonInstallFilesRootDir)\DotNetFiles\Extract.Interop.dll" /b >>"$(ExtractCommonInstallFilesRootDir)\NonSelfRegFiles\ExtractCommon.nl
	
CreateExtractCommonMergeModule: CopyExtractCommonFiles  
	@ECHO Creating ExtractCommon merge module...
	$(SetProductVerScript) "$(PDCommonDir)\ExtractCommon\ExtractCommonMM.ism" "$(FlexIndexVersion)"
    @"$(DEV_STUDIO_DIR)\System\IsCmdBld.exe" -p "$(PDCommonDir)\ExtractCommon\ExtractCommonMM.ism"

