#############################################################################
# I N C L U D E S
#
# the common.mak file being included here counts on $(ProductRootDirName)
# ProductRootDirName defined as a non-null string
#

!include ComponentVersions.mak
!include ..\..\..\Common\Common.mak

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
LabDEBleedingEdgeDir=R:\FlexIndex\Internal\BleedingEdge\$(FlexIndexVersion)
RulesDir=$(EngineeringRootDirectory)\Rules

AFInstallRootDir=P:\AttributeFinder
AFCoreInstallFilesRootDir=$(AFInstallRootDir)\CoreInstallation\Files
DemoShieldRunFilesDir=$(AFInstallRootDir)\DemoShieldFiles

LabDEDir=$(PDRootDir)\DataEntry\LabDE
LabDEInstallRootDir=$(LabDEDir)\Installation
DataEntryInstallFiles=P:\DataEntry
DataEntryCoreInstallFilesDir=$(DataEntryInstallFiles)\CoreInstallation\Files
LabDEInstallFiles =$(DataEntryInstallFiles)\LabDE\Files
DataEntryInstallMediaDir=$(LabDEInstallRootDir)\DataEntry\Media\CD-ROM\DiskImages\DISK1
LabDEInstallMediaDir=$(LabDEInstallRootDir)\LabDE\Media\CD-ROM\DiskImages\DISK1
LabDEInstallDir=$(LabDEBleedingEdgeDir)\LabDEInstall

RDTInstallProjectRootDir=$(EngineeringRootDirectory)\ProductDevelopment\AttributeFinder\Installation\RuleDevelopmentKit
RDTInstallMediaDir=$(RDTInstallProjectRootDir)\Media\CD-ROM\DiskImages\Disk1

LabResultsDir=$(AFRootDirectory)\IndustrySpecific\LabResults
LabDERulesDir=$(RulesDir)\LabDE\Demo_LabDE\Rules

DataEntryApplicationDir=$(RCNETDir)\DataEntry\Utilities\DataEntryApplication\Core\Code
BinariesFolder=$(EngineeringRootDirectory)\Binaries\$(BuildOutputDir)
InternalUseBuildFilesArchive=P:\DataEntry\LabDE\Archive\InternalUseBuildFiles\InternalBuilds\$(FlexIndexVersion)

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
CopyFilesToInstallFolder:
	@ECHO Moving files to LabDE Installation
	@IF NOT EXIST "$(DataEntryCoreInstallFilesDir)\DotNet" @MKDIR "$(DataEntryCoreInstallFilesDir)\DotNet" 
	@IF NOT EXIST "$(DataEntryCoreInstallFilesDir)\Misc" @MKDIR "$(DataEntryCoreInstallFilesDir)\Misc" 
	@IF NOT EXIST "$(InternalUseBuildFilesArchive)" @MKDIR "$(InternalUseBuildFilesArchive)" 
	@IF NOT EXIST "$(LabDEInstallFiles)\Reports" @MKDIR "$(LabDEInstallFiles)\Reports"
	@IF NOT EXIST "$(LabDEInstallFiles)\NonSelfRegFiles" @MKDIR "$(LabDEInstallFiles)\NonSelfRegFiles"
	@IF NOT EXIST "$(DataEntryCoreInstallFilesDir)\NonSelfRegFiles" @MKDIR "$(DataEntryCoreInstallFilesDir)\NonSelfRegFiles"
	
	@DeleteFiles  "$(DataEntryCoreInstallFilesDir)\DotNet\*.*" /S
	@DeleteFiles "$(LabDEInstallFiles)\Reports\*.*" /S
	@DeleteFiles "$(LabDEInstallFiles)\NonSelfRegFiles\*.*" /S
	@COPY /V "$(ReusableComponentsRootDirectory)\APIs\LeadTools_17\Dotnet\leadtools*.dll" "$(DataEntryCoreInstallFilesDir)\DotNet"
	@COPY /v "$(BinariesFolder)\Obfuscated\*.dll" "$(DataEntryCoreInstallFilesDir)\DotNet" 
	@COPY /v "$(BinariesFolder)\Extract.DataEntry.DEP.StandardLabDE.dll" "$(DataEntryCoreInstallFilesDir)\DotNet" 
	@COPY /v "$(BinariesFolder)\DataEntryCC.dll" "$(DataEntryCoreInstallFilesDir)\DotNet" 
	@COPY /v "$(BinariesFolder)\Obfuscated\DataEntryApplication.exe" "$(DataEntryCoreInstallFilesDir)\DotNet" 
	@COPY /v "$(BinariesFolder)\Interop.*.dll" "$(DataEntryCoreInstallFilesDir)\DotNet" 
	@COPY /v "$(BinariesFolder)\Obfuscated\SqlCompactImporter.exe" "$(DataEntryCoreInstallFilesDir)\DotNet" 
	@COPY /v "$(BinariesFolder)\Obfuscated\SqlCompactExporter.exe" "$(DataEntryCoreInstallFilesDir)\DotNet" 
	@COPY /v "$(LabDEDir)\Misc\DisabledThemes.sdb" "$(DataEntryCoreInstallFilesDir)\Misc" 
	@COPY /v "$(LabDEDir)\Misc\DisabledThemes.sdb" "$(DataEntryCoreInstallFilesDir)\Misc" 
	@COPY /v "$(BinariesFolder)\DataEntryApplication.LabDE.resources" "$(LabDEInstallFiles)\NonSelfRegFiles"
	@XCOPY "$(LabDEDir)\Reports\*.*" "$(LabDEInstallFiles)\Reports" /v /s /e /y
# Make .nl files to register the COM .NET files
	DIR "$(DataEntryCoreInstallFilesDir)\DotNet\Extract.LabResultsCustomComponents.dll" /b >"$(DataEntryCoreInstallFilesDir)\NonSelfRegFiles\LabDE.nl"

CopyFilesForLabDEInstall: CopyFilesToInstallFolder 
	@ECHO Copying files for the LabDE Install
	@IF NOT EXIST "$(DataEntryCoreInstallFilesDir)\MergeModules" @MKDIR "$(DataEntryCoreInstallFilesDir)\MergeModules" 
	@DeleteFiles  "$(DataEntryCoreInstallFilesDir)\MergeModules\*.*"
	@COPY /v "$(DataEntryInstallMediaDir)\*.msm" "$(DataEntryCoreInstallFilesDir)\MergeModules"

BuildLabDEInstall: CopyFilesForLabDEInstall
    @ECHO Building Extract Systems LabDE Install...
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(ReusableComponentsRootDirectory)\APIs\Nuance_18\bin;$(ReusableComponentsRootDirectory)\APIs\LeadTools_17\Bin;;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(WINDOWS_SDK)\BIN;C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319;$(VCPP_DIR)\VCPackages;$(ReusableComponentsRootDirectory)\APIs\LeadTools_17\Dotnet
	$(SetProductVerScript) "$(LabDEInstallRootDir)\LabDE\LabDE.ism" "$(FlexIndexVersion)"
    @"$(DEV_STUDIO_DIR)\System\IsCmdBld.exe" -p "$(LabDEInstallRootDir)\LabDE\LabDE.ism"

CreateLabDEInstallCD: BuildLabDEInstall
	@ECHO Copying DataEntry Install files ...
    @IF NOT EXIST "$(LabDEBleedingEdgeDir)\LabDE" MKDIR "$(LabDEBleedingEdgeDir)\LabDE"
    @XCOPY "$(LabDEInstallMediaDir)\*.*" "$(LabDEBleedingEdgeDir)\LabDE" /v /s /e /y
    $(VerifyDir) "$(LabDEInstallMediaDir)" "$(LabDEBleedingEdgeDir)\LabDE"
	@COPY / v "$(LabDEInstallFiles)\InstallHelp\*.*" "$(LabDEBleedingEdgeDir)\LabDE"
    @DeleteFiles "$(LabDEBleedingEdgeDir)\LabDE\vssver.scc"

CreateDemoShieldInstall: CreateLabDEInstallCD 
	@ECHO Copying Required installs
	@IF NOT EXIST "$(LabDEBleedingEdgeDir)\Corepoint Integration Engine" MKDIR "$(LabDEBleedingEdgeDir)\Corepoint Integration Engine"
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
	@XCOPY "$(AFInstallRootDir)\Demo_LabDE\Sanitized\*.*" "$(LabDEBleedingEdgeDir)\Demo_LabDE\Input" /v /s /e /y
	@XCOPY "$(RulesDir)\LabDE\Demo_LabDE\Solution\*.*" "$(LabDEBleedingEdgeDir)\Demo_LabDE\Solution" /v /s /e /y	
	@COPY /v  "$(BinariesFolder)\Obfuscated\AlternateTestNameManager.plugin" "$(LabDEBleedingEdgeDir)\Demo_LabDE\Solution\Database Files"
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

DoEverything: DisplayTimeStamp SetupBuildEnv CreateDemoShieldInstall CreateDemo_LabDE
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @ECHO LabDE Build process completed.
    @ECHO.

