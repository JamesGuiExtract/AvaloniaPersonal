namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFVALUEMODIFIERSLib

module RemoveCharacters =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IRemoveCharacters): Dto.RemoveCharacters =
    { Characters = domain.Characters
      IsCaseSensitive = domain.IsCaseSensitive
      RemoveAll = domain.RemoveAll
      Consolidate = domain.Consolidate
      TrimLeading = domain.TrimLeading
      TrimTrailing = domain.TrimTrailing }

  let fromDto (dto: Dto.RemoveCharacters) =
    RemoveCharactersClass
      ( Characters=dto.Characters,
        IsCaseSensitive=dto.IsCaseSensitive,
        RemoveAll=dto.RemoveAll,
        Consolidate=dto.Consolidate,
        TrimLeading=dto.TrimLeading,
        TrimTrailing=dto.TrimTrailing )

type RemoveCharactersConverter() =
  inherit RuleObjectConverter<RemoveCharactersClass, IRemoveCharacters, Dto.RemoveCharacters>()
  override _.toDto _mc domain = domain |> RemoveCharacters.toDto
  override _.fromDto _mc dto = dto |> RemoveCharacters.fromDto
