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
ExtractFlexCommonInstallDir=$(PDRootDir)\AttributeFinder\Installation\ExtractFlexCommon
ExtractFlexCommonInstallFilesRootDir=P:\ExtractFlexCommon
RequiredInstallsDir=P:\AttributeFinder\RequiredInstalls

AFBleedingEdgeDir=R:\FlexIndex\Internal\BleedingEdge
FKBUpdateReleaseDir=$(AFBleedingEdgeDir)\$(FKBVersion)
FKBUpdateInstallRoot=$(PDRootDir)\AttributeFinder\Installation\FKBInstall
FKBInstallMediaDir=$(FKBUpdateInstallRoot)\Media\CD-ROM\DiskImages\DISK1

AFCoreInstallFilesRootDir=P:\AttributeFinder\CoreInstallation\Files
LMInstallFilesRootDir=P:\LicenseManager\Files
AFCoreMergeModuleInstallRoot=$(PDRootDir)\AttributeFinder\Installation\UCLID FlexIndex

PDCommonDir=$(PDRootDir)\Common

IDShieldInstallFilesRootDir=P:\AttributeFinder\IDShieldInstallation\Files

DataEntryDir=$(PDRootDir)\DataEntry
LabDEDir=$(DataEntryDir)\LabDE
LabDEInstallRootDir=$(LabDEDir)\Installation
DataEntryInstallFiles=P:\DataEntry
DataEntryCoreInstallFilesDir=$(DataEntryInstallFiles)\CoreInstallation\Files

ExtractCommonInstallFilesRootDir=P:\ExtractCommon

ClearImageInstallFilesDir=P:\AttributeFinder\InliteInstall

InternalUseBuildFilesArchive=M:\ProductDevelopment\AttributeFinder\Archive\InternalUseBuildFiles\InternalBuilds\$(FlexIndexVersion)

ClearImage_5_7_BinDir=$(ReusableComponentsRootDirectory)\APIs\Inlite_5_7\bin
ClearImage_7_0_BinDir=$(ReusableComponentsRootDirectory)\APIs\ClearImage_7_0\bin

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
LabelCommonFolder: 
    $(Label) $$/Engineering/ProductDevelopment/Common -I- -L"$(FlexIndexVersion)" -O

BuildPDUtils: BuildAttributeFinderCore
	@ECHO Building PD Utils...
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @CD "$(PDUtilsRootDir)\UCLIDUtilApps\Code"
    @devenv Utils.sln /BUILD $(BuildConfig) 
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.

BuildAttributeFinderCore:  
	@ECHO Building AFCore...
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @CD "$(AFRootDirectory)\AFCore\AFCoreTest\Code"
    @devenv AFCoreTest.sln /BUILD $(BuildConfig)
	@DeleteFiles "$(BinariesFolder)\DataEntryApplication.exe.config"
	$(CScriptProgram) "$(CommonDirectory)\Make DEPs Compatible.vbs" "$(BinariesFolder)"
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
	
UnregisterClearImage5_7:
	@ECHO Unregistering ClearImage 5_7...
	@CD "$(ClearImage_5_7_BinDir)"
	@IF EXIST "$(ClearImage_5_7_BinDir)" @FOR /R %%i IN (ClearMicr.dll CiCxImage.dll ClearImage.dll ClearCheckIQA.dll) do @regsvr32 "%%~nxi" /u /s
		
RegisterClearImage_7_0: UnregisterClearImage5_7
	@Echo Registering ClearImage 7_0...
	@CD "$(ClearImage_5_7_BinDir)"
	@IF EXIST "$(ClearImage_5_7_BinDir)" @FOR /R %%i IN (ClearMicr.dll CiCxImage.dll ClearImage.dll ClearCheckIQA.dll) do @regsvr32 "%%~nxi" /s
	
ObfuscateFiles: BuildAttributeFinderCore
	@ECHO Obfuscating for FlexIndex...
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
#	Copy the strong naming key to location dotfuscator xml file expects
	@IF NOT EXIST "$(StrongNameKeyDir)" @MKDIR "$(StrongNameKeyDir)"
	@COPY /V "$(RCNETDir)\Core\Code\ExtractInternalKey.snk" "$(StrongNameKeyDir)"
	@IF NOT EXIST "$(BinariesFolder)\Obfuscated" @MKDIR "$(BinariesFolder)\Obfuscated"
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(NUANCE_API_DIR);$(LEADTOOLS_API_DIR);;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(WINDOWS_SDK)\BIN;C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319;$(VCPP_DIR)\VCPackages;$(DOTFUSCATOR)
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Redaction.Verification.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Redaction.Verification.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Redaction.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Redaction.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Database.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Database.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\SQLServerInfo.exe" /mapout:"$(BinariesFolder)\Map\SQLServerInfo.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\ReportViewer.exe" /mapout:"$(BinariesFolder)\Map\ReportViewer.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.AttributeFinder.dll" /mapout:"$(BinariesFolder)\Map\Extract.AttributeFinder.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.FileActionManager.FileProcessors.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.FileActionManager.FileProcessors.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.FileActionManager.FileSuppliers.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.FileActionManager.FileSuppliers.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\SplitMultiPageImage.exe" /mapout:"$(BinariesFolder)\Map\mapSplitMultiPageImage.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\ESFAMService.exe" /mapout:"$(BinariesFolder)\Map\mapESFAMService.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\ESFDRSService.exe" /mapout:"$(BinariesFolder)\Map\mapESFDRSService.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.FileActionManager.Forms.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.FileActionManager.Forms.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\IDShieldStatisticsReporter.exe" /mapout:"$(BinariesFolder)\Map\mapIDShieldStatisticsReporter.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\TestTextFunctionExpander.exe" /mapout:"$(BinariesFolder)\Map\mapTestTextFunctionExpander.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.DataEntry.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.DataEntry.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\DataEntryApplication.exe" /mapout:"$(BinariesFolder)\Map\mapDataEntryApplication.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	editbin.exe /STACK:4194304 "$(BinariesFolder)\Obfuscated\DataEntryApplication.exe"
	editbin.exe /largeaddressaware "$(BinariesFolder)\Obfuscated\DataEntryApplication.exe"
	sn -Ra "$(BinariesFolder)\Obfuscated\DataEntryApplication.exe" "$(StrongNameKeyDir)\ExtractInternalKey.snk"
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\SQLCDBEditor.exe" /mapout:"$(BinariesFolder)\Map\mapSQLCDBEditor.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\RunFPSFile.exe" /mapout:"$(BinariesFolder)\Map\mapRunFPSFile.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	editbin.exe /largeaddressaware "$(BinariesFolder)\Obfuscated\RunFPSFile.exe"
	sn -Ra "$(BinariesFolder)\Obfuscated\RunFPSFile.exe" "$(StrongNameKeyDir)\ExtractInternalKey.snk"
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\SQLCompactExporter.exe" /mapout:"$(BinariesFolder)\Map\mapSQLCompactExporter.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\SQLCompactImporter.exe" /mapout:"$(BinariesFolder)\Map\mapSQLCompactImporter.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\ExtractImageViewer.exe" /mapout:"$(BinariesFolder)\Map\mapExtractImageViewer.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\FAMNetworkManager.exe" /mapout:"$(BinariesFolder)\Map\mapFAMNetworkManager.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\FAMDBLockChecker.exe" /mapout:"$(BinaribesFolder)\Map\mapFAMDBLockChecker.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Office2007ToTif.exe" /mapout:"$(BinariesFolder)\Map\mapOffice2007ToTif.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\OfficeToTif.exe" /mapout:"$(BinaribesFolder)\Map\mapOfficeToTif.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Office.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Office.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.FileActionManager.Database.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.FileActionManager.Database.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Redaction.CustomComponentsHelper.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Redaction.CustomComponentsHelper.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\SetOperations.exe" /mapout:"$(BinariesFolder)\Map\mapSetOperations.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\ModifyPdfFile.exe" /mapout:"$(BinariesFolder)\Map\mapModifyPdfFile.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Utilities.Ftp.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Utilities.Ftp.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\ExceptionHelper.exe" /mapout:"$(BinariesFolder)\Map\mapExceptionHelper.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\IDShieldOnDemand.exe" /mapout:"$(BinariesFolder)\Map\mapIDShieldOnDemand.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	editbin.exe /largeaddressaware "$(BinariesFolder)\Obfuscated\IDShieldOnDemand.exe"
	sn -Ra "$(BinariesFolder)\Obfuscated\IDShieldOnDemand.exe" "$(StrongNameKeyDir)\ExtractInternalKey.snk"
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.FileActionManager.Conditions.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.FileActionManager.Conditions.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\ESIPCService.exe" /mapout:"$(BinariesFolder)\Map\mapESIPCService.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\ExtractDebugData.exe" /mapout:"$(BinariesFolder)\Map\mapExtractDebugData.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.ExceptionUtilities.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.ExceptionUtilities.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.AttributeFinder.Rules.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.AttributeFinder.Rules.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.LabResultsCustomComponents.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.LabResultsCustomComponents.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.NetDMSCustomComponents.dll" /mapout:"$(BinariesFolder)\Map\mapEExtract.NetDMSCustomComponents.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.NetDMSUtilities.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.NetDMSUtilities.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.NetDMSExporter.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.NetDMSExporter.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Microsoft.Data.ConnectionUI.SqlCeDataProvider.dll" /mapout:"$(BinariesFolder)\Map\mapMicrosoft.Data.ConnectionUI.SqlCeDataProvider.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Microsoft.Data.ConnectionUI.Dialog.dll" /mapout:"$(BinariesFolder)\Map\mapMicrosoft.Data.ConnectionUI.Dialog.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Microsoft.Data.ConnectionUI.dll" /mapout:"$(BinariesFolder)\Map\mapMicrosoft.Data.ConnectionUI.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\DataEntryPrompt.exe" /mapout:"$(BinariesFolder)\Map\mapDataEntryPrompt.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\AlternateTestNameManager.plugin" /mapout:"$(BinariesFolder)\Map\mapAlternateTestNameManager.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\PaginationUtility.exe" /mapout:"$(BinariesFolder)\Map\mapPaginationUtility.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\FAMFileInspector.exe" /mapout:"$(BinariesFolder)\Map\mapFAMFileInspector.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.FileActionManager.FAMFileInspector.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.FileActionManager.FAMFileInspector.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\PrintDocument.exe" /mapout:"$(BinariesFolder)\Map\mapPrintDocument.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\ESAppMonitorService.exe" /mapout:"$(BinariesFolder)\Map\mapESAppMonitorService.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.

CopyCommonFiles:
	@ECHO Copying common .NET files to installation build folders...
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
	@XCOPY "$(ExtractFlexCommonInstallFilesRootDir)\DotNetFiles" "$(AFCoreInstallFilesRootDir)\DotNetGAC" /Y/E
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
	
CopyFilesToInstallFolder: BuildPDUtils ObfuscateFiles
    @ECHO Copying the AttributeFinderCore files to installation directory...
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
	@COPY /v  "$(BinariesFolder)\UCLIDAFConditions.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /v  "$(BinariesFolder)\UCLIDAFCore.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /v  "$(BinariesFolder)\UCLIDAFDataScorers.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /v  "$(BinariesFolder)\UCLIDAFFileProcessors.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /v  "$(BinariesFolder)\UCLIDAFUtils.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /v  "$(BinariesFolder)\UCLIDAFValueFinders.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /v  "$(BinariesFolder)\UCLIDAFValueModifiers.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /v  "$(BinariesFolder)\UCLIDAFOutputHandlers.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /v  "$(BinariesFolder)\UCLIDAFSplitters.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /v  "$(BinariesFolder)\UCLIDAFPreProcessors.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
	@COPY /v  "$(BinariesFolder)\ESAFSelectors.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /v  "$(BinariesFolder)\CountyCustomComponents.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /v  "$(BinariesFolder)\ProcessFiles.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /v  "$(BinariesFolder)\RuleTester.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /v  "$(BinariesFolder)\RunRules.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v  "$(BinariesFolder)\FAMDBAdmin.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v  "$(BinariesFolder)\ConvertFPSFile.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v  "$(BinariesFolder)\*.config" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /v  "$(BinariesFolder)\FileProcessors.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /v  "$(BinariesFolder)\UCLIDFileProcessing.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
	@COPY /v  "$(BinariesFolder)\ESFileSuppliers.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles" 
	@COPY /v  "$(BinariesFolder)\ESFAMConditions.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles" 
	@COPY /v  "$(BinariesFolder)\ESImageCleanup.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles" 
    @COPY /v  "$(BinariesFolder)\RedactionCC.dll" "$(IDShieldInstallFilesRootDir)\SelfRegIDShieldComponents"
	@COPY /V  "$(BinariesFolder)\UCLIDTestingFrameworkCore.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles" 
	@COPY /V  "$(BinariesFolder)\RedactionTester.dll" "$(IDShieldInstallFilesRootDir)\SelfRegIDShieldComponents"
    @COPY /v  "$(AFRootDirectory)\IndustrySpecific\Redaction\RedactionCustomComponents\Misc\IDShield.ini" "$(IDShieldInstallFilesRootDir)\NonSelfRegFiles" /y
    @COPY /v  "$(BinariesFolder)\RedactFromXML.exe" "$(IDShieldInstallFilesRootDir)\NonSelfRegFiles"
    @XCOPY   "$(AFRootDirectory)\IndustrySpecific\Redaction\Reports\*.*" "$(IDShieldInstallFilesRootDir)\Reports" /Y/E
	@XCOPY   "$(AFRootDirectory)\IndustrySpecific\Redaction\RedactionCustomComponents\ExemptionCodes\*.xml" "$(IDShieldInstallFilesRootDir)\ExemptionCodes" /Y/E
	@COPY /v  "$(BinariesFolder)\DataEntryApplication.FlexIndex.resources" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles" /y 
    @COPY /v  "$(BinariesFolder)\AFcppUtils.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /v  "$(AFRootDirectory)\Misc\UCLIDAFCore.ini" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /v  "$(AFRootDirectory)\Misc\RunRules_s.bat" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /v "$(BinariesFolder)\USSFileViewer.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /v "$(BinariesFolder)\VOAFileViewer.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v  "$(BinariesFolder)\GetFullUserName.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.Database.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v  "$(BinariesFolder)\Obfuscated\SQLServerInfo.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v  "$(BinariesFolder)\Obfuscated\DataEntryPrompt.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v  "$(BinariesFolder)\Obfuscated\PaginationUtility.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v  "$(BinariesFolder)\Obfuscated\PrintDocument.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY "$(RCNETDir)\APIs\Aspose\Aspose.Pdf for .NET\License\Aspose.Pdf.lic" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles" 
	@COPY /v  "$(BinariesFolder)\Obfuscated\*.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /v  "$(BinariesFolder)\Obfuscated\*.exe" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V  "$(BinariesFolder)\Extract.DataEntry.DEP.DemoFlexIndex.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V "$(BinariesFolder)\Interop.*.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /v "$(BinariesFolder)\Extract.ExceptionService.exe" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /v "$(BinariesFolder)\Extract.ExceptionService.WCFInterface.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY "$(RCNETDir)\APIs\Aspose\Aspose.Pdf for .NET\Bin\net4.0\Aspose.Pdf.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC" 
	@COPY "$(RCNETDir)\APIs\Spring.NET\1.3.1\bin\net\4.0\release\Spring.Core.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC" 
	@COPY "$(RCNETDir)\APIs\Spring.NET\1.3.1\bin\net\4.0\release\Common.Logging.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC" 
	@COPY "$(RCNETDir)\APIs\Spring.NET\1.3.1\bin\net\4.0\release\SpringFrameworkLicense.txt" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY "$(RCNETDir)\APIs\LogicNP\EZShellExtensions.Net\2011\LogicNP.EZShellExtensions.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC" 
	@COPY "$(BinariesFolder)\Extract.Utilities.ShellExtensions.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC" 
	@COPY /v "$(BinariesFolder)\DataEntryCC.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY "$(RCNETDir)\APIs\ScintillaNET v2.4\Dist\*.*" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	
# Need the .net DLLs  in the same folder as Extract.Utilities.Parsers.dll
	@COPY /V  "$(BinariesFolder)\Obfuscated\TestTextFunctionExpander.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v  "$(BinariesFolder)\FAMUtils.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v  "$(BinariesFolder)\ConvertFAMDB.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /v  "$(BinariesFolder)\EmailFile.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v  "$(BinariesFolder)\ESOCR.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /v "$(BinariesFolder)\SplitFile.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v "$(BinariesFolder)\AdjustImageResolution.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v "$(BinariesFolder)\CreateMultiPageImage.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v "$(BinariesFolder)\ImageFormatConverter.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v "$(BinariesFolder)\ESConvertToPDF.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v "$(BinariesFolder)\Sleep.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V "$(BinariesFolder)\LogProcessStats.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v "$(BinariesFolder)\CleanupImage.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v "$(BinariesFolder)\FAMProcess.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v "$(BinariesFolder)\Obfuscated\ReportViewer.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY "$(RCNETDir)\APIs\MSSQLConnectionDialog\References\Microsoft.SqlServerCe.Client.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles" 
	@COPY /v "$(ReusableComponentsRootDirectory)\Scripts\BatchFiles\KillAllOCRInstances.bat" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V "$(PDCommonDir)\SetNuanceServicePermissions.bat" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@XCOPY "$(ReusableComponentsRootDirectory)\APIs\ClearImage_7_0\Installer\*.*" "$(ClearImageInstallFilesDir)\" /v /s /e /y
	@XCOPY "$(ReusableComponentsRootDirectory)\COMComponents\UCLIDFileProcessing\Reports\*.*" "$(AFCoreInstallFilesRootDir)\Reports" /Y/E
	@COPY "$(RCNETDir)\APIs\MSOffice\Office2007\installer\o2007pia.msi" "$(AFCoreInstallFilesRootDir)\OfficeRedist"
	@COPY /v "$(ReusableComponentsRootDirectory)\COMComponents\UCLIDFileProcessing\Utils\ProcessFiles\Code\res\ProcessFiles.ico" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	
    @COPY /V "$(BinariesFolder)\ImageEdit.ocx" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /V "$(BinariesFolder)\UCLIDGenericDisplay2.ocx" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /V "$(BinariesFolder)\UCLIDTestingFramework.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    	
    @COPY /V "$(BinariesFolder)\IFCore.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /V "$(BinariesFolder)\SubImageHandlers.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /V "$(BinariesFolder)\SpotRecognitionIR.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /V "$(BinariesFolder)\GeneralIV.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /V "$(BinariesFolder)\RegExprIV.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
#    @COPY /V "$(BinariesFolder)\SpeechIRs.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"

    @COPY /V "$(BinariesFolder)\OCRFilteringBase.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /V "$(BinariesFolder)\DetectAndReportFailure.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V "$(BinariesFolder)\Obfuscated\ExtractDebugData.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V "$(BinariesFolder)\Obfuscated\ESAppMonitorService.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V "$(RCNETDir)\UtilityApplications\Services\ESAppMonitorService\Core\Install ESAppMonitorService.bat" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V "$(BinariesFolder)\Obfuscated\Extract.ExceptionUtilities.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /V "$(PDUtilsRootDir)\DetectAndReportFailure\Misc\DetectAndReportFailure.ini" "$(AFCoreInstallFilesRootDir)\ProgramDataFiles"
	
	@COPY /V "$(RCNETDir)\APIs\LogicNP\EZShellExtensions.Net\2011\*.*" "$(RequiredInstallsDir)\LogicNP"
	
# Copy testing dlls to archive
	@IF NOT EXIST "$(InternalUseBuildFilesArchive)\NUnitDlls" @MKDIR "$(InternalUseBuildFilesArchive)\NUnitDlls"
	@COPY "$(BinariesFolder)\Extract.Test.dll" "$(InternalUseBuildFilesArchive)\NUnitDlls"
	@COPY "$(BinariesFolder)\Extract.Testing.Utilities.dll" "$(InternalUseBuildFilesArchive)\NUnitDlls"
	@COPY "$(BinariesFolder)\Extract.Encryption.Test.dll" "$(InternalUseBuildFilesArchive)\NUnitDlls"
	@COPY "$(BinariesFolder)\Extract.Geometry.Test.dll" "$(InternalUseBuildFilesArchive)\NUnitDlls"
	@COPY "$(BinariesFolder)\Extract.Imaging.Forms.Test.dll" "$(InternalUseBuildFilesArchive)\NUnitDlls"
	@COPY "$(BinariesFolder)\Extract.Utilities.Test.dll" "$(InternalUseBuildFilesArchive)\NUnitDlls"
	@COPY "$(BinariesFolder)\Extract.SetOperations.Test.dll" "$(InternalUseBuildFilesArchive)\NUnitDlls"
	
# Create .rl files for registration
	@DIR "$(AFCoreInstallFilesRootDir)\SelfRegFiles\*.*" /b >"$(AFCoreInstallFilesRootDir)\NonSelfRegFiles\AFCommon.rl"
	@DIR "$(IDShieldInstallFilesRootDir)\SelfRegIDShieldComponents\*.*" /b >"$(IDShieldInstallFilesRootDir)\NonSelfRegFiles\IDShield.rl"
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\DataEntryCC.dll" /b >"$(DataEntryCoreInstallFilesDir)\NonSelfRegFiles\DataEntry.rl"
	
# Add .net com objects to the .nl file
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.Redaction.Verification.dll" /b >>"$(IDShieldInstallFilesRootDir)\NonSelfRegFiles\IDShield.nl"
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.Redaction.CustomComponentsHelper.dll" /b >>"$(IDShieldInstallFilesRootDir)\NonSelfRegFiles\IDShield.nl"
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.FileActionManager.FileProcessors.dll" /b >>"$(AFCoreInstallFilesRootDir)\NonSelfRegFiles\AFCore.nl"
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.FileActionManager.FileSuppliers.dll" /b >>"$(AFCoreInstallFilesRootDir)\NonSelfRegFiles\AFCore.nl"
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.FileActionManager.Conditions.dll" /b >>"$(AFCoreInstallFilesRootDir)\NonSelfRegFiles\AFCore.nl"
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.FileActionManager.Forms.dll" /b >>"$(AFCoreInstallFilesRootDir)\NonSelfRegFiles\AFCore.nl"
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.AttributeFinder.Rules.dll" /b >>"$(AFCoreInstallFilesRootDir)\NonSelfRegFiles\AFCore.nl"
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.Utilities.ShellExtensions.dll" /b >>"$(AFCoreInstallFilesRootDir)\NonSelfRegFiles\AFCore.nl"
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.Redaction.dll" /b >>"$(IDShieldInstallFilesRootDir)\NonSelfRegFiles\IDShield.nl"
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\IDShieldStatisticsReporter.exe" /b >>"$(IDShieldInstallFilesRootDir)\NonSelfRegFiles\IDShield.nl"
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.DataEntry.dll" /b >"$(DataEntryCoreInstallFilesDir)\NonSelfRegFiles\DataEntry.nl"
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\DataEntryApplication.exe" /b >>"$(DataEntryCoreInstallFilesDir)\NonSelfRegFiles\DataEntry.nl"
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.FileActionManager.FAMFileInspector.dll" /b >>"$(AFCoreInstallFilesRootDir)\NonSelfRegFiles\AFCore.nl"

#    @COPY /V "$(BinariesFolder)\sit_grammar.xml" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /V "$(BinariesFolder)\ImageViewer.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY "$(NUANCE_API_ROOT)\NuanceLicensing.msm" "$(MERGE_MODULE_DIR)"
    @DeleteFiles "$(AFCoreInstallFilesRootDir)\vssver.scc"
    @DeleteFiles "$(AFCoreInstallFilesRootDir)\mssccprj.scc"
	@DeleteFiles "$(ClearImageInstallFilesDir)\vssver.scc"
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
	
CleanupPreviousBuildFolders: CleanUpMergeModulesFromPreviousBuilds
	@ECHO Removing files from previous builds...
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
	@IF NOT EXIST "$(AFCoreInstallFilesRootDir)\SelfRegFiles" @MKDIR "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @IF NOT EXIST "$(AFCoreInstallFilesRootDir)\SelfRegFiles" @MKDIR "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @IF NOT EXIST "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles" @MKDIR "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@IF NOT EXIST "$(AFCoreInstallFilesRootDir)\DotNetGAC" @MKDIR "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@IF NOT EXIST "$(AFCoreInstallFilesRootDir)\Reports" @MKDIR "$(AFCoreInstallFilesRootDir)\Reports"
	@IF NOT EXIST "$(AFCoreInstallFilesRootDir)\OfficeRedist" @MKDIR "$(AFCoreInstallFilesRootDir)\OfficeRedist"
	@IF NOT EXIST "$(IDShieldInstallFilesRootDir)\NonSelfRegFiles" @MKDIR "$(IDShieldInstallFilesRootDir)\NonSelfRegFiles"
	@IF NOT EXIST "$(DataEntryCoreInstallFilesDir)\NonSelfRegFiles" @MKDIR "$(DataEntryCoreInstallFilesDir)\NonSelfRegFiles"
	@IF NOT EXIST "$(RequiredInstallsDir)\LogicNP" @MKDIR "$(RequiredInstallsDir)\LogicNP"
    @IF NOT EXIST "$(AFCoreInstallFilesRootDir)\ProgramDataFiles" @MKDIR "$(AFCoreInstallFilesRootDir)\ProgramDataFiles"
	@IF NOT EXIST "$(IDShieldInstallFilesRootDir)\SelfRegIDShieldComponents" @MKDIR "$(IDShieldInstallFilesRootDir)\SelfRegIDShieldComponents"
	@IF NOT EXIST "$(IDShieldInstallFilesRootDir)\Reports" @MKDIR "$(IDShieldInstallFilesRootDir)\Reports"
	@IF NOT EXIST "$(IDShieldInstallFilesRootDir)\ExemptionCodes" @MKDIR "$(IDShieldInstallFilesRootDir)\ExemptionCodes"
	
    @DeleteFiles "$(AFCoreInstallFilesRootDir)\SelfRegFiles\*.*"
    @DeleteFiles "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles\*.*"
    @DeleteFiles "$(IDShieldInstallFilesRootDir)\NonSelfRegFiles\*.*"
	@DeleteFiles "$(IDShieldInstallFilesRootDir)\Reports\*.*"
	@DeleteFiles "$(IDShieldInstallFilesRootDir)\ExemptionCodes\*.*"
	@DeleteFiles "$(IDShieldInstallFilesRootDir)\SelfRegIDShieldComponents\*.*"
	@DeleteFiles "$(IDShieldInstallFilesRootDir)\NonSelfRegFiles\*.*"
	@DeleteFiles "$(ClearImageInstallFilesDir)\*.*"
	@DeleteFiles "$(AFCoreInstallFilesRootDir)\DotNetGAC\*.*"
	@DeleteFiles "$(AFCoreInstallFilesRootDir)\Reports\*.*"
	@DeleteFiles "$(DataEntryCoreInstallFilesDir)\NonSelfRegFiles\*.*"
	@Deletefiles "$(ExtractCommonInstallFilesRootDir)\NonSelfRegFiles\*.*"
	@Deletefiles "$(ExtractCommonInstallFilesRootDir)\SelfRegFiles\*.*"	
	@Deletefiles "$(AFCoreInstallFilesRootDir)\OfficeRedist\*.*"	
	@Deletefiles "$(RequiredInstallsDir)\LogicNP\*.*"
	@Deletefiles "$(AFCoreInstallFilesRootDir)\ProgramDataFiles\*.*"
	@Deletefiles "$(MERGE_MODULE_DIR)\NuanceLicensing.msm"
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
	
CleanUpMergeModulesFromPreviousBuilds: 
	@ECHO Deleting old merge modules....
	@DeleteFiles "$(MergeModuleDir)\DataEntry.msm"
	@DeleteFiles "$(MergeModuleDir)\UCLIDFlexIndex.msm"
	@DeleteFiles "$(MergeModuleDir)\ExtractBaseMM.msm"
	@DeleteFiles "$(MergeModuleDir)\ExtractCommonMM.msm"

MakeExtractBaseMergeModule: BuildPDUtils
	@ECHO Making ExtractBaseMM...
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
	@CD "$(PDCommonDir)"
    @nmake /F ExtractBase.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" ProductVersion="$(FlexIndexVersion)" CreateExtractBaseMergeModule
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
	
MakeExtractCommonMergeModule: MakeExtractBaseMergeModule
	@ECHO Making ExtractCommonMM...
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
	@CD "$(PDCommonDir)"
    @nmake /F ExtractCommon.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" ProductVersion="$(FlexIndexVersion)" CreateExtractCommonMergeModule
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.

MakeExtractFlexCommonMergeModule: MakeExtractCommonMergeModule
	@ECHO Making ExtractFlexCommonMM...
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @CD "$(AFRootDirectory)\Build
    @nmake /F ExtractFlexCommon.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" ProductVersion="$(FlexIndexVersion)" CreateExtractFlexCommonMergeModule
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
	
BuildFKBUpdateIfRequired:
	@IF NOT EXIST "$(FKBUpdateReleaseDir)" (
		@ECHO Building FKB update
		@ECHO.
		@DATE /T
		@TIME /T
		@ECHO.
		@CD "$(AFRootDirectory)\Build"
		@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(NUANCE_API_DIR);$(LEADTOOLS_API_DIR);;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(WINDOWS_SDK)\BIN;C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319;$(VCPP_DIR)\VCPackages
		NMAKE /F FKBUpdate.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" ProductVersion="$(FlexIndexVersion)" CreateFKBInstall
		@IF NOT EXIST "$(AFCoreInstallFilesRootDir)\FKBInstall" @MKDIR "$(AFCoreInstallFilesRootDir)\FKBInstall"
		@COPY "$(FKBUpdateReleaseDir)\*.*" "$(AFCoreInstallFilesRootDir)\FKBInstall"
		@ECHO.
		@DATE /T
		@TIME /T
		@ECHO.
	)
	
BuildAFCoreMergeModule: CleanupPreviousBuildFolders MakeExtractFlexCommonMergeModule CopyFilesToInstallFolder  
    @ECHO Buliding the UCLIDFlexIndex Merge Module installation...
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(NUANCE_API_DIR);$(LEADTOOLS_API_DIR);;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(WINDOWS_SDK)\BIN;C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319;$(VCPP_DIR)\VCPackages
	$(SetProductVerScript) "$(AFCoreMergeModuleInstallRoot)\UCLID FlexIndex.ism" "$(FlexIndexVersion)"
    @"$(DEV_STUDIO_DIR)\System\IsCmdBld.exe" -p "$(AFCoreMergeModuleInstallRoot)\UCLID FlexIndex.ism"
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.

BuildDataEntryMergeModule: BuildAFCoreMergeModule
    @ECHO Building Extract Systems DataEntry Merge Module...
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(NUANCE_API_DIR);$(LEADTOOLS_API_DIR);;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(WINDOWS_SDK)\BIN;C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319;$(VCPP_DIR)\VCPackages;$(ReusableComponentsRootDirectory)\APIs\LeadTools_17\Dotnet
	$(SetProductVerScript) "$(LabDEInstallRootDir)\DataEntry\DataEntry.ism" "$(FlexIndexVersion)"
    @"$(DEV_STUDIO_DIR)\System\IsCmdBld.exe" -p "$(LabDEInstallRootDir)\DataEntry\DataEntry.ism"
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.

GetAllFiles: GetEngineering

MakeMergeModules: CleanUpMergeModulesFromPreviousBuilds BuildAFCoreMergeModule BuildDataEntryMergeModule 

DoBuilds: SetupBuildEnv BuildPDUtils BuildFKBUpdateIfRequired

DoEverythingNoGet: DoBuilds MakeMergeModules RegisterClearImage_7_0 CopyCommonFiles
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @ECHO Attribute Finder Core Build process completed.
    @ECHO.
  
DoEverything: SetupBuildEnv GetAllFiles DoEverythingNoGet
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @ECHO Attribute Finder Core Build process completed.
    @ECHO.
