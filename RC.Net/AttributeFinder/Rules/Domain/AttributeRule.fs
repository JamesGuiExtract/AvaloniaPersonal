namespace Extract.AttributeFinder.Rules.Domain

module AttributeRule =
  open Extract.AttributeFinder.Rules
  open Extract.AttributeFinder.Rules.Dto
  open UCLID_AFCORELib

  let toDto mc (domain: IAttributeRule): Dto.AttributeRule =
    { IsEnabled = domain.IsEnabled
      Description = domain.Description
      IgnorePreprocessorErrors = domain.IgnorePreprocessorErrors
      RuleSpecificDocPreprocessor = domain.RuleSpecificDocPreprocessor |> ObjectWithDescription.toDto mc
      AttributeFindingRule = domain.AttributeFindingRule |> ObjectWithType.toDto mc
      IgnoreErrors = domain.IgnoreErrors
      ApplyModifyingRules = domain.ApplyModifyingRules
      AttributeModifyingRuleInfos = domain.AttributeModifyingRuleInfos |> ObjectWithDescriptionVector.toDto mc
      IgnoreModifierErrors = domain.IgnoreModifierErrors
      RuleSpecificOutputHandler = domain.RuleSpecificOutputHandler |> ObjectWithDescription.toDto mc
      IgnoreOutputHandlerErrors = domain.IgnoreOutputHandlerErrors }

  let fromDto mc (dto: Dto.AttributeRule) =
    AttributeRuleClass
      ( IsEnabled=dto.IsEnabled,
        Description=dto.Description,
        IgnorePreprocessorErrors=dto.IgnorePreprocessorErrors,
        RuleSpecificDocPreprocessor=(dto.RuleSpecificDocPreprocessor |> ObjectWithDescription.fromDto mc),
        AttributeFindingRule=(dto.AttributeFindingRule |> ObjectWithType.fromDto mc),
        IgnoreErrors=dto.IgnoreErrors,
        ApplyModifyingRules=dto.ApplyModifyingRules,
        AttributeModifyingRuleInfos=(dto.AttributeModifyingRuleInfos |> ObjectWithDescriptionVector.fromDto mc),
        IgnoreModifierErrors=dto.IgnoreModifierErrors,
        RuleSpecificOutputHandler=(dto.RuleSpecificOutputHandler |> ObjectWithDescription.fromDto mc),
        IgnoreOutputHandlerErrors=dto.IgnoreOutputHandlerErrors )
