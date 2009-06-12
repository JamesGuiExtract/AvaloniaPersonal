
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

Get=ss get
GetOptions=-R -W -I-

GetPDCommonFiles:
	@ECHO Getting Common folder for $(ProductVersion)
    @IF NOT EXIST "$(PDRootDir)\Common" MKDIR "$(PDRootDir)\Common"
	$(BUILD_DRIVE) 
    @CD "$(PDRootDir)\Common"
    $(Get) $$/Engineering/ProductDevelopment/Common $(GetOptions) -VL"$(ProductVersion)" 
	
GetAttributeFinderFiles:
	@ECHO Getting $(FlexIndexVersion) 
	@IF NOT EXIST "$(AFRootDirectory)" @MKDIR "$(AFRootDirectory)"
	$(BUILD_DRIVE) 
    @CD "$(AFRootDirectory)"
    $(Get) $$/Engineering/ProductDevelopment/AttributeFinder $(GetOptions) -VL"$(FlexIndexVersion)"
    @SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(FlexIndexVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cs 1 1 $(UpdateFileVersion) "$(FlexIndexVersion)"

GetRCdotNETFiles: 
    @ECHO Getting $(RCDotNetVersion) 
	@IF NOT EXIST "$(RCDotNETDir)" @MKDIR "$(RCDotNETDir)"
	$(BUILD_DRIVE) 
	@CD "$(RCDotNETDir)"
    $(Get) "$$/Engineering/RC.Net" $(GetOptions) -VL"$(RCDotNetVersion)"
    @SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(RCDotNetVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cs 1 1 $(UpdateFileVersion) "$(RCDotNetVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cpp 1 1 $(UpdateFileVersion) "$(RCDotNetVersion)"
	
GetReusableComponentFiles:
	@ECHO Getting $(ReusableComponentsRootDirectory) ...
	@IF NOT EXIST "$(ReusableComponentsRootDirectory)" @MKDIR "$(ReusableComponentsRootDirectory)"
	$(BUILD_DRIVE) 
	@CD "$(ReusableComponentsRootDirectory)"
	@$(Get) $$/Engineering/ReusableComponents $(GetOptions) -VL"$(ReusableComponentsVersion)"
	@SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(ReusableComponentsVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cs 1 1 $(UpdateFileVersion) "$(ReusableComponentsVersion)"

GetPDUtilsFiles :
    @ECHO Getting $(PDUtilsVersion)
    @ECHO Please wait...
    @IF NOT EXIST "$(PDUtilsRootDir)" @MKDIR "$(PDUtilsRootDir)"
	$(BUILD_DRIVE) 
    @CD "$(PDUtilsRootDir)"
    $(Get) $$/Engineering/ProductDevelopment/Utils $(GetOptions) -VL"$(PDUtilsVersion)"
    @SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(PDUtilsVersion)"

GetIDShieldOfficeFiles: 
	@ECHO Getting $(IDShieldOfficeDir) ...
	@ECHO Please wait...
	@IF NOT EXIST "$(IDShieldOfficeDir)" @MKDIR "$(IDShieldOfficeDir)"
	$(BUILD_DRIVE) 
	@CD "$(IDShieldOfficeDir)"
	$(Get) "$$/Engineering/ProductDevelopment/IDShieldOffice" $(GetOptions)  -VL"$(IDShieldOfficeVersion)"
    @SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(IDShieldOfficeVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cs 1 1 $(UpdateFileVersion) "$(IDShieldOfficeVersion)"

GetLabDEFiles:
	@ECHO Getting $(LabDEDir) ...
	@ECHO Please wait...
	@IF NOT EXIST "$(LabDEDir)" @MKDIR "$(LabDEDir)"
	$(BUILD_DRIVE) 
	@CD "$(LabDEDir)"
	$(Get) "$$/Engineering/ProductDevelopment/LabDE" $(GetOptions)  -VL"$(LabDEVersion)"
    @SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(LabDEVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cs 1 1 $(UpdateFileVersion) "$(LabDEVersion)"

GetLaserFicheFiles: 
    @IF NOT EXIST "$(LaserFicheDir)" @MKDIR "$(LaserFicheDir)"
    @ECHO Getting files from LaserFiche folder 
    @ECHO Please wait...
	$(BUILD_DRIVE) 
    @CD "$(LaserFicheDir)"
    $(Get) $$/Engineering/ProductDevelopment/AFIntegrations/Laserfiche $(GetOptions) -VL"$(LaserficheVersion)"
    @SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(LaserficheVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cs 1 1 $(UpdateFileVersion) "$(LaserficheVersion)"

	