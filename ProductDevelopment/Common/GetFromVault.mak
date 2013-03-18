
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
DataEntryBranding=$(RCDotNETDir)\DataEntry\Utilities\DataEntryApplication\Core\Code\BrandingResources
PDUtilsRootDir=$(EngineeringRootDirectory)\ProductDevelopment\Utils
IDShieldOfficeDir=$(PDRootDir)\IDShieldOffice
DataEntryDir=$(PDRootDir)\DataEntry
LabDEDir=$(DataEntryDir)\LabDE
LaserFicheDir=$(PDRootDir)\AFIntegrations\Laserfiche
NetDMSDir=$(PDRootDir)\AFIntegrations\NetDMS
RulesDir=$(EngineeringRootDirectory)\Rules

Get="$(VAULT_DIR)\vault" GETLABEL 
GetOptions=-server $(VAULT_SERVER) -repository $(VAULT_REPOSITORY) -makewritable 


GetEngineering:
	@ECHO Getting Engineering for $(FlexIndexVersion)
	@IF NOT EXIST "$(EngineeringRootDirectory)" MKDIR "$(EngineeringRootDirectory)"
	$(BUILD_DRIVE)
	@CD "$(EngineeringRootDirectory)"
	$(Get) $(GetOptions) -nonworkingfolder "$(EngineeringRootDirectory)" $$$(Branch)/Engineering  "$(FlexIndexVersion)" 
    @SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(FlexIndexVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cs 1 1 $(UpdateFileVersion) "$(FlexIndexVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cpp 1 1 $(UpdateFileVersion) "$(FlexIndexVersion)"
	@CD "$(RCDotNETDir)"
	$(UpdateFileVersion)  "$(DataEntryBranding)\FlexIndex.resx" "$(FlexIndexVersion)"
	$(UpdateFileVersion)  "$(DataEntryBranding)\LabDE.resx" "$(LabDEVersion)"
	@CD "$(DataEntryDir)\FlexIndex"
	@SendFilesAsArgumentToApplication *.resx 1 1 $(UpdateFileVersion) "$(FlexIndexVersion)"
	@CD "$(DataEntryDir)\LabDE"
	@SendFilesAsArgumentToApplication *.resx 1 1 $(UpdateFileVersion) "$(LabDEVersion)"
	

