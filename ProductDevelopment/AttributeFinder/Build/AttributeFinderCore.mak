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
RCNETDir=$(EngineeringRootDirectory)\RC.Net
RulesDir=$(EngineeringRootDirectory)\Rules

AFCoreInstallFilesRootDir=P:\AttributeFinder\CoreInstallation\Files
LMInstallFilesRootDir=P:\LicenseManager\Files
AFCoreMergeModuleInstallRoot=$(PDRootDir)\AttributeFinder\Installation\UCLID FlexIndex

InputFunnelBuildDir=$(ReusableComponentsRootDirectory)\InputFunnel\Build
PDCommonDir=$(PDRootDir)\Common

IDShieldInstallFilesRootDir=P:\AttributeFinder\IDShieldInstallation\Files

LabDEDir=$(PDRootDir)\LabDE
LabDEInstallRootDir=$(LabDEDir)\Installation
DataEntryInstallFiles=P:\DataEntry
DataEntryCoreInstallFilesDir=$(DataEntryInstallFiles)\CoreInstallation\Files

ExtractCommonInstallFilesRootDir=P:\ExtractCommon

ClearImageInstallFilesDir=P:\AttributeFinder\ClearImageFiles

InternalUseBuildFilesArchive=P:\AttributeFinder\Archive\InternalUseBuildFiles\InternalBuilds\$(FlexIndexVersion)


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
    @CD "$(PDUtilsRootDir)\UCLIDUtilApps\Code"
    @devenv Utils.sln /BUILD $(BuildConfig) /USEENV

BuildAttributeFinderCore:  
	@ECHO Building AFCore...
    @CD "$(AFRootDirectory)\AFCore\AFCoreTest\Code"
    @devenv AFCoreTest.sln /BUILD $(BuildConfig) /USEENV
	
ObfuscateFiles: BuildAttributeFinderCore
	@ECHO Obfuscating for FlexIndex...
#	Copy the strong naming key to location dotfuscator xml file expects
	@IF NOT EXIST "$(AFCoreInstallFilesRootDir)\StrongNameKey" @MKDIR "$(AFCoreInstallFilesRootDir)\StrongNameKey"
	@DeleteFiles "$(AFCoreInstallFilesRootDir)\StrongNameKey\*.*"
	@COPY /V "$(RCNETDir)\Core\Code\ExtractInternalKey.snk" "$(AFCoreInstallFilesRootDir)\StrongNameKey"
	@IF NOT EXIST "$(BinariesFolder)\Obfuscated" @MKDIR "$(BinariesFolder)\Obfuscated"
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(ReusableComponentsRootDirectory)\APIs\Nuance_16.3\bin;$(ReusableComponentsRootDirectory)\APIs\LeadTools_16.5\bin;$(ReusableComponentsRootDirectory)\APIs\RogueWave\bin;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(VCPP_DIR)\PlatformSDK\bin;$(VISUAL_STUDIO)\SDK\v2.0\bin;C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727;$(VCPP_DIR)\VCPackages;C:\Program Files\PreEmptive Solutions\Dotfuscator Professional Edition 4.7
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Utilities.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Drawing.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Utilities.Forms.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Utilities.Forms.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Drawing.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Utilities.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Licensing.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Licensing.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Imaging.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Imaging.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Interop.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Interop.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Redaction.Verification.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Redaction.Verification.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Redaction.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Redaction.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Utilities.Parsers.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Utilities.Parsers.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Database.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Database.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\SQLServerInfo.exe" /mapout:"$(BinariesFolder)\Map\SQLServerInfo.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\ReportViewer.exe" /mapout:"$(BinariesFolder)\Map\ReportViewer.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.AttributeFinder.dll" /mapout:"$(BinariesFolder)\Map\Extract.AttributeFinder.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Imaging.Forms.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Imaging.Forms.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.FileActionManager.FileProcessors.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.FileActionManager.FileProcessors.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Imaging.Utilities.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Imaging.Utilities.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\SplitMultiPageImage.exe" /mapout:"$(BinariesFolder)\Map\mapSplitMultiPageImage.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\ESFAMService.exe" /mapout:"$(BinariesFolder)\Map\mapESFAMService.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.FileActionManager.Forms.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.FileActionManager.Forms.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\IDShieldStatisticsReporter.exe" /mapout:"$(BinariesFolder)\Map\mapIDShieldStatisticsReporter.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\TestTextFunctionExpander.exe" /mapout:"$(BinariesFolder)\Map\mapTestTextFunctionExpander.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.DataEntry.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.DataEntry.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\DataEntryApplication.exe" /mapout:"$(BinariesFolder)\Map\mapDataEntryApplication.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Rules.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Rules.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Encryption.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Encryption.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\SQLCDBEditor.exe" /mapout:"$(BinariesFolder)\Map\mapSQLCDBEditor.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\RunFPSFile.exe" /mapout:"$(BinariesFolder)\Map\mapRunFPSFile.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\SQLCompactExporter.exe" /mapout:"$(BinariesFolder)\Map\mapSQLCompactExporter.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\SQLCompactImporter.exe" /mapout:"$(BinariesFolder)\Map\mapSQLCompactImporter.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\ExtractImageViewer.exe" /mapout:"$(BinariesFolder)\Map\mapExtractImageViewer.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml

EncryptAndCopyComponentDataFiles: 
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

CopyFilesToInstallFolder: BuildPDUtils CleanupPreviousBuildFolders ObfuscateFiles
    @ECHO Copying the AttributeFinderCore files to installation directory...
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
    @COPY /v  "$(BinariesFolder)\UCLIDAFFeedback.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @COPY /v  "$(BinariesFolder)\Feedback.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /v  "$(BinariesFolder)\ProcessFiles.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /v  "$(BinariesFolder)\RuleTester.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /v  "$(BinariesFolder)\RunRules.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v  "$(BinariesFolder)\FAMDBAdmin.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v  "$(BinariesFolder)\ConvertFPSFile.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
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
    @COPY /v  "$(AFRootDirectory)\Utils\FeedbackManager\Req-Design\Feedback.dsn" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles" /y
    @COPY /v  "$(AFRootDirectory)\Utils\FeedbackManager\Req-Design\Feedback.mdb" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles" /y
    @COPY /v  "$(BinariesFolder)\AFcppUtils.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /v  "$(AFRootDirectory)\Misc\UCLIDAFCore.ini" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /v  "$(AFRootDirectory)\Misc\RunRules_s.bat" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /v "$(BinariesFolder)\USSFileViewer.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /v "$(BinariesFolder)\VOAFileViewer.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v  "$(BinariesFolder)\GetFullUserName.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.Database.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v  "$(BinariesFolder)\Obfuscated\SQLServerInfo.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.Redaction.Verification.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.Redaction.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.AttributeFinder.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.FileActionManager.FileProcessors.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /v  "$(BinariesFolder)\Obfuscated\SplitMultiPageImage.exe" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /v  "$(BinariesFolder)\Obfuscated\ESFAMService.exe" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.FileActionManager.Forms.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V  "$(BinariesFolder)\Obfuscated\IDShieldStatisticsReporter.exe" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V  "$(BinariesFolder)\Extract.DataEntry.DEP.DemoFlexIndex.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V "$(BinariesFolder)\Interop.*.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY "$(RCNETDir)\APIs\Accusoft\PDFExpress\bin\PegasusImaging.WinForms.PdfXpress3.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC" 
	@COPY /v "$(BinariesFolder)\DataEntryCC.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /v "$(BinariesFolder)\Obfuscated\DataEntryApplication.exe" "$(AFCoreInstallFilesRootDir)\DotNetGAC" 
	@COPY /v "$(BinariesFolder)\Obfuscated\Extract.DataEntry.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC" 
	@COPY /v "$(BinariesFolder)\Obfuscated\SQLCDBEditor.exe" "$(AFCoreInstallFilesRootDir)\DotNetGAC" 
	@COPY /v "$(BinariesFolder)\Obfuscated\RunFPSFile.exe" "$(AFCoreInstallFilesRootDir)\DotNetGAC" 
	@COPY /v "$(BinariesFolder)\Obfuscated\ExtractImageViewer.exe" "$(AFCoreInstallFilesRootDir)\DotNetGAC" 
	@COPY /v "$(BinariesFolder)\Obfuscated\SQLCompactExporter.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles" 
	@COPY /v "$(BinariesFolder)\Obfuscated\SQLCompactImporter.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles" 
	
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
	@COPY /v "$(ReusableComponentsRootDirectory)\Scripts\BatchFiles\KillAllOCRInstances.bat" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	@XCOPY "$(ReusableComponentsRootDirectory)\APIs\Inlite_5_7\bin\*.*" "$(ClearImageInstallFilesDir)\" /v /s /e /y
	@XCOPY "$(ReusableComponentsRootDirectory)\COMComponents\UCLIDFileProcessing\Reports\*.*" "$(AFCoreInstallFilesRootDir)\Reports" /Y/E

    @COPY /V "$(BinariesFolder)\UCLIDDistanceConverter.dll" "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
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
    @COPY /V "$(PDUtilsRootDir)\DetectAndReportFailure\Misc\DetectAndReportFailure.ini" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
	
# Copy pdb and map files to archive
	@COPY  "$(BinariesFolder)\*.pdb" "$(InternalUseBuildFilesArchive)" 
	@COPY  "$(BinariesFolder)\Obfuscated\*.pdb" "$(InternalUseBuildFilesArchive)" 
	@COPY  "$(BinariesFolder)\Map\*.xml" "$(InternalUseBuildFilesArchive)" 
	
# Create .rl files for registration
	@DIR "$(AFCoreInstallFilesRootDir)\SelfRegFiles\*.*" /b >"$(AFCoreInstallFilesRootDir)\NonSelfRegFiles\AFCommon.rl"
	@DIR "$(IDShieldInstallFilesRootDir)\SelfRegIDShieldComponents\*.*" /b >"$(IDShieldInstallFilesRootDir)\NonSelfRegFiles\IDShield.rl"
	@DIR "$(ClearImageInstallFilesDir)\*Image.dll" /b >"$(AFCoreInstallFilesRootDir)\NonSelfRegFiles\ClearImage.rl"
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\DataEntryCC.dll" /b >"$(DataEntryCoreInstallFilesDir)\NonSelfRegFiles\DataEntry.rl"
	
# Add .net com objects to the .nl file
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.Utilities.Parsers.dll" /b >>"$(AFCoreInstallFilesRootDir)\NonSelfRegFiles\AFCore.nl
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.Imaging.dll" /b >>"$(AFCoreInstallFilesRootDir)\NonSelfRegFiles\AFCore.nl
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.Redaction.Verification.dll" /b >>"$(IDShieldInstallFilesRootDir)\NonSelfRegFiles\IDShield.nl"
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.FileActionManager.FileProcessors.dll" /b >>"$(AFCoreInstallFilesRootDir)\NonSelfRegFiles\AFCore.nl
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.Redaction.dll" /b >>"$(IDShieldInstallFilesRootDir)\NonSelfRegFiles\IDShield.nl"
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\IDShieldStatisticsReporter.exe" /b >>"$(IDShieldInstallFilesRootDir)\NonSelfRegFiles\IDShield.nl"
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.DataEntry.dll" /b >"$(DataEntryCoreInstallFilesDir)\NonSelfRegFiles\DataEntry.nl"
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\DataEntryApplication.exe" /b >>"$(DataEntryCoreInstallFilesDir)\NonSelfRegFiles\DataEntry.nl"
		
#    @COPY /V "$(BinariesFolder)\sit_grammar.xml" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @COPY /V "$(BinariesFolder)\ImageViewer.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @DeleteFiles "$(AFCoreInstallFilesRootDir)\vssver.scc"
    @DeleteFiles "$(AFCoreInstallFilesRootDir)\mssccprj.scc"
	@DeleteFiles "$(ClearImageInstallFilesDir)\vssver.scc"
	
CleanupPreviousBuildFolders:
	@ECHO Removing files from previous builds...
	@IF NOT EXIST "$(AFCoreInstallFilesRootDir)\SelfRegFiles" @MKDIR "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @IF NOT EXIST "$(AFCoreInstallFilesRootDir)\SelfRegFiles" @MKDIR "$(AFCoreInstallFilesRootDir)\SelfRegFiles"
    @IF NOT EXIST "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles" @MKDIR "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles"
    @IF NOT EXIST "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles" @MKDIR "$(AFCoreInstallFilesRootDir)\NonSelfRegFiles\pdf"
	@IF NOT EXIST "$(AFCoreInstallFilesRootDir)\DotNetGAC" @MKDIR "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@IF NOT EXIST "$(AFCoreInstallFilesRootDir)\Reports" @MKDIR "$(AFCoreInstallFilesRootDir)\Reports"
	@IF NOT EXIST "$(InternalUseBuildFilesArchive)" @MKDIR "$(InternalUseBuildFilesArchive)"
	@IF NOT EXIST "$(IDShieldInstallFilesRootDir)\NonSelfRegFiles" @MKDIR "$(IDShieldInstallFilesRootDir)\NonSelfRegFiles"
	@IF NOT EXIST "$(DataEntryCoreInstallFilesDir)\NonSelfRegFiles" @MKDIR "$(DataEntryCoreInstallFilesDir)\NonSelfRegFiles"
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
	
CleanUpMergeModulesFromPreviousBuilds:
	@ECHO Deleting old merge modules....
	@DeleteFiles "$(MergeModuleDir)\DataEntry.msm"
	@DeleteFiles "$(MergeModuleDir)\UCLIDFlexIndex.msm"
	@DeleteFiles "$(MergeModuleDir)\UCLIDInputFunnel.msm"

MakeExtractCommonMergeModule: BuildAttributeFinderCore
	@ECHO Making ExtractCommonMM...
	@CD "$(PDCommonDir)\"
    @nmake /F ExtractCommon.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" ProductVersion="$(ProductVersion)" CreateExtractCommonMergeModule

MakeExtractFlexCommonMergeModule: MakeExtractCommonMergeModule
	@ECHO Making ExtractFlexCommonMM...
    @CD "$(AFRootDirectory)\Build
    @nmake /F ExtractFlexCommon.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" ProductVersion="$(ProductVersion)" CreateExtractFlexCommonMergeModule
	
BuildAFCoreMergeModule: CreateVersionISImportFile CopyFilesToInstallFolder EncryptAndCopyComponentDataFiles MakeExtractCommonMergeModule 
    @ECHO Buliding the UCLIDFlexIndex Merge Module installation...
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(ReusableComponentsRootDirectory)\APIs\Nuance_16.3\bin;$(ReusableComponentsRootDirectory)\APIs\LeadTools_16.5\bin;$(ReusableComponentsRootDirectory)\APIs\RogueWave\bin;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(VCPP_DIR)\PlatformSDK\bin;$(VISUAL_STUDIO)\SDK\v2.0\bin;C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727;$(VCPP_DIR)\VCPackages
	$(SetProductVerScript) "$(AFCoreMergeModuleInstallRoot)\UCLID FlexIndex.ism" "$(FlexIndexVersion)"
    @"$(DEV_STUDIO_DIR)\System\IsCmdBld.exe" -p "$(AFCoreMergeModuleInstallRoot)\UCLID FlexIndex.ism"

BuildDataEntryMergeModule: CreateVersionISImportFile BuildAFCoreMergeModule
    @ECHO Building Extract Systems DataEntry Merge Module...
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(ReusableComponentsRootDirectory)\APIs\Nuance_16.3\bin;$(ReusableComponentsRootDirectory)\APIs\LeadTools_16.5\Bin;$(ReusableComponentsRootDirectory)\APIs\RogueWave\bin;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(VCPP_DIR)\PlatformSDK\bin;$(VISUAL_STUDIO)\SDK\v2.0\bin;C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727;$(VCPP_DIR)\VCPackages;$(ReusableComponentsRootDirectory)\APIs\LeadTools_16.5\Dotnet
	$(SetProductVerScript) "$(LabDEInstallRootDir)\DataEntry\DataEntry.ism" "$(LabDEVersion)"
    @"$(DEV_STUDIO_DIR)\System\IsCmdBld.exe" -p "$(LabDEInstallRootDir)\DataEntry\DataEntry.ism"

GetAllFiles: GetPDCommonFiles GetReusableComponentFiles GetRCdotNETFiles GetAttributeFinderFiles GetPDUtilsFiles GetComponentDataFiles GetDataEntryInstall

DoEverythingNoGet: SetupBuildEnv CleanUpMergeModulesFromPreviousBuilds BuildAFCoreMergeModule BuildDataEntryMergeModule
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
