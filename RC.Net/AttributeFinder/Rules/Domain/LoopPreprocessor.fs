namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFPREPROCESSORSLib

module LoopPreprocessor =
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

  let toDto (mc: IMasterRuleObjectConverter) (domain: ILoopPreprocessor): Dto.LoopPreprocessor =
    { LoopType = domain.LoopType |> LoopType.toDto
      Condition = domain.Condition |> ObjectWithDescription.toDto mc
      ConditionValue = domain.ConditionValue
      Preprocessor = domain.Preprocessor |> ObjectWithDescription.toDto mc
      Iterations = domain.Iterations
      LogExceptionForMaxIterations = domain.LogExceptionForMaxIterations }

  let fromDto (mc: IMasterRuleObjectConverter) (dto: Dto.LoopPreprocessor) =
    let domain =
      LoopPreprocessorClass
        ( LoopType=(dto.LoopType |> LoopType.fromDto),
          ConditionValue=dto.ConditionValue,
          Preprocessor=(dto.Preprocessor |> ObjectWithDescription.fromDto mc),
          Iterations=dto.Iterations,
          LogExceptionForMaxIterations=dto.LogExceptionForMaxIterations )

    if dto.Condition <> ObjectWithDescription.empty
    then domain.Condition <- dto.Condition |> ObjectWithDescription.fromDto mc

    domain

type LoopPreprocessorConverter() =
  inherit RuleObjectConverter<LoopPreprocessorClass, ILoopPreprocessor, Dto.LoopPreprocessor>()
  override _.toDto mc domain = domain |> LoopPreprocessor.toDto mc
  override _.fromDto mc dto = dto |> LoopPreprocessor.fromDto mc
