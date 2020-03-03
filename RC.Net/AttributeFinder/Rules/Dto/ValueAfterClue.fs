namespace Extract.AttributeFinder.Rules.Dto

type RuleRefiningType =
| NoRefiningType = 0
| UptoXWords = 1
| ClueLine = 2
| UptoXLines = 3
| ClueToString = 4

type ValueAfterClue = {
  Clues: string list
  IsCaseSensitive: bool
  ClueAsRegExpr: bool
  RefiningType: RuleRefiningType
  ClueToString: string
  ClueToStringAsRegExpr: bool
  NumOfLines: int
  IncludeClueLine: bool
  NumOfWords: int
  Punctuations: string
  StopAtNewLine: bool
  StopChars: string
}
