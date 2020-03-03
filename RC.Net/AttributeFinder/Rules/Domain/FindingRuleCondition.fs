namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFCONDITIONSLib

module FindingRuleCondition =
  let toDto (mc: IMasterRuleObjectConverter) (domain: IFindingRuleCondition): Dto.FindingRuleCondition =
    { AFRule = domain.AFRule |> ObjectWithType.toDto mc }

  let fromDto (mc: IMasterRuleObjectConverter) (dto: Dto.FindingRuleCondition) =
    FindingRuleConditionClass
      ( AFRule=(dto.AFRule |> ObjectWithType.fromDto mc) )

type FindingRuleConditionConverter() =
  inherit RuleObjectConverter<FindingRuleConditionClass, IFindingRuleCondition, Dto.FindingRuleCondition>()
  override _.toDto mc domain = domain |> FindingRuleCondition.toDto mc
  override _.fromDto mc dto = dto |> FindingRuleCondition.fromDto mc
