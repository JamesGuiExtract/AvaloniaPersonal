namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFVALUEMODIFIERSLib

module LimitAsRightPart =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: ILimitAsRightPart): Dto.LimitAsRightPart =
    { NumberOfCharacters = domain.NumberOfCharacters
      AcceptSmallerLength = domain.AcceptSmallerLength
      Extract = domain.Extract }

  let fromDto (dto: Dto.LimitAsRightPart) =
    LimitAsRightPartClass
      ( NumberOfCharacters=dto.NumberOfCharacters,
        AcceptSmallerLength=dto.AcceptSmallerLength,
        Extract=dto.Extract )

type LimitAsRightPartConverter() =
  inherit RuleObjectConverter<LimitAsRightPartClass, ILimitAsRightPart, Dto.LimitAsRightPart>()
  override _.toDto _mc domain = domain |> LimitAsRightPart.toDto
  override _.fromDto _mc dto = dto |> LimitAsRightPart.fromDto
