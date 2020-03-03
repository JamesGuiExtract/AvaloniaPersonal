namespace Extract.AttributeFinder.Rules.Dto

type ConditionalPreprocessor = {
  Condition: ObjectWithType
  InvertCondition: bool
  Rule: ObjectWithType
}