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

AFInstallPDRootDir=P:\AttributeFinder
AFInstallFilesRootDir=$(AFInstallPDRootDir)\SDKInstallation\Files
AFRequiredInstallsDir=$(AFInstallPDRootDir)\RequiredInstalls
DemoShieldRunFilesDir=$(AFInstallPDRootDir)\DemoShieldFiles
IDShieldInstallFilesRootDir=P:\AttributeFinder\IDShieldInstallation\Files

AFBleedingEdgeDir=R:\FlexIndex\Internal\BleedingEdge
AFReleaseBleedingEdgeDir=$(AFBleedingEdgeDir)\$(FlexIndexVersion)\FlexIndex
VOAClientReleaseBleedingEdgeDir=$(AFBleedingEdgeDir)\$(FlexIndexVersion)\VOAClient
FlexDataEntryReleaseDir=$(AFBleedingEdgeDir)\$(FlexIndexVersion)\Demo_FlexIndex
FlexDataEntryInstallationFilesDir=P:\AttributeFinder\FlexDataEntryInstallation
ExtractLMReleaseBleedingEdgeDir=$(AFBleedingEdgeDir)\$(FlexIndexVersion)\Extract Systems LM
IDShieldReleaseBleedingEdgeDir=$(AFBleedingEdgeDir)\$(FlexIndexVersion)\IDShield
FlexIndexInstallDir=$(AFBleedingEdgeDir)\$(FlexIndexVersion)\FlexIndexInstall
IDShieldInstallDir=$(AFBleedingEdgeDir)\$(FlexIndexVersion)\IDShieldInstall
DotNetFiles=P:\AttributeFinder\CoreInstallation\Files\DotNetGAC

MergeModuleRootDir=C:\InstallShield 12 Projects\MergeModules
FlexIndexSDKInstallRootDir=$(PDRootDir)\AttributeFinder\Installation\UCLID FlexIndex SDK
FlexIndexSDKInstallMediaDir=$(FlexIndexSDKInstallRootDir)\Media\CD-ROM\DiskImages\DISK1

VOAClientInstallRootDir=$(PDRootDir)\AttributeFinder\Installation\UCLID VOAClient
VOAClientInstallMediaDir=$(VOAClientInstallRootDir)\Media\CD-ROM\DiskImages\DISK1

ExtractLMInstallRootDir=$(ReusableComponentsRootDirectory)\VendorSpecificUtils\SafeNetUtils\Installation\Extract Systems LM
ExtractLMInstallMediaDir=$(ExtractLMInstallRootDir)\Media\CD-ROM\DiskImages\DISK1

IDShieldInstallRootDir=$(PDRootDir)\AttributeFinder\IndustrySpecific\Redaction\Installation\IDShield
IDShieldInstallMediaDir=$(IDShieldInstallRootDir)\Media\CD-ROM\DiskImages\DISK1

RedactionDemoBuildDir=$(AFRootDirectory)\Utils\RedactionDemo\Build

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
BuildAttributeFinderCore:
	@Echo Building AttributeFinderCore...
	@CD "$(AFRootDirectory)\Build"
    @nmake /F AttributeFinderCore.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" ProductVersion="$(ProductVersion)" DoEverythingNoGet
	
CopyFilesToInstallFolder: 
    @ECHO Copying the AttributeFinder files to installation directory...
	@DeleteFiles "$(AFInstallFilesRootDir)\NonSelfRegSDKComponents\*.*" /S /Q
	@DeleteFiles "$(AFInstallFilesRootDir)\Redist"\*.*" /S /Q
    @COPY /v "$(MergeModuleRootDir)\UCLIDInputFunnel.msm" "$(AFInstallFilesRootDir)\Redist"
    @COPY /v "$(MergeModuleRootDir)\UCLIDFlexIndex.msm" "$(AFInstallFilesRootDir)\Redist"
    @DeleteFiles "$(AFInstallFilesRootDir)\vssver.scc"
    @DeleteFiles "$(AFInstallFilesRootDir)\mssccprj.scc"

CopyComponentVersionFile:
	@ECHO Copying Component Version file...
    @COPY /v "$(PDRootDir)\Common\LatestComponentVersions.mak" "$(AFReleaseBleedingEdgeDir)\ComponentsVersions.txt"

BuildFlexIndexSDKInstall: BuildAttributeFinderCore CopyFilesToInstallFolder CreateVersionISImportFile
    @ECHO Building UCLID FlexIndex SDK installation...
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(ReusableComponentsRootDirectory)\APIs\Nuance_16\bin;$(ReusableComponentsRootDirectory)\APIs\LeadTools_16\Bin;$(ReusableComponentsRootDirectory)\APIs\RogueWave\bin;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DevEnvDir);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(ReusableComponentsRootDirectory)\APIs\Inlite_5_7\bin;$(VCPP_DIR)\PlatformSDK\bin;$(VISUAL_STUDIO)\SDK\v2.0\bin;C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727;$(VCPP_DIR)\VCPackages
	$(SetProductVerScript) "$(FlexIndexSDKInstallRootDir)\UCLID FlexIndex SDK.ism" "$(FlexIndexVersion)"
    @"$(DEV_STUDIO_DIR)\System\IsCmdBld.exe" -p "$(FlexIndexSDKInstallRootDir)\UCLID FlexIndex SDK.ism"

CreateAttributeFinderInstallCD: BuildFlexIndexSDKInstall
	@ECHO Copying to FlexIndex install folders
    @IF NOT EXIST "$(AFReleaseBleedingEdgeDir)" MKDIR "$(AFReleaseBleedingEdgeDir)"
    @XCOPY "$(FlexIndexSDKInstallMediaDir)\*.*" "$(AFReleaseBleedingEdgeDir)" /v /s /e /y
    $(VerifyDir) "$(FlexIndexSDKInstallMediaDir)" "$(AFReleaseBleedingEdgeDir)"
	@COPY "$(AFInstallFilesRootDir)\InstallHelp\*.*" "$(AFReleaseBleedingEdgeDir)"
    @DeleteFiles "$(AFReleaseBleedingEdgeDir)\*.scc"

CreateExtractLMInstallCD: BuildAttributeFinderCore
	@ECHO Createing License Manager Install...
	@CD "$(ReusableComponentsRootDirectory)\VendorSpecificUtils\SafeNetUtils\Build"
    @nmake /F LicenseManager.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" ProductVersion="$(ProductVersion)" CreateFlexLMInstall
    
CreateFlexDataEntryInstallDir:
	@ECHO Creating FlexDataEntryInstallDir
	@IF NOT EXIST "$(FlexDataEntryReleaseDir)\Bin" MKDIR "$(FlexDataEntryReleaseDir)\Bin"
	@IF NOT EXIST "$(FlexDataEntryReleaseDir)\Input" MKDIR "$(FlexDataEntryReleaseDir)\Input"
	@ECHO Copying the FlexDataEntry related files
	@COPY /v "$(BinariesFolder)\FlexDataEntry.exe" "$(FlexDataEntryReleaseDir)\Bin"
	@XCOPY "$(DotNetFiles)\*.*" "$(FlexDataEntryReleaseDir)\Bin" /v /s /e /y
	@XCOPY "$(AFRootDirectory)\Utils\FlexDataEntry\Files\*.*" "$(FlexDataEntryReleaseDir)" /v /s /e /y
	$(VerifyDir) "$(AFRootDirectory)\Utils\FlexDataEntry\Files" "$(FlexDataEntryReleaseDir)"
	@XCOPY "$(FlexDataEntryInstallationFilesDir)\Images\*.*" "$(FlexDataEntryReleaseDir)\Input" /v /s /e /y
	$(VerifyDir) "$(FlexDataEntryInstallationFilesDir)\Images" "$(FlexDataEntryReleaseDir)\Input"
	@ECHO Encrypting FlexDataEntry rsd Files
	@SendFilesAsArgumentToApplication "$(FlexDataEntryReleaseDir)\*.rsd" 1 1 "$(BinariesFolder)\EncryptFile.exe"
	@DeleteFiles "$(FlexDataEntryReleaseDir)\*.rsd"
	@DeleteFiles "$(FlexDataEntryReleaseDir)\*.scc"
	
CreateDemoShieldInstall:
	@ECHO Copying Required installs
	@IF NOT EXIST "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\DotNet 3.5 Framework" MKDIR "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\DotNet 3.5 Framework"
	@IF NOT EXIST "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\SQLServerExpress2005" MKDIR "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\SQLServerExpress2005"
	@XCOPY "$(AFRequiredInstallsDir)\DotNet 3.5 Framework\*.*" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\DotNet 3.5 Framework" /v /s /e /y
	@XCOPY "$(AFRequiredInstallsDir)\SQLServerExpress2005\*.*" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\SQLServerExpress2005" /v /s /e /y
	@ECHO Copying DemoShield Files
	@IF NOT EXIST "$(FlexIndexInstallDir)" MKDIR "$(FlexIndexInstallDir)"
	@XCOPY "$(DemoShieldRunFilesDir)\*.*" "$(FlexIndexInstallDir)" /v /s /e /y
	@COPY "$(AFRootDirectory)\Installation\FlexInstall\Launch.ini" "$(FlexIndexInstallDir)"
	@COPY "$(AFRootDirectory)\Installation\FlexInstall\FlexInstall.dbd" "$(FlexIndexInstallDir)"
		
BuildIDShieldInstall: CreateVersionISImportFile
    @ECHO Building Extract Systems IDShield installation...
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(ReusableComponentsRootDirectory)\APIs\Nuance_16\bin;$(ReusableComponentsRootDirectory)\APIs\LeadTools_16\Bin;$(ReusableComponentsRootDirectory)\APIs\RogueWave\bin;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(ReusableComponentsRootDirectory)\APIs\Inlite_5_7\bin;$(VCPP_DIR)\PlatformSDK\bin;$(VISUAL_STUDIO)\SDK\v2.0\bin;C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727;$(VCPP_DIR)\VCPackages
	$(SetProductVerScript) "$(IDShieldInstallRootDir)\IDShield.ism" "$(FlexIndexVersion)"
    @"$(DEV_STUDIO_DIR)\System\IsCmdBld.exe" -p "$(IDShieldInstallRootDir)\IDShield.ism"

CreateIDShieldInstallCD: BuildIDShieldInstall
	@ECHO Copying IDShield Install files ...
    @IF NOT EXIST "$(IDShieldReleaseBleedingEdgeDir)" MKDIR "$(IDShieldReleaseBleedingEdgeDir)"
    @XCOPY "$(IDShieldInstallMediaDir)\*.*" "$(IDShieldReleaseBleedingEdgeDir)" /v /s /e /y
    $(VerifyDir) "$(IDShieldInstallMediaDir)" "$(IDShieldReleaseBleedingEdgeDir)"
    @DeleteFiles "$(IDShieldReleaseBleedingEdgeDir)\vssver.scc"
	@IF NOT EXIST "$(IDShieldInstallDir)" MKDIR "$(IDShieldInstallDir)"
	@XCOPY "$(DemoShieldRunFilesDir)\*.*" "$(IDShieldInstallDir)" /v /s /e /y
	@COPY "$(AFRootDirectory)\IndustrySpecific\Redaction\Installation\IDShieldInstall\Launch.ini" "$(IDShieldInstallDir)"
	@COPY "$(AFRootDirectory)\IndustrySpecific\Redaction\Installation\IDShieldInstall\IDShieldInstall.dbd" "$(IDShieldInstallDir)"
	@COPY "$(IDShieldInstallFilesRootDir)\InstallHelp\*.*" "$(IDShieldReleaseBleedingEdgeDir)"
    
CreateRedactionDemoInstall:
	@ECHO Creating Redaction Demo Install Directory ...
	@CD "$(RedactionDemoBuildDir)"
	@nmake /F $(RedactionDemoBuildDir)\RedactionDemo.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" ProductVersion="$(ProductVersion)" DoEverything

CreateInstalls: BuildIDShieldInstall CreateAttributeFinderInstallCD CreateExtractLMInstallCD  CreateIDShieldInstallCD CreateDemoShieldInstall

DoDemos:CreateFlexDataEntryInstallDir CreateRedactionDemoInstall

GetAllFiles: GetPDCommonFiles GetAttributeFinderFiles GetRCdotNETFiles GetReusableComponentFiles GetPDUtilsFiles

DoEverythingNoGet: DisplayTimeStamp SetupBuildEnv BuildAttributeFinderCore CreateInstalls CopyComponentVersionFile DoDemos
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @ECHO Attribute Finder SDK Build process completed.
    @ECHO.
  
DoEverything: DisplayTimeStamp SetupBuildEnv GetAllFiles DoEverythingNoGet
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @ECHO Attribute Finder SDK Build process completed.
    @ECHO.
