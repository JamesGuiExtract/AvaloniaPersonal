namespace Extract.AttributeFinder.Rules.Dto

type LoopPreprocessor = {
  LoopType: LoopType
  Condition: ObjectWithDescription
  ConditionValue: bool
  Preprocessor: ObjectWithDescription
  Iterations: int
  LogExceptionForMaxIterations: bool
}