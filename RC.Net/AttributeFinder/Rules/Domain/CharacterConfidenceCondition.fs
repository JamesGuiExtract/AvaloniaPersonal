namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFCONDITIONSLib
open UCLID_COMUTILSLib

module AggregateFunction =
  let toDto = function
  | EAggregateFunctions.kAverage -> Dto.AggregateFunction.Average
  | EAggregateFunctions.kMinimum -> Dto.AggregateFunction.Minimum
  | EAggregateFunctions.kMaximum -> Dto.AggregateFunction.Maximum
  | other -> failwithf "Not a valid EAggregateFunctions! %A" other

  let fromDto = function
  | Dto.AggregateFunction.Average -> EAggregateFunctions.kAverage
  | Dto.AggregateFunction.Minimum -> EAggregateFunctions.kMinimum
  | Dto.AggregateFunction.Maximum -> EAggregateFunctions.kMaximum
  | other -> failwithf "Not a valid AggregateFunction! %A" other

module ConditionalOp =
  let toDto = function
  | EConditionalOp.kEQ -> Dto.ConditionalOp.EQ
  | EConditionalOp.kNEQ -> Dto.ConditionalOp.NEQ
  | EConditionalOp.kLT -> Dto.ConditionalOp.LT
  | EConditionalOp.kGT -> Dto.ConditionalOp.GT
  | EConditionalOp.kLEQ -> Dto.ConditionalOp.LEQ
  | EConditionalOp.kGEQ -> Dto.ConditionalOp.GEQ
  | other when int other = -1 -> Dto.ConditionalOp.None
  | other -> failwithf "Not a valid EConditionalOp! %A" other

  let fromDto = function
  | Dto.ConditionalOp.EQ -> EConditionalOp.kEQ
  | Dto.ConditionalOp.NEQ -> EConditionalOp.kNEQ
  | Dto.ConditionalOp.LT -> EConditionalOp.kLT
  | Dto.ConditionalOp.GT -> EConditionalOp.kGT
  | Dto.ConditionalOp.LEQ -> EConditionalOp.kLEQ
  | Dto.ConditionalOp.GEQ -> EConditionalOp.kGEQ
  | other when other |> int = -1 -> other |> int |> enum
  | other -> failwithf "Not a valid ConditionalOp! %A" other

module CharacterConfidenceCondition =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: ICharacterConfidenceCondition): Dto.CharacterConfidenceCondition =
    { IsMet = domain.IsMet
      AggregateFunction = domain.AggregateFunction |> AggregateFunction.toDto
      FirstScoreCondition = domain.FirstScoreCondition |> ConditionalOp.toDto
      FirstScoreToCompare = domain.FirstScoreToCompare
      IsSecondCondition = domain.IsSecondCondition
      AndSecondCondition = domain.AndSecondCondition
      SecondScoreCondition = domain.SecondScoreCondition |> ConditionalOp.toDto
      SecondScoreToCompare = domain.SecondScoreToCompare }

  let fromDto (dto: Dto.CharacterConfidenceCondition) =
    CharacterConfidenceConditionClass
      ( IsMet=dto.IsMet,
        AggregateFunction=(dto.AggregateFunction |> AggregateFunction.fromDto),
        FirstScoreCondition=(dto.FirstScoreCondition |> ConditionalOp.fromDto),
        FirstScoreToCompare=dto.FirstScoreToCompare,
        IsSecondCondition=dto.IsSecondCondition,
        AndSecondCondition=dto.AndSecondCondition,
        SecondScoreCondition=(dto.SecondScoreCondition |> ConditionalOp.fromDto),
        SecondScoreToCompare=dto.SecondScoreToCompare )

type CharacterConfidenceConditionConverter() =
  inherit RuleObjectConverter<CharacterConfidenceConditionClass, ICharacterConfidenceCondition, Dto.CharacterConfidenceCondition>()
  override _.toDto _mc domain = domain |> CharacterConfidenceCondition.toDto
  override _.fromDto _mc dto = dto |> CharacterConfidenceCondition.fromDto
