namespace Extract.AttributeFinder.Rules.Dto

type ConditionalValueFinder = {
  Condition: ObjectWithType
  InvertCondition: bool
  Rule: ObjectWithType
}