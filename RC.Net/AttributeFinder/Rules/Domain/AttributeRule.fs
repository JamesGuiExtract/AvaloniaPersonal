namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open Extract.AttributeFinder.Rules.Dto
open UCLID_AFCORELib

module AttributeModifierCollection =
  let toDto (mc: IMasterRuleObjectConverter) (attributeRule: IAttributeRule): Dto.AttributeModifierCollection =
    { Enabled = attributeRule.ApplyModifyingRules
      ObjectsVector = attributeRule.AttributeModifyingRuleInfos |> ObjectWithDescriptionVector.toDto mc }

module AttributeRule =
  let toDto mc (domain: IAttributeRule): Dto.AttributeRule =
    { Enabled = domain.IsEnabled
      Description = domain.Description
      IgnorePreprocessorErrors = domain.IgnorePreprocessorErrors
      RuleSpecificDocPreprocessor = domain.RuleSpecificDocPreprocessor |> ObjectWithDescription.toDto mc
      AttributeFindingRule = domain.AttributeFindingRule |> ObjectWithType.toDto mc
      IgnoreErrors = domain.IgnoreErrors
      AttributeModifiers = domain |> AttributeModifierCollection.toDto mc
      IgnoreModifierErrors = domain.IgnoreModifierErrors
      RuleSpecificOutputHandler = domain.RuleSpecificOutputHandler |> ObjectWithDescription.toDto mc
      IgnoreOutputHandlerErrors = domain.IgnoreOutputHandlerErrors }

  let fromDto mc (dto: Dto.AttributeRule) =
    AttributeRuleClass
      ( IsEnabled=dto.Enabled,
        Description=dto.Description,
        IgnorePreprocessorErrors=dto.IgnorePreprocessorErrors,
        RuleSpecificDocPreprocessor=(dto.RuleSpecificDocPreprocessor |> ObjectWithDescription.fromDto mc),
        AttributeFindingRule=(dto.AttributeFindingRule |> ObjectWithType.fromDto mc),
        IgnoreErrors=dto.IgnoreErrors,
        ApplyModifyingRules=dto.AttributeModifiers.Enabled,
        AttributeModifyingRuleInfos=(dto.AttributeModifiers.ObjectsVector |> ObjectWithDescriptionVector.fromDto mc),
        IgnoreModifierErrors=dto.IgnoreModifierErrors,
        RuleSpecificOutputHandler=(dto.RuleSpecificOutputHandler |> ObjectWithDescription.fromDto mc),
        IgnoreOutputHandlerErrors=dto.IgnoreOutputHandlerErrors )
