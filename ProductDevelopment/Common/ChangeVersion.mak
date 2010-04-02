
#############################################################################
# E N S U R E   P R E - C O N D I T I O N S   A R E   M E T
#
# The user must tell which configuration to build (e.g. "Win32 Release")
# Otherwise, we cannot continue.
#

!include LatestComponentVersions.mak
#############################################################################
# M A K E F I L E   V A R I A B L E S
#
!IF "$(EngineeringDir)" == ""
!ERROR Must define EngineeringDir eg nmake /f ChangeVersion.mak EngineeringDir="D:\Engineering"
!ENDIF

PDRootDir=$(EngineeringDir)\ProductDevelopment
AFRootDirectory=$(PDRootDir)\AttributeFinder
RCDotNETDir=$(EngineeringDir)\RC.Net
ReusableComponentsRootDirectory=$(EngineeringDir)\ReusableComponents
PDUtilsRootDir=$(EngineeringDir)\ProductDevelopment\Utils
IDShieldOfficeDir=$(PDRootDir)\IDShieldOffice
LabDEDir=$(PDRootDir)\LabDE
LaserFicheDir=$(PDRootDir)\AFIntegrations\Laserfiche
UpdateFileVersion="I:\Common\Engineering\Tools\Utils\UpdateFileVersion\UpdateFileVersion.exe"

SetVersionAttributeFinderFiles:
	@ECHO Setting Version to $(FlexIndexVersion) 
    @CD "$(AFRootDirectory)"
    @SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(FlexIndexVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cs 1 1 $(UpdateFileVersion) "$(FlexIndexVersion)"

SetVersionRCdotNETFiles: 
    @ECHO Setting Version to $(RCDotNetVersion) 
	@CD "$(RCDotNETDir)"
	@SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(RCDotNetVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cs 1 1 $(UpdateFileVersion) "$(RCDotNetVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cpp 1 1 $(UpdateFileVersion) "$(RCDotNetVersion)"
	
SetVersionReusableComponentFiles:
	@ECHO Setting Version to $(ReusableComponentsRootDirectory) ...
	@CD "$(ReusableComponentsRootDirectory)"
	@SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(ReusableComponentsVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cs 1 1 $(UpdateFileVersion) "$(ReusableComponentsVersion)"

SetVersionPDUtilsFiles :
    @ECHO Setting Version to  $(PDUtilsVersion)
    @ECHO Please wait...
    @CD "$(PDUtilsRootDir)"
    @SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(PDUtilsVersion)"

SetVersionIDShieldOfficeFiles: 
	@ECHO Setting Version to  $(IDShieldOfficeDir) ...
	@ECHO Please wait...
	@CD "$(IDShieldOfficeDir)"
    @SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(IDShieldOfficeVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cs 1 1 $(UpdateFileVersion) "$(IDShieldOfficeVersion)"

SetVersionLabDEFiles:
	@ECHO Setting Version to  $(LabDEDir) ...
	@ECHO Please wait...
	@CD "$(LabDEDir)"
    @SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(LabDEVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cs 1 1 $(UpdateFileVersion) "$(LabDEVersion)"

SetVersionLaserFicheFiles: 
    @ECHO Setting Version to  files from LaserFiche folder
    @ECHO Please wait...
    @CD "$(LaserFicheDir)"
    @SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(LaserficheVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cs 1 1 $(UpdateFileVersion) "$(LaserficheVersion)"

SetAllVersions: SetVersionAttributeFinderFiles SetVersionRCdotNETFiles SetVersionReusableComponentFiles SetVersionPDUtilsFiles SetVersionIDShieldOfficeFiles SetVersionLabDEFiles SetVersionLaserFicheFiles

