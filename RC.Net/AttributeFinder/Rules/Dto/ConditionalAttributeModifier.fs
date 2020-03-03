namespace Extract.AttributeFinder.Rules.Dto

type ConditionalAttributeModifier = {
  Condition: ObjectWithType
  InvertCondition: bool
  Rule: ObjectWithType
}