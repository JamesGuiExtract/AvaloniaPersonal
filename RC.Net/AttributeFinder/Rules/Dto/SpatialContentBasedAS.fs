namespace Extract.AttributeFinder.Rules.Dto

type SpatialContentBasedAS = {
  Contains: bool
  ConsecutiveRows: int
  MinPercent: int
  MaxPercent: int
  IncludeNonSpatial: bool
}