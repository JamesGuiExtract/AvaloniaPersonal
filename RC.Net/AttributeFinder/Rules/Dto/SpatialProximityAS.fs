namespace Extract.AttributeFinder.Rules.Dto

type AnchorType =
| ReferenceAttribute = 0
| Page = 1

type ReferenceUnits =
| Lines = 0
| Characters = 1
| Inches = 2
| Pixels = 3

type ReferenceRegionBoundary = {
  Anchor: AnchorType 
  AnchorSide: Boundary 
  ExpandDirection: ExpandDirection 
  ExpandBy: double 
  ExpandUnits: ReferenceUnits 
}

type SpatialProximityAS = {
  TargetQuery: string
  TargetsMustContainReferences: bool
  RequireCompleteInclusion: bool
  ReferenceQuery: string
  Left: ReferenceRegionBoundary
  Top: ReferenceRegionBoundary
  Right: ReferenceRegionBoundary
  Bottom: ReferenceRegionBoundary
  CompareLinesSeparately: bool
  IncludeDebugAttributes: bool
}