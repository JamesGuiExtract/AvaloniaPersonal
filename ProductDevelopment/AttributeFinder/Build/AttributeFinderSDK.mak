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

MergeModuleRootDir=$(INSTALLSHIELD_PROJECTS_DIR)\MergeModules
FlexIndexSDKInstallRootDir=$(PDRootDir)\AttributeFinder\Installation\UCLID FlexIndex SDK
FlexIndexSDKInstallMediaDir=$(FlexIndexSDKInstallRootDir)\Media\CD-ROM\DiskImages\DISK1

VOAClientInstallRootDir=$(PDRootDir)\AttributeFinder\Installation\UCLID VOAClient
VOAClientInstallMediaDir=$(VOAClientInstallRootDir)\Media\CD-ROM\DiskImages\DISK1

ExtractLMInstallRootDir=$(ReusableComponentsRootDirectory)\VendorSpecificUtils\SafeNetUtils\Installation\Extract Systems LM
ExtractLMInstallMediaDir=$(ExtractLMInstallRootDir)\Media\CD-ROM\DiskImages\DISK1

IDShieldInstallRootDir=$(PDRootDir)\AttributeFinder\IndustrySpecific\Redaction\Installation\IDShield
IDShieldInstallMediaDir=$(IDShieldInstallRootDir)\Media\CD-ROM\DiskImages\DISK1

RedactionDemoBuildDir=$(AFRootDirectory)\Utils\RedactionDemo\Build

NetDMSRootDir=$(PDRootDir)\AFIntegrations\NetDMS

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
BuildAttributeFinderCore:
	@Echo Building AttributeFinderCore...
	@CD "$(AFRootDirectory)\Build"
    @nmake /F AttributeFinderCore.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" ProductVersion="$(FlexIndexVersion)" $(AttributeCoreTarget)
	
CopyFilesToInstallFolder: 
    @ECHO Copying the AttributeFinder files to installation directory...
	@IF NOT EXIST "$(AFInstallFilesRootDir)\Redist" @MKDIR "$(AFInstallFilesRootDir)\Redist"
	@DeleteFiles "$(AFInstallFilesRootDir)\Redist"\*.*" /S /Q
    @COPY /v "$(MergeModuleRootDir)\ExtractCommonMM.msm" "$(AFInstallFilesRootDir)\Redist"
    @COPY /v "$(MergeModuleRootDir)\UCLIDFlexIndex.msm" "$(AFInstallFilesRootDir)\Redist"
    @COPY /v "$(MergeModuleRootDir)\DataEntry.msm" "$(AFInstallFilesRootDir)\Redist"
	@COPY /v "$(MergeModuleRootDir)\ExtractFlexCommonMM.msm" "$(AFInstallFilesRootDir)\Redist"
    @DeleteFiles "$(AFInstallFilesRootDir)\vssver.scc"
    @DeleteFiles "$(AFInstallFilesRootDir)\mssccprj.scc"

CopyComponentVersionFile:
	@ECHO Copying Component Version file...
    @COPY /v "$(PDRootDir)\Common\LatestComponentVersions.mak" "$(AFReleaseBleedingEdgeDir)\ComponentsVersions.txt"

BuildFlexIndexSDKInstall: BuildAttributeFinderCore CopyFilesToInstallFolder
    @ECHO Building UCLID FlexIndex SDK installation...
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(NUANCE_API_DIR);$(LEADTOOLS_API_DIR);$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DevEnvDir);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(WINDOWS_SDK)\BIN;C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319;$(VCPP_DIR)\VCPackages
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
    @nmake /F LicenseManager.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" ProductVersion="$(FlexIndexVersion)" CreateFlexLMInstall
    
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
	@IF NOT EXIST "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\DotNet 3.5 Framework" MKDIR "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\DotNet 3.5 Framework"
	@IF NOT EXIST "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\SQLServerExpress2014" MKDIR "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\SQLServerExpress2014"
	@IF NOT EXIST "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\SQLServerExpress2014Mgr" MKDIR "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\SQLServerExpress2014Mgr"
	@IF NOT EXIST "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\Powershell" MKDIR "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\Powershell"
	@IF NOT EXIST "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\WindowsInstaller" MKDIR "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\WindowsInstaller"
	@XCOPY "$(AFRequiredInstallsDir)\DotNet 4.0 Framework\*.*" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\DotNet 4.0 Framework" /v /s /e /y
	@XCOPY "$(AFRequiredInstallsDir)\DotNet 3.5 Framework\*.*" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\DotNet 3.5 Framework" /v /s /e /y
	@XCOPY "$(AFRequiredInstallsDir)\SQLServerExpress2014\*.*" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\SQLServerExpress2014" /v /s /e /y
	@XCOPY "$(AFRequiredInstallsDir)\SQLServerExpress2014Mgr\*.*" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\SQLServerExpress2014Mgr" /v /s /e /y
	@XCOPY "$(AFRequiredInstallsDir)\WindowsInstaller\*.*" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\WindowsInstaller" /v /s /e /y
	@XCOPY "$(AFRequiredInstallsDir)\Powershell\*.*" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\Powershell" /v /s /e /y
	@COPY "$(CommonDirectory)\OSSI\PowerShell\OSSI.INI" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\Powershell"
	@COPY "$(CommonDirectory)\OSSI\WindowsInstaller\OSSI.INI" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\WindowsInstaller"
	@COPY "$(CommonDirectory)\OSSI\SQLServer\OSSI.INI" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\SQLServerExpress2014"
	@COPY "$(CommonDirectory)\OSSI\SQLServerMgr\OSSI.INI" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\SQLServerExpress2014Mgr"
	@COPY "$(CommonDirectory)\OSSI\DotNet 4.0 Framework\OSSI.INI" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\DotNet 4.0 Framework"
	@COPY "$(CommonDirectory)\OSSI\DotNet 3.5 Framework\OSSI.INI" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\DotNet 3.5 Framework"
	@COPY "$(BinariesFolder)\OSSI.EXE" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\DotNet 4.0 Framework"
	@COPY "$(BinariesFolder)\OSSI.EXE" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\Powershell"
	@COPY "$(BinariesFolder)\OSSI.EXE" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\WindowsInstaller"
	@COPY "$(BinariesFolder)\OSSI.EXE" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\SQLServerExpress2014"
	@COPY "$(BinariesFolder)\OSSI.EXE" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\SQLServerExpress2014Mgr"
	@COPY "$(BinariesFolder)\OSSI.EXE" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\DotNet 3.5 Framework"
	@ECHO Copying DemoShield Files
	@IF NOT EXIST "$(FlexIndexInstallDir)" MKDIR "$(FlexIndexInstallDir)"
	@XCOPY "$(DemoShieldRunFilesDir)\*.*" "$(FlexIndexInstallDir)" /v /s /e /y
	@COPY "$(AFRootDirectory)\Installation\FlexInstall\Launch.ini" "$(FlexIndexInstallDir)"
	@COPY "$(AFRootDirectory)\Installation\FlexInstall\FlexInstall.dbd" "$(FlexIndexInstallDir)"
		
BuildIDShieldInstall: 
    @ECHO Building Extract Systems IDShield installation...
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(NUANCE_API_DIR);$(LEADTOOLS_API_DIR);$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(WINDOWS_SDK)\BIN;C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319;$(VCPP_DIR)\VCPackages
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
	@nmake /F $(RedactionDemoBuildDir)\RedactionDemo.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" ProductVersion="$(FlexIndexVersion)" DoEverything
	
CreateOtherDemos:
	@ECHO Creating other demos...
	@IF NOT EXIST "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\Other Demos" MKDIR "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\Other Demos"
	@XCOPY  "$(AFRootDirectory)\Utils\Demo_RedactionGame\*.*" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\Other Demos" /v /s /e /y

CreateLabDEInstall:
	@Echo Building LabDE...
	@CD "$(LabDEBuildDir)"
    @nmake /F LabDE.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" ProductVersion="$(LabDEVersion)" DoEverything
	
CreateNetDMSInstall:
	@Echo Creating NetDMS install...
	@IF NOT EXIST "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\NetDMSIntegrationInstall" MKDIR "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\NetDMSIntegrationInstall"
	@COPY "$(BinariesFolder)\Obfuscated\Extract.NetDMSExporter.dll" "$(NetDMSRootDir)\NetDMSIntegrationInstall\Exporter"
	@COPY "$(BinariesFolder)\Obfuscated\Extract.NetDMSUtilities.dll" "$(NetDMSRootDir)\NetDMSIntegrationInstall\ProgramFiles"
	@COPY "$(BinariesFolder)\Obfuscated\Extract.NetDMSCustomComponents.dll" "$(NetDMSRootDir)\NetDMSIntegrationInstall\ProgramFiles"
	@COPY "$(BinariesFolder)\Interop.Weak.*.dll" "$(NetDMSRootDir)\NetDMSIntegrationInstall\ProgramFiles"
	@XCOPY "$(NetDMSRootDir)\NetDMSIntegrationInstall\*.*" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\NetDMSIntegrationInstall" /v /s /e /y
	
BuildExtractUninstaller:
	@ECHO Creating ExtractUninstaller...
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(NUANCE_API_DIR);$(LEADTOOLS_API_DIR);;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(WINDOWS_SDK)\BIN;C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319;$(VCPP_DIR)\VCPackages;$(ReusableComponentsRootDirectory)\APIs\LeadTools_17\Dotnet
	$(SetProductVerScript) "$(CommonDirectory)\ExtractUninstaller\ExtractUninstaller.ism" "$(FlexIndexVersion)"
    @"$(DEV_STUDIO_DIR)\System\IsCmdBld.exe" -p "$(CommonDirectory)\ExtractUninstaller\ExtractUninstaller.ism"
	
CreateExtractUninstallerFolder: BuildExtractUninstaller
	@ECHO Copying ExtractUninstaller...
	@IF NOT EXIST "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\ExtractUninstaller" MKDIR "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\ExtractUninstaller"
	@XCOPY "$(CommonDirectory)\ExtractUninstaller\Media\CDROM\DiskImages\DISK1\*.*" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\ExtractUninstaller" /v /s /e /y
	
CopySilentInstallsDir:
	@ECHO Copying SilentInstalls folder
	@IF NOT EXIST "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\SilentInstalls" MKDIR "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\SilentInstalls"
	@XCOPY "$(AFRootDirectory)\SilentInstalls\*.*" "$(AFBleedingEdgeDir)\$(FlexIndexVersion)\SilentInstalls"
	
CreateSharepointInstall:
	@Echo Creating Sharepoint Installs...
	@CD $(PDRootDir)\AFIntegrations\Sharepoint\Build
	@nmake /F FlexIDSSP.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" ProductVersion="$(FlexIndexVersion)" BuildAfterAF
	@CD \Engineering\ProductDevelopment\AttributeFinder\Build
	
UpdateLicenseFiles:
	@IF "$(Branch)"=="" (
		@Echo Updating Licensing Files...
		@XCOPY "$(ReusableComponentsRootDirectory)\COMComponents\UCLIDComponentsLM\COMLMCore\Code\*.dat" "$(BinariesFolder)"
		@Copy "$(BinariesFolder)\Components.dat" "$(DeveloperLicensing)"
		@Copy "$(BinariesFolder)\Packages.dat" "$(DeveloperLicensing)"
		$(Replace) "$(BinariesFolder)\Packages.dat" ".*<DevOnly>.*\r?\n" "" /e
		$(Replace) "$(BinariesFolder)\Components.dat" ".*<DevOnly>.*\r?\n" "" /e
		@Copy "$(BinariesFolder)\Components.dat" "$(RuleWriterLicensing)"
		@Copy "$(BinariesFolder)\Packages.dat" "$(RuleWriterLicensing)"
		$(Replace) "$(BinariesFolder)\Packages.dat" ".*<RWOnly>.*\r?\n" "" /e
		$(Replace) "$(BinariesFolder)\Components.dat" ".*<RWOnly>.*\r?\n" "" /e
		@Copy "$(BinariesFolder)\Components.dat" "$(SupportLicensing)"
		@Copy "$(BinariesFolder)\Packages.dat" "$(SupportLicensing)"
	)
	
CreateInstalls: BuildIDShieldInstall CreateAttributeFinderInstallCD CreateExtractLMInstallCD  CreateIDShieldInstallCD CreateDemoShieldInstall CreateLabDEInstall CreateNetDMSInstall CopySilentInstallsDir CreateSharepointInstall

DoDemos:CreateFlexDataEntryInstallDir CreateRedactionDemoInstall CreateOtherDemos

GetAllFiles: GetEngineering

DoBuilds: DisplayTimeStamp SetupBuildEnv BuildAttributeFinderCore

DoEverythingNoGet: DoBuilds CreateInstalls CopyComponentVersionFile DoDemos UpdateLicenseFiles
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
