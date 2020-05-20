namespace Extract.AttributeFinder.Rules.Dto

type AttributeModifierCollection = {
  Enabled: bool
  ObjectsVector: ObjectWithDescription list
}

type AttributeRule = {
  Enabled: bool
  Description: string
  IgnorePreprocessorErrors: bool
  RuleSpecificDocPreprocessor: ObjectWithDescription
  AttributeFindingRule: ObjectWithType
  IgnoreErrors: bool
  AttributeModifiers: AttributeModifierCollection
  IgnoreModifierErrors: bool
  RuleSpecificOutputHandler: ObjectWithDescription
  IgnoreOutputHandlerErrors: bool
}
