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
RulesDir=$(EngineeringRootDirectory)\Rules

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
LabDEInstallDir=$(LabDEBleedingEdgeDir)\LabDEInstall

RDTInstallProjectRootDir=$(EngineeringRootDirectory)\ProductDevelopment\AttributeFinder\Installation\RuleDevelopmentKit
RDTInstallMediaDir=$(RDTInstallProjectRootDir)\Media\CD-ROM\DiskImages\Disk1
RDTReleaseBleedingEdgeDir=S:\LabDE\Internal\BleedingEdge\$(LabDEVersion)\RDT

LabResultsDir=$(AFRootDirectory)\IndustrySpecific\LabResults
LabDERulesDir=$(RulesDir)\LabDE\Demo_LabDE\Rules

DataEntryApplicationDir=$(RCNETDir)\DataEntry\Utilities\DataEntryApplication\Core\Code
BinariesFolder=$(EngineeringRootDirectory)\Binaries\$(BuildOutputDir)
InternalUseBuildFilesArchive=P:\DataEntry\LabDE\Archive\InternalUseBuildFiles\InternalBuilds\$(LabDEVersion)

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

BuildRDT: BuildAFCore
	@ECHO Building AttributeFinderCore...
	@CD "$(AFRootDirectory)\Build"
    @nmake /F RuleDevelopmentKit.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" ProductVersion="$(ProductVersion)" BuildRDTInstall

CopyRDTToInstallFolder: BuildRDT
    @IF NOT EXIST "$(RDTReleaseBleedingEdgeDir)" MKDIR "$(RDTReleaseBleedingEdgeDir)"
    @XCOPY "$(RDTInstallMediaDir)\*.*" "$(RDTReleaseBleedingEdgeDir)" /v /s /e /y
    $(VerifyDir) "$(RDTInstallMediaDir)" "$(RDTReleaseBleedingEdgeDir)"
    @DeleteFiles "$(RDTReleaseBleedingEdgeDir)\vssver.scc"

BuildLabDEApplication: BuildRDT
	@ECHO Building LabDE...
	@CD "$(LabDEDir)\Core\Code"
    @devenv LabDE.sln /BUILD $(BuildConfig) /USEENV

ObfuscateFiles: BuildLabDEApplication
#	Copy the strong naming key to location dotfuscator xml file expects
	@IF NOT EXIST "$(AFCoreInstallFilesRootDir)\StrongNameKey" @MKDIR "$(AFCoreInstallFilesRootDir)\StrongNameKey"
	@DeleteFiles "$(AFCoreInstallFilesRootDir)\StrongNameKey\*.*"
	@COPY /V "$(RCNETDir)\Core\Code\ExtractInternalKey.snk" "$(AFCoreInstallFilesRootDir)\StrongNameKey"
	@IF NOT EXIST "$(BinariesFolder)\Obfuscated" @MKDIR "$(BinariesFolder)\Obfuscated"
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(ReusableComponentsRootDirectory)\APIs\Nuance_16.3\bin;$(ReusableComponentsRootDirectory)\APIs\LeadTools_16.5\bin;;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(WINDOWS_SDK)\BIN;C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319;$(VCPP_DIR)\VCPackages;C:\Program Files\PreEmptive Solutions\Dotfuscator Professional Edition 4.7;$(ReusableComponentsRootDirectory)\APIs\LeadTools_16.5\Dotnet
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.DataEntry.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.DataEntry.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Imaging.Forms.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Imaging.Forms.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\DataEntryApplication.exe" /mapout:"$(BinariesFolder)\Map\mapDataEntryApplication.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.LabResultsCustomComponents.dll" /mapout:"$(BinariesFolder)\Map\mapEExtract.LabResultsCustomComponents.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\SqlCompactImporter.exe" /mapout:"$(BinariesFolder)\Map\mapSqlCompactImporter.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\SqlCompactExporter.exe" /mapout:"$(BinariesFolder)\Map\SqlCompactExporter.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(AFRootDirectory)\Build\ObfuscateConfig.xml
	
CopyFilesToInstallFolder: ObfuscateFiles
	@ECHO Moving files to LabDE Installation
	@IF NOT EXIST "$(DataEntryCoreInstallFilesDir)\DotNet" @MKDIR "$(DataEntryCoreInstallFilesDir)\DotNet" 
	@IF NOT EXIST "$(DataEntryCoreInstallFilesDir)\Misc" @MKDIR "$(DataEntryCoreInstallFilesDir)\DotNet\Misc" 
	@IF NOT EXIST "$(InternalUseBuildFilesArchive)" @MKDIR "$(InternalUseBuildFilesArchive)" 
	@IF NOT EXIST "$(LabDEInstallFiles)\Reports" @MKDIR "$(LabDEInstallFiles)\Reports"
	
	@DeleteFiles  "$(DataEntryCoreInstallFilesDir)\DotNet\*.*" /S
	@DeleteFiles "$(LabDEInstallFiles)\Reports\*.*" /S
	@COPY /V "$(ReusableComponentsRootDirectory)\APIs\LeadTools_16.5\Dotnet\leadtools*.dll" "$(DataEntryCoreInstallFilesDir)\DotNet"
	@COPY /v "$(BinariesFolder)\Obfuscated\*.dll" "$(DataEntryCoreInstallFilesDir)\DotNet" 
	@COPY /v "$(BinariesFolder)\Extract.DataEntry.DEP.StandardLabDE.dll" "$(DataEntryCoreInstallFilesDir)\DotNet" 
	@COPY /v "$(BinariesFolder)\DataEntryCC.dll" "$(DataEntryCoreInstallFilesDir)\DotNet" 
	@COPY /v "$(BinariesFolder)\Obfuscated\DataEntryApplication.exe" "$(DataEntryCoreInstallFilesDir)\DotNet" 
	@COPY /v "$(BinariesFolder)\Interop.*.dll" "$(DataEntryCoreInstallFilesDir)\DotNet" 
	@COPY /v "$(BinariesFolder)\Obfuscated\SqlCompactImporter.exe" "$(DataEntryCoreInstallFilesDir)\DotNet" 
	@COPY /v "$(BinariesFolder)\Obfuscated\SqlCompactExporter.exe" "$(DataEntryCoreInstallFilesDir)\DotNet" 
	@COPY /v "$(LabDEDir)\Misc\DisabledThemes.sdb" "$(DataEntryCoreInstallFilesDir)\Misc" 
	@COPY /v "$(LabDEDir)\Reports\*.*" "$(LabDEInstallFiles)\Reports"
# Copy pdb and map files to archive
	@COPY  "$(BinariesFolder)\*.pdb" "$(InternalUseBuildFilesArchive)" 
	@COPY  "$(BinariesFolder)\Obfuscated\*.pdb" "$(InternalUseBuildFilesArchive)" 
	@COPY  "$(BinariesFolder)\Map\*.xml" "$(InternalUseBuildFilesArchive)" 
# Make .nl files to register the COM .NET files
	DIR "$(DataEntryCoreInstallFilesDir)\DotNet\Extract.LabResultsCustomComponents.dll" /b >"$(DataEntryCoreInstallFilesDir)\NonSelfRegisteredFiles\DataEntry.nl"
	DIR "$(DataEntryCoreInstallFilesDir)\DotNet\Extract.DataEntry.dll" /b >>"$(DataEntryCoreInstallFilesDir)\NonSelfRegisteredFiles\DataEntry.nl"
	DIR "$(DataEntryCoreInstallFilesDir)\DotNet\DataEntryApplication.exe" /b >>"$(DataEntryCoreInstallFilesDir)\NonSelfRegisteredFiles\DataEntry.nl"
	DIR "$(DataEntryCoreInstallFilesDir)\DotNet\DataEntryCC.dll" /b >"$(DataEntryCoreInstallFilesDir)\NonSelfRegisteredFiles\DataEntry.rl"

CopyFilesForLabDEInstall: CopyFilesToInstallFolder 
	@ECHO Copying files for the LabDE Install
	@IF NOT EXIST "$(DataEntryCoreInstallFilesDir)\MergeModules" @MKDIR "$(DataEntryCoreInstallFilesDir)\MergeModules" 
	@DeleteFiles  "$(DataEntryCoreInstallFilesDir)\MergeModules\*.*"
	@COPY /v "$(DataEntryInstallMediaDir)\*.msm" "$(DataEntryCoreInstallFilesDir)\MergeModules"

BuildLabDEInstall: CopyFilesForLabDEInstall
    @ECHO Building Extract Systems LabDE Install...
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(ReusableComponentsRootDirectory)\APIs\Nuance_16.3\bin;$(ReusableComponentsRootDirectory)\APIs\LeadTools_16.5\Bin;;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(WINDOWS_SDK)\BIN;C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319;$(VCPP_DIR)\VCPackages;$(ReusableComponentsRootDirectory)\APIs\LeadTools_16.5\Dotnet
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
    @nmake /F LicenseManager.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" ProductVersion="$(ProductVersion)" ProductInstallFolder="$(LabDEBleedingEdgeDir)\Extract Systems LM"  CopyLMInstallToProductInstallFolder
	
CreateDemoShieldInstall: CopyLMFolderToInstall CreateLabDEInstallCD 
	@ECHO Copying Required installs
	@IF NOT EXIST "$(LabDEBleedingEdgeDir)\DotNet 3.5 Framework" MKDIR "$(LabDEBleedingEdgeDir)\DotNet 3.5 Framework"
	@IF NOT EXIST "$(LabDEBleedingEdgeDir)\SQLServerExpress2005" MKDIR "$(LabDEBleedingEdgeDir)\SQLServerExpress2005"
	@IF NOT EXIST "$(LabDEBleedingEdgeDir)\Corepoint Integration Engine" MKDIR "$(LabDEBleedingEdgeDir)\Corepoint Integration Engine"
	@XCOPY "$(AFInstallRootDir)\RequiredInstalls\DotNet 3.5 Framework\*.*" "$(LabDEBleedingEdgeDir)\DotNet 3.5 Framework" /v /s /e /y
	@XCOPY "$(AFInstallRootDir)\RequiredInstalls\SQLServerExpress2005\*.*" "$(LabDEBleedingEdgeDir)\SQLServerExpress2005" /v /s /e /y
	@XCOPY "$(DataEntryInstallFiles)\RequiredInstalls\Corepoint Integration Engine\*.*" "$(LabDEBleedingEdgeDir)\Corepoint Integration Engine" /v /s /e /y
	@ECHO Copying DemoShield Files
	@IF NOT EXIST "$(LabDEInstallDir)" MKDIR "$(LabDEInstallDir)"
	@XCOPY "$(DemoShieldRunFilesDir)\*.*" "$(LabDEInstallDir)" /v /s /e /y
	@COPY "$(LabDEInstallRootDir)\LabDEInstall\Launch.ini" "$(LabDEInstallDir)"
	@COPY "$(LabDEInstallRootDir)\LabDEInstall\LabDEInstall.dbd" "$(LabDEInstallDir)"
	@COPY "$(LabDEDir)\DEPs\StandardLabDE\Core\Code\Resources\LabDE.ico" "$(LabDEInstallDir)"
	@COPY "$(LabDEInstallRootDir)\LabDEInstall\autorun.inf" "$(LabDEBleedingEdgeDir)"

CreateDemo_LabDE: 
	@ECHO Copying Demo_LabDE files...
    @IF NOT EXIST "$(LabDEBleedingEdgeDir)\Demo_LabDE\Solution\Rules" MKDIR "$(LabDEBleedingEdgeDir)\Demo_LabDE\Solution\Rules"
    @IF NOT EXIST "$(LabDEBleedingEdgeDir)\Demo_LabDE\Solution\Corepoint Integration" MKDIR "$(LabDEBleedingEdgeDir)\Demo_LabDE\Solution\Corepoint Integration"
	@IF NOT EXIST "$(LabDEBleedingEdgeDir)\Demo_LabDE\Solution\Database Files" MKDIR "$(LabDEBleedingEdgeDir)\Demo_LabDE\Solution\Database Files"
	@XCOPY "$(LabResultsDir)\Utils\LabDEDemo\Files\*.*" "$(LabDEBleedingEdgeDir)\Demo_LabDE" /v /s /e /y
	@XCOPY "$(AFInstallRootDir)\Demo_LabDE\Sanitized\*.*" "$(LabDEBleedingEdgeDir)\Demo_LabDE\DemoFiles\Installs\LongDemo\TIF" /v /s /e /y
	@XCOPY "$(RulesDir)\LabDE\Demo_LabDE\Solution\*.*" "$(LabDEBleedingEdgeDir)\Demo_LabDE\Solution" /v /s /e /y	
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

GetAllFiles: GetPDCommonFiles GetAttributeFinderFiles GetRCdotNETFiles GetReusableComponentFiles GetPDUtilsFiles GetComponentDataFiles GetLabDEFiles GetDemo_LabDERules

DoEverythingNoGet: DisplayTimeStamp SetupBuildEnv BuildLabDEApplication CreateDemoShieldInstall CopyRDTToInstallFolder CreateDemo_LabDE
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
	
