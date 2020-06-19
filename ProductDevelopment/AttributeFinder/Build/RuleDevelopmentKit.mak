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
TestingFilesDirectory="T:"

RDTInstallFilesRootDir=P:\AttributeFinder\RDTInstallation\Files
RDTInstallProjectRootDir=$(EngineeringRootDirectory)\ProductDevelopment\AttributeFinder\Installation\RuleDevelopmentKit
RDTInstallScriptFile=$(RDTInstallProjectRootDir)\Script Files\setup.rul
RDTInstallMediaDir=$(RDTInstallProjectRootDir)\Media\CD-ROM\DiskImages\Disk1

FLEXCustomerRDTInstallProjectRoot=$(EngineeringRootDirectory)\ProductDevelopment\AttributeFinder\Installation\FlexIndexCustomerRDT
FLEXCustomerRDTMediaDir=$(FLEXCustomerRDTInstallProjectRoot)\Media\CD-ROM\DiskImages\Disk1

IDSInstallProjectDirectory=$(PDRootDir)\Installation\IDShieldCustomerRDT
IDSRDTInstallMediaDir=$(IDSInstallProjectDirectory)\Media\CD-ROM\DiskImages\Disk1

RDTReleaseBleedingEdgeDir=S:\FlexIndex\Internal\BleedingEdge\$(FlexIndexVersion)\RDT

RCDotNetDir=$(EngineeringRootDirectory)\RC.Net

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

CreateDestinationFolders:
	@IF NOT EXIST "$(RDTInstallFilesRootDir)\NonSelfRegRDTComponents" @MKDIR "$(RDTInstallFilesRootDir)\NonSelfRegRDTComponents"
	@IF NOT EXIST "$(RDTInstallFilesRootDir)\SelfRegRDTComponents" @MKDIR "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
	@IF NOT EXIST "$(RDTInstallFilesRootDir)\SelfRegRDTNetCommon" @MKDIR "$(RDTInstallFilesRootDir)\SelfRegRDTNetCommon"
	@IF NOT EXIST "$(RDTInstallFilesRootDir)\SelfRegCommonComponents" @MKDIR "$(RDTInstallFilesRootDir)\SelfRegCommonComponents"
	@IF NOT EXIST "$(RDTInstallFilesRootDir)\NonSelfRegCommonComponents" @MKDIR "$(RDTInstallFilesRootDir)\NonSelfRegCommonComponents"

CopyFilesToInstallFolder: CreateDestinationFolders
    @ECHO Copying the TCL files to installation directory...
	@DeleteFiles "$(RDTInstallFilesRootDir)\SelfRegRDTComponents\*.*" /S
	@DeleteFiles "$(RDTInstallFilesRootDir)\SelfRegRDTNetCommon\*.*" /S
	@DeleteFiles "$(RDTInstallFilesRootDir)\NonSelfRegRDTComponents\*.*" /S
	@DeleteFiles "$(RDTInstallFilesRootDir)\SelfRegCommonComponents\*.*" /S
	@DeleteFiles "$(RDTInstallFilesRootDir)\NonSelfRegCommonComponents\*.*" /S
	
    @ECHO Copying the Binary files to installation directory...
    @COPY /V /Y  "$(BinariesFolder)\RuleSetEditor.exe" "$(RDTInstallFilesRootDir)\NonSelfRegRDTComponents"
    @COPY /V /Y  "$(BinariesFolder)\TestHarness.exe" "$(RDTInstallFilesRootDir)\NonSelfRegRDTComponents"
    @COPY /V /Y  "$(BinariesFolder)\DocumentSorterConsoleApp.exe" "$(RDTInstallFilesRootDir)\NonSelfRegRDTComponents"
    @COPY /V /Y  "$(BinariesFolder)\IndexConverter.exe" "$(RDTInstallFilesRootDir)\NonSelfRegRDTComponents"
    @COPY /V /Y  "$(BinariesFolder)\RDTConfig.exe" "$(RDTInstallFilesRootDir)\NonSelfRegRDTComponents"
	@COPY /V /Y  "$(BinariesFolder)\FindString.exe" "$(RDTInstallFilesRootDir)\NonSelfRegRDTComponents"
	@COPY /V /Y  "$(BinariesFolder)\ImageCleanupSettingsEditor.exe" "$(RDTInstallFilesRootDir)\NonSelfRegRDTComponents"
	@COPY /V /Y  "$(BinariesFolder)\ModifyVOA.exe" "$(RDTInstallFilesRootDir)\NonSelfRegRDTComponents"
	@COPY /V /Y  "$(BinariesFolder)\ESConvertUSSToTXT.exe" "$(RDTInstallFilesRootDir)\NonSelfRegRDTComponents"
	@COPY /V /Y  "$(BinariesFolder)\AFCoreTest.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
    @COPY /V /Y  "$(BinariesFolder)\AFSplittersTest.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
    @COPY /V /Y  "$(BinariesFolder)\AFUtilsTest.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
    @COPY /V /Y  "$(BinariesFolder)\CountyTester.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
    @COPY /V /Y  "$(BinariesFolder)\UCLIDTestingFrameworkCore.dll" "$(RDTInstallFilesRootDir)\SelfRegCommonComponents"
    @COPY /V /Y  "$(BinariesFolder)\AFDataScorersTest.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
	@COPY /V /Y  "$(BinariesFolder)\AFValueFinderTest.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
	@COPY /V /Y  "$(BinariesFolder)\SpatialStringAutomatedTest.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
	@COPY /V /Y  "$(BinariesFolder)\SpatialStringSearcherTest.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
	@COPY /V /Y  "$(BinariesFolder)\SpotRecIRAutoTest.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
	@COPY /V /Y  "$(BinariesFolder)\StringPatternMatcherAutoTest.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
    @COPY /V /Y  "$(BinariesFolder)\RedactionTester.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
    @COPY /V /Y  "$(BinariesFolder)\RedactionCC.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
	@COPY /V /Y  "$(BinariesFolder)\RasterZoneTester.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
	@COPY /V /Y  "$(BinariesFolder)\BaseUtilsTest.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
 	@COPY /V /Y  "$(BinariesFolder)\VBScriptUtils.dll" "$(RDTInstallFilesRootDir)\SelfRegRDTComponents"
 	@COPY /V /Y  "$(BinariesFolder)\OcrSingleDocument.exe" "$(RDTInstallFilesRootDir)\NonSelfRegCommonComponents"
 	@COPY /V /Y  "$(BinariesFolder)\GetWordLengthDist.exe" "$(RDTInstallFilesRootDir)\NonSelfRegCommonComponents"
 	@COPY /V /Y  "$(BinariesFolder)\CopyNumberedFiles.exe" "$(RDTInstallFilesRootDir)\NonSelfRegCommonComponents"
 	@COPY /V /Y  "$(BinariesFolder)\LearningMachineEditor.exe" "$(RDTInstallFilesRootDir)\NonSelfRegRDTComponents"	
	@COPY /V /Y  "$(BinariesFolder)\ExpressionAndQueryTester.exe" "$(RDTInstallFilesRootDir)\NonSelfRegCommonComponents"
	@COPY /V /Y  "$(BinariesFolder)\Extract*test*.dll" "$(RDTInstallFilesRootDir)\NonSelfRegCommonComponents"
	@COPY /V /Y  "$(RCDotNetDir)\Core\Testing\Automated\Extract-RDT-ConsoleRunner.ps1" "$(RDTInstallFilesRootDir)\NonSelfRegCommonComponents"
	@COPY /V /Y  "$(BinariesFolder)\Extract.DataEntry.DEP.*.dll" "$(RDTInstallFilesRootDir)\NonSelfRegCommonComponents"
 	@COPY /V /Y  "$(BinariesFolder)\NERAnnotator.exe" "$(RDTInstallFilesRootDir)\NonSelfRegRDTComponents"	
	@COPY /V /Y  "$(BinariesFolder)\TestAppForSystemMethods.exe" "$(RDTInstallFilesRootDir)\NonSelfRegRDTComponents"
 	@COPY /V /Y  "$(BinariesFolder)\DiffLib.dll" "$(RDTInstallFilesRootDir)\NonSelfRegRDTComponents"	
	@COPY /V /Y  "$(BinariesFolder)\DatabaseMigrationWizard.Test.dll" "$(RDTInstallFilesRootDir)\NonSelfRegCommonComponents"
	
	@DIR "$(RDTInstallFilesRootDir)\SelfRegRDTNetCommon\*.*" /b >"$(RDTInstallFilesRootDir)\NonSelfRegCommonComponents\RDTCommon.nl"
	@DIR "$(RDTInstallFilesRootDir)\SelfRegCommonComponents\*.*" /b >"$(RDTInstallFilesRootDir)\NonSelfRegCommonComponents\RDTCommon.rl"
	@DIR "$(RDTInstallFilesRootDir)\SelfRegRDTComponents\*.*" /b >"$(RDTInstallFilesRootDir)\NonSelfRegRDTComponents\RDT.rl"
	@DIR "$(BinariesFolder)\AFCoreTest.dll" /b >"$(BinariesFolder)\CustomerRDT.rl"
	@DIR "$(BinariesFolder)\RedactionTester.dll" /b >"$(BinariesFolder)\IDShieldCustomerRDT.rl"
  
    @DeleteFiles "$(RDTInstallFilesRootDir)\vssver.scc"
    @DeleteFiles "$(RDTInstallFilesRootDir)\mssccprj.scc"

BuildRDTInstall: DisplayTimeStamp CopyFilesToInstallFolder CopyTestFiles
    @ECHO Building the RDT installation...
	$(SetProductVerScript) "$(RDTInstallProjectRootDir)\RuleDevelopmentKit.ism" "$(FlexIndexVersion)"
    @"$(DEV_STUDIO_DIR)\System\IsCmdBld.exe" -p "$(RDTInstallProjectRootDir)\RuleDevelopmentKit.ism"

CreateRDTInstallCD: BuildRDTInstall
    @IF NOT EXIST "$(RDTReleaseBleedingEdgeDir)" MKDIR "$(RDTReleaseBleedingEdgeDir)"
    @XCOPY "$(RDTInstallMediaDir)\*.*" "$(RDTReleaseBleedingEdgeDir)" /v /s /e /y
    $(VerifyDir) "$(RDTInstallMediaDir)" "$(RDTReleaseBleedingEdgeDir)"
    @DeleteFiles "$(RDTReleaseBleedingEdgeDir)\vssver.scc"

CopyTestFiles:
	@ECHO Copying Automated Test Files
	@DeleteFiles "$(TestingFilesDirectory)\*.*" /S /Q 
	@IF EXIST "$(TestingFilesDirectory)\ProductDevelopment" @RMDIR "$(TestingFilesDirectory)\ProductDevelopment" /S /Q
    @IF EXIST "$(TestingFilesDirectory)\ReusableComponents" @RMDIR "$(TestingFilesDirectory)\ReusableComponents" /S /Q
    @IF EXIST "$(TestingFilesDirectory)\FileActionManager" @RMDIR "$(TestingFilesDirectory)\FileActionManager" /S /Q
    @IF EXIST "$(TestingFilesDirectory)\RC.Net" @RMDIR "$(TestingFilesDirectory)\RC.Net" /S /Q
    @XCOPY "$(AFRootDirectory)\AFConditions\AutomatedTest\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\AFConditions\AutomatedTest\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\AFCore\AutomatedTest\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\AFCore\AutomatedTest\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\AFDataScorers\AFDataScorersTest\TestFiles\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\AFDataScorers\AFDataScorersTest\TestFiles\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\AFOutputHandlers\AutomatedTest\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\AFOutputHandlers\AutomatedTest\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\AFPreProcessors\AutomatedTest\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\AFPreProcessors\AutomatedTest\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\AFPreProcessors\InteractiveTest\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\AFPreProcessors\InteractiveTest\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\AFSelectors\AutomatedTest\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\AFSelectors\AutomatedTest\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\AFSplitters\AutomatedTest\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\AFSplitters\AutomatedTest\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\AFSplitters\AFSplittersTest\TestFiles\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\AFSplitters\AFSplittersTest\TestFiles\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\AFUtils\AutomatedTest\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\AFUtils\AutomatedTest\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\AFUtils\AFUtilsTest\TestFiles\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\AFUtils\AFUtilsTest\TestFiles\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\AFValueFinders\AutomatedTest\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\AFValueFinders\AutomatedTest\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\AFValueFinders\Test\AFValueFinderTest\TestFiles\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\AFValueFinders\Test\AFValueFinderTest\TestFiles\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\AFValueModifiers\AutomatedTest\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\AFValueModifiers\AutomatedTest\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\InputValidators\AutomatedTest\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\InputValidators\AutomatedTest\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\IndustrySpecific\Redaction\AutomatedTest\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\IndustrySpecific\Redaction\AutomatedTest\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\IndustrySpecific\Redaction\InteractiveTest\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\IndustrySpecific\Redaction\InteractiveTest\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\IndustrySpecific\Redaction\RedactionTester\AutomatedTest\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\IndustrySpecific\Redaction\RedactionTester\AutomatedTest\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\IndustrySpecific\DataEntry\AutomatedTest\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\IndustrySpecific\DataEntry\AutomatedTest\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\IndustrySpecific\DataEntry\InteractiveTest\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\IndustrySpecific\DataEntry\InteractiveTest\" /s /e /y /I
	@XCOPY "$(AFRootDirectory)\IndustrySpecific\LabResults\AutomatedTest\*.*" "$(TestingFilesDirectory)\ProductDevelopment\AttributeFinder\IndustrySpecific\LabResults\AutomatedTest\" /s /e /y /I
	@XCOPY "$(ReusableComponentsRootDirectory)\COMComponents\UCLIDRasterAndOCRMgmt\Core\Test\SpatialStringAutomatedTest\TestFiles\*.*" "$(TestingFilesDirectory)\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\Core\Test\SpatialStringAutomatedTest\TestFiles\" /s /e /y /I
	@XCOPY "$(ReusableComponentsRootDirectory)\COMComponents\UCLIDRasterAndOCRMgmt\Core\Test\SpatialStringSearcherTest\TestFiles\*.*" "$(TestingFilesDirectory)\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\Core\Test\SpatialStringSearcherTest\TestFiles\" /s /e /y /I
	@XCOPY "$(ReusableComponentsRootDirectory)\COMComponents\UCLIDRasterAndOCRMgmt\Core\Test\RasterZoneTester\TestFiles\*.*" "$(TestingFilesDirectory)\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\Core\Test\RasterZoneTester\TestFiles\" /s /e /y /I
	@XCOPY "$(ReusableComponentsRootDirectory)\BaseUtils\AutomatedTest\TestFiles\*.*" "$(TestingFilesDirectory)\ReusableComponents\BaseUtils\AutomatedTest\TestFiles\" /s /e /y /I
	@XCOPY "$(ReusableComponentsRootDirectory)\BaseUtils\InteractiveTest\*.*" "$(TestingFilesDirectory)\ReusableComponents\BaseUtils\InteractiveTest\" /s /e /y /I
	@XCOPY "$(ReusableComponentsRootDirectory)\COMComponents\UCLIDFileProcessing\AutomatedTest\*.*" "$(TestingFilesDirectory)\ReusableComponents\COMComponents\UCLIDFileProcessing\AutomatedTest\" /s /e /y /I
	@XCOPY "$(ReusableComponentsRootDirectory)\COMComponents\UCLIDFileProcessing\ESFileSuppliers\AutomatedTest\*.*" "$(TestingFilesDirectory)\ReusableComponents\COMComponents\UCLIDFileProcessing\ESFileSuppliers\AutomatedTest\" /s /e /y /I
	@XCOPY "$(ReusableComponentsRootDirectory)\COMComponents\UCLIDFileProcessing\ESSkipConditions\InteractiveTest\*.*" "$(TestingFilesDirectory)\ReusableComponents\COMComponents\UCLIDFileProcessing\ESSkipConditions\InteractiveTest\" /s /e /y /I
	@XCOPY "$(RCDotNetDir)\FileActionManager\FileProcessors\Core\Testing\*.*" "$(TestingFilesDirectory)\RC.Net\FileActionManager\FileProcessors\Core\Testing\" /s /e /y /I
	@XCOPY "$(RCDotNetDir)\AttributeFinder\Rules\Core\AutomatedTest\*.*" "$(TestingFilesDirectory)\RC.Net\AttributeFinder\Rules\Core\AutomatedTest\" /s /e /y /I

DoEverythingNoGet: DoEverything

DoEverything: DisplayTimeStamp CreateRDTInstallCD
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @ECHO Attribute Finder SDK Build process completed.
    @ECHO.
