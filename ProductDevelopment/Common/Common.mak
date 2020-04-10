#############################################################################
# E N S U R E   P R E - C O N D I T I O N S   A R E   M E T
#
# TODO: make sure that the necessary variables like VCPP_DIR
# etc, are defined as environment variables.
#
# The user $(ProductRootDirName) must be defined
#
!IF "$(BUILD_DRIVE)" == ""
!ERROR Environment variable 'BUILD_DRIVE' must be defined
!ENDIF
#!IF "$(BUILD_DIRECTORY)" == ""
#!ERROR Environment variable 'BUILD_DRIVE' must be defined
#!ENDIF

#!IF "$(ProductRootDirName)" == ""
#!ERROR Build variable 'ProductRootDirName' must be defined (e.g. "Standalone IcoMap")
#!ENDIF

!include LatestComponentVersions.mak
!include $(BUILD_VSS_ROOT)\Engineering\Rules\Build_FKB\FKBVersion.mak

#############################################################################
# M A K E F I L E   V A R I A B L E S
#
BuildRootDirectory=$(BUILD_DRIVE)$(BUILD_DIRECTORY)\$(ProductRootDirName)
EngineeringRootDirectory=z:\Engineering
PDUtilsRootDir=$(EngineeringRootDirectory)\ProductDevelopment\Utils
ReusableComponentsRootDirectory=$(EngineeringRootDirectory)\ReusableComponents
CommonDirectory=$(EngineeringRootDirectory)\ProductDevelopment\Common
FXCopAppDirectory=$(FX_COP)
MergeModuleDir=$(MERGE_MODULE_DIR)
StrongNameKeyDir=P:\StrongNameKey
RCNETDir=$(EngineeringRootDirectory)\RC.Net

# Create macros for the Visual C++ include and lib directories
VcppIncludeDirs=$(VCPP_DIR)\ATLMFC\INCLUDE;$(VCPP_DIR)\INCLUDE;$(WINDOWS_SDK)\include;
VcppLibDirs=$(VCPP_DIR)\ATLMFC\LIB;$(VCPP_DIR)\LIB;$(WINDOWS_SDK)\lib

# Set the include and lib paths to the paths of all reusable components that 
# are going to be used + the paths of VC++ itself
IncludeDirs=$(VcppIncludeDirs)
LibDirs=$(VcppLibDirs)

CScriptProgram=$(CSCRIPT_PATH)\cscript

DelOptions=/Q /F /S
ISCompile=Compile
ISCompileOptions=-I"$(INSTALL_SHIELD_DIR)\Include"
InstallBuilder=ISBuild
PerlInterpreter=$(PERL_BIN_DIR)\Perl.exe
VerifyDir=$(EngineeringRootDirectory)\ProductDevelopment\Common\VerifyDir.bat
SetProductVerScript=$(CScriptProgram) "$(CommonDirectory)\SetProductVersion.vbs"
UpdateFileVersion="I:\Common\Engineering\Tools\Utils\UpdateFileVersion\UpdateFileVersion.exe"
DataEntryBranding=$(RCNETDir)\DataEntry\Utilities\DataEntryApplication\Core\Code\BrandingResources

#Target path for symbolic links to shared installs - must have the ending 
SharedInstallsPath=$(NAS_BUILD_BASE)\SharedInstalls
MakeCommonLinks=$(CommonDirectory)\PowerShell\MakeLinksToCommonInstallsOnNas.ps1
MakeSymLink=$(CommonDirectory)\PowerShell\MakeSymLinkOnNas.ps1
LinkShared=powershell -NoProfile -ExecutionPolicy Bypass -Command

#Version specific paths for install files
AFBleedingEdgeDir=R:\FlexIndex\Internal\BleedingEdge
BleedingEdgeVersionDir=$(AFBleedingEdgeDir)\$(FlexIndexVersion)
BleedingEdgeVersionUNCDir=$(NAS_BUILD_BASE)\FlexIndex\Internal\BleedingEdge\$(FlexIndexVersion)

#Extract Platform installer install paths
ExtractSoftware=$(BleedingEdgeVersionDir)
ExtractSoftwareMKLink=$(BleedingEdgeVersionUNCDir)
ExtractSoftwareInstallFiles=$(ExtractSoftware)\Install
FLEXIndexDemo=$(ExtractSoftware)\Demo_FlexIndex
IDShieldDemo=$(ExtractSoftware)\Demo_IDShield
LabDEDemo=$(ExtractSoftware)\Demo_LabDE
FLEXIndexSilentInstallDir=$(ExtractSoftware)\SilentInstalls

#Git tags converted from label
GitTagFlexIndexVersion=$(FlexIndexVersion:v.=)
GitTagFlexIndexVersion=$(GitTagFlexIndexVersion:/=)
GitTagFlexIndexVersion=$(GitTagFlexIndexVersion:Ver. =)
GitTagFlexIndexVersion=$(GitTagFlexIndexVersion: =/)
GitTagFlexIndexVersion=$(GitTagFlexIndexVersion:.=/)
GitTagFlexIndexVersion=$(GitTagFlexIndexVersion:-=/)

GitTagFKBVersion=$(FKBVersion:v.=)
GitTagFKBVersion=$(GitTagFKBVersion:/=)
GitTagFKBVersion=$(GitTagFKBVersion:Ver. =)
GitTagFKBVersion=$(GitTagFKBVersion: =/)
GitTagFKBVersion=$(GitTagFKBVersion:.=/)
GitTagFKBVersion=$(GitTagFKBVersion:-=/)


#############################################################################
# C O M M O N   T A R G E T S
#
# Many times, users forget to specify target, and as a result
# The first target is automatically built.  Define the first
# target so that it displays an error message
CommonEntryPoint:
	@ECHO.
	@ECHO Please specify target that you would like to build!
	@ECHO.

SetupBuildEnv:
	@SET INCLUDE=$(IncludeDirs)
	@SET LIB=$(LibDirs)
	@SET LIBPATH=C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319;$(VCPP_DIR)\ATLMFC\LIB
	
DisplayTimeStamp:
	@ECHO.
	@DATE /T
	@TIME /T
	@ECHO.