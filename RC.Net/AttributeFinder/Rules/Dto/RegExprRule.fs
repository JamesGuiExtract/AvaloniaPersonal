namespace Extract.AttributeFinder.Rules.Dto

type RegExprRule = {
  IsRegExpFromFile: bool
  RegExpFileName: string
  Pattern: string
  IsCaseSensitive: bool
  FirstMatchOnly: bool
  CreateSubAttributesFromNamedMatches: bool
  OnlyCreateOneSubAttributePerGroup: bool
}