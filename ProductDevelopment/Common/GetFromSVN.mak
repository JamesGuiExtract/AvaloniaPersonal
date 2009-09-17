
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
RCDotNETDir=$(EngineeringRootDirectory)\RC.Net
PDUtilsRootDir=$(EngineeringRootDirectory)\ProductDevelopment\Utils
IDShieldOfficeDir=$(PDRootDir)\IDShieldOffice
LabDEDir=$(PDRootDir)\LabDE
LaserFicheDir=$(PDRootDir)\AFIntegrations\Laserfiche
RulesDir=$(EngineeringRootDirectory)\Rules

SVN="C:\Program Files\CollabNet Subversion\svn.exe" export
SVNOptions=--force

# Getting from SVN assumes the files to get are based under $(SVN_REPOSITORY)/tags/$(ProductVersion)
GetPDCommonFiles:
	@ECHO Getting Common folder for $(ProductVersion)
    @IF NOT EXIST "$(PDRootDir)\Common" MKDIR "$(PDRootDir)\Common"
	$(BUILD_DRIVE) 
    @CD "$(PDRootDir)\Common"
    $(SVN) "$(SVN_REPOSITORY)/tags/$(ProductVersion)/Engineering/ProductDevelopment/Common" "$(PDRootDir)\Common" $(SVNOptions)
	
GetAttributeFinderFiles:
	@ECHO Getting $(FlexIndexVersion) 
	@IF NOT EXIST "$(AFRootDirectory)" @MKDIR "$(AFRootDirectory)"
	$(BUILD_DRIVE) 
    @CD "$(AFRootDirectory)"
    $(SVN) "$(SVN_REPOSITORY)/tags/$(ProductVersion)/Engineering/ProductDevelopment/AttributeFinder" "$(AFRootDirectory)" $(SVNOptions)
    @SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(FlexIndexVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cs 1 1 $(UpdateFileVersion) "$(FlexIndexVersion)"

GetRCdotNETFiles: 
    @ECHO Getting $(RCDotNetVersion) 
	@IF NOT EXIST "$(RCDotNETDir)" @MKDIR "$(RCDotNETDir)"
	$(BUILD_DRIVE) 
	@CD "$(RCDotNETDir)"
    $(SVN) "$(SVN_REPOSITORY)/tags/$(ProductVersion)/Engineering/RC.Net" "$(RCDotNETDir)" $(SVNOptions) 
    @SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(RCDotNetVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cs 1 1 $(UpdateFileVersion) "$(RCDotNetVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cpp 1 1 $(UpdateFileVersion) "$(RCDotNetVersion)"
	
GetReusableComponentFiles:
	@ECHO Getting $(ReusableComponentsRootDirectory) ...
	@IF NOT EXIST "$(ReusableComponentsRootDirectory)" @MKDIR "$(ReusableComponentsRootDirectory)"
	$(BUILD_DRIVE) 
	@CD "$(ReusableComponentsRootDirectory)"
	@$(SVN) "$(SVN_REPOSITORY)/tags/$(ProductVersion)/Engineering/ReusableComponents" "$(ReusableComponentsRootDirectory)" $(SVNOptions)
	@SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(ReusableComponentsVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cs 1 1 $(UpdateFileVersion) "$(ReusableComponentsVersion)"

GetPDUtilsFiles :
    @ECHO Getting $(PDUtilsVersion)
    @ECHO Please wait...
    @IF NOT EXIST "$(PDUtilsRootDir)" @MKDIR "$(PDUtilsRootDir)"
	$(BUILD_DRIVE) 
    @CD "$(PDUtilsRootDir)"
    $(SVN) "$(SVN_REPOSITORY)/tags/$(ProductVersion)/Engineering/ProductDevelopment/Utils" "$(PDUtilsRootDir)" $(SVNOptions)
    @SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(PDUtilsVersion)"

GetIDShieldOfficeFiles: 
	@ECHO Getting $(IDShieldOfficeDir) ...
	@ECHO Please wait...
	@IF NOT EXIST "$(IDShieldOfficeDir)" @MKDIR "$(IDShieldOfficeDir)"
	$(BUILD_DRIVE) 
	@CD "$(IDShieldOfficeDir)"
	$(SVN) "$(SVN_REPOSITORY)/tags/$(ProductVersion)/Engineering/ProductDevelopment/IDShieldOffice" "$(IDShieldOfficeDir)" $(SVNOptions)
    @SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(IDShieldOfficeVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cs 1 1 $(UpdateFileVersion) "$(IDShieldOfficeVersion)"

GetLabDEFiles:
	@ECHO Getting $(LabDEDir) ...
	@ECHO Please wait...
	@IF NOT EXIST "$(LabDEDir)" @MKDIR "$(LabDEDir)"
	$(BUILD_DRIVE) 
	@CD "$(LabDEDir)"
	$(SVN) "$(SVN_REPOSITORY)/tags/$(ProductVersion)/Engineering/ProductDevelopment/LabDE" "$(LabDEDir)" $(SVNOptions)
    @SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(LabDEVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cs 1 1 $(UpdateFileVersion) "$(LabDEVersion)"

GetLaserFicheFiles: 
    @IF NOT EXIST "$(LaserFicheDir)" @MKDIR "$(LaserFicheDir)"
    @ECHO Getting files from LaserFiche folder 
    @ECHO Please wait...
	$(BUILD_DRIVE) 
    @CD "$(LaserFicheDir)"
    $(SVN) "$(SVN_REPOSITORY)/tags/$(ProductVersion)/Engineering/ProductDevelopment/AFIntegrations/Laserfiche" "$(LaserFicheDir)" $(SVNOptions)
    @SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(LaserficheVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cs 1 1 $(UpdateFileVersion) "$(LaserficheVersion)"

GetComponentDataFiles:
	@ECHO Getting ComponentDataFiles $(RulesVersion)
	@IF NOT EXIST "$(RulesDir)\ComponentData" @MKDIR "$(RulesDir)\ComponentData"
	$(BUILD_DRIVE) 
	@CD "$(RulesDir)\ComponentData"
    $(SVN) "$(SVN_REPOSITORY)/tags/$(ProductVersion)/Engineering/Rules/ComponentData" "$(RulesDir)\ComponentData" $(SVNOptions)
	
GetDemo_IDShieldRules:
	@ECHO Getting Demo_IDShield Rules $(RulesVersion)
	@IF NOT EXIST "$(RulesDir)\IDShield\Demo_IDShield\Rules" @MKDIR "$(RulesDir)\IDShield\Demo_IDShield\Rules"
	$(BUILD_DRIVE) 
	@CD "$(RulesDir)\IDShield\Demo_IDShield\Rules"
    $(SVN) "$(SVN_REPOSITORY)/tags/$(ProductVersion)/Engineering/Rules/IDShield/Demo_IDShield/Rules" "$(RulesDir)\IDShield\Demo_IDShield\Rules" $(SVNOptions)

GetDemo_FLEXIndexRules:
	@ECHO Getting Demo_FLEXIndex Rules $(RulesVersion)
	@IF NOT EXIST "$(RulesDir)\FLEXIndex\Demo_FLEXIndex\Rules" @MKDIR "$(RulesDir)\FLEXIndex\Demo_FLEXIndex\Rules"
	$(BUILD_DRIVE) 
	@CD "$(RulesDir)\FLEXIndex\Demo_FLEXIndex\Rules"
    $(SVN) "$(SVN_REPOSITORY)/tags/$(ProductVersion)/Engineering/Rules/FLEXIndex/Demo_FLEXIndex/Rules" "$(RulesDir)\FLEXIndex\Demo_FLEXIndex\Rules" $(SVNOptions)

GetDemo_LabDERules:
	@ECHO Getting Demo_LabDE Rules $(RulesVersion)
	@IF NOT EXIST "$(RulesDir)\LabDE\Demo_LabDE\Rules" @MKDIR "$(RulesDir)\LabDE\Demo_LabDE\Rules"
	$(BUILD_DRIVE) 
	@CD "$(RulesDir)\LabDE\Demo_LabDE\Rules"
    $(SVN) "$(SVN_REPOSITORY)/tags/$(ProductVersion)/Engineering/Rules/LabDE/Demo_LabDE/Rules" "$(RulesDir)\LabDE\Demo_LabDE\Rules" $(SVNOptions)
