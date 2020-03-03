namespace Extract.AttributeFinder.Rules.Dto

type ConditionalOutputHandler = {
  Condition: ObjectWithType
  InvertCondition: bool
  Rule: ObjectWithType
}