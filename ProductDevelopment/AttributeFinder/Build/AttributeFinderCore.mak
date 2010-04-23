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
USBLicenseKeyRootDir=P:\AttributeFinder\USBLicenseKey\Files 
AFCoreMergeModuleInstallRoot=$(PDRootDir)\AttributeFinder\Installation\UCLID FlexIndex

InputFunnelBuildDir=$(ReusableComponentsRootDirectory)\InputFunnel\Build
PDCommonDir=$(PDRootDir)\Common

IDShieldInstallFilesRootDir=P:\AttributeFinder\IDShieldInstallation\Files

LabDEDir=$(PDRootDir)\LabDE
LabDEInstallRootDir=$(LabDEDir)\Installation
DataEntryInstallFiles=P:\DataEntry
DataEntryCoreInstallFilesDir=$(DataEntryInstallFiles)\CoreInstallation\Files

ClearImageInstallFilesDir=P:\AttributeFinder\ClearImageFiles

InternalUseBuildFilesArchive=P:\AttributeFinder\Archive\InternalUseBuildFiles\InternalBuilds\$(FlexIndexVersion)

MergeModuleDir=C:\InstallShield 2010 Projects\MergeModules

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

BuildInputFunnelCore: 
	@ECHO Building InputFunnel...
	$(BUILD_DRIVE) 
    @CD "$(InputFunnelBuildDir)"
    @nmake /F InputFunnel.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" ProductVersion="$(ProductVersion)" DoEverythingNoGet

BuildAttributeFinderCore: BuildInputFunnelCore 
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
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(ReusableComponentsRootDirectory)\APIs\Nuance_16.3\bin;$(ReusableComponentsRootDirectory)\APIs\LeadTools_16.5\bin;$(ReusableComponentsRootDirectory)\APIs\RogueWave\bin;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(VCPP_DIR)\PlatformSDK\bin;$(VISUAL_STUDIO)\SDK\v2.0\bin;C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727;$(VCPP_DIR)\VCPackages;C:\Program Files\PreEmptive Solutions\Dotfuscator Professional Edition 4.6
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

CopyFilesToInstallFolder: ObfuscateFiles
    @ECHO Copying the AttributeFinderCore files to installation directory...
	@IF NOT EXIST "$(AFCoreInstallFilesRootDir)\DotNetGAC" @MKDIR "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@IF NOT EXIST "$(AFCoreInstallFilesRootDir)\Reports" @MKDIR "$(AFCoreInstallFilesRootDir)\Reports"
	@IF NOT EXIST "$(InternalUseBuildFilesArchive)" @MKDIR "$(InternalUseBuildFilesArchive)"
    @DeleteFiles "$(IDShieldInstallFilesRootDir)\NonSelfRegCommonComponents\*.*"
	@DeleteFiles "$(IDShieldInstallFilesRootDir)\Reports\*.*"
	@DeleteFiles "$(IDShieldInstallFilesRootDir)\ExemptionCodes\*.*"
	@DeleteFiles "$(IDShieldInstallFilesRootDir)\SelfRegIDShieldComponents\*.*"
	@DeleteFiles "$(IDShieldInstallFilesRootDir)\NonSelfRegComponents\*.*"
    @DeleteFiles "$(AFCoreInstallFilesRootDir)\NonSelfRegCoreComponents\*.*"
	@DeleteFiles "$(AFCoreInstallFilesRootDir)\SelfRegCoreComponents\*.*"
	@DeleteFiles "$(AFCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
	@DeleteFiles "$(AFCoreInstallFilesRootDir)\SelfRegCommonComponents\*.*"
	@DeleteFiles "$(USBLicenseKeyRootDir)\*.*"
	@DeleteFiles "$(ClearImageInstallFilesDir)\*.*"
	@DeleteFiles "$(AFCoreInstallFilesRootDir)\DotNetGAC\*.*"
	@DeleteFiles "$(AFCoreInstallFilesRootDir)\Reports\*.*"
	@DeleteFiles "$(DataEntryCoreInstallFilesDir)\NonSelfRegisteredFiles\*.*"
	
	@COPY /v  "$(BinariesFolder)\UCLIDAFConditions.dll" "$(AFCoreInstallFilesRootDir)\SelfRegCoreComponents"
    @COPY /v  "$(BinariesFolder)\UCLIDAFCore.dll" "$(AFCoreInstallFilesRootDir)\SelfRegCoreComponents"
    @COPY /v  "$(BinariesFolder)\UCLIDAFDataScorers.dll" "$(AFCoreInstallFilesRootDir)\SelfRegCoreComponents"
    @COPY /v  "$(BinariesFolder)\UCLIDAFFileProcessors.dll" "$(AFCoreInstallFilesRootDir)\SelfRegCoreComponents"
    @COPY /v  "$(BinariesFolder)\UCLIDAFUtils.dll" "$(AFCoreInstallFilesRootDir)\SelfRegCoreComponents"
    @COPY /v  "$(BinariesFolder)\UCLIDAFValueFinders.dll" "$(AFCoreInstallFilesRootDir)\SelfRegCoreComponents"
    @COPY /v  "$(BinariesFolder)\UCLIDAFValueModifiers.dll" "$(AFCoreInstallFilesRootDir)\SelfRegCoreComponents"
    @COPY /v  "$(BinariesFolder)\UCLIDAFOutputHandlers.dll" "$(AFCoreInstallFilesRootDir)\SelfRegCoreComponents"
    @COPY /v  "$(BinariesFolder)\UCLIDAFSplitters.dll" "$(AFCoreInstallFilesRootDir)\SelfRegCoreComponents"
    @COPY /v  "$(BinariesFolder)\UCLIDAFPreProcessors.dll" "$(AFCoreInstallFilesRootDir)\SelfRegCoreComponents"
	@COPY /v  "$(BinariesFolder)\ESAFSelectors.dll" "$(AFCoreInstallFilesRootDir)\SelfRegCoreComponents"
    @COPY /v  "$(BinariesFolder)\CountyCustomComponents.dll" "$(AFCoreInstallFilesRootDir)\SelfRegCoreComponents"
    @COPY /v  "$(BinariesFolder)\UCLIDAFFeedback.dll" "$(AFCoreInstallFilesRootDir)\SelfRegCoreComponents"
    @COPY /v  "$(BinariesFolder)\Feedback.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegCoreComponents"
    @COPY /v  "$(BinariesFolder)\ProcessFiles.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
    @COPY /v  "$(BinariesFolder)\RuleTester.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegCoreComponents"
    @COPY /v  "$(BinariesFolder)\RunRules.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegCoreComponents"
	@COPY /v  "$(BinariesFolder)\FAMDBAdmin.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
	@COPY /v  "$(BinariesFolder)\ConvertFPSFile.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
    @COPY /v  "$(BinariesFolder)\FileProcessors.dll" "$(AFCoreInstallFilesRootDir)\SelfRegCommonComponents"
    @COPY /v  "$(BinariesFolder)\UCLIDFileProcessing.dll" "$(AFCoreInstallFilesRootDir)\SelfRegCommonComponents"
	@COPY /v  "$(BinariesFolder)\ESFileSuppliers.dll" "$(AFCoreInstallFilesRootDir)\SelfRegCommonComponents" 
	@COPY /v  "$(BinariesFolder)\ESFAMConditions.dll" "$(AFCoreInstallFilesRootDir)\SelfRegCommonComponents" 
	@COPY /v  "$(BinariesFolder)\ESImageCleanup.dll" "$(AFCoreInstallFilesRootDir)\SelfRegCommonComponents" 
    @COPY /v  "$(BinariesFolder)\RedactionCC.dll" "$(IDShieldInstallFilesRootDir)\SelfRegIDShieldComponents"
	@COPY /V  "$(BinariesFolder)\UCLIDTestingFrameworkCore.dll" "$(IDShieldInstallFilesRootDir)\SelfRegIDShieldComponents"
	@COPY /V  "$(BinariesFolder)\RedactionTester.dll" "$(IDShieldInstallFilesRootDir)\SelfRegIDShieldComponents"
    @COPY /v  "$(AFRootDirectory)\IndustrySpecific\Redaction\RedactionCustomComponents\Misc\IDShield.ini" "$(IDShieldInstallFilesRootDir)\NonSelfRegComponents" /y
    @COPY /v  "$(BinariesFolder)\RedactFromXML.exe" "$(IDShieldInstallFilesRootDir)\NonSelfRegComponents"
    @XCOPY   "$(AFRootDirectory)\IndustrySpecific\Redaction\Reports\*.*" "$(IDShieldInstallFilesRootDir)\Reports" /Y/E
	@XCOPY   "$(AFRootDirectory)\IndustrySpecific\Redaction\RedactionCustomComponents\ExemptionCodes\*.xml" "$(IDShieldInstallFilesRootDir)\ExemptionCodes" /Y/E
    @COPY /v  "$(AFRootDirectory)\Utils\FeedbackManager\Req-Design\Feedback.dsn" "$(AFCoreInstallFilesRootDir)\NonSelfRegCoreComponents" /y
    @COPY /v  "$(AFRootDirectory)\Utils\FeedbackManager\Req-Design\Feedback.mdb" "$(AFCoreInstallFilesRootDir)\NonSelfRegCoreComponents" /y
    @COPY /v  "$(BinariesFolder)\AFcppUtils.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegCoreComponents"
    @COPY /v  "$(AFRootDirectory)\Misc\UCLIDAFCore.ini" "$(AFCoreInstallFilesRootDir)\NonSelfRegCoreComponents"
    @COPY /v  "$(AFRootDirectory)\Misc\RunRules_s.bat" "$(AFCoreInstallFilesRootDir)\NonSelfRegCoreComponents"
    @COPY /v "$(BinariesFolder)\USSFileViewer.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegCoreComponents"
    @COPY /v "$(BinariesFolder)\VOAFileViewer.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegCoreComponents"
	@COPY /v  "$(BinariesFolder)\GetFullUserName.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
	@COPY /v  "$(BinariesFolder)\Obfuscated\CSharpDatabaseUtilities.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
	@COPY /v  "$(BinariesFolder)\Obfuscated\SQLServerInfo.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.Utilities.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.Drawing.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.Utilities.Forms.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.Licensing.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.Imaging.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.Imaging.Forms.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.Interop.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.Redaction.Verification.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.Redaction.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.Rules.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.Encryption.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.Utilities.Parsers.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.AttributeFinder.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.FileActionManager.FileProcessors.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.Imaging.Utilities.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /v  "$(BinariesFolder)\Obfuscated\SplitMultiPageImage.exe" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /v  "$(BinariesFolder)\Obfuscated\ESFAMService.exe" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /v  "$(BinariesFolder)\Obfuscated\Extract.FileActionManager.Forms.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V  "$(BinariesFolder)\Obfuscated\IDShieldStatisticsReporter.exe" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V  "$(BinariesFolder)\Extract.DataEntry.DEP.DemoFlexIndex.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V "$(BinariesFolder)\Interop.*.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /V "$(ReusableComponentsRootDirectory)\APIs\LeadTools_16.5\Dotnet\Leadtools*.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY "$(RCNETDir)\APIs\Accusoft\PDFExpress\bin\PegasusImaging.WinForms.PdfXpress3.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC" 
	@COPY "$(RCNETDir)\APIs\Divelements\SandDock\bin\SandDock.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC" 
	@COPY /v "$(BinariesFolder)\DataEntryCC.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC"
	@COPY /v "$(BinariesFolder)\Obfuscated\DataEntryApplication.exe" "$(AFCoreInstallFilesRootDir)\DotNetGAC" 
	@COPY /v "$(BinariesFolder)\Obfuscated\Extract.DataEntry.dll" "$(AFCoreInstallFilesRootDir)\DotNetGAC" 
	@COPY /v "$(BinariesFolder)\Obfuscated\SQLCDBEditor.exe" "$(AFCoreInstallFilesRootDir)\DotNetGAC" 
	@COPY /v "$(BinariesFolder)\Obfuscated\RunFPSFile.exe" "$(AFCoreInstallFilesRootDir)\DotNetGAC" 
	@COPY /v "$(BinariesFolder)\Obfuscated\SQLCompactExporter.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegCommonComponents" 
	@COPY /v "$(BinariesFolder)\Obfuscated\SQLCompactImporter.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegCommonComponents" 
	
# Need the .net DLLs  in the same folder as Extract.Utilities.Parsers.dll
	@COPY /V  "$(BinariesFolder)\Obfuscated\TestTextFunctionExpander.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
	@COPY /v  "$(BinariesFolder)\FAMUtils.dll" "$(AFCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
	@COPY /v  "$(BinariesFolder)\ConvertFAMDB.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
    @COPY /v  "$(BinariesFolder)\SafeNetUtils.dll" "$(USBLicenseKeyRootDir)"
    @COPY /v  "$(BinariesFolder)\USBLicenseKeyManager.exe" "$(USBLicenseKeyRootDir)"
    @COPY /v  "$(BinariesFolder)\EmailFile.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
	@COPY /v  "$(BinariesFolder)\ESOCR.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
    @COPY /v "$(BinariesFolder)\SplitFile.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
	@COPY /v "$(BinariesFolder)\AdjustImageResolution.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
	@COPY /v "$(BinariesFolder)\CreateMultiPageImage.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
	@COPY /v "$(BinariesFolder)\ImageFormatConverter.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
	@COPY /v "$(BinariesFolder)\ESConvertToPDF.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
	@COPY /v "$(BinariesFolder)\Sleep.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
	@COPY /V "$(BinariesFolder)\LogProcessStats.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
	@COPY /v "$(BinariesFolder)\CleanupImage.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
	@COPY /v "$(BinariesFolder)\FAMProcess.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
	@COPY /v "$(BinariesFolder)\Obfuscated\ReportViewer.exe" "$(AFCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
	@COPY /v "$(ReusableComponentsRootDirectory)\Scripts\BatchFiles\KillAllOCRInstances.bat" "$(AFCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
	@XCOPY "$(ReusableComponentsRootDirectory)\APIs\Inlite_5_7\bin\*.*" "$(ClearImageInstallFilesDir)\" /v /s /e /y
	@XCOPY "$(ReusableComponentsRootDirectory)\COMComponents\UCLIDFileProcessing\Reports\*.*" "$(AFCoreInstallFilesRootDir)\Reports" /Y/E
	
# Copy pdb and map files to archive
	@COPY  "$(BinariesFolder)\*.pdb" "$(InternalUseBuildFilesArchive)" 
	@COPY  "$(BinariesFolder)\Obfuscated\*.pdb" "$(InternalUseBuildFilesArchive)" 
	@COPY  "$(BinariesFolder)\Map\*.xml" "$(InternalUseBuildFilesArchive)" 
	
# Create RegList.dat file for registration
	@DIR "$(AFCoreInstallFilesRootDir)\SelfRegCoreComponents\*.*" /b >"$(AFCoreInstallFilesRootDir)\NonSelfRegCoreComponents\AFCore.rl"
	@DIR "$(AFCoreInstallFilesRootDir)\SelfRegCommonComponents\*.*" /b >"$(AFCoreInstallFilesRootDir)\NonSelfRegCommonComponents\AFCommon.rl"
	@Dir "$(IDShieldInstallFilesRootDir)\SelfRegIDShieldComponents\*.*" /b >"$(IDShieldInstallFilesRootDir)\NonSelfRegComponents\IDShield.rl"
	@DIR "$(ClearImageInstallFilesDir)\*Image.dll" /b >"$(AFCoreInstallFilesRootDir)\ClearImage.rl"
	DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\DataEntryCC.dll" /b >"$(DataEntryCoreInstallFilesDir)\NonSelfRegisteredFiles\DataEntry.rl"
# Add .net com objects to the .nl file
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.Utilities.Parsers.dll" /b >>"$(AFCoreInstallFilesRootDir)\NonSelfRegCommonComponents\AFCore.nl
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.Imaging.dll" /b >>"$(AFCoreInstallFilesRootDir)\NonSelfRegCommonComponents\AFCore.nl
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.Redaction.Verification.dll" /b >>"$(IDShieldInstallFilesRootDir)\NonSelfRegComponents\IDShield.nl"
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.FileActionManager.FileProcessors.dll" /b >>"$(AFCoreInstallFilesRootDir)\NonSelfRegCommonComponents\AFCore.nl
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.Redaction.dll" /b >>"$(IDShieldInstallFilesRootDir)\NonSelfRegComponents\IDShield.nl"
	@DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\IDShieldStatisticsReporter.exe" /b >>"$(IDShieldInstallFilesRootDir)\NonSelfRegComponents\IDShield.nl"
	DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\Extract.DataEntry.dll" /b >>"$(DataEntryCoreInstallFilesDir)\NonSelfRegisteredFiles\DataEntry.nl"
	DIR "$(AFCoreInstallFilesRootDir)\DotNetGAC\DataEntryApplication.exe" /b >>"$(DataEntryCoreInstallFilesDir)\NonSelfRegisteredFiles\DataEntry.nl"
    @DeleteFiles "$(AFCoreInstallFilesRootDir)\NonSelfRegCommonComponents\vssver.scc" 
    @DeleteFiles "$(AFCoreInstallFilesRootDir)\vssver.scc"
    @DeleteFiles "$(AFCoreInstallFilesRootDir)\mssccprj.scc"
	@DeleteFiles "$(ClearImageInstallFilesDir)\vssver.scc"

CleanUpMergeModulesFromPreviousBuilds:
	@ECHO Deleting old merge modules....
	@DeleteFiles "$(MergeModuleDir)\DataEntry.msm"
	@DeleteFiles "$(MergeModuleDir)\UCLIDFlexIndex.msm"
	@DeleteFiles "$(MergeModuleDir)\UCLIDInputFunnel.msm"
	
BuildAFCoreMergeModule: CreateVersionISImportFile CopyFilesToInstallFolder EncryptAndCopyComponentDataFiles
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
