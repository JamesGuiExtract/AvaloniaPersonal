namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules

module FSharpPreprocessor =
  type FSharpPreprocessorClass = FSharpPreprocessor
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IFSharpPreprocessor): Dto.FSharpPreprocessor =
    { ScriptPath = domain.ScriptPath
      FunctionName = domain.FunctionName }

  let fromDto (dto: Dto.FSharpPreprocessor) =
    FSharpPreprocessorClass
      ( ScriptPath=dto.ScriptPath,
        FunctionName=dto.FunctionName )

type FSharpPreprocessorConverter() =
  inherit RuleObjectConverter<FSharpPreprocessor, IFSharpPreprocessor, Dto.FSharpPreprocessor>()
  override _.toDto _mc domain = domain |> FSharpPreprocessor.toDto
  override _.fromDto _mc dto = dto |> FSharpPreprocessor.fromDto
