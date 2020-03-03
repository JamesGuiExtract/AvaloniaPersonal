namespace Extract.AttributeFinder.Rules.Dto

type ImageLineUtility = {
  AlignmentScoreExact: int
  AlignmentScoreMin: int
  ColumnCountMax: int
  ColumnCountMin: int
  ColumnSpacingMax: int
  ColumnWidthMax: int
  ColumnWidthMin: int
  ExtendLineFragments: bool
  ExtensionConsecutiveMin: int
  ExtensionGapAllowance: int
  ExtensionScanWidth: int
  ExtensionTelescoping: int
  LineBridgeGap: int
  LineGapMax: int
  LineLengthMin: int
  LineSpacingMax: int
  LineSpacingMin: int
  LineThicknessMax: int
  LineVarianceMax: int
  LineWall: int
  LineWallPercentMax: int
  OverallWidthMax: int
  OverallWidthMin: int
  RowCountMax: int
  RowCountMin: int
  SpacingScoreExact: int
  SpacingScoreMin: int
}

type ImageRegionWithLines = {
  AttributeText: string
  IncludeLines: bool
  LineUtil: ImageLineUtility
  NumFirstPages: int
  NumLastPages: int
  PageSelectionMode: PageSelectionMode
  SpecifiedPages: string
}