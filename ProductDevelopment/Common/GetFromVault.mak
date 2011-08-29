
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
Branch=

Get=vault GETLABEL 
GetOptions=-server $(VAULT_SERVER) -repository $(VAULT_REPOSITORY) -makewritable 

GetPDCommonFiles:
	@ECHO Getting Common folder for $(ProductVersion)
    @IF NOT EXIST "$(PDRootDir)\Common" MKDIR "$(PDRootDir)\Common"
	$(BUILD_DRIVE) 
    @CD  "$(PDRootDir)\Common"
    $(Get) $(GetOptions) -nonworkingfolder "$(PDRootDir)\Common" $$$(Branch)/Engineering/ProductDevelopment/Common  "$(ProductVersion)" 
	
GetAttributeFinderFiles:
	@ECHO Getting $(FlexIndexVersion) 
	@IF NOT EXIST "$(AFRootDirectory)" @MKDIR "$(AFRootDirectory)"
	$(BUILD_DRIVE) 
    @CD "$(AFRootDirectory)"
    $(Get) $(GetOptions) -nonworkingfolder "$(AFRootDirectory)" $$$(Branch)/Engineering/ProductDevelopment/AttributeFinder  "$(FlexIndexVersion)"
    @SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(FlexIndexVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cs 1 1 $(UpdateFileVersion) "$(FlexIndexVersion)"

GetRCdotNETFiles: 
    @ECHO Getting $(RCDotNetVersion) 
	@IF NOT EXIST "$(RCDotNETDir)" @MKDIR "$(RCDotNETDir)"
	$(BUILD_DRIVE) 
	@CD "$(RCDotNETDir)"
    $(Get) $(GetOptions) -nonworkingfolder "$(RCDotNETDir)" "$$$(Branch)/Engineering/RC.Net" "$(RCDotNetVersion)"
    @SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(RCDotNetVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cs 1 1 $(UpdateFileVersion) "$(RCDotNetVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cpp 1 1 $(UpdateFileVersion) "$(RCDotNetVersion)"
	
GetReusableComponentFiles:
	@ECHO Getting $(ReusableComponentsRootDirectory) ...
	@IF NOT EXIST "$(ReusableComponentsRootDirectory)" @MKDIR "$(ReusableComponentsRootDirectory)"
	$(BUILD_DRIVE) 
	@CD "$(ReusableComponentsRootDirectory)"
	@$(Get) $(GetOptions) -nonworkingfolder "$(ReusableComponentsRootDirectory)" $$$(Branch)/Engineering/ReusableComponents "$(ReusableComponentsVersion)"
	@SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(ReusableComponentsVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cs 1 1 $(UpdateFileVersion) "$(ReusableComponentsVersion)"

GetPDUtilsFiles :
    @ECHO Getting $(PDUtilsVersion)
    @ECHO Please wait...
    @IF NOT EXIST "$(PDUtilsRootDir)" @MKDIR "$(PDUtilsRootDir)"
	$(BUILD_DRIVE) 
    @CD "$(PDUtilsRootDir)"
    $(Get) $(GetOptions) -nonworkingfolder "$(PDUtilsRootDir)" $$$(Branch)/Engineering/ProductDevelopment/Utils "$(PDUtilsVersion)"
    @SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(PDUtilsVersion)"

GetIDShieldOfficeFiles: 
	@ECHO Getting $(IDShieldOfficeDir) ...
	@ECHO Please wait...
	@IF NOT EXIST "$(IDShieldOfficeDir)" @MKDIR "$(IDShieldOfficeDir)"
	$(BUILD_DRIVE) 
	@CD "$(IDShieldOfficeDir)"
	$(Get) $(GetOptions) -nonworkingfolder "$(IDShieldOfficeDir)" "$$$(Branch)/Engineering/ProductDevelopment/IDShieldOffice" "$(IDShieldOfficeVersion)"
    @SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(IDShieldOfficeVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cs 1 1 $(UpdateFileVersion) "$(IDShieldOfficeVersion)"

GetDataEntryInstall:
	@ECHO Getting $(LabDEDir)\Installation\DataEntry
	@ECHO Please wait...
	@IF NOT EXIST "$(LabDEDir)\Installation\DataEntry" @MKDIR "$(LabDEDir)\Installation\DataEntry"
	$(BUILD_DRIVE) 
	@CD "$(LabDEDir)\Installation\DataEntry"
	$(Get) $(GetOptions) -nonworkingfolder "$(LabDEDir)\Installation\DataEntry" "$$$(Branch)/Engineering/ProductDevelopment/LabDE/Installation/DataEntry" "$(LabDEVersion)"
 	
GetLabDEFiles:
	@ECHO Getting $(LabDEDir) ...
	@ECHO Please wait...
	@IF NOT EXIST "$(LabDEDir)" @MKDIR "$(LabDEDir)"
	$(BUILD_DRIVE) 
	@CD "$(LabDEDir)"
	$(Get) $(GetOptions) -nonworkingfolder "$(LabDEDir)" "$$$(Branch)/Engineering/ProductDevelopment/LabDE" "$(LabDEVersion)"
    @SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(LabDEVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cs 1 1 $(UpdateFileVersion) "$(LabDEVersion)"

GetLaserFicheFiles: 
    @IF NOT EXIST "$(LaserFicheDir)" @MKDIR "$(LaserFicheDir)"
    @ECHO Getting files from LaserFiche folder
    @ECHO Please wait...
	$(BUILD_DRIVE) 
    @CD "$(LaserFicheDir)"
    $(Get) $(GetOptions) -nonworkingfolder "$(LaserFicheDir)" $$$(Branch)/Engineering/ProductDevelopment/AFIntegrations/Laserfiche "$(LaserficheVersion)"
    @SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(LaserficheVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cs 1 1 $(UpdateFileVersion) "$(LaserficheVersion)"

GetComponentDataFiles:
	@ECHO Getting ComponentDataFiles $(FKBVersion)
	@IF NOT EXIST "$(RulesDir)\ComponentData" @MKDIR "$(RulesDir)\ComponentData"
	$(BUILD_DRIVE) 
	@CD "$(RulesDir)\ComponentData"
	@$(Get) $(GetOptions) -nonworkingfolder "$(RulesDir)\ComponentData" $$/Engineering/Rules/ComponentData "$(FKBVersion)"
	
GetDemo_IDShieldRules:
	@ECHO Getting Demo_IDShield Rules $(RulesVersion)
	@IF NOT EXIST "$(RulesDir)\IDShield\Demo_IDShield\Rules" @MKDIR "$(RulesDir)\IDShield\Demo_IDShield\Rules"
	$(BUILD_DRIVE) 
	@CD "$(RulesDir)\IDShield\Demo_IDShield\Rules"
	@$(Get) $(GetOptions) -nonworkingfolder "$(RulesDir)\IDShield\Demo_IDShield\Rules" $$/Engineering/Rules/IDShield/Demo_IDShield/Rules "$(RulesVersion)"

GetDemo_FLEXIndexRules:
	@ECHO Getting Demo_FLEXIndex Rules $(RulesVersion)
	@IF NOT EXIST "$(RulesDir)\FLEXIndex\Demo_FLEXIndex\Rules" @MKDIR "$(RulesDir)\FLEXIndex\Demo_FLEXIndex\Rules"
	$(BUILD_DRIVE) 
	@CD "$(RulesDir)\FLEXIndex\Demo_FLEXIndex\Rules"
	@$(Get) $(GetOptions) -nonworkingfolder "$(RulesDir)\FLEXIndex\Demo_FLEXIndex\Rules" $$/Engineering/Rules/FLEXIndex/Demo_FLEXIndex/Rules "$(RulesVersion)"

GetDemo_LabDERules:
	@ECHO Getting Demo_LabDE Rules $(RulesVersion)
	@IF NOT EXIST "$(RulesDir)\LabDE\Demo_LabDE\Solution" @MKDIR "$(RulesDir)\LabDE\Demo_LabDE\Solution"
	$(BUILD_DRIVE) 
	@CD "$(RulesDir)\LabDE\Demo_LabDE\Solution"
	@$(Get) $(GetOptions) -nonworkingfolder "$(RulesDir)\LabDE\Demo_LabDE\Solution" $$/Engineering/Rules/LabDE/Demo_LabDE/Solution "$(RulesVersion)"

