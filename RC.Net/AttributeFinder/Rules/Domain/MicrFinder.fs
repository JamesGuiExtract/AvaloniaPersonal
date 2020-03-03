namespace Extract.AttributeFinder.Rules.Domain

module MicrFinderV2 =
  open Extract.AttributeFinder.Rules

  module MicrFinder =
    type MicrFinderClass = MicrFinder
    open Extract.AttributeFinder.Rules.Dto

    let toDto (domain: IMicrFinder): Dto.MicrFinderV2 =
      { FilterCharsWhenSplitting = domain.FilterCharsWhenSplitting
        FilterRegex = domain.FilterRegex
        HighConfidenceThreshold = domain.HighConfidenceThreshold
        InheritOCRParameters = domain.InheritOCRParameters
        LowConfidenceThreshold = domain.LowConfidenceThreshold
        MicrSplitterRegex = domain.MicrSplitterRegex
        ReturnUnrecognizedCharacters = domain.ReturnUnrecognizedCharacters
        SplitAccountNumber = domain.SplitAccountNumber
        SplitAmount = domain.SplitAmount
        SplitCheckNumber = domain.SplitCheckNumber
        SplitRoutingNumber = domain.SplitRoutingNumber
        UseLowConfidenceThreshold = domain.UseLowConfidenceThreshold }

    let fromDto (dto: Dto.MicrFinderV2) =
      MicrFinderClass
        ( FilterCharsWhenSplitting=dto.FilterCharsWhenSplitting,
          FilterRegex=dto.FilterRegex,
          HighConfidenceThreshold=dto.HighConfidenceThreshold,
          InheritOCRParameters=dto.InheritOCRParameters,
          LowConfidenceThreshold=dto.LowConfidenceThreshold,
          MicrSplitterRegex=dto.MicrSplitterRegex,
          ReturnUnrecognizedCharacters=dto.ReturnUnrecognizedCharacters,
          SplitAccountNumber=dto.SplitAccountNumber,
          SplitAmount=dto.SplitAmount,
          SplitCheckNumber=dto.SplitCheckNumber,
          SplitRoutingNumber=dto.SplitRoutingNumber,
          UseLowConfidenceThreshold=dto.UseLowConfidenceThreshold )

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
