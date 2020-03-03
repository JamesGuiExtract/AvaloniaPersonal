namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFCORELib
open UCLID_AFVALUEFINDERSLib

module ConditionalValueFinder =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (mc: IMasterRuleObjectConverter) (domain: IConditionalRule): Dto.ConditionalValueFinder =
    { Condition = domain.GetCondition () |> ObjectWithType.toDto mc
      InvertCondition = domain.InvertCondition
      Rule = domain.GetRule () |> ObjectWithType.toDto mc }

  let fromDto (mc: IMasterRuleObjectConverter) (dto: Dto.ConditionalValueFinder) =
    let domain = ConditionalValueFinderClass ()
    domain.SetCondition (dto.Condition |> ObjectWithType.fromDto mc)
    domain.InvertCondition <- dto.InvertCondition
    domain.SetRule (dto.Rule |> ObjectWithType.fromDto mc)
    domain

type ConditionalValueFinderConverter() =
  inherit RuleObjectConverter<ConditionalValueFinderClass, IConditionalRule, Dto.ConditionalValueFinder>()
  override _.toDto mc domain = domain |> ConditionalValueFinder.toDto mc
  override _.fromDto mc dto = dto |> ConditionalValueFinder.fromDto mc
