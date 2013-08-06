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
MergeModuleDir=C:\InstallShield 2010 Projects\MergeModules
StrongNameKeyDir=P:\StrongNameKey
RCNETDir=$(EngineeringRootDirectory)\RC.Net

# Create macros for the Visual C++ include and lib directories
VcppIncludeDirs=$(VCPP_DIR)\ATLMFC\INCLUDE;$(VCPP_DIR)\INCLUDE;$(WINDOWS_SDK)\include;
VcppLibDirs=$(VCPP_DIR)\ATLMFC\LIB;$(VCPP_DIR)\LIB;$(WINDOWS_SDK)\lib

# Set the include and lib paths to the paths of all reusable components that 
# are going to be used + the paths of VC++ itself
IncludeDirs=$(VcppIncludeDirs)
LibDirs=$(VcppLibDirs)

Label="$(VAULT_DIR)\vault" LABEL -server $(VAULT_SERVER) -repository "Extract"
DelOptions=/Q /F /S
ISCompile=Compile
ISCompileOptions=-I"$(INSTALL_SHIELD_DIR)\Include"
InstallBuilder=ISBuild
PerlInterpreter=$(PERL_BIN_DIR)\Perl.exe
VerifyDir=$(EngineeringRootDirectory)\ProductDevelopment\Common\VerifyDir.bat
SetProductVerScript=CScript "$(CommonDirectory)\SetProductVersion.vbs"
UpdateFileVersion="I:\Common\Engineering\Tools\Utils\UpdateFileVersion\UpdateFileVersion.exe"

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