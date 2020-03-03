namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open Extract.Utilities
open UCLID_AFVALUEFINDERSLib
open System

module ClueLocation =
  let toDto = function
  | EClueLocation.kSameBox -> Dto.ClueLocation.SameBox
  | EClueLocation.kBoxToTopLeft -> Dto.ClueLocation.BoxToTopLeft
  | EClueLocation.kBoxToTop -> Dto.ClueLocation.BoxToTop
  | EClueLocation.kBoxToTopRight -> Dto.ClueLocation.BoxToTopRight
  | EClueLocation.kBoxToRight -> Dto.ClueLocation.BoxToRight
  | EClueLocation.kBoxToBottomRight -> Dto.ClueLocation.BoxToBottomRight
  | EClueLocation.kBoxToBottom -> Dto.ClueLocation.BoxToBottom
  | EClueLocation.kBoxToBottomLeft -> Dto.ClueLocation.BoxToBottomLeft
  | EClueLocation.kBoxToLeft -> Dto.ClueLocation.BoxToLeft
  | other -> failwithf "Not a valid EClueLocation! %A" other

  let fromDto = function
  | Dto.ClueLocation.SameBox -> EClueLocation.kSameBox
  | Dto.ClueLocation.BoxToTopLeft -> EClueLocation.kBoxToTopLeft
  | Dto.ClueLocation.BoxToTop -> EClueLocation.kBoxToTop
  | Dto.ClueLocation.BoxToTopRight -> EClueLocation.kBoxToTopRight
  | Dto.ClueLocation.BoxToRight -> EClueLocation.kBoxToRight
  | Dto.ClueLocation.BoxToBottomRight -> EClueLocation.kBoxToBottomRight
  | Dto.ClueLocation.BoxToBottom -> EClueLocation.kBoxToBottom
  | Dto.ClueLocation.BoxToBottomLeft -> EClueLocation.kBoxToBottomLeft
  | Dto.ClueLocation.BoxToLeft -> EClueLocation.kBoxToLeft
  | other -> failwithf "Not a valid ClueLocation! %A" other

module FindType =
  let toDto = function
  | EFindType.kText -> Dto.FindType.Text
  | EFindType.kImageRegion -> Dto.FindType.ImageRegion
  | other -> failwithf "Not a valid EFindType! %A" other

  let fromDto = function
  | Dto.FindType.Text -> EFindType.kText
  | Dto.FindType.ImageRegion -> EFindType.kImageRegion
  | other -> failwithf "Not a valid FindType! %A" other

module PageSelectionMode =
  let toDto = function
  | EPageSelectionMode.kAllPages -> Dto.PageSelectionMode.AllPages
  | EPageSelectionMode.kFirstPages -> Dto.PageSelectionMode.FirstPages
  | EPageSelectionMode.kLastPages -> Dto.PageSelectionMode.LastPages
  | EPageSelectionMode.kSpecifiedPages -> Dto.PageSelectionMode.SpecifiedPages
  | other -> failwithf "Not a valid EPageSelectionMode! %A" other

  let fromDto = function
  | Dto.PageSelectionMode.AllPages -> EPageSelectionMode.kAllPages
  | Dto.PageSelectionMode.FirstPages -> EPageSelectionMode.kFirstPages
  | Dto.PageSelectionMode.LastPages -> EPageSelectionMode.kLastPages
  | Dto.PageSelectionMode.SpecifiedPages -> EPageSelectionMode.kSpecifiedPages
  | other -> failwithf "Not a valid PageSelectionMode! %A" other

module BoxDimensions =
  let toDto d = if Double.IsNaN d then None else Some d

  // Convert None to 'signaling NaN' to match the c++ representation
  let fromDto = function
  | Some d -> d
  | None -> BitConverter.ToDouble([| 0x01uy; 0x00uy; 0x00uy; 0x00uy; 0x00uy; 0x00uy; 0xf8uy; 0x7fuy |], 0)
  
  
module BoxFinder =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IBoxFinder): Dto.BoxFinder =
    { AttributeText = domain.AttributeText
      BoxHeightMax = domain.BoxHeightMax |> BoxDimensions.toDto
      BoxHeightMin = domain.BoxHeightMin |> BoxDimensions.toDto
      BoxWidthMax = domain.BoxWidthMax |> BoxDimensions.toDto
      BoxWidthMin = domain.BoxWidthMin |> BoxDimensions.toDto
      ClueLocation = domain.ClueLocation |> ClueLocation.toDto
      Clues = downcast domain.Clues |> VariantVector.toList
      CluesAreCaseSensitive = domain.CluesAreCaseSensitive
      CluesAreRegularExpressions = domain.CluesAreRegularExpressions
      ExcludeClueArea = domain.ExcludeClueArea
      FindType = domain.FindType |> FindType.toDto
      FirstBoxOnly = domain.FirstBoxOnly
      IncludeClueText = domain.IncludeClueText
      IncludeLines = domain.IncludeLines
      NumFirstPages = domain.NumFirstPages
      NumLastPages = domain.NumLastPages
      PageSelectionMode = domain.PageSelectionMode |> PageSelectionMode.toDto
      SpecifiedPages = domain.SpecifiedPages }

  let fromDto (dto: Dto.BoxFinder) =
    BoxFinderClass
      ( AttributeText=dto.AttributeText,
        BoxHeightMax=(dto.BoxHeightMax |> BoxDimensions.fromDto),
        BoxHeightMin=(dto.BoxHeightMin |> BoxDimensions.fromDto),
        BoxWidthMax=(dto.BoxWidthMax |> BoxDimensions.fromDto),
        BoxWidthMin=(dto.BoxWidthMin |> BoxDimensions.fromDto),
        ClueLocation=(dto.ClueLocation |> ClueLocation.fromDto),
        Clues=dto.Clues.ToVariantVector(),
        CluesAreCaseSensitive=dto.CluesAreCaseSensitive,
        CluesAreRegularExpressions=dto.CluesAreRegularExpressions,
        ExcludeClueArea=dto.ExcludeClueArea,
        FindType=(dto.FindType |> FindType.fromDto),
        FirstBoxOnly=dto.FirstBoxOnly,
        IncludeClueText=dto.IncludeClueText,
        IncludeLines=dto.IncludeLines,
        NumFirstPages=dto.NumFirstPages,
        NumLastPages=dto.NumLastPages,
        PageSelectionMode=(dto.PageSelectionMode |> PageSelectionMode.fromDto),
        SpecifiedPages=dto.SpecifiedPages )

type BoxFinderConverter() =
  inherit RuleObjectConverter<BoxFinderClass, IBoxFinder, Dto.BoxFinder>()
  override _.toDto _mc domain = domain |> BoxFinder.toDto
  override _.fromDto _mc dto = dto |> BoxFinder.fromDto
