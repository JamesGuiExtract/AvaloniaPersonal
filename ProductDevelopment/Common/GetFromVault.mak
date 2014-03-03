
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

GetCommon:
	@ECHO Getting Common ...
	@ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
	@IF NOT EXIST "$(EngineeringRootDirectory)\ProductDevelopment\Common" MKDIR "$(EngineeringRootDirectory)\ProductDevelopment\Common"
	$(Get) $(GetOptions) -nonworkingfolder "$(EngineeringRootDirectory)\ProductDevelopment\Common" $$$(Branch)/Engineering/ProductDevelopment/Common  "$(FlexIndexVersion)"
	@nmake /F "$(EngineeringRootDirectory)\ProductDevelopment\Common\CompareVersions.mak" ProductVersion="$(FlexIndexVersion)" PDRootDir="$(PDRootDir)" DoCheck
	@ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
	
GetProductDevelopment:
	@ECHO Getting ProductDevelopment for $(FlexIndexVersion)
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
	@IF NOT EXIST "$(EngineeringRootDirectory)\ProductDevelopment" MKDIR "$(EngineeringRootDirectory)\ProductDevelopment"
	$(Get) $(GetOptions) -nonworkingfolder "$(EngineeringRootDirectory)\ProductDevelopment" $$$(Branch)/Engineering/ProductDevelopment  "$(FlexIndexVersion)"
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
	
	
GetRCDotNET:
	@ECHO Getting RC.Net for $(FlexIndexVersion)
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
	@IF NOT EXIST "$(EngineeringRootDirectory)\RC.Net" MKDIR "$(EngineeringRootDirectory)\RC.Net"
	$(Get) $(GetOptions) -nonworkingfolder "$(EngineeringRootDirectory)\RC.Net" $$$(Branch)/Engineering/RC.Net  "$(FlexIndexVersion)"
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.

GetReusableComponents:
	@ECHO Getting ReusableComponents for $(FlexIndexVersion)
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
	@IF NOT EXIST "$(EngineeringRootDirectory)\ReusableComponents" MKDIR "$(EngineeringRootDirectory)\ReusableComponents"
	$(Get) $(GetOptions) -nonworkingfolder "$(EngineeringRootDirectory)\ReusableComponents" $$$(Branch)/Engineering/ReusableComponents  "$(FlexIndexVersion)"
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.

GetRules:
	@ECHO Getting Rules needed for $(FlexIndexVersion)
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
	@IF NOT EXIST "$(RulesDir)\ComponentData" MKDIR "$(RulesDir)\ComponentData"
	$(Get) $(GetOptions) -nonworkingfolder "$(RulesDir)\ComponentData" "$$$(Branch)/Engineering/Rules/ComponentData"  "$(FKBVersion)"
	@IF NOT EXIST "$(RulesDir)\FLEXIndex\Demo_FLEXIndex" MKDIR "$(RulesDir)\FLEXIndex\Demo_FLEXIndex"
	$(Get) $(GetOptions) -nonworkingfolder "$(RulesDir)\FLEXIndex\Demo_FLEXIndex" "$$$(Branch)/Engineering/Rules/FLEXIndex/Demo_FLEXIndex"  "$(FlexIndexVersion)" Rules/FLEXIndex/Demo_FLEXIndex
	@IF NOT EXIST "$(RulesDir)\IDShield\Demo_IDShield" MKDIR "$(RulesDir)\IDShield\Demo_IDShield"
	$(Get) $(GetOptions) -nonworkingfolder "$(RulesDir)\IDShield\Demo_IDShield" "$$$(Branch)/Engineering/Rules/IDShield/Demo_IDShield"  "$(FlexIndexVersion)" Rules/IDShield/Demo_IDShield
	@IF NOT EXIST "$(RulesDir)\LabDE\Demo_LabDE" MKDIR "$(RulesDir)\LabDE\Demo_LabDE"
	$(Get) $(GetOptions) -nonworkingfolder "$(RulesDir)\LabDE\Demo_LabDE" "$$$(Branch)/Engineering/Rules/LabDE/Demo_LabDE"  "$(FlexIndexVersion)" Rules/LabDE/Demo_LabDE
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.

GetEngineering: GetCommon GetProductDevelopment GetRCDotNET GetReusableComponents GetRules
	@ECHO Updating Versions for $(FlexIndexVersion)
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
	@IF NOT EXIST "$(EngineeringRootDirectory)" MKDIR "$(EngineeringRootDirectory)"
	$(BUILD_DRIVE)
	@CD "$(EngineeringRootDirectory)"
    @SendFilesAsArgumentToApplication *.rc 1 1 $(UpdateFileVersion) "$(FlexIndexVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cs 1 1 $(UpdateFileVersion) "$(FlexIndexVersion)"
	@SendFilesAsArgumentToApplication AssemblyInfo.cpp 1 1 $(UpdateFileVersion) "$(FlexIndexVersion)"
	@CD "$(RCDotNETDir)"
	$(UpdateFileVersion)  "$(DataEntryBranding)\FlexIndex.resx" "$(FlexIndexVersion)"
	$(UpdateFileVersion)  "$(DataEntryBranding)\LabDE.resx" "$(FlexIndexVersion)"
	@CD "$(DataEntryDir)\FlexIndex"
	@SendFilesAsArgumentToApplication *.resx 1 1 $(UpdateFileVersion) "$(FlexIndexVersion)"
	@CD "$(DataEntryDir)\LabDE"
	@SendFilesAsArgumentToApplication *.resx 1 1 $(UpdateFileVersion) "$(FlexIndexVersion)"
    @ECHO.
    @DATE /T
    @TIME /T
    @ECHO.
	

