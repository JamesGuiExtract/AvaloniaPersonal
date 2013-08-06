
!include LatestComponentVersions.mak

IF "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
	SET PROGRAM_ROOT=%ProgramFiles(x86)%
) ELSE (
	SET PROGRAM_ROOT=%ProgramFiles%
)

Label="$(PROGRAM_ROOT)\SourceGear\Vault Client\vault" LABEL -server $(VAULT_SERVER) -repository "Extract"

LabelCommonDir:
	$(Label) "$$$(Branch)/Engineering/ProductDevelopment" "$(FlexIndexVersion)"
	$(Label) "$$$(Branch)/Engineering/RC.Net" "$(FlexIndexVersion)"
	$(Label) "$$$(Branch)/Engineering/ReusableComponents" "$(FlexIndexVersion)"
	$(Label) "$$$(Branch)/Engineering/Rules" "$(FlexIndexVersion)"
  $(Label) "$$$(Branch)/Engineering/Rules/ComponentData" "$(FKBVersion)"
	$(Label) "$$$(Branch)/Engineering/ProductDevelopment/Common" "$(FKBVersion)"
