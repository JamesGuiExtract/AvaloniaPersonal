namespace Extract.AttributeFinder.Rules.Dto

type DataScorerBasedAS = {
  FirstScoreCondition: ConditionalOp
  FirstScoreToCompare: int
  IsSecondCondition: bool
  AndSecondCondition: bool
  SecondScoreCondition: ConditionalOp
  SecondScoreToCompare: int
  DataScorer: ObjectWithDescription
}