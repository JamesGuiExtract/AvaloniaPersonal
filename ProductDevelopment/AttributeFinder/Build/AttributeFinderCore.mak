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

FKBUpdateReleaseBaseDir=R:\FlexIndex\FKB
FKBUpdateReleaseDir=$(FKBUpdateReleaseBaseDir)\$(FKBVersion)
FKBUpdateInstallRoot=$(PDRootDir)\AttributeFinder\Installation\FKBInstall
FKBInstallMediaDir=$(FKBUpdateInstallRoot)\Media\CD-ROM\DiskImages\DISK1

AFCoreInstallFilesRootDir=P:\AttributeFinder\CoreInstallation\Files
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

WebAPI=$(EngineeringRootDirectory)\Web\WebAPI\CommonAPICode\bin\Release

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
SetVersions:
	@ECHO Updating Versions for $(FlexIndexVersion)
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
	@IF NOT EXIST "$(EngineeringRootDirectory)" MKDIR "$(EngineeringRootDirectory)"
	$(BUILD_DRIVE)
	@CD "$(EngineeringRootDirectory)"
	@SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(FlexIndexVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cs 1 1 $(UpdateFileVersion) "$(FlexIndexVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.fs 1 1 $(UpdateFileVersion) "$(FlexIndexVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cpp 1 1 $(UpdateFileVersion) "$(FlexIndexVersion)"
	@SendFilesAsArgumentToApplication *.csproj 1 1 $(UpdateFileVersion) "$(FlexIndexVersion)"
	@SendFilesAsArgumentToApplication *.fsproj 1 1 $(UpdateFileVersion) "$(FlexIndexVersion)"
	@CD "$(RCNETDir)"
	$(UpdateFileVersion)  "$(DataEntryBranding)\FlexIndex.resx" "$(FlexIndexVersion)"
	$(UpdateFileVersion)  "$(DataEntryBranding)\LabDE.resx" "$(FlexIndexVersion)"
	@CD "$(DataEntryDir)\FlexIndex"
	@SendFilesAsArgumentToApplication *.resx 1 1 $(UpdateFileVersion) "$(FlexIndexVersion)"
	@CD "$(DataEntryDir)\LabDE"
	@SendFilesAsArgumentToApplication *.resx 1 1 $(UpdateFileVersion) "$(FlexIndexVersion)"
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
	
BuildDashboards: BuildPDUtils
	@ECHO Building Dashboard apps...
	@Echo.
    @DATE /T
    @TIME /T
    @ECHO.
    @CD "$(RCNETDir)\Dashboards"
    @devenv Dashboards.sln /BUILD $(BuildConfig) 
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
	
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

CopyAPIFiles:
	@ECHO Copying API files to Release
	IF NOT EXIST "$(BinariesFolder)" @MKDIR "$(BinariesFolder)"
	@COPY /V /Y "$(LEADTOOLS_API_DIR)\*.*"  "$(BinariesFolder)"
	@COPY /V /Y "$(NUANCE_API_DIR)\*.*" "$(BinariesFolder)"
	
BuildAttributeFinderCore: CopyAPIFiles 
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
	@COPY /V /Y "$(RCNETDir)\Core\Code\ExtractInternalKey.snk" "$(StrongNameKeyDir)"
	@IF NOT EXIST "$(BinariesFolder)\Obfuscated" @MKDIR "$(BinariesFolder)\Obfuscated"
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Redaction.Verification.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Redaction.Verification.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Redaction.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Redaction.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Database.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Database.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\SQLServerInfo.exe" /mapout:"$(BinariesFolder)\Map\SQLServerInfo.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\ReportViewer.exe" /mapout:"$(BinariesFolder)\Map\ReportViewer.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	editbin.exe /largeaddressaware "$(BinariesFolder)\Obfuscated\ReportViewer.exe"
	sn -Ra "$(BinariesFolder)\Obfuscated\ReportViewer.exe" "$(StrongNameKeyDir)\ExtractInternalKey.snk"
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.AttributeFinder.dll" /mapout:"$(BinariesFolder)\Map\Extract.AttributeFinder.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.AttributeFinder.Tabula.dll" /mapout:"$(BinariesFolder)\Map\Extract.AttributeFinder.Tabula.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.FileActionManager.FileProcessors.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.FileActionManager.FileProcessors.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.FileActionManager.FileSuppliers.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.FileActionManager.FileSuppliers.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\SplitMultiPageImage.exe" /mapout:"$(BinariesFolder)\Map\mapSplitMultiPageImage.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\ESFAMService.exe" /mapout:"$(BinariesFolder)\Map\mapESFAMService.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	editbin.exe /largeaddressaware "$(BinariesFolder)\Obfuscated\ESFAMService.exe"
	sn -Ra "$(BinariesFolder)\Obfuscated\ESFAMService.exe" "$(StrongNameKeyDir)\ExtractInternalKey.snk"
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
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\SpatialStringChecksum.exe" /mapout:"$(BinariesFolder)\Map\mapSpatialStringChecksum.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\SQLCompactExporter.exe" /mapout:"$(BinariesFolder)\Map\mapSQLCompactExporter.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\SQLCompactImporter.exe" /mapout:"$(BinariesFolder)\Map\mapSQLCompactImporter.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\ExtractImageViewer.exe" /mapout:"$(BinariesFolder)\Map\mapExtractImageViewer.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\FAMNetworkManager.exe" /mapout:"$(BinariesFolder)\Map\mapFAMNetworkManager.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\FAMDBCounterManager.exe" /mapout:"$(BinaribesFolder)\Map\mapFAMDBCounterManager.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\FAMDBLockChecker.exe" /mapout:"$(BinaribesFolder)\Map\mapFAMDBLockChecker.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
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
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.AttributeFinder.Forms.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.AttributeFinder.Forms.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.LabResultsCustomComponents.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.LabResultsCustomComponents.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Microsoft.Data.ConnectionUI.SQLiteDataProvider.local.dll" /mapout:"$(BinariesFolder)\Map\mapMicrosoft.Data.ConnectionUI.SQLiteDataProvider.local.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Microsoft.Data.ConnectionUI.SqlCeDataProvider.local.dll" /mapout:"$(BinariesFolder)\Map\mapMicrosoft.Data.ConnectionUI.SqlCeDataProvider.local.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Microsoft.Data.ConnectionUI.Dialog.local.dll" /mapout:"$(BinariesFolder)\Map\mapMicrosoft.Data.ConnectionUI.Dialog.local.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\DataEntryPrompt.exe" /mapout:"$(BinariesFolder)\Map\mapDataEntryPrompt.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\AlternateTestNameManager.plugin" /mapout:"$(BinariesFolder)\Map\mapAlternateTestNameManager.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\PaginationUtility.exe" /mapout:"$(BinariesFolder)\Map\mapPaginationUtility.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\FAMFileInspector.exe" /mapout:"$(BinariesFolder)\Map\mapFAMFileInspector.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.FileActionManager.FAMFileInspector.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.FileActionManager.FAMFileInspector.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\PrintDocument.exe" /mapout:"$(BinariesFolder)\Map\mapPrintDocument.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\ESAppMonitorService.exe" /mapout:"$(BinariesFolder)\Map\mapESAppMonitorService.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\SpecialImageFormatConverter.exe" /mapout:"$(BinariesFolder)\Map\mapSpecialImageFormatConverter.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.DataEntry.LabDE.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.DataEntry.LabDE.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Utilities.ContextTags.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Utilities.ContextTags.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\ResolutionNormalizer.exe" /mapout:"$(BinariesFolder)\Map\mapResolutionNormalizer.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\LearningMachineEditor.exe" /mapout:"$(BinariesFolder)\Map\mapLearningMachineEditor.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	editbin.exe /largeaddressaware "$(BinariesFolder)\Obfuscated\LearningMachineEditor.exe"
	sn -Ra "$(BinariesFolder)\Obfuscated\LearningMachineEditor.exe" "$(StrongNameKeyDir)\ExtractInternalKey.snk"
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Interop.Zip.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Interop.Zip.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\StatisticsReporter.exe" /mapout:"$(BinariesFolder)\Map\mapStatisticsReporter.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	editbin.exe /largeaddressaware "$(BinariesFolder)\Obfuscated\StatisticsReporter.exe"
	sn -Ra "$(BinariesFolder)\Obfuscated\StatisticsReporter.exe" "$(StrongNameKeyDir)\ExtractInternalKey.snk"
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.DataCaptureStats.dll" /mapout:"$(BinariesFolder)\Map\Extract.DataCaptureStats.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\ZstdNet.dll" /mapout:"$(BinariesFolder)\Map\ZstdNet.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\NERAnnotator.exe" /mapout:"$(BinariesFolder)\Map\mapNERAnnotator.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	editbin.exe /largeaddressaware "$(BinariesFolder)\Obfuscated\NERAnnotator.exe"
	sn -Ra "$(BinariesFolder)\Obfuscated\NERAnnotator.exe" "$(StrongNameKeyDir)\ExtractInternalKey.snk"
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\TrainingDataCollector.exe" /mapout:"$(BinariesFolder)\Map\mapTrainingDataCollector.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	editbin.exe /largeaddressaware "$(BinariesFolder)\Obfuscated\TrainingDataCollector.exe"
	sn -Ra "$(BinariesFolder)\Obfuscated\TrainingDataCollector.exe" "$(StrongNameKeyDir)\ExtractInternalKey.snk"
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\TrainingCoordinator.exe" /mapout:"$(BinariesFolder)\Map\mapTrainingCoordinator.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\LearningMachineTrainer.exe" /mapout:"$(BinariesFolder)\Map\mapLMTrainer.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\MLModelTrainer.exe" /mapout:"$(BinariesFolder)\Map\mapMLModelTrainer.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\DashboardCreator.exe" /mapout:"$(BinariesFolder)\Map\mapDashboardCreator.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	editbin.exe /largeaddressaware "$(BinariesFolder)\Obfuscated\DashboardCreator.exe"
	sn -Ra "$(BinariesFolder)\Obfuscated\DashboardCreator.exe" "$(StrongNameKeyDir)\ExtractInternalKey.snk"
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\DashboardViewer.exe" /mapout:"$(BinariesFolder)\Map\mapDashboardViewer.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	editbin.exe /largeaddressaware "$(BinariesFolder)\Obfuscated\DashboardViewer.exe"
	sn -Ra "$(BinariesFolder)\Obfuscated\DashboardViewer.exe" "$(StrongNameKeyDir)\ExtractInternalKey.snk"
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.ETL.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.ETL.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.ETL.Management.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.ETL.Management.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Dashboard.Utilities.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Dashboard.Utilities.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Dashboard.Forms.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Dashboard.Forms.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract64.Core.dll" /mapout:"$(BinariesFolder)\Map\mapExtract64.Cores.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Code.Attributes.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Code.Attributes.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Process.Logger.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Process.Logger.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.GoogleCloud.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.GoogleCloud.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.AttributeFinder.Rules.FSharp.dll" /mapout:"$(BinariesFolder)\Map\Extract.AttributeFinder.Rules.FSharp.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Utilities.FSharp.CSharpInterop.dll" /mapout:"$(BinariesFolder)\Map\Extract.Utilities.FSharp.CSharpInterop.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Utilities.FSharp.NERAnnotation.dll" /mapout:"$(BinariesFolder)\Map\Extract.Utilities.FSharp.NERAnnotation.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Dashboard.ETL.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Dashboard.ETL.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.Reporting.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Reporting.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.ReportingDevExpress.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.ReportingDevExpress.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\ReportDesigner.exe" /mapout:"$(BinariesFolder)\Map\mapReportDesigner.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml
	editbin.exe /largeaddressaware "$(BinariesFolder)\Obfuscated\ReportDesigner.exe"
	sn -Ra "$(BinariesFolder)\Obfuscated\ReportDesigner.exe" "$(StrongNameKeyDir)\ExtractInternalKey.snk"
	dotfuscator.exe /nologo /in:"$(BinariesFolder)\Extract.SqlDatabase.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.SqlDatabase.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(PDCommonDir)\ObfuscateConfig.xml


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
	@COPY /V /Y  "$(BinariesFolder)\UCLIDAFConditions.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /V /Y  "$(BinariesFolder)\UCLIDAFCore.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /V /Y  "$(BinariesFolder)\UCLIDAFDataScorers.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /V /Y  "$(BinariesFolder)\UCLIDAFFileProcessors.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /V /Y  "$(BinariesFolder)\UCLIDAFUtils.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /V /Y  "$(BinariesFolder)\UCLIDAFValueFinders.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /V /Y  "$(BinariesFolder)\UCLIDAFValueModifiers.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /V /Y  "$(BinariesFolder)\UCLIDAFOutputHandlers.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /V /Y  "$(BinariesFolder)\UCLIDAFSplitters.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /V /Y  "$(BinariesFolder)\UCLIDAFPreProcessors.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
	@COPY /V /Y  "$(BinariesFolder)\ESAFSelectors.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /V /Y  "$(BinariesFolder)\CountyCustomComponents.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /V /Y  "$(BinariesFolder)\ProcessFiles.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /V /Y  "$(BinariesFolder)\RuleTester.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /V /Y  "$(BinariesFolder)\RunRules.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y  "$(BinariesFolder)\FAMDBAdmin.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y  "$(BinariesFolder)\ConvertFPSFile.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y  "$(BinariesFolder)\*.config" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /V /Y  "$(BinariesFolder)\FileProcessors.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /V /Y  "$(BinariesFolder)\UCLIDFileProcessing.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
	@COPY /V /Y  "$(BinariesFolder)\ESFileSuppliers.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles" 
	@COPY /V /Y  "$(BinariesFolder)\ESFAMConditions.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles" 
	@COPY /V /Y  "$(BinariesFolder)\ESImageCleanup.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles" 
	@COPY /V /Y  "$(BinariesFolder)\AttributeDBMgrComponents.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles" 
    @COPY /V /Y  "$(BinariesFolder)\RedactionCC.dll" "$(IDShieldInstallFilesRootDir)\SelfRegIDShieldComponents"
	@COPY /V /Y  "$(BinariesFolder)\UCLIDTestingFrameworkCore.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles" 
	@COPY /V /Y  "$(BinariesFolder)\RedactionTester.dll" "$(IDShieldInstallFilesRootDir)\SelfRegIDShieldComponents"
    @COPY /V /Y  "$(AFRootDirectory)\IndustrySpecific\Redaction\RedactionCustomComponents\Misc\IDShield.ini" "$(IDShieldInstallFilesRootDir)\NonSelfRegFiles" /y
    @COPY /V /Y  "$(BinariesFolder)\RedactFromXML.exe" "$(IDShieldInstallFilesRootDir)\NonSelfRegFiles"
    @XCOPY   "$(AFRootDirectory)\IndustrySpecific\Redaction\Reports\*.*" "$(IDShieldInstallFilesRootDir)\Reports" /Y/E
	@XCOPY   "$(AFRootDirectory)\IndustrySpecific\Redaction\RedactionCustomComponents\ExemptionCodes\*.xml" "$(IDShieldInstallFilesRootDir)\ExemptionCodes" /Y/E
	@COPY /V /Y  "$(BinariesFolder)\DataEntryApplication.FlexIndex.resources" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles" /y 
    @COPY /V /Y  "$(BinariesFolder)\AFcppUtils.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /V /Y  "$(AFRootDirectory)\Misc\UCLIDAFCore.ini" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /V /Y  "$(AFRootDirectory)\Misc\RunRules_s.bat" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /V /Y "$(BinariesFolder)\USSFileViewer.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /V /Y "$(BinariesFolder)\VOAFileViewer.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y  "$(BinariesFolder)\GetFullUserName.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\Interop.*.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\ADODB.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\MahApps.Metro.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\MahApps.Metro.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V /Y "$(BinariesFolder)\ControlzEx.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\ControlzEx.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V /Y "$(BinariesFolder)\Microsoft.Xaml.Behaviors.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\Microsoft.Xaml.Behaviors.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V /Y "$(BinariesFolder)\DatabaseMigrationWizard.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\Extract.FileConverter.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\Extract.FileConverter.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V /Y "$(BinariesFolder)\Microsoft.Office.Interop.*.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V /Y "$(BinariesFolder)\Microsoft.Vbe.Interop.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V /Y "$(BinariesFolder)\Microsoft.Vbe.Interop.Forms.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V /Y "$(BinariesFolder)\Office.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V /Y  "$(BinariesFolder)\Obfuscated\SQLServerInfo.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y  "$(BinariesFolder)\Obfuscated\SpatialStringChecksum.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y  "$(BinariesFolder)\Obfuscated\DataEntryPrompt.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y  "$(BinariesFolder)\Obfuscated\PaginationUtility.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y  "$(BinariesFolder)\Obfuscated\PrintDocument.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y  "$(BinariesFolder)\Obfuscated\SpecialImageFormatConverter.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y  "$(BinariesFolder)\Obfuscated\ResolutionNormalizer.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y  "$(BinariesFolder)\Obfuscated\DashboardViewer.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y  "$(BinariesFolder)\Obfuscated\DashboardCreator.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y  "$(BinariesFolder)\Obfuscated\ReportDesigner.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y  "$(BinariesFolder)\Obfuscated\NERAnnotator.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y  "$(BinariesFolder)\Obfuscated\TrainingDataCollector.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y  "$(BinariesFolder)\Obfuscated\TrainingCoordinator.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y  "$(BinariesFolder)\Obfuscated\LearningMachineTrainer.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y  "$(BinariesFolder)\Obfuscated\MLModelTrainer.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y  "$(BinariesFolder)\Obfuscated\Extract.SqlDatabase.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y  "$(BinariesFolder)\Obfuscated\Microsoft.Data.ConnectionUI.SQLiteDataProvider.local.dll"" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y  "$(BinariesFolder)\Obfuscated\Microsoft.Data.ConnectionUI.SqlCeDataProvider.local.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y  "$(BinariesFolder)\Obfuscated\Microsoft.Data.ConnectionUI.Dialog.local.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y  "$(BinariesFolder)\Microsoft.Data.ConnectionUI.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y  "$(BinariesFolder)\RedactionPredictor.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y  "$(BinariesFolder)\PredictionEvaluator.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(RCNETDir)\APIs\Aspose\Aspose.Pdf for .Net 9.8\License\Aspose.Pdf.lic" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles" 
	@COPY /V /Y "$(CommonDirectory)\FixMachineConfig.vbs" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles" 
	@COPY /V /Y "$(CommonDirectory)\FixEverything.bat" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles" 
	@COPY /V /Y  "$(BinariesFolder)\Obfuscated\*.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V /Y  "$(BinariesFolder)\Obfuscated\*.exe" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V /Y  "$(BinariesFolder)\Extract.DataEntry.DEP.DemoFlexIndex.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V /Y "$(BinariesFolder)\Interop.*.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V /Y "$(BinariesFolder)\ADODB.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V /Y "$(BinariesFolder)\Extract.ExceptionService.exe" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V /Y "$(BinariesFolder)\Extract.ExceptionService.WCFInterface.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V /Y "$(BinariesFolder)\Extract.AttributeFinder.Rules.Domain.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V /Y "$(BinariesFolder)\Extract.AttributeFinder.Rules.Dto.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V /Y "$(BinariesFolder)\Extract.AttributeFinder.Rules.Json.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V /Y  "$(BinariesFolder)\ResolutionNormalizer2.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(RCNETDir)\APIs\Aspose\Aspose.Pdf for .Net 9.8\Bin\net4.0\Aspose.Pdf.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC" 
	@COPY /V /Y "$(RCNETDir)\APIs\Spring.NET\1.3.1\bin\net\4.0\release\Spring.Core.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC" 
	@COPY /V /Y "$(RCNETDir)\APIs\Spring.NET\1.3.1\bin\net\4.0\release\Common.Logging.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V /Y "$(RCNETDir)\APIs\DevExpress\v19.2\*.*" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(RCNETDir)\APIs\Lucene.Net.4.8.0\lib\net45\*.*" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(RCNETDir)\APIs\Lucene.Net.4.8.0\lib\net45\*.*" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V /Y "$(RCNETDir)\APIs\zstd\1.1.0\build\VS_scripts\bin\Release\x64\zstdlib_x64.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(RCNETDir)\APIs\ZstdNet\ZstdNet\bin\x64\Release\ZstdNet64.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\Google*.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\Microsoft.Office.Interop.*.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\Microsoft.Vbe.Interop.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\Microsoft.Vbe.Interop.Forms.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\Office.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\GalaSoft*.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\FirstFloor*.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\grpc*.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\FSharp.*.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\protobuf-net.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\PdfSharp.*.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\Elmish.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\Elmish.WPF.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\FAMServiceManager.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\Extract.FileActionManager.Utilities.FAMServiceManager.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\Extract.Utilities.FSharp.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"

# This includes things in the install
	@COPY /V /Y "$(BinariesFolder)\Newtonsoft.Json.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\System.Reactive.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\System.Reactive.Linq.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
# This makes things available when installshield runs regasm
	@COPY /V /Y "$(BinariesFolder)\Newtonsoft.Json.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V /Y "$(BinariesFolder)\System.Reactive.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V /Y "$(BinariesFolder)\System.Reactive.Linq.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"


	@COPY /V /Y "$(RCNETDir)\APIs\LogicNP\EZShellExtensions.Net\2011\LogicNP.EZShellExtensions.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC" 
	@COPY /V /Y "$(BinariesFolder)\Extract.Utilities.ShellExtensions.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC" 
	@COPY /V /Y "$(BinariesFolder)\DataEntryCC.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V /Y "$(BinariesFolder)\StatisticsReporter.exe.config" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V /Y "$(BinariesFolder)\zstdlib_x86.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V /Y "$(BinariesFolder)\OpenNLP.IKVM.exe" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V /Y "$(BinariesFolder)\Tabula.IKVM.exe" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V /Y "$(LEADTOOLS_API_DOTNET)\*.*"  "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V /Y "$(RCNETDir)\APIs\ScintillaNET v2.4\Dist\*.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	
# Copy all the FOSS license files
	@COPY /V /Y "$(RCNETDir)\APIs\Licenses\*.*" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"	
	
# Need the .net DLLs  in the same folder as Extract.Utilities.Parsers.dll
	@COPY /V /Y  "$(BinariesFolder)\Obfuscated\TestTextFunctionExpander.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y  "$(BinariesFolder)\FAMUtils.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y  "$(BinariesFolder)\ConvertFAMDB.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /V /Y  "$(BinariesFolder)\EmailFile.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y  "$(BinariesFolder)\ESOCR.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /V /Y "$(BinariesFolder)\SplitFile.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\AdjustImageResolution.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\CreateMultiPageImage.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\ImageFormatConverter.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\ESConvertToPDF.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\Sleep.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\LogProcessStats.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\CleanupImage.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\Obfuscated\ReportViewer.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(RCNETDir)\APIs\MSSQLConnectionDialog\References\Microsoft.SqlServerCe.Client.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles" 
	@COPY /V /Y "$(ReusableComponentsRootDirectory)\Scripts\BatchFiles\KillAllOCRInstances.bat" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(PDCommonDir)\SetNuanceServicePermissions.bat" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(PDCommonDir)\RegisterShellExtension.bat" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(PDCommonDir)\UnRegisterShellExtension.bat" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@XCOPY "$(ReusableComponentsRootDirectory)\APIs\ClearImage_7_0\Installer\*.*" "$(ClearImageInstallFilesDir)\" /v /s /e /y
	@XCOPY "$(ReusableComponentsRootDirectory)\COMComponents\UCLIDFileProcessing\Reports\*.*" "$(AFCoreInstallFilesRootDir)\Reports" /Y/E
	@COPY /V /Y "$(ReusableComponentsRootDirectory)\COMComponents\UCLIDFileProcessing\Utils\ProcessFiles\Code\res\ProcessFiles.ico" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(ReusableComponentsRootDirectory)\APIs\Nuance_20\Bin\CAPI_PInvoke.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@XCOPY "$(RCNETDir)\APIs\IKVM\lib\*.*" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles" /v /s /e /y
# Copy to DotNetGAC for Installshield
	@XCOPY "$(RCNETDir)\APIs\IKVM\lib\*.*" "$(AFCoreInstallFilesRootDir)\DotNetGAC" /v /s /e /y
	@XCOPY "$(BinariesFolder)\Microsoft.WindowsAPICodePack.*" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles" /v /s /e /y
# Copy WindowsAPICodePack to DotNetGAC for installshield
	@XCOPY "$(BinariesFolder)\Microsoft.WindowsAPICodePack.*" "$(AFCoreInstallFilesRootDir)\DotNetGAC" /v /s /e /y
	@COPY /V /Y "$(BinariesFolder)\OpenNLP.IKVM.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\Tabula.IKVM.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\YamlDotNet.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	
    @COPY /V /Y "$(BinariesFolder)\ImageEdit.ocx" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /V /Y "$(BinariesFolder)\UCLIDGenericDisplay2.ocx" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /V /Y "$(BinariesFolder)\UCLIDTestingFramework.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\FAMProcess.exe" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    	
    @COPY /V /Y "$(BinariesFolder)\IFCore.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /V /Y "$(BinariesFolder)\SubImageHandlers.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /V /Y "$(BinariesFolder)\SpotRecognitionIR.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /V /Y "$(BinariesFolder)\GeneralIV.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /V /Y "$(BinariesFolder)\RegExprIV.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
#    @COPY /V /Y "$(BinariesFolder)\SpeechIRs.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"

    @COPY /V /Y "$(BinariesFolder)\OCRFilteringBase.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /V /Y "$(BinariesFolder)\DetectAndReportFailure.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\Obfuscated\ExtractDebugData.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\Obfuscated\ESAppMonitorService.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(RCNETDir)\UtilityApplications\Services\ESAppMonitorService\Core\Install ESAppMonitorService.bat" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\Obfuscated\Extract.ExceptionUtilities.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\Obfuscated\Extract.ETL.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /V /Y "$(PDUtilsRootDir)\DetectAndReportFailure\Misc\DetectAndReportFailure.ini" "$(AFCoreInstallFilesRootDir)\ProgramDataFiles"
	@COPY /V /Y "$(BinariesFolder)\Obfuscated\Extract.GoogleCloud.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\Obfuscated\Extract.Utilities.FSharp.CSharpInterop.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\Obfuscated\Extract.Utilities.FSharp.NERAnnotation.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\ShrinkLargePages.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\Unquote.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\Argu.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\CpuMathNative.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\Extract.AttributeFinder.MLNet.ClassifyCandidates.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\FastTreeNative.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\FsPickler.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\LdaNative.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\MLNetQueue.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\MatrixFactorizationNative.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\MklImports.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\MklProxyNative.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\NumSharp.Lite.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\Protobuf.Text.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\SymSgdNative.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\System.CodeDom.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\System.Configuration.ConfigurationManager.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\System.Drawing.Common.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\System.IO.FileSystem.AccessControl.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\System.Threading.Channels.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\System.Task.Extensions.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\System.Runtime.CompilerServices.Unsafe.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\Microsoft.Bcl.AsyncInterfaces.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\TensorFlow.NET.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\lib_lightgbm.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\libiomp5md.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\lightgbm.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@XCOPY "$(BinariesFolder)\Microsoft.ML.*.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles" /v /s /e /y
	@COPY /V /Y "$(BinariesFolder)\Extract.Utilities.SqlCompactToSqliteConverter.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\SqlCompactToSqliteConverter.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\ISqlCeScripting.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\SqlCeScripting.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\linq2db.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(BinariesFolder)\System.Data.SQLite.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@XCOPY "$(BinariesFolder)\x64\SQLite.Interop.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles\x64\" /Y
	@XCOPY "$(BinariesFolder)\x86\SQLite.Interop.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles\x86\" /Y
	@COPY /V /Y "$(BinariesFolder)\DEPChecker.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"

# Copy Web files
	@XCOPY "$(WebAPI)\*.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles" /Y
	@XCOPY "$(WebAPI)\*.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles" /Y
	@XCOPY "$(WebAPI)\*.xml" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles" /Y
	@XCOPY "$(WebAPI)\*.config" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles" /Y
	@XCOPY "$(WebAPI)\*.json" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles" /Y
	
	@COPY /V /Y "$(RCNETDir)\APIs\LogicNP\EZShellExtensions.Net\2011\*.*" "$(RequiredInstallsDir)\LogicNP"
	@COPY /V /Y "$(RCNETDir)\APIs\LogicNP\EZShellExtensions.Net\2011\RegisterExtensionDotNet40_x*.*" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles" 
	@COPY /V /Y  "$(BinariesFolder)\EncryptFile.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	
# Copy testing dlls to archive
	@IF NOT EXIST "$(InternalUseBuildFilesArchive)\NUnitDlls" @MKDIR "$(InternalUseBuildFilesArchive)\NUnitDlls"
	@COPY /V /Y "$(BinariesFolder)\Extract.Test.dll" "$(InternalUseBuildFilesArchive)\NUnitDlls"
	@COPY /V /Y "$(BinariesFolder)\Extract.Testing.Utilities.dll" "$(InternalUseBuildFilesArchive)\NUnitDlls"
	@COPY /V /Y "$(BinariesFolder)\Extract.Encryption.Test.dll" "$(InternalUseBuildFilesArchive)\NUnitDlls"
	@COPY /V /Y "$(BinariesFolder)\Extract.Imaging.Forms.Test.dll" "$(InternalUseBuildFilesArchive)\NUnitDlls"
	@COPY /V /Y "$(BinariesFolder)\Extract.Utilities.Test.dll" "$(InternalUseBuildFilesArchive)\NUnitDlls"
	@COPY /V /Y "$(BinariesFolder)\Extract.SetOperations.Test.dll" "$(InternalUseBuildFilesArchive)\NUnitDlls"
	
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
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.AttributeFinder.Forms.dll" /b >>"$(AFCoreInstallFilesRootDir)\NonSelfRegFiles\AFCore.nl"	
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.Redaction.dll" /b >>"$(IDShieldInstallFilesRootDir)\NonSelfRegFiles\IDShield.nl"
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\IDShieldStatisticsReporter.exe" /b >>"$(IDShieldInstallFilesRootDir)\NonSelfRegFiles\IDShield.nl"
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.DataEntry.dll" /b >"$(DataEntryCoreInstallFilesDir)\NonSelfRegFiles\DataEntry.nl"
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\DataEntryApplication.exe" /b >>"$(DataEntryCoreInstallFilesDir)\NonSelfRegFiles\DataEntry.nl"
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.FileActionManager.FAMFileInspector.dll" /b >>"$(AFCoreInstallFilesRootDir)\NonSelfRegFiles\AFCore.nl"
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.Utilities.ContextTags.dll" /b >>"$(AFCoreInstallFilesRootDir)\NonSelfRegFiles\AFCore.nl"
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.Process.Logger.dll" /b >>"$(AFCoreInstallFilesRootDir)\NonSelfRegFiles\AFCore.nl"
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.AttributeFinder.Rules.FSharp.dll" /b >>"$(AFCoreInstallFilesRootDir)\NonSelfRegFiles\AFCore.nl"
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.AttributeFinder.Rules.Json.dll" /b >>"$(AFCoreInstallFilesRootDir)\NonSelfRegFiles\AFCore.nl"

#    @COPY /V /Y "$(BinariesFolder)\sit_grammar.xml" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /V /Y "$(BinariesFolder)\ImageViewer.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /V /Y "$(NUANCE_API_ROOT)\NuanceLicensing.msm" "$(MERGE_MODULE_DIR)"
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
	
CopyFKB:
	@ECHO Copy FKB update
	@ECHO.
	@DATE /T
	@TIME /T
	@ECHO.
	@IF NOT EXIST "$(AFCoreInstallFilesRootDir)\FKBInstall" @MKDIR "$(AFCoreInstallFilesRootDir)\FKBInstall"
	@COPY /V /Y "$(FKBUpdateReleaseDir)\*.*" "$(AFCoreInstallFilesRootDir)\FKBInstall"
	@ECHO.
	@DATE /T
	@TIME /T
	@ECHO.
	
BuildAFCoreMergeModule: CleanupPreviousBuildFolders MakeExtractFlexCommonMergeModule CopyFilesToInstallFolder  
    @ECHO Buliding the UCLIDFlexIndex Merge Module installation...
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
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
	$(SetProductVerScript) "$(LabDEInstallRootDir)\DataEntry\DataEntry.ism" "$(FlexIndexVersion)"
    @"$(DEV_STUDIO_DIR)\System\IsCmdBld.exe" -p "$(LabDEInstallRootDir)\DataEntry\DataEntry.ism"
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.

MakeMergeModules: CleanUpMergeModulesFromPreviousBuilds BuildAFCoreMergeModule BuildDataEntryMergeModule 

DoBuilds: SetupBuildEnv SetVersions BuildPDUtils BuildDashboards CopyFKB

DoEverythingNoGet: DoBuilds MakeMergeModules RegisterClearImage_7_0 CopyCommonFiles
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @ECHO Attribute Finder Core Build process completed.
    @ECHO.
  
DoEverything: SetupBuildEnv DoEverythingNoGet
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @ECHO Attribute Finder Core Build process completed.
    @ECHO.
