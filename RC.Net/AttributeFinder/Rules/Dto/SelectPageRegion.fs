namespace Extract.AttributeFinder.Rules.Dto

type PageSelectionType =
| SelectAll = 0
| SelectSpecified = 1
| SelectWithRegExp = 2

type RegExpPageSelectionType =
| SelectAllPagesWithRegExp = 0
| SelectLeadingPagesWithRegExp = 1
| SelectTrailingPagesWithRegExp = 2

type SelectPageRegionReturnType =
| ReturnText = 0
| ReturnReOcr = 1
| ReturnImageRegion = 2

type SelectPageRegion = {
  IncludeRegionDefined: bool
  PageSelectionType: PageSelectionType
  SpecificPages: string
  RegExpPageSelectionType: RegExpPageSelectionType
  Pattern: string
  IsRegExp: bool
  IsCaseSensitive: bool
  HorizontalStart: int
  HorizontalEnd: int
  VerticalStart: int
  VerticalEnd: int
  SelectPageRegionReturnType: SelectPageRegionReturnType
  IncludeIntersectingText: bool
  TextIntersectionType: SpatialEntity
  SelectedRegionRotation: int
  TextToAssignToRegion: string
}