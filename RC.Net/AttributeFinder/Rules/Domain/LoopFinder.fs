namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFVALUEFINDERSLib

module LoopFinder =
  module LoopType =
    let toDto = function
    | ELoopType.kDoLoop -> Dto.LoopType.DoLoop
    | ELoopType.kWhileLoop -> Dto.LoopType.WhileLoop
    | ELoopType.kForLoop -> Dto.LoopType.ForLoop
    | other -> failwithf "Not a valid ELoopType! %A" other

    let fromDto = function
    | Dto.LoopType.DoLoop -> ELoopType.kDoLoop
    | Dto.LoopType.WhileLoop -> ELoopType.kWhileLoop
    | Dto.LoopType.ForLoop -> ELoopType.kForLoop
    | other -> failwithf "Not a valid LoopType! %A" other

  open Extract.AttributeFinder.Rules.Dto

  let toDto (mc: IMasterRuleObjectConverter) (domain: ILoopFinder): Dto.LoopFinder =
    { LoopType = domain.LoopType |> LoopType.toDto
      Condition = domain.Condition |> ObjectWithDescription.toDto mc
      ConditionValue = domain.ConditionValue
      FindingRule = domain.FindingRule |> ObjectWithDescription.toDto mc
      Preprocessor = domain.Preprocessor |> ObjectWithDescription.toDto mc
      Iterations = domain.Iterations
      LogExceptionForMaxIterations = domain.LogExceptionForMaxIterations }

  let fromDto (mc: IMasterRuleObjectConverter) (dto: Dto.LoopFinder) =
    let domain =
      LoopFinderClass
        ( LoopType=(dto.LoopType |> LoopType.fromDto),
          ConditionValue=dto.ConditionValue,
          FindingRule=(dto.FindingRule |> ObjectWithDescription.fromDto mc),
          Preprocessor=(dto.Preprocessor |> ObjectWithDescription.fromDto mc),
          Iterations=dto.Iterations,
          LogExceptionForMaxIterations=dto.LogExceptionForMaxIterations )

    if dto.Condition <> ObjectWithDescription.empty
    then domain.Condition <- dto.Condition |> ObjectWithDescription.fromDto mc

    domain

type LoopFinderConverter() =
  inherit RuleObjectConverter<LoopFinderClass, ILoopFinder, Dto.LoopFinder>()
  override _.toDto mc domain = domain |> LoopFinder.toDto mc
  override _.fromDto mc dto = dto |> LoopFinder.fromDto mc
