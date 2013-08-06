InitBuildEnv.bat

!include LatestComponentVersions.mak

Label="$(PROGRAM_ROOT)\SourceGear\Vault Client\vault" LABEL -server $(VAULT_SERVER) -repository "Extract"

LabelCommonDir:
	$(Label) "$$$(Branch)/Engineering/ProductDevelopment" "$(FlexIndexVersion)"
	$(Label) "$$$(Branch)/Engineering/RC.Net" "$(FlexIndexVersion)"
	$(Label) "$$$(Branch)/Engineering/ReusableComponents" "$(FlexIndexVersion)"
	$(Label) "$$$(Branch)/Engineering/Rules" "$(FlexIndexVersion)"
  $(Label) "$$$(Branch)/Engineering/Rules/ComponentData" "$(FKBVersion)"
	$(Label) "$$$(Branch)/Engineering/ProductDevelopment/Common" "$(FKBVersion)"
