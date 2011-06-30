#############################################################################
# I N C L U D E S
#
# the common.mak file being included here counts on $(ProductRootDirName)
# ProductRootDirName defined as a non-null string
#

!include ..\..\..\..\ProductDevelopment\Common\LatestComponentVersions.mak
!include ..\..\..\..\ProductDevelopment\Common\Common.mak

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
AFBleedingEdgeDir=R:\FlexIndex\Internal\BleedingEdge\$(FlexIndexVersion)
IcoMapBleedingEdgeDir=R:\IcoMapESRI\Internal\BleedingEdge\$(IcoMapESRIVersion)\IcoMapForArcGIS

LMInstallFilesRootDir=P:\LicenseManager\Files
AFExtractLMBleedingEdgeDir=$(AFBleedingEdgeDir)\Extract Systems LM
IcoExtractLMBleedingEdgeDir=$(IcoMapBleedingEdgeDir)\ExtractSystemsLM

ExtractLMInstallRootDir=$(ReusableComponentsRootDirectory)\VendorSpecificUtils\SafeNetUtils\Installation\Extract Systems LM
ExtractLMInstallMediaDir=$(ExtractLMInstallRootDir)\Media\CD-ROM\DiskImages\DISK1

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
# This file requires the appropriate project has been built before calling
#
CopyFilesToExtractLMInstall:
	@Echo Copying Extract LM files to Installation directory...
	@IF NOT EXIST "$(LMInstallFilesRootDir)\FieldActivation" MKDIR "$(LMInstallFilesRootDir)\FieldActivation"
	@IF NOT EXIST "$(LMInstallFilesRootDir)\NonSelfRegCC" MKDIR "$(LMInstallFilesRootDir)\NonSelfRegCC"
	@DeleteFiles "$(LMInstallFilesRootDir)\FieldActivation\*.*"
    @DeleteFiles "$(LMInstallFilesRootDir)\NonSelfRegCC\*.*"
    @XCOPY "$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\FieldActivationUtility\*.*" "$(LMInstallFilesRootDir)\FieldActivation" /v /y
    $(VerifyDir) "$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\FieldActivationUtility" "$(LMInstallFilesRootDir)\FieldActivation"
    @COPY /v  "$(BinariesFolder)\USBLicenseKeyManager.exe" "$(LMInstallFilesRootDir)\NonSelfRegCC"

BuildExtractLMInstall:CopyFilesToExtractLMInstall
    @ECHO Building Extract Systems LM installation...
	@SET PATH=%windir%;%windir%\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;%VAULT_DIR%\win32;%BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs\Nuance_16.3\bin;%BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs\LeadTools_17\Bin;%BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs\RogueWave\bin;%BUILD_VSS_ROOT%\Engineering\ReusableComponents\APIs\SafeNetUltraPro\Bin;%DevEnvDir%;%VCPP_DIR%\BIN;%VS_COMMON%\Tools;%VS_COMMON%\Tools\bin;%VCPP_DIR%\PlatformSDK\bin;C:\Program Files\Microsoft Visual Studio 8\SDK\v2.0\bin;C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727;%VCPP_DIR%\VCPackages
	$(SetProductVerScript) "$(ExtractLMInstallRootDir)\Extract Systems LM.ism" "$(ReusableComponentsVersion)"
    @"$(DEV_STUDIO_DIR)\System\IsCmdBld.exe" -p "$(ExtractLMInstallRootDir)\Extract Systems LM.ism"
	
CopyLMInstallToProductInstallFolder: BuildExtractLMInstall
#This label requires the ProductInstallFolder macro to be defined
	@ECHO Copying LM Install to $(ProductInstallFolder) Directory...
    @IF NOT EXIST "$(ProductInstallFolder)" MKDIR "$(ProductInstallFolder)"
    @XCOPY "$(ExtractLMInstallMediaDir)\*.*" "$(ProductInstallFolder)" /v /s /e /y
    $(VerifyDir) "$(ExtractLMInstallMediaDir)" "$(ProductInstallFolder)"
    @DeleteFiles "$(ProductInstallFolder)\vssver.scc"
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @ECHO Flex Index License Manager Build process completed.
    @ECHO.

CreateFlexLMInstall: BuildExtractLMInstall
	@ECHO Copying LM Install to Flex Index Release Directory...
    @IF NOT EXIST "$(AFExtractLMBleedingEdgeDir)" MKDIR "$(AFExtractLMBleedingEdgeDir)"
    @XCOPY "$(ExtractLMInstallMediaDir)\*.*" "$(AFExtractLMBleedingEdgeDir)" /v /s /e /y
    $(VerifyDir) "$(ExtractLMInstallMediaDir)" "$(AFExtractLMBleedingEdgeDir)"
	@COPY /v "$(LMInstallFilesRootDir)\64BitDrivers\*.*" "$(AFExtractLMBleedingEdgeDir)"
    @DeleteFiles "$(AFExtractLMBleedingEdgeDir)\vssver.scc"
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @ECHO Flex Index License Manager Build process completed.
    @ECHO.

CreateIcoMapLMInstall: BuildExtractLMInstall
	@ECHO Copying LM Install to Ico Map Release Directory...
    @IF NOT EXIST "$(IcoExtractLMBleedingEdgeDir)" MKDIR "$(IcoExtractLMBleedingEdgeDir)"
    @XCOPY "$(ExtractLMInstallMediaDir)\*.*" "$(IcoExtractLMBleedingEdgeDir)" /v /s /e /y
    $(VerifyDir) "$(ExtractLMInstallMediaDir)" "$(IcoExtractLMBleedingEdgeDir)"
	@COPY /v "$(LMInstallFilesRootDir)\64BitDrivers\*.*" "$(IcoExtractLMBleedingEdgeDir)"
    @DeleteFiles "$(IcoExtractLMBleedingEdgeDir)\vssver.scc"
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @ECHO Ico Map License Manager Build process completed.
    @ECHO.
