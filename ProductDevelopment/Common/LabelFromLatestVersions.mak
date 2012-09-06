
!include LatestComponentVersions.mak

Label="C:\Program Files\SourceGear\Vault Client\vault" LABEL -server white.extract.local -repository "Extract"

LabelCommonDir:
	$(Label) $$/Engineering/ProductDevelopment/AttributeFinder "$(FlexIndexVersion)"
	$(Label) $$/Engineering/ProductDevelopment/AFIntegrations/Laserfiche "$(LaserficheVersion)"
	$(Label) $$/Engineering/ProductDevelopment/AFIntegrations/NetDMS "$(FlexIDSNetDMSVersion)"
	$(Label) $$/SharePoint "$(FlexIDSSPVersion)"
	$(Label) "$$/Engineering/ProductDevelopment/IDShieldOffice" "$(IDShieldOfficeVersion)"
	$(Label) "$$/Engineering/ProductDevelopment/DataEntry" "$(DataEntryVersion)"
	$(Label) "$$/Engineering/ProductDevelopment/DataEntry/LabDE" "$(LabDEVersion)"
	$(Label) $$/Engineering/ReusableComponents "$(ReusableComponentsVersion)" 
	$(Label) $$/Engineering/ProductDevelopment/Utils "$(PDUtilsVersion)"
	$(Label) $$/Engineering/ProductDevelopment/PlatformSpecificUtils "$(PlatformSpecificUtilsVersion)"
	$(Label) "$$/Engineering/Rules/ComponentData" "$(FKBVersion)"
	$(Label) "$$/Engineering/RC.Net" "$(RCDotNetVersion)"
	$(Label) "$$/Engineering/Rules" "$(RulesVersion)"
	$(Label) "$$/Branches/InternalRules" "$(RulesVersion)"
	$(Label) $$/Engineering/ProductDevelopment/Common "$(FlexIndexVersion)"
	$(Label) $$/Engineering/ProductDevelopment/Common "$(LaserficheVersion)"
	$(Label) $$/Engineering/ProductDevelopment/Common "$(FlexIDSSPVersion)"
	$(Label) $$/Engineering/ProductDevelopment/Common "$(FlexIDSNetDMSVersion)"
	$(Label) $$/Engineering/ProductDevelopment/Common "$(IDShieldOfficeVersion)"
	$(Label) $$/Engineering/ProductDevelopment/Common "$(DataEntryVersion)"
	$(Label) $$/Engineering/ProductDevelopment/Common "$(LabDEVersion)"
	$(Label) $$/Engineering/ProductDevelopment/Common "$(FKBVersion)"
