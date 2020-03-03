namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFVALUEMODIFIERSLib

module LimitAsMidPart =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: ILimitAsMidPart): Dto.LimitAsMidPart =
    { StartPosition = domain.StartPosition
      EndPosition = domain.EndPosition
      AcceptSmallerLength = domain.AcceptSmallerLength
      Extract = domain.Extract }

  let fromDto (dto: Dto.LimitAsMidPart) =
    LimitAsMidPartClass
      ( StartPosition=dto.StartPosition,
        EndPosition=dto.EndPosition,
        AcceptSmallerLength=dto.AcceptSmallerLength,
        Extract=dto.Extract )

type LimitAsMidPartConverter() =
  inherit RuleObjectConverter<LimitAsMidPartClass, ILimitAsMidPart, Dto.LimitAsMidPart>()
  override _.toDto _mc domain = domain |> LimitAsMidPart.toDto
  override _.fromDto _mc dto = dto |> LimitAsMidPart.fromDto
