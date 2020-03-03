namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFCORELib
open UCLID_AFOUTPUTHANDLERSLib

module ConditionalOutputHandler =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (mc: IMasterRuleObjectConverter) (domain: IConditionalRule): Dto.ConditionalOutputHandler =
    { Condition = domain.GetCondition () |> ObjectWithType.toDto mc
      InvertCondition = domain.InvertCondition
      Rule = domain.GetRule () |> ObjectWithType.toDto mc }

  let fromDto (mc: IMasterRuleObjectConverter) (dto: Dto.ConditionalOutputHandler) =
    let domain = ConditionalOutputHandlerClass ()
    domain.SetCondition (dto.Condition |> ObjectWithType.fromDto mc)
    domain.InvertCondition <- dto.InvertCondition
    domain.SetRule (dto.Rule |> ObjectWithType.fromDto mc)
    domain

type ConditionalOutputHandlerConverter() =
  inherit RuleObjectConverter<ConditionalOutputHandlerClass, IConditionalRule, Dto.ConditionalOutputHandler>()
  override _.toDto mc domain = domain |> ConditionalOutputHandler.toDto mc
  override _.fromDto mc dto = dto |> ConditionalOutputHandler.fromDto mc
