
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
IcoMapESRIRootDirectory=$(EngineeringRootDirectory)\ProductDevelopment\IcoMapESRI
IcoMapCoreRootDirectory=$(EngineeringRootDirectory)\ProductDevelopment\IcoMapCore
GridGeneratorDirectory=$(EngineeringRootDirectory)\ProductDevelopment\Utils\GridGenerator
ArcGISIcoMapCodeDirectory=$(IcoMapESRIRootDirectory)\ArcGISIcoMap\Code

GISPlatInterfacesDir=$(EngineeringRootDirectory)\ProductDevelopment\PlatformSpecificUtils\GISPlatInterfaces
ArcGISUtilsDir=$(EngineeringRootDirectory)\ProductDevelopment\PlatformSpecificUtils\ArcGISUtils

IcoMapESRIInstallFilesRootDir=I:\Common\Engineering\ProductDevelopment\IcoMapESRI\Installation\Files
IcoMapESRIInstallProjectRootDir=$(IcoMapESRIRootDirectory)\Installation
IcoMapESRIInstallMediaDir=$(IcoMapESRIInstallProjectRootDir)\IcoMap For ArcGIS\Media\CD-ROM\DiskImages\Disk1

InputFunnelBuildDir=$(ReusableComponentsRootDirectory)\InputFunnel\Build

ProductReleaseBleedingEdgeDir=I:\Common\Engineering\ProductReleases\IcoMapESRI\Internal\BleedingEdge\$(IcoMapESRIVersion)
PDCommonDir=$(EngineeringRootDirectory)\ProductDevelopment\Common

Start=start
StartOptions=CMD /x /a /k

# determine the name of the release output directory based upon the build
# configuration that is being built
# NOTE: the installation target of this make file will only execute its
# code if ARXOutputDir is defined.  Currently, since we do not want to build
# installsets using the debug versions of the ARX/DLL files, the ARXOutputDir
# variable only gets set for the "Win32 Release" and "Win32 OEM Release"
# configurations
!IF "$(BuildConfig)" == "Release"
BuildOutputDir=Release
!ELSEIF "$(BuildConfig)" == "Debug"
BuildOutputDir=Debug
!ELSE
!ERROR Internal error - invalid value for BuildConfig variable!
!ENDIF

BinariesFolder=$(EngineeringRootDirectory)\Binaries\$(BuildOutputDir)

#############################################################################
# I N T E R N A L    B U I L D    T A R G E T S
#
GetIcoMapCoreFiles:
	@IF NOT EXIST "$(IcoMapCoreRootDirectory)" @MKDIR "$(IcoMapCoreRootDirectory)"
	@ECHO Getting $(IcoMapCoreVersion) 
	@ECHO Please wait...
	@CD "$(IcoMapCoreRootDirectory)"
	$(Get) $$/Engineering/ProductDevelopment/IcoMapCore $(GetOptions) -V"$(IcoMapCoreVersion)"
	@SendFilesAsArgumentToApplication *.rc 1 1 UpdateFileVersion "$(IcoMapCoreVersion)"

GetGISUtils:
	@IF NOT EXIST "$(GISPlatInterfacesDir)" @MKDIR "$(GISPlatInterfacesDir)"
	@ECHO Getting $(PlatformSpecificUtilsVersion) 
	@ECHO Please wait...
	@CD "$(GISPlatInterfacesDir)"
	$(Get) $$/Engineering/ProductDevelopment/PlatformSpecificUtils/GISPlatInterfaces $(GetOptions) -V"$(PlatformSpecificUtilsVersion)"
	@SendFilesAsArgumentToApplication *.rc 1 1 UpdateFileVersion "$(PlatformSpecificUtilsVersion)"	
	@IF NOT EXIST "$(ArcGISUtilsDir)" @MKDIR "$(ArcGISUtilsDir)"
	@ECHO Getting $(PlatformSpecificUtilsVersion) 
	@ECHO Please wait...
	@CD "$(ArcGISUtilsDir)"
	$(Get) $$/Engineering/ProductDevelopment/PlatformSpecificUtils/ArcGISUtils $(GetOptions) -V"$(PlatformSpecificUtilsVersion)"
	@SendFilesAsArgumentToApplication *.rc 1 1 UpdateFileVersion "$(PlatformSpecificUtilsVersion)"	

GetIcoMapESRIFiles: GetIcoMapCoreFiles GetGISUtils
	@IF NOT EXIST "$(IcoMapESRIRootDirectory)" @MKDIR "$(IcoMapESRIRootDirectory)"
	@ECHO Getting $(IcoMapESRIVersion) 
	@ECHO Please wait...
	@CD "$(IcoMapESRIRootDirectory)"
	$(Get) $$/Engineering/ProductDevelopment/IcoMapESRI $(GetOptions) -V"$(IcoMapESRIVersion)"
	@SendFilesAsArgumentToApplication *.rc 1 1 UpdateFileVersion "$(IcoMapESRIVersion)"

PreRegisterFiles:
	@ECHO Registering ArcGIS Type libraries...
	@CD "$(EngineeringRootDirectory)\ReusableComponents\APIs\ArcGIS\Bin"
	@SendFilesAsArgumentToApplication *.olb 0 1 RegSvr32 /s
	
GetPDCommonFiles:
    @IF NOT EXIST "$(PDCommonDir)" @MKDIR "$(PDCommonDir)"
    @ECHO Getting files from Common folder 
    @ECHO Please wait...
    @CD "$(PDCommonDir)"
    $(Get) $$/Engineering/ProductDevelopment/Common $(GetOptions) -V"$(IcoMapESRIVersion)"

#############################################################################
# E X T E R N A L    B U I L D    T A R G E T S
#

BuildInputFunnelCore: GetReusableComponentFiles GetPDCommonFiles
	@ECHO Making InputFunnelCore build...
	@CD "$(InputFunnelBuildDir)"
    @nmake /F InputFunnel.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" DoEverything

BuildIcoMapESRIProduct: SetupBuildEnv BuildInputFunnelCore GetIcoMapESRIFiles PreRegisterFiles
	@IF NOT EXIST "$(ArcGISIcoMapCodeDirectory)\..\$(BuildOutputDir)" @MKDIR "$(ArcGISIcoMapCodeDirectory)\..\$(BuildOutputDir)"
	@CD "$(ArcGISIcoMapCodeDirectory)"
	@devenv ArcGISIcoMap.sln /BUILD $(BuildConfig) /USEENV

CopyIcoMapESRIFilesToInstallDir:
 	@ECHO Copying the IcoMapESRI files to installation directory...
    @COPY /V  "$(BinariesFolder)\IcoMapApp.dll" "$(IcoMapESRIInstallFilesRootDir)\SelfRegProgFile\Bin"
    @COPY /V  "$(BinariesFolder)\ArcGISIcoMap.dll" "$(IcoMapESRIInstallFilesRootDir)\SelfRegProgFile\Bin"
    @COPY /V  "$(BinariesFolder)\UCLIDCurveParameter.dll" "$(IcoMapESRIInstallFilesRootDir)\SelfRegRC"
    @COPY /V  "$(BinariesFolder)\UCLIDFeatureMgmt.dll" "$(IcoMapESRIInstallFilesRootDir)\SelfRegRC"
    @COPY /V  "$(BinariesFolder)\GISPlatInterfaces.dll" "$(IcoMapESRIInstallFilesRootDir)\SelfRegRC"
    @COPY /V  "$(BinariesFolder)\ArcGISUtils.dll" "$(IcoMapESRIInstallFilesRootDir)\SelfRegRC"
    @COPY /V  "$(BinariesFolder)\AttributeViewerDlg.dll" "$(IcoMapESRIInstallFilesRootDir)\ProgramFiles\Bin"
    @COPY /V  "$(BinariesFolder)\IcoMapCoreUtils.dll" "$(IcoMapESRIInstallFilesRootDir)\ProgramFiles\Bin"
    @COPY /V  "$(BinariesFolder)\IcoMapLicenseUtil.exe" "$(IcoMapESRIInstallFilesRootDir)\ProgramFiles\Bin"
    @COPY /V  "$(BinariesFolder)\CurveCalculator.dll" "$(IcoMapESRIInstallFilesRootDir)\ReusableComponents"
    @COPY /V  "$(BinariesFolder)\CCE.dll" "$(IcoMapESRIInstallFilesRootDir)\ReusableComponents"
    @COPY /V  "$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\bin\ux32w.dll" "$(IcoMapESRIInstallFilesRootDir)\ReusableComponents"
    @COPY /V  "$(BinariesFolder)\SafeNetUtils.dll" "$(IcoMapESRIInstallFilesRootDir)\ReusableComponents"
    @COPY /V  "$(BinariesFolder)\DetectAndReportFailure.exe" "$(IcoMapESRIInstallFilesRootDir)\ReusableComponents"
    @COPY /V  "$(BinariesFolder)\IcoMapLicenseUtil.exe" "$(IcoMapESRIInstallFilesRootDir)\ProgramFiles\Bin"
    @COPY /V  "$(EngineeringRootDirectory)\ReusableComponents\InputFunnel\Packages\LandRecords\AdditionalFiles\Cartographic Point.FOD" "$(IcoMapESRIInstallFilesRootDir)\ReusableComponents"
    @COPY /V  "$(EngineeringRootDirectory)\ReusableComponents\InputFunnel\Packages\LandRecords\AdditionalFiles\Common Angle.FOD" "$(IcoMapESRIInstallFilesRootDir)\ReusableComponents"
    @COPY /V  "$(EngineeringRootDirectory)\ReusableComponents\InputFunnel\Packages\LandRecords\AdditionalFiles\Common Bearing.FOD" "$(IcoMapESRIInstallFilesRootDir)\ReusableComponents"
    @COPY /V  "$(EngineeringRootDirectory)\ReusableComponents\InputFunnel\Packages\LandRecords\AdditionalFiles\Deg-Min-Sec.FOD" "$(IcoMapESRIInstallFilesRootDir)\ReusableComponents"
    @COPY /V  "$(EngineeringRootDirectory)\ReusableComponents\InputFunnel\Packages\LandRecords\AdditionalFiles\Distance.FOD" "$(IcoMapESRIInstallFilesRootDir)\ReusableComponents"
    @COPY /V  "$(EngineeringRootDirectory)\ReusableComponents\InputFunnel\Packages\LandRecords\AdditionalFiles\No Filtering.FSD" "$(IcoMapESRIInstallFilesRootDir)\ReusableComponents"
    @COPY /V  "$(EngineeringRootDirectory)\ReusableComponents\InputFunnel\Packages\LandRecords\AdditionalFiles\Simple Maps.FSD" "$(IcoMapESRIInstallFilesRootDir)\ReusableComponents"
    @COPY /V  "$(EngineeringRootDirectory)\ReusableComponents\InputFunnel\Packages\LandRecords\AdditionalFiles\Start-End Directions.FOD" "$(IcoMapESRIInstallFilesRootDir)\ReusableComponents"
# Create RegList.dat file for registration
	@DIR "$(IcoMapESRIInstallFilesRootDir)\SelfRegProgFile\Bin\*.*" /b >"$(IcoMapESRIInstallFilesRootDir)\ProgramFiles\Bin\IcoMap.rl"
	@DIR "$(IcoMapESRIInstallFilesRootDir)\SelfRegRC\*.*" /b >"$(IcoMapESRIInstallFilesRootDir)\ReusableComponents\IcoMapCommon.rl"
    @DeleteFiles "$(IcoMapESRIInstallFilesRootDir)\vssver.scc"
    @DeleteFiles "$(IcoMapESRIInstallFilesRootDir)\mssccprj.scc"

CopyComponentVersionFile:
	@IF NOT EXIST "$(EngineeringRootDirectory)\ProductDevelopment\Common" MKDIR "$(EngineeringRootDirectory)\ProductDevelopment\Common"
	@CD "$(EngineeringRootDirectory)\ProductDevelopment\Common"
	$(Get) $$/Engineering/ProductDevelopment/Common $(GetOptions) -V"$(IcoMapESRIVersion)"	
	@COPY /V  "$(EngineeringRootDirectory)\ProductDevelopment\Common\LatestComponentVersions.mak" "$(ProductReleaseBleedingEdgeDir)\IcoMapForArcGIS\ComponentsVersions.txt"

LabelCommonFolder:
	$(Label) $$/Engineering/ProductDevelopment/Common -I- -L"$(IcoMapESRIVersion)" -O

BuildIcoMapESRIInstall: CopyIcoMapESRIFilesToInstallDir
	@ECHO Buliding the main UCLID Software installation...
	@SET PATH=$(WINDIR);$(WINDIR)\System32;$(BinariesFolder);I:\Common\Engineering\Tools\Utils;$(VSS_DIR)\win32;$(ReusableComponentsRootDirectory)\APIs\Nuance_18\bin;$(ReusableComponentsRootDirectory)\APIs\LeadTools_16.5\Bin;$(ReusableComponentsRootDirectory)\APIs\RogueWave\bin;$(ReusableComponentsRootDirectory)\APIs\SafeNetUltraPro\Bin;$(DEVENVDIR);$(VCPP_DIR)\BIN;$(VS_COMMON)\Tools;$(VS_COMMON)\Tools\bin;$(VCPP_DIR)\PlatformSDK\bin;$(VISUAL_STUDIO)\SDK\v2.0\bin;C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727;$(VCPP_DIR)\VCPackages
	$(SetProductVerScript) "$(IcoMapESRIInstallProjectRootDir)\IcoMap For ArcGIS\Extract Systems IcoMap For ArcGIS.ism" "$(IcoMapESRIVersion)"
    @"$(DEV_STUDIO_DIR)\System\IsCmdBld.exe" -p "$(IcoMapESRIInstallProjectRootDir)\IcoMap For ArcGIS\Extract Systems IcoMap For ArcGIS.ism"

CreateExtractLMInstallCD: BuildIcoMapESRIProduct
	@ECHO Creating License Manager Install...
	@CD ""$(ReusableComponentsRootDirectory)\VendorSpecificUtils\SafeNetUtils\Build"
    @nmake /F LicenseManager.mak BuildConfig="Release" ProductRootDirName="$(ProductRootDirName)" CreateIcoMapLMInstall
	
CreateIcoMapESRIInstallCD: BuildIcoMapESRIInstall  
	@IF NOT EXIST "$(ProductReleaseBleedingEdgeDir)\IcoMapForArcGIS" MKDIR "$(ProductReleaseBleedingEdgeDir)\IcoMapForArcGIS"
	@XCOPY "$(IcoMapESRIInstallMediaDir)\*.*" "$(ProductReleaseBleedingEdgeDir)\IcoMapForArcGIS" /v /s /e /y
	$(VerifyDir) "$(IcoMapESRIInstallMediaDir)" "$(ProductReleaseBleedingEdgeDir)\IcoMapForArcGIS"
	@DeleteFiles "$(ProductReleaseBleedingEdgeDir)\IcoMapForArcGIS\vssver.scc"
	
CopyGridGenratorFiles: CopyIcoMapESRIFilesToInstallDir
	@ECHO Copy Grid Generator Files
	@IF NOT EXIST "$(ProductReleaseBleedingEdgeDir)\GridGenerator" MKDIR "$(ProductReleaseBleedingEdgeDir)\GridGenerator"
	@COPY /V "$(BinariesFolder)\GridGenerator.dll" "$(ProductReleaseBleedingEdgeDir)\GridGenerator" 
	@COPY /V "$(GridGeneratorDirectory)\Misc\GridGenerator.ini" "$(ProductReleaseBleedingEdgeDir)\GridGenerator" 
	
DoEverything: DisplayTimeStamp BuildIcoMapESRIProduct CreateIcoMapESRIInstallCD CopyComponentVersionFile CreateExtractLMInstallCD CopyGridGenratorFiles
    @ECHO.
	@DATE /T
	@TIME /T
	@ECHO.
	@ECHO Build process completed.
	@ECHO.
