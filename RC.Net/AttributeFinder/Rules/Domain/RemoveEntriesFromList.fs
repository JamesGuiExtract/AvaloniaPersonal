namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open Extract.Utilities
open UCLID_AFOUTPUTHANDLERSLib

module RemoveEntriesFromList =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IRemoveEntriesFromList): Dto.RemoveEntriesFromList =
    { EntryList = domain.EntryList |> VariantVector.toList
      IsCaseSensitive = domain.IsCaseSensitive }

  let fromDto (dto: Dto.RemoveEntriesFromList) =
    RemoveEntriesFromListClass
      ( EntryList=dto.EntryList.ToVariantVector(),
        IsCaseSensitive=dto.IsCaseSensitive )

type RemoveEntriesFromListConverter() =
  inherit RuleObjectConverter<RemoveEntriesFromListClass, IRemoveEntriesFromList, Dto.RemoveEntriesFromList>()
  override _.toDto _mc domain = domain |> RemoveEntriesFromList.toDto
  override _.fromDto _mc dto = dto |> RemoveEntriesFromList.fromDto
