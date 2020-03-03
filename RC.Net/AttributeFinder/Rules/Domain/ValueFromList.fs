namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open Extract.Utilities
open UCLID_AFVALUEFINDERSLib

module ValueFromList =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IValueFromList): Dto.ValueFromList =
    { ValueList = domain.ValueList |> VariantVector.toList
      IsCaseSensitive = domain.IsCaseSensitive }

  let fromDto (dto: Dto.ValueFromList) =
    ValueFromListClass
      ( ValueList=dto.ValueList.ToVariantVector(),
        IsCaseSensitive=dto.IsCaseSensitive )

type ValueFromListConverter() =
  inherit RuleObjectConverter<ValueFromListClass, IValueFromList, Dto.ValueFromList>()
  override _.toDto _mc domain = domain |> ValueFromList.toDto
  override _.fromDto _mc dto = dto |> ValueFromList.fromDto
