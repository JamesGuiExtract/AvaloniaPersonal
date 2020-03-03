namespace Extract.AttributeFinder.Rules.Dto

type ReplacementOccurrenceType =
| All = 0
| First = 1
| Last = 2
| Specified = 3

type AdvancedReplaceString = {
  StrToBeReplaced: string
  AsRegularExpression: bool
  IsCaseSensitive: bool
  Replacement: string
  ReplacementOccurrenceType: ReplacementOccurrenceType
  SpecifiedOccurrence: int
}