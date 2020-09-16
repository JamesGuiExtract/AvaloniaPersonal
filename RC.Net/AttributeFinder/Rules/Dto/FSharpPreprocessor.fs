namespace Extract.AttributeFinder.Rules.Dto

type FSharpPreprocessor = {
  ScriptPath: string
  FunctionName: string
}

type FSharpPreprocessorV2 = {
  ScriptPath: string
  FunctionName: string
  Collectible: bool
}