namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFVALUEMODIFIERSLib

module PadValue =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IPadValue): Dto.PadValue =
    { PadLeft = domain.PadLeft
      PaddingCharacter = domain.PaddingCharacter
      RequiredSize = domain.RequiredSize }

  let fromDto (dto: Dto.PadValue) =
    PadValueClass
      ( PadLeft=dto.PadLeft,
        PaddingCharacter=dto.PaddingCharacter,
        RequiredSize=dto.RequiredSize )

type PadValueConverter() =
  inherit RuleObjectConverter<PadValueClass, IPadValue, Dto.PadValue>()
  override _.toDto _mc domain = domain |> PadValue.toDto
  override _.fromDto _mc dto = dto |> PadValue.fromDto
