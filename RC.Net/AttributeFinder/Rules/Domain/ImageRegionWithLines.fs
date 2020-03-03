namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFVALUEFINDERSLib
open UCLID_IMAGEUTILSLib

module ImageLineUtility =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IImageLineUtility): Dto.ImageLineUtility =
    { AlignmentScoreExact = domain.AlignmentScoreExact
      AlignmentScoreMin = domain.AlignmentScoreMin
      ColumnCountMax = domain.ColumnCountMax
      ColumnCountMin = domain.ColumnCountMin
      ColumnSpacingMax = domain.ColumnSpacingMax
      ColumnWidthMax = domain.ColumnWidthMax
      ColumnWidthMin = domain.ColumnWidthMin
      ExtendLineFragments = domain.ExtendLineFragments
      ExtensionConsecutiveMin = domain.ExtensionConsecutiveMin
      ExtensionGapAllowance = domain.ExtensionGapAllowance
      ExtensionScanWidth = domain.ExtensionScanWidth
      ExtensionTelescoping = domain.ExtensionTelescoping
      LineBridgeGap = domain.LineBridgeGap
      LineGapMax = domain.LineGapMax
      LineLengthMin = domain.LineLengthMin
      LineSpacingMax = domain.LineSpacingMax
      LineSpacingMin = domain.LineSpacingMin
      LineThicknessMax = domain.LineThicknessMax
      LineVarianceMax = domain.LineVarianceMax
      LineWall = domain.LineWall
      LineWallPercentMax = domain.LineWallPercentMax
      OverallWidthMax = domain.OverallWidthMax
      OverallWidthMin = domain.OverallWidthMin
      RowCountMax = domain.RowCountMax
      RowCountMin = domain.RowCountMin
      SpacingScoreExact = domain.SpacingScoreExact
      SpacingScoreMin = domain.SpacingScoreMin }

  let fromDto (dto: Dto.ImageLineUtility) =
    ImageLineUtilityClass
      ( AlignmentScoreExact=dto.AlignmentScoreExact,
        AlignmentScoreMin=dto.AlignmentScoreMin,
        ColumnCountMax=dto.ColumnCountMax,
        ColumnCountMin=dto.ColumnCountMin,
        ColumnSpacingMax=dto.ColumnSpacingMax,
        ColumnWidthMax=dto.ColumnWidthMax,
        ColumnWidthMin=dto.ColumnWidthMin,
        ExtendLineFragments=dto.ExtendLineFragments,
        ExtensionConsecutiveMin=dto.ExtensionConsecutiveMin,
        ExtensionGapAllowance=dto.ExtensionGapAllowance,
        ExtensionScanWidth=dto.ExtensionScanWidth,
        ExtensionTelescoping=dto.ExtensionTelescoping,
        LineBridgeGap=dto.LineBridgeGap,
        LineGapMax=dto.LineGapMax,
        LineLengthMin=dto.LineLengthMin,
        LineSpacingMax=dto.LineSpacingMax,
        LineSpacingMin=dto.LineSpacingMin,
        LineThicknessMax=dto.LineThicknessMax,
        LineVarianceMax=dto.LineVarianceMax,
        LineWall=dto.LineWall,
        LineWallPercentMax=dto.LineWallPercentMax,
        OverallWidthMax=dto.OverallWidthMax,
        OverallWidthMin=dto.OverallWidthMin,
        RowCountMax=dto.RowCountMax,
        RowCountMin=dto.RowCountMin,
        SpacingScoreExact=dto.SpacingScoreExact,
        SpacingScoreMin=dto.SpacingScoreMin )

module ImageRegionWithLines =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IImageRegionWithLines): Dto.ImageRegionWithLines =
    { AttributeText = domain.AttributeText
      IncludeLines = domain.IncludeLines
      LineUtil = downcast domain.LineUtil |> ImageLineUtility.toDto
      NumFirstPages = domain.NumFirstPages
      NumLastPages = domain.NumLastPages
      PageSelectionMode = domain.PageSelectionMode |> PageSelectionMode.toDto
      SpecifiedPages = domain.SpecifiedPages }

  let fromDto (dto: Dto.ImageRegionWithLines) =
    ImageRegionWithLinesClass
      ( AttributeText=dto.AttributeText,
        IncludeLines=dto.IncludeLines,
        LineUtil=(dto.LineUtil |> ImageLineUtility.fromDto),
        NumFirstPages=dto.NumFirstPages,
        NumLastPages=dto.NumLastPages,
        PageSelectionMode=(dto.PageSelectionMode |> PageSelectionMode.fromDto),
        SpecifiedPages=dto.SpecifiedPages )

type ImageRegionWithLinesConverter() =
  inherit RuleObjectConverter<ImageRegionWithLinesClass, IImageRegionWithLines, Dto.ImageRegionWithLines>()
  override _.toDto _mc domain = domain |> ImageRegionWithLines.toDto
  override _.fromDto _mc dto = dto |> ImageRegionWithLines.fromDto
