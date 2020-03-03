namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFCORELib
open UCLID_AFVALUEMODIFIERSLib

module ConditionalAttributeModifier =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (mc: IMasterRuleObjectConverter) (domain: IConditionalRule): Dto.ConditionalAttributeModifier =
    { Condition = domain.GetCondition () |> ObjectWithType.toDto mc
      InvertCondition = domain.InvertCondition
      Rule = domain.GetRule () |> ObjectWithType.toDto mc }

  let fromDto (mc: IMasterRuleObjectConverter) (dto: Dto.ConditionalAttributeModifier) =
    let domain = ConditionalAttributeModifierClass ()
    domain.SetCondition (dto.Condition |> ObjectWithType.fromDto mc)
    domain.InvertCondition <- dto.InvertCondition
    domain.SetRule (dto.Rule |> ObjectWithType.fromDto mc)
    domain

type ConditionalAttributeModifierConverter() =
  inherit RuleObjectConverter<ConditionalAttributeModifierClass, IConditionalRule, Dto.ConditionalAttributeModifier>()
  override _.toDto mc domain = domain |> ConditionalAttributeModifier.toDto mc
  override _.fromDto mc dto = dto |> ConditionalAttributeModifier.fromDto mc
