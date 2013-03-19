
!include LatestComponentVersions.mak

Label="C:\Program Files\SourceGear\Vault Client\vault" LABEL -server white.extract.local -repository "Extract"

LabelCommonDir:
	$(Label) "$$$(Branch)/Engineering/ProductDevelopment" "$(FlexIndexVersion)"
	$(Label) "$$$(Branch)/Engineering/RC.Net" "$(FlexIndexVersion)"
	$(Label) "$$$(Branch)/Engineering/ReusableComponents" "$(FlexIndexVersion)"
	$(Label) "$$$(Branch)/Engineering/Rules" "$(FlexIndexVersion)"
  $(Label) "$$$(Branch)/Engineering/Rules/ComponentData" "$(FKBVersion)"
	$(Label) "$$$(Branch)/Engineering/ProductDevelopment/Common" "$(FKBVersion)"
