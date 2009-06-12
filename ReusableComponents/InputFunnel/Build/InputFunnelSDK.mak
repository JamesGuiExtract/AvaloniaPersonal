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
InputFunnelRootDirectory=$(EngineeringRootDirectory)\ProductDevelopment\InputFunnel
LRInputFunnelPackageRootDir=$(InputFunnelRootDirectory)\Packages\LandRecords

TestHarnessDir=$(ReusableComponentsRootDirectory)\COMComponents\UCLIDTestingFramework\TestHarness\Code

InputFunnelInstallFilesRootDir=I:\Common\Engineering\ProductDevelopment\InputFunnel\Installation\Files
InputFunnelInstallProjectRootDir=$(EngineeringRootDirectory)\ProductDevelopment\InputFunnel\Installation\UCLID InputFunnel SDK
InputFunnelUserDocRootDir=$(EngineeringRootDirectory)\ProductDevelopment\InputFunnel\UserDocumentation
InputFunnelInstallScriptFile=$(InputFunnelInstallProjectRootDir)\Script Files\setup.rul
InputFunnelInstallMediaDir=$(InputFunnelInstallProjectRootDir)\Media\CD-ROM\Disk Images\Disk1
InputFunnelCoreInstallMediaDir=$(EngineeringRootDirectory)\ProductDevelopment\InputFunnel\Installation\InputFunnelCore\Media\CD-ROM\Disk Images\Disk1

InputFunnelReleaseBleedingEdgeDir=I:\Common\Engineering\ProductReleases\InputFunnel\Internal\BleedingEdge\$(InputFunnelVersion)

MainUCLIDInstallProjectRootDir=$(EngineeringRootDirectory)\ProductDevelopment\UCLIDSoftwareInstallation
MainUCLIDInstallScriptFile=$(MainUCLIDInstallProjectRootDir)\Script Files\setup.rul
MainUCLIDInstallMediaDir=$(MainUCLIDInstallProjectRootDir)\Media\CD-ROM\Disk Images\Disk1

# determine the name of the release output directory based upon the build
# configuration that is being built
!IF "$(BuildConfig)" == "Win32 Release"
BuildOutputDir=Release
!ELSEIF "$(BuildConfig)" == "Win32 Debug"
BuildOutputDir=Debug
!ELSE
!ERROR Internal error - invalid value for BuildConfig variable!
!ENDIF

BinariesFolder=$(EngineeringRootDirectory)\Binaries\$(BuildOutputDir)

#############################################################################
# B U I L D    T A R G E T S
#
BuildInputFunnelCore:
    @CD "$(MAKEDIR)"
    @nmake /F InputFunnel.mak BuildConfig="Win32 Release" ProductRootDirName="$(ProductRootDirName)" DoEverything

BuildTestHarnessApps:
    @CD "$(TestHarnessDir)"
    @msdev TestHarness.dsp /MAKE "TestHarness - $(BuildConfig)" /USEENV

InitAutomatedTest:
    @ECHO @SET PATH=$(ReusableComponentsRootDirectory)\APIs\Nuance\bin;$(ReusableComponentsRootDirectory)\APIs\LeadTools\bin;%PATH% > temp.bat
    @ECHO @SET LOCAL_VSS_ROOT=$(EngineeringRootDirectory)\.. >> temp.bat
    @ECHO @SET TEST_RESULTS_FOLDER=$(EngineeringRootDirectory)\..\Testing>> temp.bat
    @ECHO @SET TEST_RESULTS_DB=%TEST_RESULTS_FOLDER%\TestResults.mdb >> temp.bat
    @ECHO @IF NOT EXIST %TEST_RESULTS_FOLDER%\Nul mkdir %TEST_RESULTS_FOLDER% >> temp.bat
    @ECHO @COPY /V  $(ReusableComponentsRootDirectory)\COMComponents\UCLIDTestingFramework\Core\Code\TestResults_template.mdb %TEST_RESULTS_DB% >> temp.bat
    @ECHO @CD /D $(BinariesFolder) >> temp.bat
    @ECHO @SendFilesAsArgumentToApplication $(BinariesFolder)\*.ocx 0 1 regsvr32 /s >> temp.bat
    @ECHO @SendFilesAsArgumentToApplication $(BinariesFolder)\*.dll 0 1 regsvr32 /s >> temp.bat
    @ECHO @$(BinariesFolder)\TestHarness.exe $(LRInputFunnelPackageRootDir)\Test\LandRecords.tcl >> temp.bat
    @temp.bat


CopyFilesToInstallFolder : 
    @ECHO Copying the InputFunnel files to installation directory...
    @COPY /V  "$(InputFunnelRootDirectory)\InputReceivers\HighlightedTextIR\Code\Core\HTCategories.h" "$(InputFunnelInstallFilesRootDir)\Include"
    @COPY /V  "$(InputFunnelRootDirectory)\IFCore\Code\IFCategories.h" "$(InputFunnelInstallFilesRootDir)\Include"
    @COPY /V  "$(InputFunnelRootDirectory)\Packages\LandRecords\AdditionalFiles\*.F?D" "$(InputFunnelInstallFilesRootDir)\ReusableComponents"
    @XCOPY "$(InputFunnelUserDocRootDir)\Tutorials\*.*" "$(InputFunnelInstallFilesRootDir)\Tutorials" /v /s /e /y
    $(VerifyDir) "$(InputFunnelUserDocRootDir)\Tutorials" "$(InputFunnelInstallFilesRootDir)\Tutorials"
    @DeleteFiles "$(InputFunnelInstallFilesRootDir)\vssver.scc"
    @DeleteFiles "$(InputFunnelInstallFilesRootDir)\mssccprj.scc"

CopyComponentVersionFile:
    @IF NOT EXIST "$(EngineeringRootDirectory)\ProductDevelopment\Common" MKDIR "$(EngineeringRootDirectory)\ProductDevelopment\Common"
    @CD "$(EngineeringRootDirectory)\ProductDevelopment\Common"
    $(Get) $$/Engineering/ProductDevelopment/Common $(GetOptions) -V"$(InputFunnelVersion)" 
    @COPY /V  "$(EngineeringRootDirectory)\ProductDevelopment\Common\LatestComponentVersions.mak" "$(InputFunnelReleaseBleedingEdgeDir)\ComponentsVersions.txt"

GetUCLIDSoftwareInstallFiles:
    @IF NOT EXIST "$(MainUCLIDInstallProjectRootDir)" @MKDIR "$(MainUCLIDInstallProjectRootDir)"
    @ECHO Getting $(UCLIDSoftwareInstallVersion) 
    @ECHO Please wait...
    @CD "$(MainUCLIDInstallProjectRootDir)"
    $(Get) $$/Engineering/ProductDevelopment/UCLIDSoftwareInstallation $(GetOptions) -V"$(UCLIDSoftwareInstallVersion)"

BuildInputFunnelInstall: GetUCLIDSoftwareInstallFiles CopyFilesToInstallFolder
    @ECHO Buliding the main UCLID Software installation...
    @$(ISCompile) "$(MainUCLIDInstallScriptFile)" $(ISCompileOptions) $(AdditionalISCompileOptions)
    @$(InstallBuilder) -p"$(MainUCLIDInstallProjectRootDir)" -m"CD-ROM"
    @ECHO Buliding the InputFunnel installation...
    @$(ISCompile) "$(InputFunnelInstallScriptFile)" $(ISCompileOptions) $(AdditionalISCompileOptions)
    @$(InstallBuilder) -p"$(InputFunnelInstallProjectRootDir)" -m"CD-ROM"
    
CreateUCLIDInstallDatFile:
    @ECHO Creating UCLIDInstall.dat file...
    @IF NOT EXIST "$(InputFunnelReleaseBleedingEdgeDir)" MKDIR "$(InputFunnelReleaseBleedingEdgeDir)"
    @ECHO Input Funnel SDK;InputFunnelSDK\setup.exe > "$(InputFunnelReleaseBleedingEdgeDir)\UCLIDInstall.dat"

CreateInputFunnelInstallCD: BuildInputFunnelInstall BuildWCUInstall CreateUCLIDInstallDatFile
    @IF NOT EXIST "$(InputFunnelReleaseBleedingEdgeDir)" MKDIR "$(InputFunnelReleaseBleedingEdgeDir)"
    @XCOPY "$(MainUCLIDInstallMediaDir)\*.*" "$(InputFunnelReleaseBleedingEdgeDir)" /v /s /e /y
    $(VerifyDir) "$(MainUCLIDInstallMediaDir)" "$(InputFunnelReleaseBleedingEdgeDir)"
    @IF NOT EXIST "$(InputFunnelReleaseBleedingEdgeDir)\InputFunnelSDK" MKDIR "$(InputFunnelReleaseBleedingEdgeDir)\InputFunnelSDK"
    @XCOPY "$(InputFunnelInstallMediaDir)\*.*" "$(InputFunnelReleaseBleedingEdgeDir)\InputFunnelSDK" /v /s /e /y
    $(VerifyDir) "$(InputFunnelInstallMediaDir)" "$(InputFunnelReleaseBleedingEdgeDir)\InputFunnelSDK"
    @IF NOT EXIST "$(InputFunnelReleaseBleedingEdgeDir)\WindowsComponentsUpdate" MKDIR "$(InputFunnelReleaseBleedingEdgeDir)\WindowsComponentsUpdate"
    @XCOPY "$(WinComponentsUpdateInstallMediaDir)\*.*" "$(InputFunnelReleaseBleedingEdgeDir)\WindowsComponentsUpdate" /v /s /e /y
    $(VerifyDir) "$(WinComponentsUpdateInstallMediaDir)" "$(InputFunnelReleaseBleedingEdgeDir)\WindowsComponentsUpdate"
    @IF NOT EXIST "$(InputFunnelReleaseBleedingEdgeDir)\InputFunnelCore" MKDIR "$(InputFunnelReleaseBleedingEdgeDir)\InputFunnelCore"
    @XCOPY "$(InputFunnelCoreInstallMediaDir)\*.*" "$(InputFunnelReleaseBleedingEdgeDir)\InputFunnelCore" /v /s /e /y
    $(VerifyDir) "$(InputFunnelCoreInstallMediaDir)" "$(InputFunnelReleaseBleedingEdgeDir)\InputFunnelCore"
    @DeleteFiles "$(InputFunnelReleaseBleedingEdgeDir)\vssver.scc"


DoNecessaryBuilds: SetupBuildEnv BuildInputFunnelCore BuildTestHarnessApps

DoEverything: DisplayTimeStamp DoNecessaryBuilds CreateInputFunnelInstallCD CopyComponentVersionFile
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
    @ECHO Input Funnel SDK Build process completed.
    @ECHO.
