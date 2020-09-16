namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules

// Legacy version
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

[<Legacy>]
type FSharpPreprocessorConverter() =
  inherit RuleObjectConverter<FSharpPreprocessor, IFSharpPreprocessor, Dto.FSharpPreprocessor>()
  override _.toDto _mc domain = domain |> FSharpPreprocessor.toDto
  override _.fromDto _mc dto = dto |> FSharpPreprocessor.fromDto


// Current version
module FSharpPreprocessorV2 =
  type FSharpPreprocessorClass = FSharpPreprocessor
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IFSharpPreprocessor): Dto.FSharpPreprocessorV2 =
    { ScriptPath = domain.ScriptPath
      FunctionName = domain.FunctionName
      Collectible = domain.Collectible }

  let fromDto (dto: Dto.FSharpPreprocessorV2) =
    FSharpPreprocessorClass
      ( ScriptPath=dto.ScriptPath,
        FunctionName=dto.FunctionName,
        Collectible=dto.Collectible )

type FSharpPreprocessorV2Converter() =
  inherit RuleObjectConverter<FSharpPreprocessor, IFSharpPreprocessor, Dto.FSharpPreprocessorV2>()
  override _.toDto _mc domain = domain |> FSharpPreprocessorV2.toDto
  override _.fromDto _mc dto = dto |> FSharpPreprocessorV2.fromDto
