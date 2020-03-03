namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFSELECTORSLib

module DataScorerBasedAS =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (mc: IMasterRuleObjectConverter) (domain: IDataScorerBasedAS): Dto.DataScorerBasedAS =
    { FirstScoreCondition = domain.FirstScoreCondition |> ConditionalOp.toDto
      FirstScoreToCompare = domain.FirstScoreToCompare
      IsSecondCondition = domain.IsSecondCondition
      AndSecondCondition = domain.AndSecondCondition
      SecondScoreCondition = domain.SecondScoreCondition |> ConditionalOp.toDto
      SecondScoreToCompare = domain.SecondScoreToCompare
      DataScorer = domain.DataScorer |> ObjectWithDescription.toDto mc }

  let fromDto (mc: IMasterRuleObjectConverter) (dto: Dto.DataScorerBasedAS) =
    DataScorerBasedASClass
      ( FirstScoreCondition=(dto.FirstScoreCondition |> ConditionalOp.fromDto),
        FirstScoreToCompare=dto.FirstScoreToCompare,
        IsSecondCondition=dto.IsSecondCondition,
        AndSecondCondition=dto.AndSecondCondition,
        SecondScoreCondition=(dto.SecondScoreCondition |> ConditionalOp.fromDto),
        SecondScoreToCompare=dto.SecondScoreToCompare,
        DataScorer=(dto.DataScorer |> ObjectWithDescription.fromDto mc) )

type DataScorerBasedASConverter() =
  inherit RuleObjectConverter<DataScorerBasedASClass, IDataScorerBasedAS, Dto.DataScorerBasedAS>()
  override _.toDto mc domain = domain |> DataScorerBasedAS.toDto mc
  override _.fromDto mc dto = dto |> DataScorerBasedAS.fromDto mc
