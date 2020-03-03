namespace Extract.AttributeFinder.Rules.Dto

type NumOfTokensType =
| Any = 0
| Equal = 1
| GreaterThan = 2
| GreaterThanOrEqual = 3

type StringTokenizerModifier = {
  Delimiter: string
  ResultExpression: string
  TextInBetween: string
  NumberOfTokensType: NumOfTokensType
  NumberOfTokensRequired: int
}