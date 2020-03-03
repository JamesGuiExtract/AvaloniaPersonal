namespace Extract.AttributeFinder.Rules.Dto

type SplitRegionIntoContentAreas = {
  AttributeName: string
  DefaultAttributeText: string
  GoodOCRType: string
  IncludeGoodOCR: bool
  IncludeOCRAsTrueSpatialString: bool
  IncludePoorOCR: bool
  MinimumHeight: double
  MinimumWidth: double
  OCRThreshold: int
  PoorOCRType: string
  ReOCRWithHandwriting: bool
  RequiredHorizontalSeparation: int
  UseLines: bool
}