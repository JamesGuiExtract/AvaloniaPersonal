namespace Extract.AttributeFinder.Rules.Dto

type RemoveCharacters = {
  Characters: string
  IsCaseSensitive: bool
  RemoveAll: bool
  Consolidate: bool
  TrimLeading: bool
  TrimTrailing: bool
}