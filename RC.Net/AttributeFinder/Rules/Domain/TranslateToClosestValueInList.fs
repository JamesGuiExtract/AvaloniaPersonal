namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open Extract.Utilities
open UCLID_AFVALUEMODIFIERSLib

module TranslateToClosestValueInList =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: ITranslateToClosestValueInList): Dto.TranslateToClosestValueInList =
    { ClosestValueList = domain.ClosestValueList |> VariantVector.toList
      IsCaseSensitive = domain.IsCaseSensitive
      IsForcedMatch = domain.IsForcedMatch }

  let fromDto (dto: Dto.TranslateToClosestValueInList) =
    TranslateToClosestValueInListClass
      ( ClosestValueList=dto.ClosestValueList.ToVariantVector(),
        IsCaseSensitive=dto.IsCaseSensitive,
        IsForcedMatch=dto.IsForcedMatch )

type TranslateToClosestValueInListConverter() =
  inherit RuleObjectConverter<TranslateToClosestValueInListClass, ITranslateToClosestValueInList, Dto.TranslateToClosestValueInList>()
  override _.toDto _mc domain = domain |> TranslateToClosestValueInList.toDto
  override _.fromDto _mc dto = dto |> TranslateToClosestValueInList.fromDto
