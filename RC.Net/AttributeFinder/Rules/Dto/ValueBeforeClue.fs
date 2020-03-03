namespace Extract.AttributeFinder.Rules.Dto

type ValueBeforeClue = {
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
