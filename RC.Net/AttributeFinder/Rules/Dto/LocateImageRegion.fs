namespace Extract.AttributeFinder.Rules.Dto

type Boundary =
| NoBoundary = 0
| Top = 1 
| Bottom = 2 
| Left = 3 
| Right = 4

type ExpandDirection =
| NoDirection = 0
| ExpandUp = 1 
| ExpandDown = 2 
| ExpandLeft = 3 
| ExpandRight = 4

type Units =
| ClueLines = 0
| PageLines = 1
| ClueCharacters = 2
| PageCharacters = 3
| Inches = 4
| Pixels = 5

type BoundaryCondition =
| NoCondition = 0
| ClueList1 = 1 
| ClueList2 = 2 
| ClueList3 = 3 
| ClueList4 = 4 
| Page = 5

type LocateImageRegionBoundary = {
  Anchor: BoundaryCondition 
  AnchorSide: Boundary 
  ExpandDirection: ExpandDirection 
  ExpandBy: double 
  ExpandUnits: Units 
}

type LocateImageRegionClueList = {
  Clues: string list 
  CaseSensitive: bool 
  Regex: bool 
  RestrictByBoundary: bool 
}

type LocateImageRegion = {
  DataInsideBoundaries: bool
  FindType: FindType
  ImageRegionText: string
  IncludeIntersectingEntities: bool
  IntersectingEntityType: SpatialEntity
  MatchMultiplePagesPerDocument: bool
  ClueList1: LocateImageRegionClueList
  ClueList2: LocateImageRegionClueList
  ClueList3: LocateImageRegionClueList
  ClueList4: LocateImageRegionClueList
  Left: LocateImageRegionBoundary
  Top: LocateImageRegionBoundary
  Right: LocateImageRegionBoundary
  Bottom: LocateImageRegionBoundary
}