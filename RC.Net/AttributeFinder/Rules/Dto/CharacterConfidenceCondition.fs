namespace Extract.AttributeFinder.Rules.Dto

type AggregateFunction =
| Average = 0
| Minimum = 1
| Maximum = 2

type ConditionalOp =
| None = -1
| EQ = 0
| NEQ = 1
| LT = 2
| GT = 3
| LEQ = 4
| GEQ = 5

type CharacterConfidenceCondition = {
  IsMet: bool
  AggregateFunction: AggregateFunction
  FirstScoreCondition: ConditionalOp
  FirstScoreToCompare: int
  IsSecondCondition: bool
  AndSecondCondition: bool
  SecondScoreCondition: ConditionalOp
  SecondScoreToCompare: int
}