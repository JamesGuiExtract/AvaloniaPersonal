#############################################################################
# I N C L U D E S
#
# the common.mak file being included here counts on $(ProductRootDirName)
# ProductRootDirName defined as a non-null string
#

!include ComponentVersions.mak
!include ..\..\..\ProductDevelopment\Common\Common.mak

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
InputFunnelRootDirectory=$(ReusableComponentsRootDirectory)\InputFunnel
LRInputFunnelPackageRootDir=$(InputFunnelRootDirectory)\Packages\LandRecords

InputFunnelCoreInstallFilesRootDir=P:\InputFunnel\CoreInstallation\Files
InputFunnelCoreInstallProjectRootDir=$(InputFunnelRootDirectory)\Installation\InputFunnelCore
InputFunnelCoreMergeModuleInstallRoot=$(InputFunnelRootDirectory)\Installation\UCLID Input Funnel
InputFunnelCoreInstallScriptFile=$(InputFunnelCoreInstallProjectRootDir)\Script Files\setup.rul

PDRootDir=$(EngineeringRootDirectory)\ProductDevelopment
PDUtilsRootDir=$(PDRootDir)\Utils
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

#############################################################################
# B U I L D    T A R G E T S
#
BuildPDUtils: 
	@ECHO Building PD Utils...
    @CD "$(PDUtilsRootDir)\UCLIDUtilApps\Code"
    @devenv Utils.sln /BUILD $(BuildConfig) /USEENV

BuildInputFunnelCore: 
	@ECHO Building LandRecords...
    @CD "$(LRInputFunnelPackageRootDir)\Code"
    @devenv LandRecords.sln /BUILD $(BuildConfig) /USEENV

CopyFilesToInstallFolder:  BuildInputFunnelCore BuildPDUtils
    @ECHO Removing old files from installation directory...
	@IF NOT EXIST "$(InputFunnelCoreInstallFilesRootDir)\RogueWaveDlls" @MKDIR "$(InputFunnelCoreInstallFilesRootDir)\RogueWaveDlls"
	@IF NOT EXIST "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCommonComponents" @MKDIR "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCommonComponents"
    @IF NOT EXIST "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCoreComponents" @MKDIR "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCoreComponents"
    @IF NOT EXIST "$(InputFunnelCoreInstallFilesRootDir)\NonSelfRegCommonComponents" @MKDIR "$(InputFunnelCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
    @IF NOT EXIST "$(InputFunnelCoreInstallFilesRootDir)\NonSelfRegCoreComponents" @MKDIR "$(InputFunnelCoreInstallFilesRootDir)\NonSelfRegCoreComponents"
	@IF NOT EXIST "$(InputFunnelCoreInstallFilesRootDir)\LeadToolsDlls" @MKDIR "$(InputFunnelCoreInstallFilesRootDir)\LeadToolsDlls"
	@IF NOT EXIST "$(InputFunnelCoreInstallFilesRootDir)\CaereDlls" @MKDIR "$(InputFunnelCoreInstallFilesRootDir)\CaereDlls"
    @DeleteFiles "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCommonComponents\*.*"
    @DeleteFiles "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCoreComponents\*.*"
    @DeleteFiles "$(InputFunnelCoreInstallFilesRootDir)\NonSelfRegCommonComponents\*.*"
    @DeleteFiles "$(InputFunnelCoreInstallFilesRootDir)\NonSelfRegCoreComponents\*.*"
	@DeleteFiles "$(InputFunnelCoreInstallFilesRootDir)\LeadToolsDlls\*.*"
	@DeleteFiles "$(InputFunnelCoreInstallFilesRootDir)\RogueWaveDlls\*.*"
	@DeleteFiles "$(InputFunnelCoreInstallFilesRootDir)\CaereDlls\*.*"
	
    @ECHO Copying the InputFunnelCore files to installation directory...
	@XCOPY "$(ReusableComponentsRootDirectory)\APIs\LeadTools_16.5\Bin\*.*" "$(InputFunnelCoreInstallFilesRootDir)\LeadToolsDlls" /v /s /e /y
	@XCOPY "$(ReusableComponentsRootDirectory)\APIs\LeadTools_16.5\pdf\*.*" "$(InputFunnelCoreInstallFilesRootDir)\LeadToolsDlls\pdf" /v /s /e /y
	@XCOPY "$(ReusableComponentsRootDirectory)\APIs\RogueWave\bin\*.*" "$(InputFunnelCoreInstallFilesRootDir)\RogueWaveDlls" /v /s /e /y
    @XCOPY "$(ReusableComponentsRootDirectory)\APIs\Nuance_16\bin\*.*" "$(InputFunnelCoreInstallFilesRootDir)\CaereDLLs" /v /s /e /y
    @COPY /V "$(BinariesFolder)\UCLIDMeasurements.dll" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCommonComponents"
    @COPY /V "$(BinariesFolder)\COMLM.dll" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCommonComponents"
    @COPY /V "$(BinariesFolder)\UCLIDRasterAndOCRMgmt.dll" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCommonComponents"
    @COPY /V "$(BinariesFolder)\UCLIDDistanceConverter.dll" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCommonComponents"
    @COPY /V "$(BinariesFolder)\UCLIDExceptionMgmt.dll" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCommonComponents"
    @COPY /V "$(BinariesFolder)\ImageEdit.ocx" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCommonComponents"
    @COPY /V "$(BinariesFolder)\UCLIDGenericDisplay2.ocx" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCommonComponents"
    @COPY /V "$(BinariesFolder)\UCLIDTestingFramework.dll" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCommonComponents"
    @COPY /V "$(BinariesFolder)\SSOCR.dll" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCommonComponents"
    @COPY /V "$(BinariesFolder)\UCLIDCOMUtils.dll" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCommonComponents"
    @COPY /V "$(BinariesFolder)\UCLIDHighlightWindow.dll" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCommonComponents"
    @COPY /V "$(BinariesFolder)\UCLIDImageUtils.dll" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCommonComponents"
    @COPY /V "$(BinariesFolder)\ssocr2.Exe" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCommonComponents"
    @COPY /V "$(ReusableComponentsRootDirectory)\APIs\LeadTools_13\bin\LTCML13n.dll" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCommonComponents"
    @COPY /V "$(BinariesFolder)\ESMessageUtils.dll" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCommonComponents"
    	
    @COPY /V "$(BinariesFolder)\HighlightedTextIR.dll" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCoreComponents"
    @COPY /V "$(BinariesFolder)\IFCore.dll" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCoreComponents"
    @COPY /V "$(BinariesFolder)\InputFinders.dll" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCoreComponents"
    @COPY /V "$(BinariesFolder)\LandRecordsIV.dll" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCoreComponents"
    @COPY /V "$(BinariesFolder)\LineTextCorrectors.dll" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCoreComponents"
    @COPY /V "$(BinariesFolder)\LineTextEvaluators.dll" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCoreComponents"
    @COPY /V "$(BinariesFolder)\ParagraphTextCorrectors.dll" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCoreComponents"
    @COPY /V "$(BinariesFolder)\ParagraphTextHandlers.dll" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCoreComponents"
    @COPY /V "$(BinariesFolder)\SubImageHandlers.dll" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCoreComponents"
    @COPY /V "$(BinariesFolder)\SpotRecognitionIR.dll" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCoreComponents"
    @COPY /V "$(BinariesFolder)\UCLIDMCRTextViewer.ocx" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCoreComponents"
    @COPY /V "$(BinariesFolder)\GeneralIV.dll" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCoreComponents"
    @COPY /V "$(BinariesFolder)\RegExprIV.dll" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCoreComponents"
    @COPY /V "$(BinariesFolder)\InputTargetFramework.dll" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCoreComponents"
    @COPY /V "$(BinariesFolder)\InputContexts.dll" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCoreComponents"
#    @COPY /V "$(BinariesFolder)\SpeechIRs.dll" "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCoreComponents"

    @COPY /V "$(BinariesFolder)\BaseUtils.dll" "$(InputFunnelCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
    @COPY /V "$(BinariesFolder)\LeadUtils.dll" "$(InputFunnelCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
    @COPY /V "$(BinariesFolder)\COMLMCore.dll" "$(InputFunnelCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
    @COPY /V "$(BinariesFolder)\Filters.dll" "$(InputFunnelCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
    @COPY /V "$(BinariesFolder)\FiltersCore.dll" "$(InputFunnelCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
    @COPY /V "$(BinariesFolder)\TopoUtils.dll" "$(InputFunnelCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
    @COPY /V "$(BinariesFolder)\OCRFilteringBase.dll" "$(InputFunnelCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
    @COPY /V "$(BinariesFolder)\ZLibUtils.dll" "$(InputFunnelCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
    @COPY /V "$(BinariesFolder)\RWUtils.dll" "$(InputFunnelCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
    @COPY /V "$(BinariesFolder)\UserLicense.Exe" "$(InputFunnelCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
    @COPY /V "$(BinariesFolder)\DetectAndReportFailure.exe" "$(InputFunnelCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
	@COPY /v "$(BinariesFolder)\UEXViewer.exe" "$(InputFunnelCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
    @COPY /V "$(PDUtilsRootDir)\DetectAndReportFailure\Misc\DetectAndReportFailure.ini" "$(InputFunnelCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
	@COPY /V "$(PDCommonDir)\RegisterAll.bat" "$(InputFunnelCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
	@COPY /V "$(BinariesFolder)\ExtractTRP2.exe" "$(InputFunnelCoreInstallFilesRootDir)\NonSelfRegCommonComponents"
	
# Create RegList.dat file for SelfRegCommonComponents
	@DIR "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCommonComponents\*.*" /b >"$(InputFunnelCoreInstallFilesRootDir)\NonSelfRegCommonComponents\IFCommon.rl"
	@DIR "$(InputFunnelCoreInstallFilesRootDir)\SelfRegCoreComponents\*.*" /b >"$(InputFunnelCoreInstallFilesRootDir)\NonSelfRegCoreComponents\IFCore.rl"
#    @COPY /V "$(BinariesFolder)\sit_grammar.xml" "$(InputFunnelCoreInstallFilesRootDir)\NonSelfRegCoreComponents"
    @COPY /V "$(BinariesFolder)\ImageViewer.exe" "$(InputFunnelCoreInstallFilesRootDir)\NonSelfRegCoreComponents"
    @DeleteFiles "$(InputFunnelCoreInstallFilesRootDir)\vssver.scc"
    @DeleteFiles "$(InputFunnelCoreInstallFilesRootDir)\mssccprj.scc"

####
# Building UCLIDInputFunnel Merge Module
####
BuildIFCoreMergeModule: CopyFilesToInstallFolder CreateVersionISImportFile
    @ECHO Buliding the UCLID InputFunnel Merge Module installation...
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VSS_DIR)\win32;$(ReusableComponentsRootDirectory)\APIs\Nuance_16\bin;$(ReusableComponentsRootDirectory)\APIs\LeadTools_16.5\Bin;$(ReusableComponentsRootDirectory)\APIs\RogueWave\bin;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(VCPP_DIR)\PlatformSDK\bin;$(VISUAL_STUDIO)\SDK\v2.0\bin;C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727;$(VCPP_DIR)\VCPackages
	$(SetProductVerScript) "$(InputFunnelCoreMergeModuleInstallRoot)\UCLID Input Funnel.ism" "$(ReusableComponentsVersion)"
    @"$(DEV_STUDIO_DIR)\System\IsCmdBld.exe" -p "$(InputFunnelCoreMergeModuleInstallRoot)\UCLID Input Funnel.ism"

DoNecessaryBuilds: BuildInputFunnelCore BuildPDUtils

GetAllFiles: GetPDUtilsFiles GetPDCommonFiles GetReusableComponentFiles

DoEverythingNoGet: DisplayTimeStamp SetupBuildEnv DoNecessaryBuilds BuildIFCoreMergeModule
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @ECHO Input Funnel Core Build process completed.
    @ECHO.
	
DoEverything: DisplayTimeStamp SetupBuildEnv GetAllFiles DoEverythingNoGet
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @ECHO Input Funnel Core Build process completed.
    @ECHO.
