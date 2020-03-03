namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFVALUEMODIFIERSLib

module SplitRegionIntoContentAreas =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: ISplitRegionIntoContentAreas): Dto.SplitRegionIntoContentAreas =
    { AttributeName = domain.AttributeName
      DefaultAttributeText = domain.DefaultAttributeText
      GoodOCRType = domain.GoodOCRType
      IncludeGoodOCR = domain.IncludeGoodOCR
      IncludeOCRAsTrueSpatialString = domain.IncludeOCRAsTrueSpatialString
      IncludePoorOCR = domain.IncludePoorOCR
      MinimumHeight = domain.MinimumHeight
      MinimumWidth = domain.MinimumWidth
      OCRThreshold = domain.OCRThreshold
      PoorOCRType = domain.PoorOCRType
      ReOCRWithHandwriting = domain.ReOCRWithHandwriting
      RequiredHorizontalSeparation = domain.RequiredHorizontalSeparation
      UseLines = domain.UseLines }

  let fromDto (dto: Dto.SplitRegionIntoContentAreas) =
    SplitRegionIntoContentAreasClass
      ( AttributeName=dto.AttributeName,
        DefaultAttributeText=dto.DefaultAttributeText,
        GoodOCRType=dto.GoodOCRType,
        IncludeGoodOCR=dto.IncludeGoodOCR,
        IncludeOCRAsTrueSpatialString=dto.IncludeOCRAsTrueSpatialString,
        IncludePoorOCR=dto.IncludePoorOCR,
        MinimumHeight=dto.MinimumHeight,
        MinimumWidth=dto.MinimumWidth,
        OCRThreshold=dto.OCRThreshold,
        PoorOCRType=dto.PoorOCRType,
        ReOCRWithHandwriting=dto.ReOCRWithHandwriting,
        RequiredHorizontalSeparation=dto.RequiredHorizontalSeparation,
        UseLines=dto.UseLines )

type SplitRegionIntoContentAreasConverter() =
  inherit RuleObjectConverter<SplitRegionIntoContentAreasClass, ISplitRegionIntoContentAreas, Dto.SplitRegionIntoContentAreas>()
  override _.toDto _mc domain = domain |> SplitRegionIntoContentAreas.toDto
  override _.fromDto _mc dto = dto |> SplitRegionIntoContentAreas.fromDto
