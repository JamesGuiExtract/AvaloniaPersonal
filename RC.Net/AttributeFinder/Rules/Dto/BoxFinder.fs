namespace Extract.AttributeFinder.Rules.Dto

type ClueLocation =
| SameBox = 0
| BoxToTopLeft = 1
| BoxToTop = 2
| BoxToTopRight = 3
| BoxToRight = 4
| BoxToBottomRight = 5
| BoxToBottom = 6
| BoxToBottomLeft = 7
| BoxToLeft = 8

type FindType =
| Text = 0
| ImageRegion = 1

type PageSelectionMode =
| AllPages = 0
| FirstPages = 1
| LastPages = 2
| SpecifiedPages = 3

type BoxFinder = {
  AttributeText: string
  BoxHeightMax: double option
  BoxHeightMin: double option
  BoxWidthMax: double option
  BoxWidthMin: double option
  ClueLocation: ClueLocation
  Clues: string list
  CluesAreCaseSensitive: bool
  CluesAreRegularExpressions: bool
  ExcludeClueArea: bool
  FindType: FindType
  FirstBoxOnly: bool
  IncludeClueText: bool
  IncludeLines: bool
  NumFirstPages: int
  NumLastPages: int
  PageSelectionMode: PageSelectionMode
  SpecifiedPages: string
}