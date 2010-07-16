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
RCNETDir=$(EngineeringRootDirectory)\RC.Net
IDShieldOfficeDir=$(PDRootDir)\IDShieldOffice
IDShieldOfficeBuildDir=$(IDShieldOfficeDir)\Build
PDCommonDir=$(PDRootDir)\Common
AFRootDirectory=$(PDRootDir)\AttributeFinder

ObfuscatorInfo=P:\IDShieldOffice\Archive\ObfuscatorFiles\InternalBuilds\$(IDShieldOfficeVersion)
IDShieldOfficeInstallRootDir=P:\IDShieldOffice
IDShieldOfficeInstallFilesRootDir=$(IDShieldOfficeInstallRootDir)\Installation\Files

IDShieldOfficeBleedingEdgeDir=R:\IDShieldOffice\Internal\BleedingEdge\$(IDShieldOfficeVersion)

MergeModuleRootDir=C:\InstallShield 2010 Projects\MergeModules

# determine the name of the release output directory based upon the build
# configuration that is being built
!IF "$(BuildConfig)" == "Release"
BuildOutputDir=Release
!ELSEIF "$(BuildConfig)" == "Debug"
BuildOutputDir=Debug
!ELSEIF "$(BuildConfig)" == "Testing"
BuildOutputDir=Testing
!ELSE
!ERROR Internal error - invalid value for BuildConfig variable!
!ENDIF

BinariesFolder=$(EngineeringRootDirectory)\Binaries\$(BuildOutputDir)
TestingBinariesFolder=$(EngineeringRootDirectory)\Binaries\Testing

ObfuscatedPath=\Obfuscated

#############################################################################
# B U I L D    T A R G E T S
#
BuildIDShieldOffice: 
	@ECHO Building ID Shield Office...
    @CD "$(IDShieldOfficeDir)\Core\Code"
    @devenv IDShieldOffice.sln /BUILD $(BuildConfig) /USEENV

ObfuscateFiles:
	@ECHO Obfuscating .Net files...
#	Copy the strong naming key to location dotfuscator xml file expects
	@IF NOT EXIST "$(IDShieldOfficeInstallFilesRootDir)\StrongNameKey" @MKDIR "$(IDShieldOfficeInstallFilesRootDir)\StrongNameKey"
	@COPY /V "$(RCNETDir)\Core\Code\ExtractInternalKey.snk" "$(IDShieldOfficeInstallFilesRootDir)\StrongNameKey"
	@IF NOT EXIST "$(BinariesFolder)\Obfuscated" @MKDIR "$(BinariesFolder)\Obfuscated"
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(IDShieldOfficeBuildDir)\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Utilities.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Utilities.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(IDShieldOfficeBuildDir)\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Imaging.Forms.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Imaging.Forms.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(IDShieldOfficeBuildDir)\ObfuscateConfig.xml
	dotfuscator.exe  /in:+"$(BinariesFolder)\IDShieldOffice.exe" /mapout:"$(BinariesFolder)\Map\mapIDShieldOffice.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(IDShieldOfficeBuildDir)\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Licensing.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Licensing.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(IDShieldOfficeBuildDir)\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Utilities.Forms.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Utilities.Forms.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(IDShieldOfficeBuildDir)\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Drawing.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Drawing.Forms.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(IDShieldOfficeBuildDir)\ObfuscateConfig.xml
	dotfuscator.exe  /in:"$(BinariesFolder)\Extract.Encryption.dll" /mapout:"$(BinariesFolder)\Map\mapExtract.Encryption.xml" /encrypt:on /enhancedOI:on /out:"$(BinariesFolder)\Obfuscated" $(IDShieldOfficeBuildDir)\ObfuscateConfig.xml
	
EncryptRuleFiles: CopyFilesToBuildInstall
	@ECHO Encrypting Rules files and copying to installation directory...
    @IF NOT EXIST "$(IDShieldOfficeInstallFilesRootDir)\Rules" @MKDIR "$(IDShieldOfficeInstallFilesRootDir)\Rules"
	@DeleteFiles "$(IDShieldOfficeInstallFilesRootDir)\Rules\*.*"
    @XCOPY "$(IDShieldOfficeDir)\Rules\*.*" "$(IDShieldOfficeInstallFilesRootDir)\Rules" /v /s /e /y
    @ECHO Encrypting Rules files...
    @SendFilesAsArgumentToApplication "$(IDShieldOfficeInstallFilesRootDir)\Rules\*.dat" 1 1 "$(BinariesFolder)\EncryptFile2.exe"
    @DeleteFiles "$(IDShieldOfficeInstallFilesRootDir)\Rules\*.dat"
    @DeleteFiles "$(IDShieldOfficeInstallFilesRootDir)\vssver.scc"
	
CopyFilesToBuildInstall: ObfuscateFiles 
	@ECHO Copying files for Install creationg...
	@DeleteFiles "$(IDShieldOfficeInstallFilesRootDir)\CaereDlls\*.*" /S
	@DeleteFiles "$(IDShieldOfficeInstallFilesRootDir)\DotNetFiles\*.*" /S
	@DeleteFiles "$(IDShieldOfficeInstallFilesRootDir)\LeadtoolsDlls\*.*" /S
	@DeleteFiles "$(IDShieldOfficeInstallFilesRootDir)\LeadtoolsDotNetDlls\*.*" /S
	@DeleteFiles "$(IDShieldOfficeInstallFilesRootDir)\NonSelfRegCommonComponents\*.*" /S
	@DeleteFiles "$(IDShieldOfficeInstallFilesRootDir)\SelfRegCommonComponents\*.*" /S
	@DeleteFiles "$(IDShieldOfficeInstallFilesRootDir)\RogueWaveDlls\*.*" /S
	@IF NOT EXIST "$(IDShieldOfficeInstallFilesRootDir)\NonSelfRegCommonComponents" @MKDIR "$(IDShieldOfficeInstallFilesRootDir)\NonSelfRegCommonComponents"
	@IF NOT EXIST "$(IDShieldOfficeInstallFilesRootDir)\DotNetFiles" @MKDIR "$(IDShieldOfficeInstallFilesRootDir)\DotNetFiles"
	@IF NOT EXIST "$(IDShieldOfficeInstallFilesRootDir)\LeadtoolsDotNetDlls" @MKDIR "$(IDShieldOfficeInstallFilesRootDir)\LeadtoolsDotNetDlls"
	@IF NOT EXIST "$(IDShieldOfficeInstallFilesRootDir)\SelfRegCommonComponents" @MKDIR "$(IDShieldOfficeInstallFilesRootDir)\SelfRegCommonComponents"
	@IF NOT EXIST "$(IDShieldOfficeInstallFilesRootDir)\LeadtoolsDlls" @MKDIR "$(IDShieldOfficeInstallFilesRootDir)\LeadtoolsDlls"
	@IF NOT EXIST "$(IDShieldOfficeInstallFilesRootDir)\LeadtoolsDlls\pdf" @MKDIR "$(IDShieldOfficeInstallFilesRootDir)\LeadtoolsDlls\pdf"
	@IF NOT EXIST "$(IDShieldOfficeInstallFilesRootDir)\RogueWaveDlls" @MKDIR "$(IDShieldOfficeInstallFilesRootDir)\RogueWaveDlls"
	@IF NOT EXIST "$(IDShieldOfficeInstallFilesRootDir)\CaereDlls" @MKDIR "$(IDShieldOfficeInstallFilesRootDir)\CaereDlls"
	@COPY /V "$(BinariesFolder)$(ObfuscatedPath)\IDShieldOffice.exe" "$(IDShieldOfficeInstallFilesRootDir)\DotNetFiles"
	@COPY /V "$(BinariesFolder)\InstallPCE.exe" "$(IDShieldOfficeInstallFilesRootDir)\NonSelfRegCommonComponents"
	@COPY /V "$(BinariesFolder)\ESPrintManager.exe" "$(IDShieldOfficeInstallFilesRootDir)\NonSelfRegCommonComponents"
	@COPY /V "$(ReusableComponentsRootDirectory)\APIs\ActMask Virtual Printer\virtual-printer-sdk-image-full.exe" "$(IDShieldOfficeInstallFilesRootDir)\NonSelfRegCommonComponents"
	@COPY /V "$(BinariesFolder)\ESActMaskPCE.dll" "$(IDShieldOfficeInstallFilesRootDir)\SelfRegCommonComponents"
	@COPY /V "$(BinariesFolder)\ESPrintCaptureCore.dll" "$(IDShieldOfficeInstallFilesRootDir)\SelfRegCommonComponents"
	@COPY /V "$(PDCommonDir)\RegisterAll.bat" "$(IDShieldOfficeInstallFilesRootDir)\NonSelfRegCommonComponents"
	@DIR "$(IDShieldOfficeInstallFilesRootDir)\SelfRegCommonComponents\*.*" /b >"$(IDShieldOfficeInstallFilesRootDir)\NonSelfRegCommonComponents\IDShieldOffice.rl"

MakeExtractCommonMergeModule: ObfuscateFiles
	@ECHO Making ExtractCommonMM...
	@CD "$(PDCommonDir)\"
    @nmake /F ExtractCommon.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" ProductVersion="$(ProductVersion)" CreateExtractCommonMergeModule

MakeExtractFlexCommonMergeModule: MakeExtractCommonMergeModule
	@ECHO Making ExtractFlexCommonMM...
    @CD "$(AFRootDirectory)\Build
    @nmake /F ExtractFlexCommon.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" ProductVersion="$(ProductVersion)" CreateExtractFlexCommonMergeModule
	
BuildIDShieldOfficeInstall:CopyFilesToBuildInstall EncryptRuleFiles CreateVersionISImportFile MakeExtractFlexCommonMergeModule
    @ECHO Building Extract Systems IDShield Office installation...
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(ReusableComponentsRootDirectory)\APIs\Nuance_16.3\bin;$(ReusableComponentsRootDirectory)\APIs\LeadTools_16.5\Bin;$(ReusableComponentsRootDirectory)\APIs\RogueWave\bin;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(VCPP_DIR)\PlatformSDK\bin;$(VISUAL_STUDIO)\SDK\v2.0\bin;C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727;$(VCPP_DIR)\VCPackages
	$(SetProductVerScript) "$(IDShieldOfficeDir)\Installation\IDShieldOffice\IDShieldOffice.ism" "$(IDShieldOfficeVersion)"
    @"$(DEV_STUDIO_DIR)\System\IsCmdBld.exe" -p "$(IDShieldOfficeDir)\Installation\IDShieldOffice\IDShieldOffice.ism"
	
CopyInstall:BuildIDShieldOfficeInstall
	@ECHO Copying files to IDShieldOffice BleedingEdge folder...
	@IF NOT EXIST "$(IDShieldOfficeBleedingEdgeDir)" @MKDIR "$(IDShieldOfficeBleedingEdgeDir)"
	@COPY /V "$(IDShieldOfficeDir)\Installation\IDShieldOffice\Media\CD-ROM\DiskImages\DISK1\*.*" "$(IDShieldOfficeBleedingEdgeDir)\*.*"
	@IF NOT EXIST "$(ObfuscatorInfo)" @MKDIR "$(ObfuscatorInfo)"
	@COPY /V "$(BinariesFolder)$(ObfuscatedPath)\*.pdb" "$(ObfuscatorInfo)"
	@COPY /V "$(BinariesFolder)\Map\*.xml" "$(ObfuscatorInfo)"
	@COPY /V "$(IDShieldOfficeInstallFilesRootDir)\InstallHelp\*.*" "$(IDShieldOfficeBleedingEdgeDir)"
	@COPY /V "$(IDShieldOfficeDir)\Installation\IDShieldOffice\Support Files\license.txt" "$(IDShieldOfficeBleedingEdgeDir)\readme.txt" 
	@COPY /V "$(IDShieldOfficeDir)\Installation\IDShieldOffice\Support Files\autorun.inf" "$(IDShieldOfficeBleedingEdgeDir)" 

GetAllFiles: GetPDCommonFiles GetAttributeFinderFiles GetRCdotNETFiles GetReusableComponentFiles GetPDUtilsFiles GetIDShieldOfficeFiles

DoEverythingNoGet: DisplayTimeStamp SetupBuildEnv BuildIDShieldOffice  CopyInstall
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @ECHO IDShieldOffice Build process completed.
    @ECHO.

DoEverything:DisplayTimeStamp SetupBuildEnv GetAllFiles DoEverythingNoGet
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @ECHO IDShieldOffice Build process completed.
    @ECHO.
