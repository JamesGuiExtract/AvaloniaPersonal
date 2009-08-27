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
!ERROR Build variable 'BuildConfig' must be defined (e.g. "Win32 Release")
!ENDIF

#############################################################################
# M A K E F I L E   V A R I A B L E S
#
PDRootDir=$(EngineeringRootDirectory)\ProductDevelopment
AFRootDirectory=$(PDRootDir)\AttributeFinder
IDSInstallProjectDirectory=$(AFRootDirectory)\IndustrySpecific\Redaction\Installation\IDShieldCustomerRDT
TestingFilesDirectory="P:\AttributeFinder\RDTInstallation\Files\TestFiles"

RDTInstallFilesRootDir=P:\AttributeFinder\RDTInstallation\Files
RDTInstallProjectRootDir=$(EngineeringRootDirectory)\ProductDevelopment\AttributeFinder\Installation\RuleDevelopmentKit
RDTInstallScriptFile=$(RDTInstallProjectRootDir)\Script Files\setup.rul
RDTInstallMediaDir=$(RDTInstallProjectRootDir)\Media\CD-ROM\DiskImages\Disk1

IDSInstallProjectDirectory=$(AFRootDirectory)\IndustrySpecific\Redaction\Installation\IDShieldCustomerRDT
IDSRDTInstallMediaDir=$(IDSInstallProjectDirectory)\Media\CD-ROM\DiskImages\Disk1

RDTReleaseBleedingEdgeDir=S:\FlexIndex\Internal\BleedingEdge\$(FlexIndexVersion)\RDT
IDSRDTReleaseBleedingEdgeDir=R:\FlexIndex\Internal\BleedingEdge\$(FlexIndexVersion)\RDT_IDShieldCustomer

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

CopyFilesToInstallFolder: 
    @ECHO Copying the TCL files to installation directory...
	@DeleteFiles "$(RDTInstallFilesRootDir)\SelfRegRDTComponents\*.*" /S
	@DeleteFiles "$(RDTInstallFilesRootDir)\NonSelfRegRDTComponents\*.*" /S
	@DeleteFiles "$(RDTInstallFilesRootDir)\SelfRegCommonComponents\*.*" /S
	@DeleteFiles "$(RDTInstallFilesRootDir)\NonSelfRegCommonComponents\*.*" /S
	
    @ECHO Copying the Binary files to installation directory...
    @COPY /V  "$(BinariesFolder)\RuleSetEditor.exe" "$(RDTInstallFilesRootDir)\NonSelfRegRDTComponents"
    @COPY /V  "$(BinariesFolder)\EncryptFile.exe" "$(RDTInstallFilesRootDir)\NonSelfRegRDTComponents"
    @COPY /V  "$(BinariesFolder)\TestHarness.exe" "$(RDTInstallFilesRootDir)\NonSelfRegRDTComponents"
    @COPY /V  "$(BinariesFolder)\DocumentSorterConsoleApp.exe" "$(RDTInstallFilesRootDir)\NonSelfRegRDTComponents"
    @COPY /V  "$(BinariesFolder)\IndexConverter.exe" "$(RDTInstallFilesRootDir)\NonSelfRegRDTComponents"
    @COPY /V  "$(BinariesFolder)\RDTConfig.exe" "$(RDTInstallFilesRootDir)\NonSelfRegRDTComponents"
	@COPY /V  "$(BinariesFolder)\FindString.exe" "$(RDTInstallFilesRootDir)\NonSelfRegRDTComponents"
	@COPY /V  "$(BinariesFolder)\ImageCleanupSettingsEditor.exe" "$(RDTInstallFilesRootDir)\NonSelfRegRDTComponents"
	@COPY /V  "$(BinariesFolder)\ModifyVOA.exe" "$(RDTInstallFilesRootDir)\NonSelfRegRDTComponents"
	@COPY /V  "$(BinariesFolder)\ESConvertUSSToTXT.exe" "$(RDTInstallFilesRootDir)\NonSelfRegRDTComponents"
	@COPY /V  "$(BinariesFolder)\AFCoreTest.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
    @COPY /V  "$(BinariesFolder)\AFSplittersTest.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
    @COPY /V  "$(BinariesFolder)\AFUtilsTest.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
    @COPY /V  "$(BinariesFolder)\CountyTester.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
    @COPY /V  "$(BinariesFolder)\UCLIDTestingFrameworkCore.dll" "$(RDTInstallFilesRootDir)\SelfRegCommonComponents"
    @COPY /V  "$(BinariesFolder)\AFDataScorersTest.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
	@COPY /V  "$(BinariesFolder)\AFValueFinderTest.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
	@COPY /V  "$(BinariesFolder)\HighlightedTextIRAutoTest.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
	@COPY /V  "$(BinariesFolder)\SafeNetUtilsTest.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
	@COPY /V  "$(BinariesFolder)\SpatialStringAutomatedTest.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
	@COPY /V  "$(BinariesFolder)\SpatialStringSearcherTest.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
	@COPY /V  "$(BinariesFolder)\SpotRecIRAutoTest.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
	@COPY /V  "$(BinariesFolder)\StringPatternMatcherAutoTest.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
    @COPY /V  "$(BinariesFolder)\RedactionTester.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
    @COPY /V  "$(BinariesFolder)\RedactionCC.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
	@COPY /V  "$(BinariesFolder)\RasterZoneTester.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
	@COPY /V  "$(BinariesFolder)\BaseUtilsTest.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
 	@COPY /V  "$(BinariesFolder)\OcrSingleDocument.exe" "$(RDTInstallFilesRootDir)\NonSelfRegCommonComponents"
 	@COPY /V  "$(BinariesFolder)\SetOp.exe" "$(RDTInstallFilesRootDir)\NonSelfRegCommonComponents"
	
	@DIR "$(RDTInstallFilesRootDir)\SelfRegRDTComponents\*.*" /b >"$(RDTInstallFilesRootDir)\NonSelfRegRDTComponents\RDT.rl"
	@DIR "$(RDTInstallFilesRootDir)\SelfRegCommonComponents\*.*" /b >"$(RDTInstallFilesRootDir)\NonSelfRegCommonComponents\RDTCommon.rl"
  
    @DeleteFiles "$(RDTInstallFilesRootDir)\vssver.scc"
    @DeleteFiles "$(RDTInstallFilesRootDir)\mssccprj.scc"

BuildRDTInstall: DisplayTimeStamp CopyFilesToInstallFolder CopyTestFiles
    @ECHO Buliding the RDT installation...
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(ReusableComponentsRootDirectory)\APIs\Nuance_16\bin;$(ReusableComponentsRootDirectory)\APIs\LeadTools_16\Bin;$(ReusableComponentsRootDirectory)\APIs\RogueWave\bin;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(VCPP_DIR)\PlatformSDK\bin;$(VISUAL_STUDIO)\SDK\v2.0\bin;C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727;$(VCPP_DIR)\VCPackages
	$(SetProductVerScript) "$(RDTInstallProjectRootDir)\RuleDevelopmentKit.ism" "$(FlexIndexVersion)"
    @"$(DEV_STUDIO_DIR)\System\IsCmdBld.exe" -p "$(RDTInstallProjectRootDir)\RuleDevelopmentKit.ism"

CreateRDTInstallCD: BuildRDTInstall
    @IF NOT EXIST "$(RDTReleaseBleedingEdgeDir)" MKDIR "$(RDTReleaseBleedingEdgeDir)"
    @XCOPY "$(RDTInstallMediaDir)\*.*" "$(RDTReleaseBleedingEdgeDir)" /v /s /e /y
    $(VerifyDir) "$(RDTInstallMediaDir)" "$(RDTReleaseBleedingEdgeDir)"
    @DeleteFiles "$(RDTReleaseBleedingEdgeDir)\vssver.scc"

BuildIDSRDTInstall: CreateRDTInstallCD
    @ECHO Buliding the ID Shield RDT installation...
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(ReusableComponentsRootDirectory)\APIs\Nuance_16\bin;$(ReusableComponentsRootDirectory)\APIs\LeadTools_16\Bin;$(ReusableComponentsRootDirectory)\APIs\RogueWave\bin;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(VCPP_DIR)\PlatformSDK\bin;$(VISUAL_STUDIO)\SDK\v2.0\bin;C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727;$(VCPP_DIR)\VCPackages
	$(SetProductVerScript) "$(IDSInstallProjectDirectory)\IDShieldCustomerRDT.ism" "$(FlexIndexVersion)"
    @"$(DEV_STUDIO_DIR)\System\IsCmdBld.exe" -p "$(IDSInstallProjectDirectory)\IDShieldCustomerRDT.ism"

CreateIDSRDTInstallCD: BuildIDSRDTInstall
    @IF NOT EXIST "$(IDSRDTReleaseBleedingEdgeDir)" MKDIR "$(IDSRDTReleaseBleedingEdgeDir)"
    @XCOPY "$(IDSRDTInstallMediaDir)\*.*" "$(IDSRDTReleaseBleedingEdgeDir)" /v /s /e /y
    $(VerifyDir) "$(IDSRDTInstallMediaDir)" "$(IDSRDTReleaseBleedingEdgeDir)"
    @DeleteFiles "$(IDSRDTReleaseBleedingEdgeDir)\vssver.scc"

CopyTestFiles:
	@ECHO Copying Automated Test Files
    @DeleteFiles /S /Q "$(TestingFilesDirectory)\*.*"
	@XCOPY "$(AFRootDirectory)\AFConditions\AutomatedTest\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\AFConditions\AutomatedTest" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\AFCore\AutomatedTest\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\AFCore\AutomatedTest\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\AFDataScorers\AFDataScorersTest\TestFiles\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\AFDataScorers\AFDataScorersTest\TestFiles\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\AFOutputHandlers\AutomatedTest\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\AFOutputHandlers\AutomatedTest\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\AFPreProcessors\AutomatedTest\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\AFPreProcessors\AutomatedTest\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\AFSelectors\AutomatedTest\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\AFSelectors\AutomatedTest\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\AFSplitters\AutomatedTest\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\AFSplitters\AutomatedTest\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\AFSplitters\AFSplittersTest\TestFiles\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\AFSplitters\AFSplittersTest\TestFiles\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\AFUtils\AutomatedTest\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\AFUtils\AutomatedTest\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\AFUtils\AFUtilsTest\TestFiles\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\AFUtils\AFUtilsTest\TestFiles\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\AFValueFinders\AutomatedTest\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\AFValueFinders\AutomatedTest\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\AFValueFinders\Test\AFValueFinderTest\TestFiles\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\AFValueFinders\Test\AFValueFinderTest\TestFiles\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\AFValueModifiers\AutomatedTest\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\AFValueModifiers\AutomatedTest\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\InputValidators\AutomatedTest\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\InputValidators\AutomatedTest\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\IndustrySpecific\Redaction\NationalRuleSet\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\IndustrySpecific\Redaction\NationalRuleSet\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\IndustrySpecific\Redaction\AutomatedTest\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\IndustrySpecific\Redaction\AutomatedTest\" /s /e /y /I
	@XCOPY "$(ReusableComponentsRootDirectory)\InputFunnel\InputReceivers\HighlightedTextIR\Test\HighlightedTextIRAutoTest\TestFiles\AutomatedTest\*.*" "$(TestingFilesDirectory)\ReusableComponents\InputFunnel\InputReceivers\HighlightedTextIR\Test\HighlightedTextIRAutoTest\TestFiles\AutomatedTest\" /s /e /y /I
	@XCOPY "$(ReusableComponentsRootDirectory)\InputFunnel\InputReceivers\SpotRecognitionIR\Test\SpotRecIRAutoTest\TestFiles\AutomatedTest\*.*" "$(TestingFilesDirectory)\ReusableComponents\InputFunnel\InputReceivers\SpotRecognitionIR\Test\SpotRecIRAutoTest\TestFiles\AutomatedTest\" /s /e /y /I
	@XCOPY "$(ReusableComponentsRootDirectory)\COMComponents\UCLIDRasterAndOCRMgmt\Core\Test\SpatialStringAutomatedTest\TestFiles\*.*" "$(TestingFilesDirectory)\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\Core\Test\SpatialStringAutomatedTest\TestFiles\" /s /e /y /I
	@XCOPY "$(ReusableComponentsRootDirectory)\COMComponents\UCLIDRasterAndOCRMgmt\Core\Test\SpatialStringSearcherTest\TestFiles\*.*" "$(TestingFilesDirectory)\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\Core\Test\SpatialStringSearcherTest\TestFiles\" /s /e /y /I
	@XCOPY "$(ReusableComponentsRootDirectory)\COMComponents\UCLIDRasterAndOCRMgmt\Core\Test\RasterZoneTester\TestFiles\*.*" "$(TestingFilesDirectory)\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\Core\Test\RasterZoneTester\TestFiles\" /s /e /y /I
	@XCOPY "$(ReusableComponentsRootDirectory)\COMComponents\UCLIDCOMUtils\Core\Test\StringPatternMatcherAutoTest\TestFiles\*.*" "$(TestingFilesDirectory)\ReusableComponents\COMComponents\UCLIDCOMUtils\Core\Test\StringPatternMatcherAutoTest\TestFiles\" /s /e /y /I
	@XCOPY "$(ReusableComponentsRootDirectory)\BaseUtils\AutomatedTest\TestFiles\*.*" "$(TestingFilesDirectory)\ReusableComponents\BaseUtils\AutomatedTest\TestFiles\" /s /e /y /I
	@XCOPY "$(ReusableComponentsRootDirectory)\COMComponents\UCLIDFileProcessing\AutomatedTest\*.*" "$(TestingFilesDirectory)\ReusableComponents\COMComponents\UCLIDFileProcessing\AutomatedTest\" /s /e /y /I
	@XCOPY "$(ReusableComponentsRootDirectory)\VendorSpecificUtils\SafeNetUtils\AutomatedTest\TestFiles\*.*" "$(TestingFilesDirectory)\ReusableComponents\VendorSpecificUtils\SafeNetUtils\AutomatedTest\TestFiles\" /s /e /y /I

DoEverythingNoGet: DoEverything

DoEverything: DisplayTimeStamp CreateIDSRDTInstallCD
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @ECHO Attribute Finder SDK Build process completed.
    @ECHO.
