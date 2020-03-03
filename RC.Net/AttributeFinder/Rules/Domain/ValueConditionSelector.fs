namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules

module ValueConditionSelector =
  type ValueConditionSelectorClass = ValueConditionSelector

  let toDto (mc: IMasterRuleObjectConverter) (domain: IValueConditionSelector): Dto.ValueConditionSelector =
    { Condition = domain.Condition |> ObjectWithType.toDto mc }

  let fromDto (mc: IMasterRuleObjectConverter) (dto: Dto.ValueConditionSelector) =
    ValueConditionSelectorClass
      ( Condition=(dto.Condition |> ObjectWithType.fromDto mc) )

type ValueConditionSelectorConverter() =
  inherit RuleObjectConverter<ValueConditionSelector, IValueConditionSelector, Dto.ValueConditionSelector>()
  override _.toDto mc domain = domain |> ValueConditionSelector.toDto mc
  override _.fromDto mc dto = dto |> ValueConditionSelector.fromDto mc
