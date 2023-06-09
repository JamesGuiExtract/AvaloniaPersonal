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
FlexDataEntryRulesDir=$(EngineeringRootDirectory)\Rules\FLEXIndex\Demo_FLEXIndex\Rules

AFInstallPDRootDir=P:\AttributeFinder
AFInstallFilesRootDir=$(AFInstallPDRootDir)\SDKInstallation\Files
AFRequiredInstallsDir=$(AFInstallPDRootDir)\RequiredInstalls
DemoShieldRunFilesDir=$(AFInstallPDRootDir)\DemoShieldFiles
IDShieldInstallFilesRootDir=P:\AttributeFinder\IDShieldInstallation\Files

FlexDataEntryInstallationFilesDir=P:\AttributeFinder\FlexDataEntryInstallation

DotNetFiles=P:\AttributeFinder\CoreInstallation\Files\DotNetGAC

MergeModuleRootDir=$(INSTALLSHIELD_PROJECTS_DIR)\MergeModules

ExtractSoftwareInstallRootDir=$(PDRootDir)\ExtractInstaller
ExtractSoftwareInstallMediaDir=$(ExtractSoftwareInstallRootDir)\Media\CD-ROM\DiskImages\DISK1

FlexIndexSDKInstallRootDir=$(PDRootDir)\AttributeFinder\Installation\UCLID FlexIndex SDK
FlexIndexSDKInstallMediaDir=$(FlexIndexSDKInstallRootDir)\Media\CD-ROM\DiskImages\DISK1

IDShieldInstallRootDir=$(PDRootDir)\Installation\IDShield
IDShieldInstallMediaDir=$(IDShieldInstallRootDir)\Media\CD-ROM\DiskImages\DISK1

InternalUseBuildFilesArchive=M:\ProductDevelopment\AttributeFinder\Archive\InternalUseBuildFiles\InternalBuilds\$(FlexIndexVersion)

RedactionDemoBuildDir=$(AFRootDirectory)\Utils\RedactionDemo\Build

LabDEBuildDir=$(PDRootDir)\DataEntry\LabDE\Build

DeveloperLicensing=I:\Common\Engineering\Tools\SecureClients\COMLicense_Developer\Current
RuleWriterLicensing=I:\Common\Engineering\Tools\SecureClients\COMLicense_RuleWriter\Current
SupportLicensing=I:\Common\Engineering\Tools\SecureClients\COMLicense\Current

# determine the name of the release output directory based upon the build
# configuration that is being built
!IF "$(BuildConfig)" == "Release"
BuildOutputDir=Release
!ELSEIF "$(BuildConfig)" == "Debug"
BuildOutputDir=Debug
!ELSE
!ERROR Internal error - invalid value for BuildConfig variable!
!ENDIF

!IF "$(ProductVersion)" != "$(FlexIndexVersion)"
!ERROR FLEX Index version being build is not current version in LatestComponentVersions.mak file.
!ENDIF

AttributeCoreTarget=DoEverythingNoGet

!IF "$(BuildScriptTarget)"== "DoBuilds"
	AttributeCoreTarget=DoBuilds
!ENDIF

BinariesFolder=$(EngineeringRootDirectory)\Binaries\$(BuildOutputDir)
Replace=$(BinariesFolder)\ReplaceString

#############################################################################
# B U I L D    T A R G E T S
#
CleanBranch:
	@CD $(EngineeringRootDirectory)
	$(GITPATH) clean -d -f -x

BuildFKDBIfRequired:
	@CD $(EngineeringRootDirectory)\Rules\Build_FKB
	@IF "$(FKBBuildNeeded)"=="True" @nmake /F FKBUpdate.mak CreateFKBInstall

BuildAttributeFinderCore: BuildFKDBIfRequired 
	@Echo Building AttributeFinderCore...
	@CD "$(AFRootDirectory)\Build"
    @nmake /F AttributeFinderCore.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" ProductVersion="$(FlexIndexVersion)" WebAppArchivePath="$(WebAppArchivePath)" $(AttributeCoreTarget)
	
CopyFilesToInstallFolder: 
    @ECHO Copying the AttributeFinder files to installation directory...
	@IF NOT EXIST "$(AFInstallFilesRootDir)\Redist" @MKDIR "$(AFInstallFilesRootDir)\Redist"
	@DeleteFiles "$(AFInstallFilesRootDir)\Redist"\*.*" /S /Q
    @COPY /V /Y "$(MergeModuleRootDir)\ExtractCommonMM.msm" "$(AFInstallFilesRootDir)\Redist"
    @COPY /V /Y "$(MergeModuleRootDir)\UCLIDFlexIndex.msm" "$(AFInstallFilesRootDir)\Redist"
    @COPY /V /Y "$(MergeModuleRootDir)\DataEntry.msm" "$(AFInstallFilesRootDir)\Redist"
	@COPY /V /Y "$(MergeModuleRootDir)\ExtractFlexCommonMM.msm" "$(AFInstallFilesRootDir)\Redist"
    @DeleteFiles "$(AFInstallFilesRootDir)\vssver.scc"
    @DeleteFiles "$(AFInstallFilesRootDir)\mssccprj.scc"

CopyComponentVersionFile:
	@ECHO Copying Component Version file...
    @COPY /v /y /a "$(PDRootDir)\Common\LatestComponentVersions.mak" /a +"$(EngineeringRootDirectory)\Rules\Build_FKB\FKBVersion.mak" "$(FLEXIndexInstallFiles)\ComponentsVersions.txt"

BuildExtractSoftware: CopyFilesToInstallFolder
    @ECHO Building Extract Platform Install
	$(SetProductVerScript) "$(ExtractSoftwareInstallRootDir)\ExtractInstaller.ism" "$(FlexIndexVersion)"
    @"$(DEV_STUDIO_DIR)\System\IsCmdBld.exe" -p "$(ExtractSoftwareInstallRootDir)\ExtractInstaller.ism"

CreateExtractSoftwareInstallCD:
	@ECHO Copying to Extract Platform files Tools install folders
    @IF NOT EXIST "$(ExtractSoftwareInstallFiles)" MKDIR "$(ExtractSoftwareInstallFiles)"
    @XCOPY "$(ExtractSoftwareInstallMediaDir)\*.*" "$(ExtractSoftwareInstallFiles)" /v /s /e /y
    $(VerifyDir) "$(ExtractSoftwareInstallMediaDir)" "$(ExtractSoftwareInstallFiles)"
	@COPY /V /Y "$(AFInstallFilesRootDir)\InstallHelp\*.*" "$(ExtractSoftwareInstallFiles)"
	@COPY /V /Y "$(ExtractSoftwareInstallRootDir)\Support Files\license.txt" "$(ExtractSoftware)\Readme.txt"
	@COPY /V /Y "$(EngineeringRootDirectory)\ReusableComponents\COMComponents\UCLIDFileProcessing\Utils\ProcessFiles\Code\res\ProcessFiles.ico" "$(ExtractSoftware)\ExtractInstall.ico"
	@IF NOT EXIST "$(ExtractSoftware)\SQLCompactEdition_3_5" MKDIR "$(ExtractSoftware)\SQLCompactEdition_3_5"
	@XCOPY "$(EngineeringRootDirectory)\SQLCompactEdition_3_5\*.*" "$(ExtractSoftware)\SQLCompactEdition_3_5"
    @DeleteFiles "$(ExtractSoftwareInstallFiles)\*.scc"

CreateFlexDataEntryInstallDir:
	@ECHO Creating Demo_FlexIndex
	@IF NOT EXIST "$(FLEXIndexDemo)\Input" MKDIR "$(FLEXIndexDemo)\Input"
	@IF NOT EXIST "$(FLEXIndexDemo)\Rules" MKDIR "$(FLEXIndexDemo)\Rules"
	@ECHO Updating Flex Data Entry rules for FKB update...
	$(CScriptProgram) "$(PDRootDir)\Utils\Scripts\UpdateFKB.js" -silent "$(FlexDataEntryRulesDir)" "$(LastBuiltFKB)"
	@ECHO Copying the Demo_FlexIndex related files
	@XCOPY "$(AFRootDirectory)\Utils\FlexDataEntry\Files\*.*" "$(FLEXIndexDemo)" /v /s /e /y
	$(VerifyDir) "$(AFRootDirectory)\Utils\FlexDataEntry\Files" "$(FLEXIndexDemo)"
	@XCOPY "$(FlexDataEntryInstallationFilesDir)\Images\*.*" "$(FLEXIndexDemo)\Input\" /v /s /e /y
	$(VerifyDir) "$(FlexDataEntryInstallationFilesDir)\Images" "$(FLEXIndexDemo)\Input"
	@XCOPY "$(FlexDataEntryRulesDir)\*.*" "$(FLEXIndexDemo)\Rules" /v /s /e /y
	$(VerifyDir) "$(FlexDataEntryRulesDir)" "$(FLEXIndexDemo)\Rules"
	@ECHO Encrypting Demo_FlexIndex rsd Files
	@SendFilesAsArgumentToApplication "$(FLEXIndexDemo)\*.rsd" 1 1 "$(BinariesFolder)\EncryptFile.exe"
	@SendFilesAsArgumentToApplication "$(FLEXIndexDemo)\*.dat" 1 1 "$(BinariesFolder)\EncryptFile.exe"
	@DeleteFiles "$(FLEXIndexDemo)\*.rsd"
	@DeleteFiles "$(FLEXIndexDemo)\*.dat"
	@DeleteFiles "$(FLEXIndexDemo)\*.scc"
		
CreateRedactionDemoInstall:
	@ECHO Creating Redaction Demo Install Directory ...
	@CD "$(RedactionDemoBuildDir)"
	@nmake /F $(RedactionDemoBuildDir)\RedactionDemo.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" ProductVersion="$(FlexIndexVersion)" DoEverything

CreateLabDEInstallFilesAndDemo:
	@Echo Building LabDE...
	@CD "$(LabDEBuildDir)"
    @nmake /F LabDE.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" ProductVersion="$(LabDEVersion)" DoEverything
	
CopySilentInstallsDir:
	@ECHO Copying SilentInstalls folder
	@IF NOT EXIST "$(ExtractSoftware)\SilentInstalls" MKDIR "$(ExtractSoftware)\SilentInstalls"
	@XCOPY "$(AFRootDirectory)\SilentInstalls\*.*" "$(ExtractSoftware)\SilentInstalls"
	
CopyFilesToInternalUse:
	@ECHO Copy files to archive
	@IF NOT EXIST "$(InternalUseBuildFilesArchive)\OriginalFiles" @MKDIR "$(InternalUseBuildFilesArchive)\OriginalFiles"
	@IF NOT EXIST "$(InternalUseBuildFilesArchive)\Obfuscated" @MKDIR "$(InternalUseBuildFilesArchive)\Obfuscated"
	@COPY  "$(BinariesFolder)\*.pdb" "$(InternalUseBuildFilesArchive)" 
	@COPY  "$(BinariesFolder)\Map\*.xml" "$(InternalUseBuildFilesArchive)"
 	@COPY  "$(BinariesFolder)\Obfuscated\*.*" "$(InternalUseBuildFilesArchive)\Obfuscated" 
	@COPY  "$(BinariesFolder)\*.exe" "$(InternalUseBuildFilesArchive)\OriginalFiles"
	@COPY  "$(BinariesFolder)\*.dll" "$(InternalUseBuildFilesArchive)\OriginalFiles"
	@COPY  "$(BinariesFolder)\*.xml" "$(InternalUseBuildFilesArchive)\OriginalFiles"

BuildRDT:
	@ECHO Build RDT...
	@CD "$(AFRootDirectory)\Build" 
	nmake /F RuleDevelopmentKit.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" ProductVersion="$(FlexIndexVersion)" DoEverything
	
CreateInstalls: BuildExtractSoftware CreateExtractSoftwareInstallCD CopySilentInstallsDir CopyFilesToInternalUse BuildRDT

DoDemos:CreateFlexDataEntryInstallDir CreateRedactionDemoInstall CreateLabDEInstallFilesAndDemo

DoBuilds: DisplayTimeStamp SetupBuildEnv CleanBranch BuildAttributeFinderCore

DoEverythingNoGet: DoBuilds CreateInstalls CopyComponentVersionFile DoDemos 
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @ECHO Attribute Finder SDK Build process completed.
    @ECHO.
  
DoEverything: DisplayTimeStamp SetupBuildEnv DoEverythingNoGet
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @ECHO Attribute Finder SDK Build process completed.
    @ECHO.
