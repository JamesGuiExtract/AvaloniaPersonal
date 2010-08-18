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

MergeModuleRootDir=C:\InstallShield 2010 Projects\MergeModules
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

!IF "$(ProductVersion)" != "$(FlexIndexVersion)"
!ERROR FLEX Index version being build is not current version in LatestComponentVersions.mak file.
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
    @COPY /v "$(MergeModuleRootDir)\ExtractCommonMM.msm" "$(AFInstallFilesRootDir)\Redist"
    @COPY /v "$(MergeModuleRootDir)\UCLIDFlexIndex.msm" "$(AFInstallFilesRootDir)\Redist"
    @COPY /v "$(MergeModuleRootDir)\DataEntry.msm" "$(AFInstallFilesRootDir)\Redist"
    @DeleteFiles "$(AFInstallFilesRootDir)\vssver.scc"
    @DeleteFiles "$(AFInstallFilesRootDir)\mssccprj.scc"

CopyComponentVersionFile:
	@ECHO Copying Component Version file...
    @COPY /v "$(PDRootDir)\Common\LatestComponentVersions.mak" "$(AFReleaseBleedingEdgeDir)\ComponentsVersions.txt"

BuildFlexIndexSDKInstall: BuildAttributeFinderCore CopyFilesToInstallFolder CreateVersionISImportFile
    @ECHO Building UCLID FlexIndex SDK installation...
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(ReusableComponentsRootDirectory)\APIs\Nuance_16.3\bin;$(ReusableComponentsRootDirectory)\APIs\LeadTools_16.5\Bin;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DevEnvDir);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(ReusableComponentsRootDirectory)\APIs\Inlite_5_7\bin;$(WINDOWS_SDK)\BIN;C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319;$(VCPP_DIR)\VCPackages
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
	@ECHO Creating Demo_FlexIndex
	@IF NOT EXIST "$(FlexDataEntryReleaseDir)\Input" MKDIR "$(FlexDataEntryReleaseDir)\Input"
	@IF NOT EXIST "$(FlexDataEntryReleaseDir)\Rules" MKDIR "$(FlexDataEntryReleaseDir)\Rules"
	@ECHO Copying the Demo_FlexIndex related files
	@XCOPY "$(AFRootDirectory)\Utils\FlexDataEntry\Files\*.*" "$(FlexDataEntryReleaseDir)" /v /s /e /y
	$(VerifyDir) "$(AFRootDirectory)\Utils\FlexDataEntry\Files" "$(FlexDataEntryReleaseDir)"
	@XCOPY "$(FlexDataEntryInstallationFilesDir)\Images\*.*" "$(FlexDataEntryReleaseDir)\Input" /v /s /e /y
	$(VerifyDir) "$(FlexDataEntryInstallationFilesDir)\Images" "$(FlexDataEntryReleaseDir)\Input"
	@XCOPY "$(FlexDataEntryRulesDir)\*.*" "$(FlexDataEntryReleaseDir)\Rules" /v /s /e /y
	$(VerifyDir) "$(FlexDataEntryRulesDir)" "$(FlexDataEntryReleaseDir)\Rules"
	@ECHO Encrypting Demo_FlexIndex rsd Files
	@SendFilesAsArgumentToApplication "$(FlexDataEntryReleaseDir)\*.rsd" 1 1 "$(BinariesFolder)\EncryptFile.exe"
	@SendFilesAsArgumentToApplication "$(FlexDataEntryReleaseDir)\*.dat" 1 1 "$(BinariesFolder)\EncryptFile.exe"
	@DeleteFiles "$(FlexDataEntryReleaseDir)\*.rsd"
	@DeleteFiles "$(FlexDataEntryReleaseDir)\*.dat"
	@DeleteFiles "$(FlexDataEntryReleaseDir)\*.scc"
	
CreateDemoShieldInstall:
	@ECHO Copying Required installs
	@IF NOT EXIST "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\DotNet 4.0 Framework" MKDIR "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\DotNet 4.0 Framework"
	@IF NOT EXIST "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\SQLServerExpress2008" MKDIR "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\SQLServerExpress2008"
	@IF NOT EXIST "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\SQLServerExpress2008Mgr" MKDIR "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\SQLServerExpress2008Mgr"
	@IF NOT EXIST "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\Powershell" MKDIR "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\Powershell"
	@IF NOT EXIST "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\WindowsInstaller" MKDIR "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\WindowsInstaller"
	@IF NOT EXIST "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\SQLServer2008_SP1" MKDIR "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\SQLServer2008_SP1"
	@XCOPY "$(AFRequiredInstallsDir)\DotNet 4.0 Framework\*.*" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\DotNet 4.0 Framework" /v /s /e /y
	@XCOPY "$(AFRequiredInstallsDir)\DotNet 3.5 Framework\*.*" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\DotNet 3.5 Framework" /v /s /e /y
	@XCOPY "$(AFRequiredInstallsDir)\SQLServerExpress2008\*.*" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\SQLServerExpress2008" /v /s /e /y
	@XCOPY "$(AFRequiredInstallsDir)\SQLServer2008_SP1\*.*" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\SQLServer2008_SP1" /v /s /e /y
	@XCOPY "$(AFRequiredInstallsDir)\SQLServerExpress2008Mgr\*.*" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\SQLServerExpress2008Mgr" /v /s /e /y
	@XCOPY "$(AFRequiredInstallsDir)\WindowsInstaller\*.*" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\WindowsInstaller" /v /s /e /y
	@XCOPY "$(AFRequiredInstallsDir)\Powershell\*.*" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\Powershell" /v /s /e /y
	@COPY "$(CommonDirectory)\OSSI\PowerShell\OSSI.INI" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\Powershell"
	@COPY "$(CommonDirectory)\OSSI\WindowsInstaller\OSSI.INI" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\WindowsInstaller"
	@COPY "$(CommonDirectory)\OSSI\SQLServer\OSSI.INI" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\SQLServerExpress2008"
	@COPY "$(CommonDirectory)\OSSI\SQLServerMgr\OSSI.INI" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\SQLServerExpress2008Mgr"
	@COPY "$(CommonDirectory)\OSSI\DotNet 4.0 Framework\OSSI.INI" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\DotNet 4.0 Framework"
	@COPY "$(CommonDirectory)\OSSI\DotNet 3.5 Framework\OSSI.INI" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\DotNet 3.5 Framework"
	@COPY "$(BinariesFolder)\OSSI.EXE" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\DotNet 4.0 Framework"
	@COPY "$(BinariesFolder)\OSSI.EXE" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\Powershell"
	@COPY "$(BinariesFolder)\OSSI.EXE" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\WindowsInstaller"
	@COPY "$(BinariesFolder)\OSSI.EXE" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\SQLServerExpress2008"
	@COPY "$(BinariesFolder)\OSSI.EXE" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\SQLServerExpress2008Mgr"
	@ECHO Copying DemoShield Files
	@IF NOT EXIST "$(FlexIndexInstallDir)" MKDIR "$(FlexIndexInstallDir)"
	@XCOPY "$(DemoShieldRunFilesDir)\*.*" "$(FlexIndexInstallDir)" /v /s /e /y
	@COPY "$(AFRootDirectory)\Installation\FlexInstall\Launch.ini" "$(FlexIndexInstallDir)"
	@COPY "$(AFRootDirectory)\Installation\FlexInstall\FlexInstall.dbd" "$(FlexIndexInstallDir)"
		
BuildIDShieldInstall: CreateVersionISImportFile
    @ECHO Building Extract Systems IDShield installation...
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(ReusableComponentsRootDirectory)\APIs\Nuance_16.3\bin;$(ReusableComponentsRootDirectory)\APIs\LeadTools_16.5\Bin;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(ReusableComponentsRootDirectory)\APIs\Inlite_5_7\bin;$(WINDOWS_SDK)\BIN;C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319;$(VCPP_DIR)\VCPackages
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

GetAllFiles: GetPDCommonFiles GetAttributeFinderFiles GetRCdotNETFiles GetReusableComponentFiles GetPDUtilsFiles GetComponentDataFiles GetDemo_IDShieldRules GetDemo_FLEXIndexRules GetDataEntryInstall

DoBuilds: DisplayTimeStamp SetupBuildEnv BuildAttributeFinderCore

DoEverythingNoGet: DoBuilds CreateInstalls CopyComponentVersionFile DoDemos
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
