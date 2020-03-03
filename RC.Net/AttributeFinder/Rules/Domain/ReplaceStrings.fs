namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFVALUEMODIFIERSLib
open UCLID_COMUTILSLib

module ReplaceStrings =
  open Extract.AttributeFinder.Rules.Dto

  module Replace =
    let toDto (domain: IStringPair): Dto.Replace =
      { Pattern = domain.StringKey
        Replacement = domain.StringValue }

    let fromDto (dto: Dto.Replace) =
      StringPairClass
        ( StringKey=dto.Pattern,
          StringValue=dto.Replacement )

  let toDto (domain: IReplaceStrings): Dto.ReplaceStrings =
    { Replacements = domain.Replacements |> IUnknownVector.toDto Replace.toDto
      AsRegularExpr = domain.AsRegularExpr
      IsCaseSensitive = domain.IsCaseSensitive }

  let fromDto (dto: Dto.ReplaceStrings) =
    ReplaceStringsClass
      ( Replacements=(dto.Replacements |> IUnknownVector.fromDto Replace.fromDto),
        AsRegularExpr=dto.AsRegularExpr,
        IsCaseSensitive=dto.IsCaseSensitive )

type ReplaceStringsConverter() =
  inherit RuleObjectConverter<ReplaceStringsClass, IReplaceStrings, Dto.ReplaceStrings>()
  override _.toDto _mc domain = domain |> ReplaceStrings.toDto
  override _.fromDto _mc dto = dto |> ReplaceStrings.fromDto
