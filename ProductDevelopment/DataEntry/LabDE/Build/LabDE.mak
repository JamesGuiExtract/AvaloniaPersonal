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
RulesDir=$(EngineeringRootDirectory)\Rules

AFInstallRootDir=P:\AttributeFinder
AFCoreInstallFilesRootDir=$(AFInstallRootDir)\CoreInstallation\Files
DemoShieldRunFilesDir=$(AFInstallRootDir)\DemoShieldFiles

LabDEDir=$(PDRootDir)\DataEntry\LabDE
LabDEInstallRootDir=$(LabDEDir)\Installation
DataEntryInstallFiles=P:\DataEntry
DataEntryCoreInstallFilesDir=$(DataEntryInstallFiles)\CoreInstallation\Files
LabDEInstallBuildFiles =$(DataEntryInstallFiles)\LabDE\Files
DataEntryInstallMediaDir=$(LabDEInstallRootDir)\DataEntry\Media\CD-ROM\DiskImages\DISK1
LabDEInstallMediaDir=$(LabDEInstallRootDir)\LabDE\Media\CD-ROM\DiskImages\DISK1

RDTInstallProjectRootDir=$(EngineeringRootDirectory)\ProductDevelopment\AttributeFinder\Installation\RuleDevelopmentKit
RDTInstallMediaDir=$(RDTInstallProjectRootDir)\Media\CD-ROM\DiskImages\Disk1

LabResultsDir=$(AFRootDirectory)\IndustrySpecific\LabResults
Demo_LabDE_DEP=$(RulesDir)\LabDE\Demo_LabDE\Demo_LabDE_DEP
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
	@IF NOT EXIST "$(LabDEInstallBuildFiles)\Reports" @MKDIR "$(LabDEInstallBuildFiles)\Reports"
	@IF NOT EXIST "$(LabDEInstallBuildFiles)\NonSelfRegFiles" @MKDIR "$(LabDEInstallBuildFiles)\NonSelfRegFiles"
	@IF NOT EXIST "$(DataEntryCoreInstallFilesDir)\NonSelfRegFiles" @MKDIR "$(DataEntryCoreInstallFilesDir)\NonSelfRegFiles"
	
	@DeleteFiles  "$(DataEntryCoreInstallFilesDir)\DotNet\*.*" /S
	@DeleteFiles "$(LabDEInstallBuildFiles)\Reports\*.*" /S
	@DeleteFiles "$(LabDEInstallBuildFiles)\NonSelfRegFiles\*.*" /S
	@XCOPY "$(ReusableComponentsRootDirectory)\APIs\Leadtools_20\Dotnet\*.*" "$(DataEntryCoreInstallFilesDir)\DotNet" /v /s /e /y
	@COPY /v "$(BinariesFolder)\Obfuscated\*.dll" "$(DataEntryCoreInstallFilesDir)\DotNet" 
	@COPY /v "$(BinariesFolder)\DataEntryCC.dll" "$(DataEntryCoreInstallFilesDir)\DotNet" 
	@COPY /v "$(BinariesFolder)\Obfuscated\DataEntryApplication.exe" "$(DataEntryCoreInstallFilesDir)\DotNet" 
	@COPY /v "$(BinariesFolder)\Interop.*.dll" "$(DataEntryCoreInstallFilesDir)\DotNet" 
	@COPY /v "$(BinariesFolder)\Obfuscated\SqlCompactImporter.exe" "$(DataEntryCoreInstallFilesDir)\DotNet" 
	@COPY /v "$(BinariesFolder)\Obfuscated\SqlCompactExporter.exe" "$(DataEntryCoreInstallFilesDir)\DotNet" 
	@COPY /v "$(BinariesFolder)\Obfuscated\SQLCDBEditor.exe" "$(DataEntryCoreInstallFilesDir)\DotNet" 
	@COPY /v "$(BinariesFolder)\LabDECppCC.dll" "$(DataEntryCoreInstallFilesDir)\DotNet" 
	@COPY "$(RCNETDir)\APIs\ScintillaNET v2.4\Dist\*.*" "$(DataEntryCoreInstallFilesDir)\DotNet" 
	@COPY /v "$(LabDEDir)\Misc\DisabledThemes.sdb" "$(DataEntryCoreInstallFilesDir)\Misc" 
	@COPY /v "$(LabDEDir)\Misc\DisabledThemes.sdb" "$(DataEntryCoreInstallFilesDir)\Misc" 
	@COPY /v "$(BinariesFolder)\DataEntryApplication.LabDE.resources" "$(LabDEInstallBuildFiles)\NonSelfRegFiles"
	@COPY /v  "$(BinariesFolder)\Obfuscated\AlternateTestNameManager.plugin" "$(LabDEInstallBuildFiles)\LabDEFolder"
	@XCOPY "$(LabDEDir)\Reports\*.*" "$(LabDEInstallBuildFiles)\Reports" /v /s /e /y
# Make .nl files to register the COM .NET files
	DIR "$(DataEntryCoreInstallFilesDir)\DotNet\Extract.LabResultsCustomComponents.dll" /b >"$(LabDEInstallBuildFiles)\NonSelfRegFiles\LabDE.nl"
	DIR "$(DataEntryCoreInstallFilesDir)\DotNet\LabDECppCC.dll" /b >"$(LabDEInstallBuildFiles)\NonSelfRegFiles\LabDE.rl"

CopyFilesForLabDEInstall: CopyFilesToInstallFolder 
	@ECHO Copying files for the LabDE Install
	@IF NOT EXIST "$(DataEntryCoreInstallFilesDir)\MergeModules" @MKDIR "$(DataEntryCoreInstallFilesDir)\MergeModules" 
	@DeleteFiles  "$(DataEntryCoreInstallFilesDir)\MergeModules\*.*"
	@COPY /v "$(DataEntryInstallMediaDir)\*.msm" "$(DataEntryCoreInstallFilesDir)\MergeModules"

BuildLabDEInstall: CopyFilesForLabDEInstall
    @ECHO Building Extract Systems LabDE Install...
	$(SetProductVerScript) "$(LabDEInstallRootDir)\LabDE\LabDE.ism" "$(FlexIndexVersion)"
    @"$(DEV_STUDIO_DIR)\System\IsCmdBld.exe" -p "$(LabDEInstallRootDir)\LabDE\LabDE.ism"
	
BuildDemoLabDE_DEP:
	@ECHO Building DemoLabDE_DEP...
	@CD $(Demo_LabDE_DEP)
	@devenv Demo_LabDE.sln /BUILD $(BuildConfig) 

CreateLabDEInstallCD: BuildLabDEInstall
	@ECHO Copying DataEntry Install files ...
    @IF NOT EXIST "$(LabDEInstallFiles)" MKDIR "$(LabDEInstallFiles)"
    @XCOPY "$(LabDEInstallMediaDir)\*.*" "$(LabDEInstallFiles)" /v /s /e /y
    $(VerifyDir) "$(LabDEInstallMediaDir)" "$(LabDEInstallFiles)"
	@COPY /v "$(LabDEInstallBuildFiles)\InstallHelp\*.*" "$(LabDEInstallFiles)"
	@COPY /v "$(LabDEInstallRootDir)\LabDE\Support Files\license.txt" "$(LabDESetupFiles)\Readme.txt"
    @DeleteFiles "$(LabDEInstallFiles)\vssver.scc"
	$(LabDELinkShared)
	$(LabDECorePointLink)

CreateDemoShieldInstall: CreateLabDEInstallCD 
	@ECHO Copying DemoShield Files
	@IF NOT EXIST "$(LabDEInstallDir)" MKDIR "$(LabDEInstallDir)"
	@XCOPY "$(DemoShieldRunFilesDir)\*.*" "$(LabDEInstallDir)" /v /s /e /y
	@COPY "$(LabDEInstallRootDir)\LabDEInstall\Launch.ini" "$(LabDEInstallDir)"
	@COPY "$(LabDEInstallRootDir)\LabDEInstall\LabDEInstall.dbd" "$(LabDEInstallDir)"
	@COPY "$(LabDEInstallRootDir)\LabDEInstall\LabDE.ico" "$(LabDEInstallDir)"
	@COPY "$(LabDEInstallRootDir)\LabDEInstall\autorun.inf" "$(LabDESetupFiles)"

CreateDemo_LabDE: BuildDemoLabDE_DEP
	@ECHO Copying Demo_LabDE files...
    @IF NOT EXIST "$(LabDEDemo)\Solution\Rules" MKDIR "$(LabDEDemo)\Solution\Rules"
    @IF NOT EXIST "$(LabDEDemo)\Solution\Corepoint Integration" MKDIR "$(LabDEDemo)\Solution\Corepoint Integration"
	@IF NOT EXIST "$(LabDEDemo)\Solution\Database Files" MKDIR "$(LabDEDemo)\Solution\Database Files"
	@IF NOT EXIST "$(LabDEDemo)\Solution\Bin\" MKDIR "$(LabDEDemo)\Solution\Bin\"
	@ECHO Updating LabDE demo rules for FKB Version...
	$(CScriptProgram) "$(PDRootDir)\Utils\Scripts\UpdateFKB.js" -silent "$(RulesDir)\LabDE\Demo_LabDE\Solution" "$(FKBVersion)"
	@XCOPY "$(LabResultsDir)\Utils\LabDEDemo\Files\*.*" "$(LabDEDemo)\" /v /s /e /y
	@XCOPY "$(AFInstallRootDir)\Demo_LabDE\Sanitized\*.*" "$(LabDEDemo)\Input\" /v /s /e /y
	@XCOPY "$(RulesDir)\LabDE\Demo_LabDE\Solution\*.*" "$(LabDEDemo)\Solution\" /v /s /e /y	
	@COPY /y "$(Demo_LabDE_DEP)\Bin\$(BuildConfig)\Extract.DataEntry.DEP.Demo_LabDE.dll" "$(LabDEDemo)\Solution\Bin\"
	@COPY /v  "$(BinariesFolder)\Obfuscated\AlternateTestNameManager.plugin" "$(LabDEDemo)\Solution\Database Files"
	@ECHO Encrypting LabDE Demo Rules...
	@SendFilesAsArgumentToApplication "$(LabDEDemo)\Solution\Rules\*.dat" 1 1 "$(BinariesFolder)\EncryptFile.exe"
	@SendFilesAsArgumentToApplication "$(LabDEDemo)\Solution\Rules\*.rsd" 1 1 "$(BinariesFolder)\EncryptFile.exe"
	@SendFilesAsArgumentToApplication "$(LabDEDemo)\Solution\Rules\*.dcc" 1 1 "$(BinariesFolder)\EncryptFile.exe"
	@SendFilesAsArgumentToApplication "$(LabDEDemo)\Solution\Rules\*.spm" 1 1 "$(BinariesFolder)\EncryptFile.exe"
	@SendFilesAsArgumentToApplication "$(LabDEDemo)\Solution\Rules\*.nlp" 1 1 "$(BinariesFolder)\EncryptFile.exe"
    @DeleteFiles "$(LabDEDemo)\Solution\Rules\*.dat"
    @DeleteFiles "$(LabDEDemo)\Solution\Rules\*.rsd"
    @DeleteFiles "$(LabDEDemo)\Solution\Rules\*.dcc"
		@DeleteFiles "$(LabDEDemo)\Solution\Rules\*.spm"
    @DeleteFiles "$(LabDEDemo)\Solution\Rules\*.nlp"
    @DeleteFiles "$(LabDEDemo)\Solution\Rules\vssver.scc"
	
CopySilentInstall:
	@ECHO Copying LabDE SilentInstall
	@IF NOT EXIST "$(LabDESilentInstallDir)" MKDIR "$(LabDESilentInstallDir)"
	@COPY "$(AFRootDirectory)\SilentInstalls\*Uninst.*" "$(LabDESilentInstallDir)"
	@COPY "$(AFRootDirectory)\SilentInstalls\InstallLabDE.bat" "$(LabDESilentInstallDir)"
	@COPY "$(AFRootDirectory)\SilentInstalls\UninstallExtract.bat" "$(LabDESilentInstallDir)"
	@COPY "$(AFRootDirectory)\SilentInstalls\LabDE.iss" "$(LabDESilentInstallDir)"
	@COPY "$(AFRootDirectory)\SilentInstalls\LabDE64.iss" "$(LabDESilentInstallDir)"

DoEverything: DisplayTimeStamp SetupBuildEnv CreateDemoShieldInstall CreateDemo_LabDE CopySilentInstall
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @ECHO LabDE Build process completed.
    @ECHO.

