namespace Extract.AttributeFinder.Rules.Dto

type MergeAttributeTreesInto =
| FirstAttribute = 0
| AttributeWithMostChildren = 1

type MergeAttributeTrees = {
  AttributesToBeMerged: string
  SubAttributesToCompare: string list
  CaseSensitive: bool
  DiscardNonMatchingComparisons: bool
  MergeAttributeTreesInto: MergeAttributeTreesInto
  RemoveEmptyHierarchy: bool
}