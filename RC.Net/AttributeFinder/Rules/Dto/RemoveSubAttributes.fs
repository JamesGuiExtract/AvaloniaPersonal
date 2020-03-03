namespace Extract.AttributeFinder.Rules.Dto

type ConditionComparisonType =
| ValueOf = 0
| Maximum = 1
| Minimum = 2

type RemoveSubAttributes = {
  AttributeSelector: ObjectWithType
  ConditionalRemove: bool
  DataScorer: ObjectWithDescription
  ScoreCondition: ConditionalOp
  CompareConditionType: ConditionComparisonType
  ScoreToCompare: int
}