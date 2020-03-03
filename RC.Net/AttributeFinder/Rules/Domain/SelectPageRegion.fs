namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFPREPROCESSORSLib

module PageSelectionType =
  let toDto = function
  | EPageSelectionType.kSelectAll -> Dto.PageSelectionType.SelectAll
  | EPageSelectionType.kSelectSpecified -> Dto.PageSelectionType.SelectSpecified
  | EPageSelectionType.kSelectWithRegExp -> Dto.PageSelectionType.SelectWithRegExp
  | other -> failwithf "Not a valid EPageSelectionType! %A" other

  let fromDto = function
  | Dto.PageSelectionType.SelectAll -> EPageSelectionType.kSelectAll
  | Dto.PageSelectionType.SelectSpecified -> EPageSelectionType.kSelectSpecified
  | Dto.PageSelectionType.SelectWithRegExp -> EPageSelectionType.kSelectWithRegExp
  | other -> failwithf "Not a valid PageSelectionType! %A" other

module RegExpPageSelectionType =
  let toDto = function
  | ERegExpPageSelectionType.kSelectAllPagesWithRegExp -> Dto.RegExpPageSelectionType.SelectAllPagesWithRegExp
  | ERegExpPageSelectionType.kSelectLeadingPagesWithRegExp -> Dto.RegExpPageSelectionType.SelectLeadingPagesWithRegExp
  | ERegExpPageSelectionType.kSelectTrailingPagesWithRegExp -> Dto.RegExpPageSelectionType.SelectTrailingPagesWithRegExp
  | other -> failwithf "Not a valid ERegExpPageSelectionType! %A" other

  let fromDto = function
  | Dto.RegExpPageSelectionType.SelectAllPagesWithRegExp -> ERegExpPageSelectionType.kSelectAllPagesWithRegExp
  | Dto.RegExpPageSelectionType.SelectLeadingPagesWithRegExp -> ERegExpPageSelectionType.kSelectLeadingPagesWithRegExp
  | Dto.RegExpPageSelectionType.SelectTrailingPagesWithRegExp -> ERegExpPageSelectionType.kSelectTrailingPagesWithRegExp
  | other -> failwithf "Not a valid RegExpPageSelectionType! %A" other

module SelectPageRegionReturnType =
  let toDto = function
  | ESelectPageRegionReturnType.kReturnText -> Dto.SelectPageRegionReturnType.ReturnText
  | ESelectPageRegionReturnType.kReturnReOcr -> Dto.SelectPageRegionReturnType.ReturnReOcr
  | ESelectPageRegionReturnType.kReturnImageRegion -> Dto.SelectPageRegionReturnType.ReturnImageRegion
  | other -> failwithf "Not a valid ESelectPageRegionReturnType! %A" other

  let fromDto = function
  | Dto.SelectPageRegionReturnType.ReturnText -> ESelectPageRegionReturnType.kReturnText
  | Dto.SelectPageRegionReturnType.ReturnReOcr -> ESelectPageRegionReturnType.kReturnReOcr
  | Dto.SelectPageRegionReturnType.ReturnImageRegion -> ESelectPageRegionReturnType.kReturnImageRegion
  | other -> failwithf "Not a valid SelectPageRegionReturnType! %A" other

module SelectPageRegion =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: ISelectPageRegion) =
    let horizontalStart = ref 0
    let horizontalEnd = ref 0
    domain.GetHorizontalRestriction(horizontalStart, horizontalEnd)
    let verticalStart = ref 0
    let verticalEnd = ref 0
    domain.GetVerticalRestriction(verticalStart, verticalEnd)

    { Dto.SelectPageRegion.IncludeRegionDefined = domain.IncludeRegionDefined
      PageSelectionType = domain.PageSelectionType |> PageSelectionType.toDto
      SpecificPages = domain.SpecificPages
      RegExpPageSelectionType = domain.RegExpPageSelectionType |> RegExpPageSelectionType.toDto
      Pattern = domain.Pattern
      IsRegExp = domain.IsRegExp
      IsCaseSensitive = domain.IsCaseSensitive
      HorizontalStart = !horizontalStart
      HorizontalEnd = !horizontalEnd
      VerticalStart = !verticalStart
      VerticalEnd = !verticalEnd
      SelectPageRegionReturnType = domain.SelectPageRegionReturnType |> SelectPageRegionReturnType.toDto
      IncludeIntersectingText = domain.IncludeIntersectingText
      TextIntersectionType = domain.TextIntersectionType |> SpatialEntity.toDto
      SelectedRegionRotation = domain.SelectedRegionRotation
      TextToAssignToRegion = domain.TextToAssignToRegion }

  let fromDto (dto: Dto.SelectPageRegion) =
    let domain =
      SelectPageRegionClass
        ( IncludeRegionDefined=dto.IncludeRegionDefined,
          PageSelectionType=(dto.PageSelectionType |> PageSelectionType.fromDto),
          RegExpPageSelectionType=(dto.RegExpPageSelectionType |> RegExpPageSelectionType.fromDto),
          IsRegExp=dto.IsRegExp,
          IsCaseSensitive=dto.IsCaseSensitive,
          SelectPageRegionReturnType=(dto.SelectPageRegionReturnType |> SelectPageRegionReturnType.fromDto),
          IncludeIntersectingText=dto.IncludeIntersectingText,
          TextIntersectionType=(dto.TextIntersectionType |> SpatialEntity.fromDto),
          SelectedRegionRotation=dto.SelectedRegionRotation,
          TextToAssignToRegion=dto.TextToAssignToRegion )

    domain.SetHorizontalRestriction(dto.HorizontalStart, dto.HorizontalEnd)
    domain.SetVerticalRestriction(dto.VerticalStart, dto.VerticalEnd)

    if dto.Pattern |> String.length > 0
    then domain.Pattern <- dto.Pattern

    if dto.SpecificPages |> String.length > 0
    then domain.SpecificPages <- dto.SpecificPages

    domain

type SelectPageRegionConverter() =
  inherit RuleObjectConverter<SelectPageRegionClass, ISelectPageRegion, Dto.SelectPageRegion>()
  override _.toDto _mc domain = domain |> SelectPageRegion.toDto
  override _.fromDto _mc dto = dto |> SelectPageRegion.fromDto
