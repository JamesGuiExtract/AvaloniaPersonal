namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFVALUEMODIFIERSLib

module ReplacementOccurrenceType =
  let toDto = function
  | EReplacementOccurrenceType.kAllOccurrences -> Dto.ReplacementOccurrenceType.All
  | EReplacementOccurrenceType.kFirstOccurrence -> Dto.ReplacementOccurrenceType.First
  | EReplacementOccurrenceType.kLastOccurrence -> Dto.ReplacementOccurrenceType.Last
  | EReplacementOccurrenceType.kSpecifiedOccurrence -> Dto.ReplacementOccurrenceType.Specified
  | other -> failwithf "Not a valid EReplacementOccurrenceType! %A" other

  let fromDto = function
  | Dto.ReplacementOccurrenceType.All -> EReplacementOccurrenceType.kAllOccurrences
  | Dto.ReplacementOccurrenceType.First -> EReplacementOccurrenceType.kFirstOccurrence
  | Dto.ReplacementOccurrenceType.Last -> EReplacementOccurrenceType.kLastOccurrence
  | Dto.ReplacementOccurrenceType.Specified -> EReplacementOccurrenceType.kSpecifiedOccurrence
  | other -> failwithf "Not a valid ReplacementOccurrenceType! %A" other

module AdvancedReplaceString =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IAdvancedReplaceString): Dto.AdvancedReplaceString =
    { StrToBeReplaced = domain.StrToBeReplaced
      AsRegularExpression = domain.AsRegularExpression
      IsCaseSensitive = domain.IsCaseSensitive
      Replacement = domain.Replacement
      ReplacementOccurrenceType = domain.ReplacementOccurrenceType |> ReplacementOccurrenceType.toDto
      SpecifiedOccurrence = domain.SpecifiedOccurrence }

  let fromDto (dto: Dto.AdvancedReplaceString) =
    let domain =
      AdvancedReplaceStringClass
        ( StrToBeReplaced=dto.StrToBeReplaced,
          AsRegularExpression=dto.AsRegularExpression,
          IsCaseSensitive=dto.IsCaseSensitive,
          Replacement=dto.Replacement,
          ReplacementOccurrenceType=(dto.ReplacementOccurrenceType |> ReplacementOccurrenceType.fromDto) )

    if dto.SpecifiedOccurrence > 0
    then domain.SpecifiedOccurrence <- dto.SpecifiedOccurrence

    domain


type AdvancedReplaceStringConverter() =
  inherit RuleObjectConverter<AdvancedReplaceStringClass, IAdvancedReplaceString, Dto.AdvancedReplaceString>()
  override _.toDto _mc domain = domain |> AdvancedReplaceString.toDto
  override _.fromDto _mc dto = dto |> AdvancedReplaceString.fromDto
