namespace Extract.AttributeFinder.Rules.Domain
   
module MicrFinderV2 =
  open Extract.AttributeFinder.Rules

  module MicrFinder =
    let toDto (domain: IMicrFinder): Dto.MicrFinderV2 = domain.DataTransferObject
    let fromDto (dto: Dto.MicrFinderV2) = MicrFinder(dto)

  type MicrFinderConverter() =
    inherit RuleObjectConverter<MicrFinder, IMicrFinder, Dto.MicrFinderV2>()
    override _.toDto _mc domain = domain |> MicrFinder.toDto
    override _.fromDto _mc dto = dto |> MicrFinder.fromDto

(****************************************************************************************************)


module MicrFinderV1 =
  open Extract.AttributeFinder.Rules
  open UCLID_AFVALUEFINDERSLib

  module MicrFinder =
    open Extract.AttributeFinder.Rules.Dto

    let toDto (domain: IMicrFinder): Dto.MicrFinderV1 =
      { SplitRoutingNumber = domain.SplitRoutingNumber
        SplitAccountNumber = domain.SplitAccountNumber
        SplitCheckNumber = domain.SplitCheckNumber
        SplitAmount = domain.SplitAmount
        Rotate0 = domain.Rotate0
        Rotate90 = domain.Rotate90
        Rotate180 = domain.Rotate180
        Rotate270 = domain.Rotate270 }

    let fromDto (dto: Dto.MicrFinderV1) =
      MicrFinderClass
        ( SplitRoutingNumber=dto.SplitRoutingNumber,
          SplitAccountNumber=dto.SplitAccountNumber,
          SplitCheckNumber=dto.SplitCheckNumber,
          SplitAmount=dto.SplitAmount,
          Rotate0=dto.Rotate0,
          Rotate90=dto.Rotate90,
          Rotate180=dto.Rotate180,
          Rotate270=dto.Rotate270 )

  type MicrFinderConverter() =
    inherit RuleObjectConverter<MicrFinderClass, IMicrFinder, Dto.MicrFinderV1>()
    override _.toDto _mc domain = domain |> MicrFinder.toDto
    override _.fromDto _mc dto = dto |> MicrFinder.fromDto
