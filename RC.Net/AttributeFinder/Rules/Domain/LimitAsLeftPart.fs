namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFVALUEMODIFIERSLib

module LimitAsLeftPart =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: ILimitAsLeftPart): Dto.LimitAsLeftPart =
    { NumberOfCharacters = domain.NumberOfCharacters
      AcceptSmallerLength = domain.AcceptSmallerLength
      Extract = domain.Extract }

  let fromDto (dto: Dto.LimitAsLeftPart) =
    LimitAsLeftPartClass
      ( NumberOfCharacters=dto.NumberOfCharacters,
        AcceptSmallerLength=dto.AcceptSmallerLength,
        Extract=dto.Extract )

type LimitAsLeftPartConverter() =
  inherit RuleObjectConverter<LimitAsLeftPartClass, ILimitAsLeftPart, Dto.LimitAsLeftPart>()
  override _.toDto _mc domain = domain |> LimitAsLeftPart.toDto
  override _.fromDto _mc dto = dto |> LimitAsLeftPart.fromDto
