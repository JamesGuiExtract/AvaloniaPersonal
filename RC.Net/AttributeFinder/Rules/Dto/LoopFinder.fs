namespace Extract.AttributeFinder.Rules.Dto

type LoopType =
| DoLoop = 0
| WhileLoop = 1
| ForLoop = 2

type LoopFinder = {
  LoopType: LoopType
  Condition: ObjectWithDescription
  ConditionValue: bool
  FindingRule: ObjectWithDescription
  Preprocessor: ObjectWithDescription
  Iterations: int
  LogExceptionForMaxIterations: bool
}