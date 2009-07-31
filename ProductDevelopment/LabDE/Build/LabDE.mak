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
RCNETDir=$(EngineeringRootDirectory)\RC.Net
LabDEBleedingEdgeDir=R:\LabDE\Internal\BleedingEdge\$(LabDEVersion)

AFInstallRootDir=P:\AttributeFinder
AFCoreInstallFilesRootDir=$(AFInstallRootDir)\CoreInstallation\Files
DemoShieldRunFilesDir=$(AFInstallRootDir)\DemoShieldFiles

LabDEDir=$(PDRootDir)\LabDE
LabDEInstallRootDir=$(LabDEDir)\Installation
DataEntryInstallFiles=P:\DataEntry
DataEntryCoreInstallFilesDir=$(DataEntryInstallFiles)\CoreInstallation\Files
LabDEInstallFiles =$(DataEntryInstallFiles)\LabDE\Files
DataEntryInstallMediaDir=$(LabDEInstallRootDir)\DataEntry\Media\CD-ROM\DiskImages\DISK1
LabDEInstallMediaDir=$(LabDEInstallRootDir)\LabDE\Media\CD-ROM\DiskImages\DISK1
LabDEObfuscationFilesArchive=$(DataEntryInstallFiles)\LabDE\Archive\ObfuscationFiles\$(LabDEVersion)
LabDEInstallDir=$(LabDEBleedingEdgeDir)\LabDEInstall

LabResultsDir=$(AFRootDirectory)\IndustrySpecific\LabResults
LabDERulesDir=$(LabResultsDir)\CustomerRules\Demo2\Rules

DataEntryApplicationDir=$(RCNETDir)\DataEntry\Utilities\DataEntryApplication\Core\Code
BinariesFolder=$(EngineeringRootDirectory)\Binaries\$(BuildOutputDir)

# determine the name of the release output directory based upon the build
# configuration that is being built
!IF "$(BuildConfig)" == "Release"
BuildOutputDir=Release
!ELSEIF "$(BuildConfig)" == "Debug"
BuildOutputDir=Debug
!ELSE
!ERROR Internal error - invalid value for BuildConfig variable!
!ENDIF

#############################################################################
# B U I L D    T A R G E T S
#
BuildAFCore: 
	@ECHO Building AttributeFinderCore...
	@CD "$(AFRootDirectory)\Build"
    @nmake /F AttributeFinderCore.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" ProductVersion="$(ProductVersion)" DoEverythingNoGet

BuildAFSDK:
	@ECHO Building AttributeFinderCore...
	@CD "$(AFRootDirectory)\Build"
    @nmake /F AttributeFinderSDK.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" ProductVersion="$(ProductVersion)" DoEverythingNoGet
    @nmake /F RuleDevelopmentKit.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" ProductVersion="$(ProductVersion)" DoEverythingNoGet

BuildLabDEApplication: BuildAFSDK
	@ECHO Building LabDE...
	@CD "$(LabDEDir)\Core\Code"
    @devenv LabDE.sln /BUILD $(BuildConfig) /USEENV

CopyComponentVersionFile:
	@ECHO Copying Conponent Version file...
    @COPY /v "$(PDRootDir)\Common\LatestComponentVersions.mak" "$(LabDEBleedingEdgeDir)\ComponentsVersions.txt"
	
ObfuscateFiles: BuildLabDEApplication
#	Copy the strong naming key to location dotfuscator xml file expects
	@IF NOT EXIST "$(AFCoreInstallFilesRootDir)\StrongNameKey" @MKDIR "$(AFCoreInstallFilesRootDir)\StrongNameKey"
	@DeleteFiles "$(AFCoreInstallFilesRootDir)\StrongNameKey\*.*"
	@COPY /V "$(RCNETDir)\Core\Code\ExtractInternalKey.snk" "$(AFCoreInstallFilesRootDir)\StrongNameKey"
	@IF NOT EXIST "$(BinariesFolder)\Obfuscated" @MKDIR "$(BinariesFolder)\Obfuscated"
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(ReusableComponentsRootDirectory)\APIs\Nuance_16\bin;$(ReusableComponentsRootDirectory)\APIs\LeadTools_16\bin;$(ReusableComponentsRootDirectory)\APIs\RogueWave\bin;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(VCPP_DIR)\PlatformSDK\bin;$(VISUAL_STUDIO)\SDK\v2.0\bin;C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727;$(VCPP_DIR)\VCPackages;C:\Program Files\PreEmptive Solutions\Dotfuscator Professional Edition 4.5;$(ReusableComponentsRootDirectory)\APIs\LeadTools_16\Dotnet
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.DataEntry.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.DataEntry.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Imaging.Forms.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Imaging.Forms.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\DataEntryApplication.exe" /mapout:"$(BinariesFolder)\Map\mapDataEntryApplication.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.LabDE.StandardLabDE.dll" /mapout:"$(BinariesFolder)\Map\mapEExtract.LabDE.StandardLabDE.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.LabResultsCustomComponents.dll" /mapout:"$(BinariesFolder)\Map\mapEExtract.LabResultsCustomComponents.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	
CopyFilesToInstallFolder: ObfuscateFiles
	@ECHO Moving files to LabDE Installation
	@IF NOT EXIST "$(DataEntryCoreInstallFilesDir)\DotNet" @MKDIR "$(DataEntryCoreInstallFilesDir)\DotNet" 
	@IF NOT EXIST "$(LabDEObfuscationFilesArchive)" @MKDIR "$(LabDEObfuscationFilesArchive)" 
	
	@DeleteFiles  "$(DataEntryCoreInstallFilesDir)\DotNet\*.*" /S
	@COPY /V "$(ReusableComponentsRootDirectory)\APIs\LeadTools_16\Dotnet\leadtools*.dll" "$(DataEntryCoreInstallFilesDir)\DotNet"
	@COPY /v "$(BinariesFolder)\Obfuscated\*.dll" "$(DataEntryCoreInstallFilesDir)\DotNet" 
	@COPY /v "$(BinariesFolder)\Obfuscated\DataEntryApplication.exe" "$(DataEntryCoreInstallFilesDir)\DotNet" 
	@COPY /v "$(BinariesFolder)\Interop.*.dll" "$(DataEntryCoreInstallFilesDir)\DotNet" 
	@XCOPY  "$(BinariesFolder)\Obfuscated\*.pdb" "$(LabDEObfuscationFilesArchive)" /Y/E
	@XCOPY  "$(BinariesFolder)\Map\*.xml" "$(LabDEObfuscationFilesArchive)" /Y/E

BuildDataEntryMergeModule: CreateVersionISImportFile CopyFilesToInstallFolder BuildLabDEApplication
    @ECHO Building Extract Systems DataEntry Merge Module...
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(ReusableComponentsRootDirectory)\APIs\Nuance_16\bin;$(ReusableComponentsRootDirectory)\APIs\LeadTools_16\Bin;$(ReusableComponentsRootDirectory)\APIs\RogueWave\bin;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(VCPP_DIR)\PlatformSDK\bin;$(VISUAL_STUDIO)\SDK\v2.0\bin;C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727;$(VCPP_DIR)\VCPackages;$(ReusableComponentsRootDirectory)\APIs\LeadTools_16\Dotnet
	$(SetProductVerScript) "$(LabDEInstallRootDir)\DataEntry\DataEntry.ism" "$(LabDEVersion)"
    @"$(DEV_STUDIO_DIR)\System\IsCmdBld.exe" -p "$(LabDEInstallRootDir)\DataEntry\DataEntry.ism"
	
CopyFilesForLabDEInstall: BuildDataEntryMergeModule
	@ECHO Copying files for the LabDE Install
	@IF NOT EXIST "$(DataEntryCoreInstallFilesDir)\MergeModules" @MKDIR "$(DataEntryCoreInstallFilesDir)\MergeModules" 
	@DeleteFiles  "$(DataEntryCoreInstallFilesDir)\MergeModules\*.*"
	@COPY /v "$(DataEntryInstallMediaDir)\*.msm" "$(DataEntryCoreInstallFilesDir)\MergeModules"

BuildLabDEInstall: CopyFilesForLabDEInstall
    @ECHO Building Extract Systems DataEntry Merge Module...
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(ReusableComponentsRootDirectory)\APIs\Nuance_16\bin;$(ReusableComponentsRootDirectory)\APIs\LeadTools_16\Bin;$(ReusableComponentsRootDirectory)\APIs\RogueWave\bin;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(VCPP_DIR)\PlatformSDK\bin;$(VISUAL_STUDIO)\SDK\v2.0\bin;C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727;$(VCPP_DIR)\VCPackages;$(ReusableComponentsRootDirectory)\APIs\LeadTools_16\Dotnet
	$(SetProductVerScript) "$(LabDEInstallRootDir)\LabDE\LabDE.ism" "$(LabDEVersion)"
    @"$(DEV_STUDIO_DIR)\System\IsCmdBld.exe" -p "$(LabDEInstallRootDir)\LabDE\LabDE.ism"

CreateLabDEInstallCD: BuildLabDEInstall
	@ECHO Copying DataEntry Install files ...
    @IF NOT EXIST "$(LabDEBleedingEdgeDir)\LabDE" MKDIR "$(LabDEBleedingEdgeDir)\LabDE"
    @XCOPY "$(LabDEInstallMediaDir)\*.*" "$(LabDEBleedingEdgeDir)\LabDE" /v /s /e /y
    $(VerifyDir) "$(LabDEInstallMediaDir)" "$(LabDEBleedingEdgeDir)\LabDE"
	@COPY / v "$(LabDEInstallFiles)\InstallHelp\*.*" "$(LabDEBleedingEdgeDir)\LabDE"
    @DeleteFiles "$(LabDEBleedingEdgeDir)\LabDE\vssver.scc"

CopyLMFolderToInstall:
	@ECHO Creating License Manager Install...
	@CD "$(ReusableComponentsRootDirectory)\VendorSpecificUtils\SafeNetUtils\Build"
    @nmake /F LicenseManager.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" ProductVersion="$(ProductVersion)" ProductInstallFolder="$(LabDEBleedingEdgeDir)"  CopyLMInstallToProductInstallFolder
	
CreateDemoShieldInstall: CopyLMFolderToInstall CreateLabDEInstallCD 
	@ECHO Copying Required installs
	@IF NOT EXIST "$(LabDEBleedingEdgeDir)\DotNet 3.5 Framework" MKDIR "$(LabDEBleedingEdgeDir)\DotNet 3.5 Framework"
	@IF NOT EXIST "$(LabDEBleedingEdgeDir)\SQLServerExpress2005" MKDIR "$(LabDEBleedingEdgeDir)\SQLServerExpress2005"
	@IF NOT EXIST "$(LabDEBleedingEdgeDir)\Corepoint Integration Engine" MKDIR "$(LabDEBleedingEdgeDir)\Corepoint Integration Engine"
	@XCOPY "$(AFInstallRootDir)\RequiredInstalls\DotNet 3.5 Framework\*.*" "$(LabDEBleedingEdgeDir)\DotNet 3.5 Framework" /v /s /e /y
	@XCOPY "$(AFInstallRootDir)\RequiredInstalls\SQLServerExpress2005\*.*" "$(LabDEBleedingEdgeDir)\SQLServerExpress2005" /v /s /e /y
	@XCOPY "$(DataEntryInstallFiles)\RequiredInstalls\Corepoint Integration Engine\*.*" "$(LabDEBleedingEdgeDir)\Corepoint Integration Engine" /v /s /e /y
	@ECHO Copying DemoShield Files
	@XCOPY "$(DemoShieldRunFilesDir)\*.*" "$(LabDEInstallDir)" /v /s /e /y
	@IF NOT EXIST "$(LabDEInstallDir)" MKDIR "$(LabDEInstallDir)"
	@COPY "$(LabDEInstallRootDir)\LabDEInstall\Launch.ini" "$(LabDEInstallDir)"
	@COPY "$(LabDEInstallRootDir)\LabDEInstall\LabDEInstall.dbd" "$(LabDEInstallDir)"

CreateDemo_LabDE: 
	@ECHO Copying Demo_LabDE files...
	@XCOPY "$(LabResultsDir)\Utils\LabDEDemo\Files\*.*" "$(LabDEBleedingEdgeDir)\Demo_LabDE" /v /s /e /y
	@XCOPY "$(AFInstallRootDir)\Demo_LabDE\Sanitized\*.*" "$(LabDEBleedingEdgeDir)\Demo_LabDE\DemoFiles\Installs\LongDemo\TIF" /v /s /e /y
	@COPY /v "$(BinariesFolder)\Obfuscated\Extract.LabDE.StandardLabDE.dll" "$(LabDEBleedingEdgeDir)\Demo_LabDE\Solution\Bin"
	@COPY /v "$(LabResultsDir)\Demo2\*.config" "$(LabDEBleedingEdgeDir)\Demo_LabDE\Solution\"
	@COPY /v "$(LabResultsDir)\Corepoint Integration\*.nix" "$(LabDEBleedingEdgeDir)\Demo_LabDE\Solution\Corepoint Integration"
	@COPY /v "$(LabResultsDir)\Miscellaneous\*.*" "$(LabDEBleedingEdgeDir)\Demo_LabDE\Solution\Database Files"
	@XCOPY "$(LabDERulesDir)\*.*" "$(LabDEBleedingEdgeDir)\Demo_LabDE\Solution\Rules" /v /s /e /y
	@ECHO Encrypting LabDE Demo Rules...
	@SendFilesAsArgumentToApplication "$(LabDEBleedingEdgeDir)\Demo_LabDE\Solution\Rules\*.dat" 1 1 "$(BinariesFolder)\EncryptFile.exe"
	@SendFilesAsArgumentToApplication "$(LabDEBleedingEdgeDir)\Demo_LabDE\Solution\Rules\*.rsd" 1 1 "$(BinariesFolder)\EncryptFile.exe"
	@SendFilesAsArgumentToApplication "$(LabDEBleedingEdgeDir)\Demo_LabDE\Solution\Rules\*.dcc" 1 1 "$(BinariesFolder)\EncryptFile.exe"
	@SendFilesAsArgumentToApplication "$(LabDEBleedingEdgeDir)\Demo_LabDE\Solution\Rules\*.spm" 1 1 "$(BinariesFolder)\EncryptFile.exe"
    @DeleteFiles "$(LabDEBleedingEdgeDir)\Demo_LabDE\Solution\Rules\*.dat"
    @DeleteFiles "$(LabDEBleedingEdgeDir)\Demo_LabDE\Solution\Rules\*.rsd"
    @DeleteFiles "$(LabDEBleedingEdgeDir)\Demo_LabDE\Solution\Rules\*.dcc"
	@DeleteFiles "$(LabDEBleedingEdgeDir)\Demo_LabDE\Solution\Rules\*.spm"
    @DeleteFiles "$(LabDEBleedingEdgeDir)\Demo_LabDE\Solution\Rules\vssver.scc"

GetAllFiles: GetPDCommonFiles GetAttributeFinderFiles GetRCdotNETFiles GetReusableComponentFiles GetPDUtilsFiles GetLabDEFiles

DoEverythingNoGet: DisplayTimeStamp SetupBuildEnv BuildDataEntryMergeModule  BuildLabDEApplication CreateDemoShieldInstall CopyComponentVersionFile CreateDemo_LabDE
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @ECHO LabDE Build process completed.
    @ECHO.

DoEverything: DisplayTimeStamp SetupBuildEnv GetAllFiles DoEverythingNoGet
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @ECHO LabDE Build process completed.
    @ECHO.
	
