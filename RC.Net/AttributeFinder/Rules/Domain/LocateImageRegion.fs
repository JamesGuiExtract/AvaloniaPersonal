namespace Extract.AttributeFinder.Rules.Domain

open UCLID_AFVALUEFINDERSLib
open Extract.AttributeFinder.Rules
open Extract.Utilities
open UCLID_COMUTILSLib

module Boundary =
  let toDto = function
  | EBoundary.kNoBoundary -> Dto.Boundary.NoBoundary
  | EBoundary.kTop -> Dto.Boundary.Top
  | EBoundary.kBottom -> Dto.Boundary.Bottom
  | EBoundary.kLeft -> Dto.Boundary.Left
  | EBoundary.kRight -> Dto.Boundary.Right
  | other -> failwithf "Not a valid EBoundary! %A" other

  let fromDto = function
  | Dto.Boundary.NoBoundary -> EBoundary.kNoBoundary
  | Dto.Boundary.Top -> EBoundary.kTop
  | Dto.Boundary.Bottom -> EBoundary.kBottom
  | Dto.Boundary.Left -> EBoundary.kLeft
  | Dto.Boundary.Right -> EBoundary.kRight
  | other -> failwithf "Not a valid Boundary! %A" other

module ExpandDirection =
  let toDto = function
  | EExpandDirection.kNoDirection -> Dto.ExpandDirection.NoDirection
  | EExpandDirection.kExpandUp -> Dto.ExpandDirection.ExpandUp
  | EExpandDirection.kExpandDown -> Dto.ExpandDirection.ExpandDown
  | EExpandDirection.kExpandLeft -> Dto.ExpandDirection.ExpandLeft
  | EExpandDirection.kExpandRight -> Dto.ExpandDirection.ExpandRight
  | other -> failwithf "Not a valid EExpandDirection! %A" other

  let fromDto = function
  | Dto.ExpandDirection.NoDirection -> EExpandDirection.kNoDirection
  | Dto.ExpandDirection.ExpandUp -> EExpandDirection.kExpandUp
  | Dto.ExpandDirection.ExpandDown -> EExpandDirection.kExpandDown
  | Dto.ExpandDirection.ExpandLeft -> EExpandDirection.kExpandLeft
  | Dto.ExpandDirection.ExpandRight -> EExpandDirection.kExpandRight
  | other -> failwithf "Not a valid ExpandDirection! %A" other

module Units =
  let toDto = function
  | EUnits.kClueLines -> Dto.Units.ClueLines
  | EUnits.kPageLines -> Dto.Units.PageLines
  | EUnits.kClueCharacters -> Dto.Units.ClueCharacters
  | EUnits.kPageCharacters -> Dto.Units.PageCharacters
  | EUnits.kInches -> Dto.Units.Inches
  | EUnits.kPixels -> Dto.Units.Pixels
  | other -> failwithf "Not a valid EUnits! %A" other

  let fromDto = function
  | Dto.Units.ClueLines -> EUnits.kClueLines
  | Dto.Units.PageLines -> EUnits.kPageLines
  | Dto.Units.ClueCharacters -> EUnits.kClueCharacters
  | Dto.Units.PageCharacters -> EUnits.kPageCharacters
  | Dto.Units.Inches -> EUnits.kInches
  | Dto.Units.Pixels -> EUnits.kPixels
  | other -> failwithf "Not a valid Units! %A" other

module BoundaryCondition =
  let toDto = function
  | EBoundaryCondition.kNoCondition -> Dto.BoundaryCondition.NoCondition
  | EBoundaryCondition.kClueList1 -> Dto.BoundaryCondition.ClueList1
  | EBoundaryCondition.kClueList2 -> Dto.BoundaryCondition.ClueList2
  | EBoundaryCondition.kClueList3 -> Dto.BoundaryCondition.ClueList3
  | EBoundaryCondition.kClueList4 -> Dto.BoundaryCondition.ClueList4
  | EBoundaryCondition.kPage -> Dto.BoundaryCondition.Page
  | other -> failwithf "Not a valid EBoundaryCondition! %A" other

  let fromDto = function
  | Dto.BoundaryCondition.NoCondition -> EBoundaryCondition.kNoCondition
  | Dto.BoundaryCondition.ClueList1 -> EBoundaryCondition.kClueList1
  | Dto.BoundaryCondition.ClueList2 -> EBoundaryCondition.kClueList2
  | Dto.BoundaryCondition.ClueList3 -> EBoundaryCondition.kClueList3
  | Dto.BoundaryCondition.ClueList4 -> EBoundaryCondition.kClueList4
  | Dto.BoundaryCondition.Page -> EBoundaryCondition.kPage
  | other -> failwithf "Not a valid BoundaryCondition! %A" other

module LocateImageRegion =
  open Extract.AttributeFinder.Rules.Dto

  let private getBoundary (source: ILocateImageRegion) boundary =
    let side = ref Unchecked.defaultof<EBoundary>
    let condition = ref Unchecked.defaultof<EBoundaryCondition>
    let expandDirection = ref Unchecked.defaultof<EExpandDirection>
    let expandNumber = ref 0.0
    let expandUnits = ref Unchecked.defaultof<EUnits>
    source.GetRegionBoundary(boundary, side, condition, expandDirection, expandNumber, expandUnits)
    { Dto.LocateImageRegionBoundary.Anchor = !condition |> BoundaryCondition.toDto
      AnchorSide = !side |> Boundary.toDto
      ExpandDirection = !expandDirection |> ExpandDirection.toDto
      ExpandBy = !expandNumber
      ExpandUnits = !expandUnits |> Units.toDto }

  let private setBoundary (target: ILocateImageRegion) (x: Dto.LocateImageRegionBoundary) anchor =
    target.SetRegionBoundary
      ( anchor,
        x.AnchorSide |> Boundary.fromDto,
        x.Anchor |> BoundaryCondition.fromDto,
        x.ExpandDirection |> ExpandDirection.fromDto,
        x.ExpandBy,
        x.ExpandUnits |> Units.fromDto )
 
  let private getClueList (source: ILocateImageRegion) clueListNumber =
    let vecClues: VariantVector ref = ref null
    let caseSensitive = ref false
    let asRegExpr = ref false
    let restrictByBoundary = ref false
    source.GetClueList(enum clueListNumber, vecClues, caseSensitive, asRegExpr, restrictByBoundary)
    { Dto.LocateImageRegionClueList.Clues = !vecClues |> VariantVector.toList
      CaseSensitive = !caseSensitive
      Regex = !asRegExpr
      RestrictByBoundary = !restrictByBoundary }

  let private setClueList (target: ILocateImageRegion) (x: Dto.LocateImageRegionClueList) clueListNumber =
    match x.Clues with
    | [] -> ()
    | _ ->
      target.SetClueList
        ( clueListNumber |> enum,
          x.Clues.ToVariantVector (),
          x.CaseSensitive,
          x.Regex,
          x.RestrictByBoundary )
 
  let toDto (domain: ILocateImageRegion): Dto.LocateImageRegion =
    { DataInsideBoundaries = domain.DataInsideBoundaries
      FindType = domain.FindType |> FindType.toDto
      ImageRegionText = domain.ImageRegionText
      IncludeIntersectingEntities = domain.IncludeIntersectingEntities
      IntersectingEntityType = domain.IntersectingEntityType |> SpatialEntity.toDto
      MatchMultiplePagesPerDocument = domain.MatchMultiplePagesPerDocument
      ClueList1 = 1 |> getClueList domain
      ClueList2 = 2 |> getClueList domain
      ClueList3 = 3 |> getClueList domain
      ClueList4 = 4 |> getClueList domain
      Left = EBoundary.kLeft |> getBoundary domain
      Top = EBoundary.kTop |> getBoundary domain
      Right = EBoundary.kRight |> getBoundary domain
      Bottom = EBoundary.kBottom |> getBoundary domain }
 
  let fromDto (dto: Dto.LocateImageRegion) =
    let domain =
      LocateImageRegionClass
        ( DataInsideBoundaries=dto.DataInsideBoundaries,
          FindType=(dto.FindType |> FindType.fromDto),
          ImageRegionText=dto.ImageRegionText,
          IncludeIntersectingEntities=dto.IncludeIntersectingEntities,
          IntersectingEntityType=(dto.IntersectingEntityType |> SpatialEntity.fromDto),
          MatchMultiplePagesPerDocument=dto.MatchMultiplePagesPerDocument )
    setClueList domain dto.ClueList1 1
    setClueList domain dto.ClueList2 2
    setClueList domain dto.ClueList3 3
    setClueList domain dto.ClueList4 4
    setBoundary domain dto.Left EBoundary.kLeft
    setBoundary domain dto.Top EBoundary.kTop
    setBoundary domain dto.Right EBoundary.kRight
    setBoundary domain dto.Bottom EBoundary.kBottom
    domain

type LocateImageRegionConverter() =
  inherit RuleObjectConverter<LocateImageRegionClass, ILocateImageRegion, Dto.LocateImageRegion>()
  override _.toDto _mc domain = domain |> LocateImageRegion.toDto
  override _.fromDto _mc dto = dto |> LocateImageRegion.fromDto
