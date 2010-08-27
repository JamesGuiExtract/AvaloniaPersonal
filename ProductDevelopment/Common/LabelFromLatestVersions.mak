
!include LatestComponentVersions.mak

Label="C:\Program Files\SourceGear\Vault Client\vault" LABEL -server white.extract.local -repository "Extract"

LabelCommonDir:
	$(Label) $$/Engineering/ProductDevelopment/AttributeFinder "$(FlexIndexVersion)"
	$(Label) $$/Engineering/ProductDevelopment/IcoMapESRI "$(IcoMapESRIVersion)" 
	$(Label) $$/Engineering/ProductDevelopment/AFIntegrations/Laserfiche "$(LaserficheVersion)"
	$(Label) $$/Engineering/ProductDevelopment/AFIntegrations/SharePoint "$(FlexIDSSPVersion)"
	$(Label) "$$/Engineering/ProductDevelopment/IDShieldOffice" "$(IDShieldOfficeVersion)"
	$(Label) "$$/Engineering/ProductDevelopment/LabDE" "$(LabDEVersion)"
	$(Label) $$/Engineering/ProductDevelopment/IcoMapCore "$(IcoMapCoreVersion)"
	$(Label) $$/Engineering/ReusableComponents "$(ReusableComponentsVersion)" 
	$(Label) $$/Engineering/ProductDevelopment/Utils "$(PDUtilsVersion)"
	$(Label) $$/Engineering/ProductDevelopment/PlatformSpecificUtils "$(PlatformSpecificUtilsVersion)"
	$(Label) "$$/Engineering/RC.Net" "$(RCDotNetVersion)"
	$(Label) "$$/Engineering/Rules" "$(RulesVersion)"
	$(Label) "$$/Branches/InternalRules" "$(RulesVersion)"
	$(Label) $$/Engineering/ProductDevelopment/Common "$(FlexIndexVersion)"
	$(Label) $$/Engineering/ProductDevelopment/Common "$(IcoMapESRIVersion)" 
	$(Label) $$/Engineering/ProductDevelopment/Common "$(LaserficheVersion)"
	$(Label) $$/Engineering/ProductDevelopment/Common "$(FlexIDSSPVersion)"
	$(Label) $$/Engineering/ProductDevelopment/Common "$(IDShieldOfficeVersion)"
	$(Label) $$/Engineering/ProductDevelopment/Common "$(LabDEVersion)"
	
