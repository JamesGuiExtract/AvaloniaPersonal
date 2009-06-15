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
FLEXBleadingEdgeDir=I:\Common\Engineering\ProductReleases\FlexIndex\Internal\BleedingEdge\$(FlexIndexVersion)

LaserFicheDir=$(PDRootDir)\AFIntegrations\Laserfiche
LaserFicheInstallDir=I:\Common\Engineering\ProductDevelopment\AttributeFinder\AFIntegrations\Laserfiche\Files
LaserficheReleaseBleedingEdgeDir=$(FLEXBleadingEdgeDir)\Integrations\$(LaserficheVersion)\IDShield
LaserficheInstallRootDir=$(LaserFicheDir)\Installation\LFPlugin
LaserficheInstallMediaDir=$(LaserficheInstallRootDir)\Media\CD-ROM\DiskImages\DISK1

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
BuildAttributeSDKandRDT:  
	@ECHO Build Attribute Finder SDK
	CD "$(AFRootDirectory)\Build"
    @nmake /F AttributeFinderSDK.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" ProductVersion="$(ProductVersion)" DoEverythingNoGet
	@nmake /F RuleDevelopmentKit.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" ProductVersion="$(ProductVersion)" DoEverythingNoGet

CopyFilesToInstallFolder: BuildLaserFiche
    @ECHO Copying the Laserfiche files to installation directory...
	@DeleteFiles "$(LaserFicheInstallDir)\NonSelfRegComponents\*.*" /S /Q
	@DeleteFiles "$(LaserFicheInstallDir)\Service"\*.*" /S /Q
    @COPY /v "$(BinariesFolder)\Interop.UCLID_COMLMLib.dll" "$(LaserFicheInstallDir)\NonSelfRegComponents"
    @COPY /v "$(BinariesFolder)\Interop.UCLID_EXCEPTIONMGMTLib.dll" "$(LaserFicheInstallDir)\NonSelfRegComponents"
    @COPY /v "$(BinariesFolder)\Interop.UCLID_LASERFICHECCLib.dll" "$(LaserFicheInstallDir)\NonSelfRegComponents"
    @COPY /v "$(BinariesFolder)\ESLaserficheUtils.dll" "$(LaserFicheInstallDir)\NonSelfRegComponents"
    @COPY /v "$(BinariesFolder)\ESLaserficheClientPlugin.exe" "$(LaserFicheInstallDir)\NonSelfRegComponents"
    @COPY /v "$(BinariesFolder)\ESLaserficheService.exe" "$(LaserFicheInstallDir)\Service"
    @COPY /v "$(LaserFicheDir)\misc\*.ico" "$(LaserFicheInstallDir)\NonSelfRegComponents"
    @COPY /v "$(BinariesFolder)\ESLaserficheCC.dll" "$(LaserFicheInstallDir)\SelfRegComponents"
    @DeleteFiles "$(LaserFicheInstallDir)\*.scc" /S /Q
	@DIR "$(LaserFicheInstallDir)\SelfRegComponents\*.*" /b >"$(LaserFicheInstallDir)\NonSelfRegComponents\Laserfiche.rl"
	
BuildLaserFiche: 
	@ECHO Build LaserFiche...
	@CD "$(LaserFicheDir)"
    @devenv Laserfiche.sln /BUILD $(BuildConfig) /USEENV
	
BuildLaserficheInstall: CopyFilesToInstallFolder
    @ECHO Building Extract Systems Laserfiche installation...
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VAULT_DIR)\win32;$(ReusableComponentsRootDirectory)\APIs\Nuance_16\bin;$(ReusableComponentsRootDirectory)\APIs\LeadTools_16\Bin;$(ReusableComponentsRootDirectory)\APIs\RogueWave\bin;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(VCPP_DIR)\PlatformSDK\bin;$(VISUAL_STUDIO)\SDK\v2.0\bin;C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727;$(VCPP_DIR)\VCPackages
	$(SetProductVerScript) "$(LaserFicheDir)\Installation\LFPlugin.ism" "$(LaserficheVersion)"
    @"$(DEV_STUDIO_DIR)\System\IsCmdBld.exe" -p "$(LaserFicheDir)\Installation\LFPlugin.ism"
	
CreateLaserficheInstallCD: BuildLaserficheInstall
	@ECHO Copying IDShield Install files ...
    @IF NOT EXIST "$(LaserficheReleaseBleedingEdgeDir)" MKDIR "$(LaserficheReleaseBleedingEdgeDir)"
    @XCOPY "$(LaserficheInstallMediaDir)\*.*" "$(LaserficheReleaseBleedingEdgeDir)" /v /s /e /y
    $(VerifyDir) "$(LaserficheInstallMediaDir)" "$(LaserficheReleaseBleedingEdgeDir)"
    @DeleteFiles "$(LaserficheReleaseBleedingEdgeDir)\vssver.scc"

DoNecessaryBuilds: SetupBuildEnv BuildAttributeSDKandRDT

GetAllFiles: GetPDCommonFiles GetAttributeFinderFiles GetRCdotNETFiles GetReusableComponentFiles GetPDUtilsFiles GetLaserFicheFiles

DoEverythingNoGet: DisplayTimeStamp DoNecessaryBuilds CreateLaserficheInstallCD
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @ECHO Laserfiche Build process completed.
    @ECHO.
	
DoEverything: DisplayTimeStamp GetAllFiles DoNecessaryBuilds CreateLaserficheInstallCD
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @ECHO Laserfiche Build process completed.
    @ECHO.
	
