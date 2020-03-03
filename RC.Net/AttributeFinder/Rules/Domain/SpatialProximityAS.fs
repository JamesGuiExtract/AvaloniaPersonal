namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFSELECTORSLib

module AnchorType =
  let toDto = function
  | EBorderRelation.kReferenceAttibute -> Dto.AnchorType.ReferenceAttribute
  | EBorderRelation.kPage -> Dto.AnchorType.Page
  | other -> failwithf "Not a valid EBorderRelation! %A" other

  let fromDto = function
  | Dto.AnchorType.ReferenceAttribute -> EBorderRelation.kReferenceAttibute
  | Dto.AnchorType.Page -> EBorderRelation.kPage
  | other -> failwithf "Not a valid AnchorType! %A" other

module Border =
  let toDto = function
  | EBorder.kNoBorder -> Dto.Boundary.NoBoundary
  | EBorder.kTop -> Dto.Boundary.Top
  | EBorder.kBottom -> Dto.Boundary.Bottom
  | EBorder.kLeft -> Dto.Boundary.Left
  | EBorder.kRight -> Dto.Boundary.Right
  | other -> failwithf "Not a valid EBorder! %A" other

  let fromDto = function
  | Dto.Boundary.NoBoundary -> EBorder.kNoBorder
  | Dto.Boundary.Top -> EBorder.kTop
  | Dto.Boundary.Bottom -> EBorder.kBottom
  | Dto.Boundary.Left -> EBorder.kLeft
  | Dto.Boundary.Right -> EBorder.kRight
  | other -> failwithf "Not a valid Boundary! %A" other

module BorderExpandDirection =
  let toDto = function
  | EBorderExpandDirection.kNoDirection -> Dto.ExpandDirection.NoDirection
  | EBorderExpandDirection.kExpandUp -> Dto.ExpandDirection.ExpandUp
  | EBorderExpandDirection.kExpandDown -> Dto.ExpandDirection.ExpandDown
  | EBorderExpandDirection.kExpandLeft -> Dto.ExpandDirection.ExpandLeft
  | EBorderExpandDirection.kExpandRight -> Dto.ExpandDirection.ExpandRight
  | other -> failwithf "Not a valid EBorderExpandDirection! %A" other

  let fromDto = function
  | Dto.ExpandDirection.NoDirection -> EBorderExpandDirection.kNoDirection
  | Dto.ExpandDirection.ExpandUp -> EBorderExpandDirection.kExpandUp
  | Dto.ExpandDirection.ExpandDown -> EBorderExpandDirection.kExpandDown
  | Dto.ExpandDirection.ExpandLeft -> EBorderExpandDirection.kExpandLeft
  | Dto.ExpandDirection.ExpandRight -> EBorderExpandDirection.kExpandRight
  | other -> failwithf "Not a valid ExpandDirection! %A" other

module ReferenceUnits =
  let toDto = function
  | EUnits.kLines -> Dto.ReferenceUnits.Lines
  | EUnits.kCharacters -> Dto.ReferenceUnits.Characters
  | EUnits.kInches -> Dto.ReferenceUnits.Inches
  | EUnits.kPixels -> Dto.ReferenceUnits.Pixels
  | other -> failwithf "Not a valid EUnits! %A" other

  let fromDto = function
  | Dto.ReferenceUnits.Lines -> EUnits.kLines
  | Dto.ReferenceUnits.Characters -> EUnits.kCharacters
  | Dto.ReferenceUnits.Inches -> EUnits.kInches
  | Dto.ReferenceUnits.Pixels -> EUnits.kPixels
  | other -> failwithf "Not a valid ReferenceUnits! %A" other

module SpatialProximityAS =
  open Extract.AttributeFinder.Rules.Dto

  let private getBoundary (source: ISpatialProximityAS) border =
    let relation = ref Unchecked.defaultof<EBorderRelation>
    let relationBorder = ref Unchecked.defaultof<EBorder>
    let expandDirection = ref Unchecked.defaultof<EBorderExpandDirection>
    let expandNumber = ref 0.0
    let expandUnits = ref Unchecked.defaultof<EUnits>
    source.GetRegionBorder(border, relation, relationBorder, expandDirection, expandNumber, expandUnits)
    { Dto.ReferenceRegionBoundary.Anchor = !relation |> AnchorType.toDto
      AnchorSide = !relationBorder |> Border.toDto
      ExpandDirection = !expandDirection |> BorderExpandDirection.toDto
      ExpandBy = !expandNumber
      ExpandUnits = !expandUnits |> ReferenceUnits.toDto }

  let private setBoundary (target: ISpatialProximityAS) (x: Dto.ReferenceRegionBoundary) border =
    target.SetRegionBorder
      ( border,
        x.Anchor |> AnchorType.fromDto,
        x.AnchorSide |> Border.fromDto,
        x.ExpandDirection |> BorderExpandDirection.fromDto,
        x.ExpandBy,
        x.ExpandUnits |> ReferenceUnits.fromDto )

  let toDto (domain: ISpatialProximityAS): Dto.SpatialProximityAS =
    { TargetQuery = domain.TargetQuery
      TargetsMustContainReferences = domain.TargetsMustContainReferences
      RequireCompleteInclusion = domain.RequireCompleteInclusion
      ReferenceQuery = domain.ReferenceQuery
      Left = EBorder.kLeft |> getBoundary domain
      Top = EBorder.kTop |> getBoundary domain
      Right = EBorder.kRight |> getBoundary domain
      Bottom = EBorder.kBottom |> getBoundary domain
      CompareLinesSeparately = domain.CompareLinesSeparately
      IncludeDebugAttributes = domain.IncludeDebugAttributes }

  let fromDto (dto: Dto.SpatialProximityAS) =
    let domain =
      SpatialProximityASClass
        ( TargetQuery=dto.TargetQuery,
          TargetsMustContainReferences=dto.TargetsMustContainReferences,
          RequireCompleteInclusion=dto.RequireCompleteInclusion,
          ReferenceQuery=dto.ReferenceQuery,
          CompareLinesSeparately=dto.CompareLinesSeparately,
          IncludeDebugAttributes=dto.IncludeDebugAttributes )
    setBoundary domain dto.Left EBorder.kLeft
    setBoundary domain dto.Top EBorder.kTop
    setBoundary domain dto.Right EBorder.kRight
    setBoundary domain dto.Bottom EBorder.kBottom
    domain
type SpatialProximityASConverter() =
  inherit RuleObjectConverter<SpatialProximityASClass, ISpatialProximityAS, Dto.SpatialProximityAS>()
  override _.toDto _mc domain = domain |> SpatialProximityAS.toDto
  override _.fromDto _mc dto = dto |> SpatialProximityAS.fromDto
