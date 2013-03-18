
!include LatestComponentVersions.mak

Label="C:\Program Files\SourceGear\Vault Client\vault" LABEL -server white.extract.local -repository "Extract"

LabelCommonDir:
	$(Label) "$$$(Branch)/Engineering" "$(FlexIndexVersion)"
	$(Label) "$$$(Branch)/Engineering/Rules/ComponentData" "$(FKBVersion)"
	$(Label) "$$$(Branch)/Engineering/ProductDevelopment/Common" "$(FlexIndexVersion)"
	$(Label) "$$$(Branch)/Engineering/ProductDevelopment/Common" "$(FKBVersion)"
