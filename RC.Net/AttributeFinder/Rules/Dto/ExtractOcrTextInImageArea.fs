namespace Extract.AttributeFinder.Rules.Dto

type SpatialEntity =
| NoEntity = 0
| Character = 1
| Word = 2
| Line = 3

type ExtractOcrTextInImageArea = {
  IncludeTextOnBoundary: bool
  SpatialEntityType: SpatialEntity
  UseOriginalDocumentOcr: bool
  UseOverallBounds: bool
}