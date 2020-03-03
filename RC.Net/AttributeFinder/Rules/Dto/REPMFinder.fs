namespace Extract.AttributeFinder.Rules.Dto

type PMReturnMatchType =
| ReturnFirstMatch = 1
| ReturnBestMatch = 2 
| ReturnAllMatches = 3 
| ReturnFirstOrBest = 4

type REPMFinder = {
  RulesFileName: string
  IgnoreInvalidTags: bool
  CaseSensitive: bool
  StoreRuleWorked: bool
  RuleWorkedName: string
  DataScorer: ObjectWithDescription
  MinScoreToConsiderAsMatch: int
  ReturnMatchType: PMReturnMatchType
  MinFirstToConsiderAsMatch: int
  OnlyCreateOneAttributePerGroup: bool
}