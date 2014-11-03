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

#############################################################################
# M A K E F I L E   V A R I A B L E S
#
BuildRootDirectory=$(BUILD_DRIVE)$(BUILD_DIRECTORY)\$(ProductRootDirName)
EngineeringRootDirectory=$(BuildRootDirectory)\Engineering
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

Label="$(VAULT_DIR)\vault" LABEL -server $(VAULT_SERVER) -repository "Extract"
DelOptions=/Q /F /S
ISCompile=Compile
ISCompileOptions=-I"$(INSTALL_SHIELD_DIR)\Include"
InstallBuilder=ISBuild
PerlInterpreter=$(PERL_BIN_DIR)\Perl.exe
VerifyDir=$(EngineeringRootDirectory)\ProductDevelopment\Common\VerifyDir.bat
SetProductVerScript=$(CScriptProgram) "$(CommonDirectory)\SetProductVersion.vbs"
UpdateFileVersion="I:\Common\Engineering\Tools\Utils\UpdateFileVersion\UpdateFileVersion.exe"

#Target path for symbolic links to shared installs - must have the ending 
SharedInstallsPath=$(BUILD_PRODUCT_RELEASE)\SharedInstalls
MakeCommonLinks=$(CommonDirectory)\PowerShell\MakeLinksToCommonInstalls.ps1
MakeSymLink=$(CommonDirectory)\PowerShell\MakeSymLink.ps1
LinkShared=powershell -NoProfile -ExecutionPolicy Bypass -Command

#Version specific paths for install files
AFBleedingEdgeDir=R:\FlexIndex\Internal\BleedingEdge
BleedingEdgeVersionDir=$(AFBleedingEdgeDir)\$(FlexIndexVersion)
BleedingEdgeVersionUNCDir=$(BUILD_PRODUCT_RELEASE)\FlexIndex\Internal\BleedingEdge\$(FlexIndexVersion)

#FLEXIndex install related paths
FLEXIndexSetupFiles=$(BleedingEdgeVersionDir)\FLEXIndex\SetupFiles
FLEXIndexSetupFilesUNC=$(BleedingEdgeVersionUNCDir)\FLEXIndex\SetupFiles
FLEXIndexInstallFiles=$(FLEXIndexSetupFiles)\FlexIndex
FLEXIndexDemo=$(FLEXIndexSetupFiles)\Demo_FlexIndex
FLEXIndexExtractLMDir=$(FLEXIndexSetupFiles)\Extract Systems LM
FLEXIndexInstallDir=$(FLEXIndexSetupFiles)\FlexIndexInstall
FLEXIndexSilentInstallDir=$(FLEXIndexSetupFiles)\SilentInstalls
FLEXIndexLinkShared=$(LinkShared) "& '$(MakeCommonLinks)' '$(FLEXIndexSetupFilesUNC)\' '$(SharedInstallsPath)\'"

#IDShield install related paths
IDShieldSetupFiles=$(BleedingEdgeVersionDir)\IDShield\SetupFiles
IDShieldSetupFilesUNC=$(BleedingEdgeVersionUNCDir)\IDShield\SetupFiles
IDShieldInstallFiles=$(IDShieldSetupFiles)\IDShield
IDShieldDemo=$(IDShieldSetupFiles)\Demo_IDShield
IDShieldExtractLMDir=$(IDShieldSetupFiles)\Extract Systems LM
IDShieldInstallDir=$(IDShieldSetupFiles)\IDShieldInstall
IDShieldSilentInstallDir=$(IDShieldSetupFiles)\SilentInstalls
IDShieldLinkShared=$(LinkShared) "& '$(MakeCommonLinks)' '$(IDShieldSetupFilesUNC)\' '$(SharedInstallsPath)\'"

#LabDE install related paths
LabDESetupFiles=$(BleedingEdgeVersionDir)\LabDE\SetupFiles
LabDESetupFilesUNC=$(BleedingEdgeVersionUNCDir)\LabDE\SetupFiles
LabDEInstallFiles=$(LabDESetupFiles)\LabDE
LabDEDemo=$(LabDESetupFiles)\Demo_LabDE
LabDEExtractLMDir=$(LabDESetupFiles)\Extract Systems LM
LabDEInstallDir=$(LabDESetupFiles)\LabDEInstall
LabDESilentInstallDir=$(LabDESetupFiles)\SilentInstalls
LabDELinkShared=$(LinkShared) "& '$(MakeCommonLinks)' '$(LabDESetupFilesUNC)\' '$(SharedInstallsPath)\'"
LabDECorePointLink=$(LinkShared) "& '$(MakeSymLink)' 'EngSvr' '$(LabDESetupFilesUNC)\' '$(SharedInstallsPath)\' 'Corepoint Integration Engine'"

#Other
OtherSetupFiles=$(BleedingEdgeVersionDir)\Other
IntegrationsSetupFiles=$(OtherSetupFiles)\Integrations

!IF "$(BUILD_FROM_SVN)" == "YES"
!include GetFromSVN.mak
!ELSE
!include GetFromVault.mak
!ENDIF

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
	
LabelCommonDir:
	$(Label) $$/Engineering/ProductDevelopment/Common "$(FlexIndexVersion)"
	
DisplayTimeStamp:
	@ECHO.
	@DATE /T
	@TIME /T
	@ECHO.