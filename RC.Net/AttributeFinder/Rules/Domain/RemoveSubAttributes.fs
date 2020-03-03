namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFOUTPUTHANDLERSLib
open UCLID_COMUTILSLib

module ConditionComparisonType =
  let toDto = function
  | EConditionComparisonType.kValueOf -> Dto.ConditionComparisonType.ValueOf
  | EConditionComparisonType.kCompareMaximum -> Dto.ConditionComparisonType.Maximum
  | EConditionComparisonType.kCompareMinimum -> Dto.ConditionComparisonType.Minimum
  | other -> failwithf "Not a valid EConditionComparisonType! %A" other

  let fromDto = function
  | Dto.ConditionComparisonType.ValueOf -> EConditionComparisonType.kValueOf
  | Dto.ConditionComparisonType.Maximum -> EConditionComparisonType.kCompareMaximum
  | Dto.ConditionComparisonType.Minimum -> EConditionComparisonType.kCompareMinimum
  | other -> failwithf "Not a valid ConditionComparisonType! %A" other

module RemoveSubAttributes =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (mc: IMasterRuleObjectConverter) (domain: IRemoveSubAttributes): Dto.RemoveSubAttributes =
    { AttributeSelector = domain.AttributeSelector |> ObjectWithType.toDto mc
      ConditionalRemove = domain.ConditionalRemove
      DataScorer = domain.DataScorer |> ObjectWithDescription.toDto mc
      ScoreCondition = domain.ScoreCondition |> ConditionalOp.toDto
      CompareConditionType = domain.CompareConditionType |> ConditionComparisonType.toDto
      ScoreToCompare = domain.ScoreToCompare }

  let fromDto (mc: IMasterRuleObjectConverter) (dto: Dto.RemoveSubAttributes) =
    RemoveSubAttributesClass
      ( AttributeSelector=(dto.AttributeSelector |> ObjectWithType.fromDto mc),
        ConditionalRemove=dto.ConditionalRemove,
        DataScorer=(dto.DataScorer |> ObjectWithDescription.fromDto mc),
        ScoreCondition=(dto.ScoreCondition |> ConditionalOp.fromDto),
        CompareConditionType=(dto.CompareConditionType |> ConditionComparisonType.fromDto),
        ScoreToCompare=dto.ScoreToCompare )

type RemoveSubAttributesConverter() =
  inherit RuleObjectConverter<RemoveSubAttributesClass, IRemoveSubAttributes, Dto.RemoveSubAttributes>()
  override _.toDto mc domain = domain |> RemoveSubAttributes.toDto mc
  override _.fromDto mc dto = dto |> RemoveSubAttributes.fromDto mc
