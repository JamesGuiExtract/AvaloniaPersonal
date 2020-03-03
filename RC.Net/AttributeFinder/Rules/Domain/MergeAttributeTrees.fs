namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFOUTPUTHANDLERSLib

module MergeAttributeTreesInto =
  let toDto = function
  | EMergeAttributeTreesInto.kFirstAttribute -> Dto.MergeAttributeTreesInto.FirstAttribute
  | EMergeAttributeTreesInto.kAttributeWithMostChildren -> Dto.MergeAttributeTreesInto.AttributeWithMostChildren
  | other -> failwithf "Not a valid EMergeAttributeTreesInto! %A" other

  let fromDto = function
  | Dto.MergeAttributeTreesInto.FirstAttribute -> EMergeAttributeTreesInto.kFirstAttribute
  | Dto.MergeAttributeTreesInto.AttributeWithMostChildren -> EMergeAttributeTreesInto.kAttributeWithMostChildren
  | other -> failwithf "Not a valid MergeAttributeTreesInto! %A" other

module MergeAttributeTrees =
  open Extract.AttributeFinder.Rules.Dto

  let private splitLines: string -> string list = function
    | null -> []
    | str -> str.Split([|'\r'; '\n'|], System.StringSplitOptions.RemoveEmptyEntries) |> Array.toList

  let private joinLines: string list -> string =
    String.concat "\r\n"

  let toDto (domain: IMergeAttributeTrees): Dto.MergeAttributeTrees =
    { AttributesToBeMerged = domain.AttributesToBeMerged
      SubAttributesToCompare = domain.SubAttributesToCompare |> splitLines
      CaseSensitive = domain.CaseSensitive
      DiscardNonMatchingComparisons = domain.DiscardNonMatchingComparisons
      MergeAttributeTreesInto = domain.MergeAttributeTreesInto |> MergeAttributeTreesInto.toDto
      RemoveEmptyHierarchy = domain.RemoveEmptyHierarchy }

  let fromDto (dto: Dto.MergeAttributeTrees) =
    MergeAttributeTreesClass
      ( AttributesToBeMerged=dto.AttributesToBeMerged,
        SubAttributesToCompare=(dto.SubAttributesToCompare |> joinLines),
        CaseSensitive=dto.CaseSensitive,
        DiscardNonMatchingComparisons=dto.DiscardNonMatchingComparisons,
        MergeAttributeTreesInto=(dto.MergeAttributeTreesInto |> MergeAttributeTreesInto.fromDto),
        RemoveEmptyHierarchy=dto.RemoveEmptyHierarchy )

type MergeAttributeTreesConverter() =
  inherit RuleObjectConverter<MergeAttributeTreesClass, IMergeAttributeTrees, Dto.MergeAttributeTrees>()
  override _.toDto _mc domain = domain |> MergeAttributeTrees.toDto
  override _.fromDto _mc dto = dto |> MergeAttributeTrees.fromDto
