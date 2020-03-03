namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFSELECTORSLib

module SpatialContentBasedAS =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: ISpatialContentBasedAS): Dto.SpatialContentBasedAS =
    { Contains = domain.Contains
      ConsecutiveRows = domain.ConsecutiveRows
      MinPercent = domain.MinPercent
      MaxPercent = domain.MaxPercent
      IncludeNonSpatial = domain.IncludeNonSpatial }

  let fromDto (dto: Dto.SpatialContentBasedAS) =
    SpatialContentBasedASClass
      ( Contains=dto.Contains,
        ConsecutiveRows=dto.ConsecutiveRows,
        MinPercent=dto.MinPercent,
        MaxPercent=dto.MaxPercent,
        IncludeNonSpatial=dto.IncludeNonSpatial )

type SpatialContentBasedASConverter() =
  inherit RuleObjectConverter<SpatialContentBasedASClass, ISpatialContentBasedAS, Dto.SpatialContentBasedAS>()
  override _.toDto _mc domain = domain |> SpatialContentBasedAS.toDto
  override _.fromDto _mc dto = dto |> SpatialContentBasedAS.fromDto
