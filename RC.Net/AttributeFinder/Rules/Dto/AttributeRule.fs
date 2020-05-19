namespace Extract.AttributeFinder.Rules.Dto

type AttributeRule = {
  IsEnabled: bool
  Description: string
  IgnorePreprocessorErrors: bool
  RuleSpecificDocPreprocessor: ObjectWithDescription
  AttributeFindingRule: ObjectWithType
  IgnoreErrors: bool
  ApplyModifyingRules: bool
  AttributeModifyingRuleInfos: ObjectWithDescription list
  IgnoreModifierErrors: bool
  RuleSpecificOutputHandler: ObjectWithDescription
  IgnoreOutputHandlerErrors: bool
}
