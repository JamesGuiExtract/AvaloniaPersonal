namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFCORELib
open UCLID_AFPREPROCESSORSLib

module ConditionalPreprocessor =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (mc: IMasterRuleObjectConverter) (domain: IConditionalRule): Dto.ConditionalPreprocessor =
    { Condition = domain.GetCondition () |> ObjectWithType.toDto mc
      InvertCondition = domain.InvertCondition
      Rule = domain.GetRule () |> ObjectWithType.toDto mc }

  let fromDto (mc: IMasterRuleObjectConverter) (dto: Dto.ConditionalPreprocessor) =
    let domain = ConditionalPreprocessorClass ()
    domain.SetCondition (dto.Condition |> ObjectWithType.fromDto mc)
    domain.InvertCondition <- dto.InvertCondition
    domain.SetRule (dto.Rule |> ObjectWithType.fromDto mc)
    domain

type ConditionalPreprocessorConverter() =
  inherit RuleObjectConverter<ConditionalPreprocessorClass, IConditionalRule, Dto.ConditionalPreprocessor>()
  override _.toDto mc domain = domain |> ConditionalPreprocessor.toDto mc
  override _.fromDto mc dto = dto |> ConditionalPreprocessor.fromDto mc
