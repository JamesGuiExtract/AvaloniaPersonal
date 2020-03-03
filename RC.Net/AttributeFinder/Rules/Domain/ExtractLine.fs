namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFVALUEFINDERSLib

module ExtractLine =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IExtractLine): Dto.ExtractLine =
    { EachLineAsUniqueValue = domain.EachLineAsUniqueValue
      IncludeLineBreak = domain.IncludeLineBreak
      LineNumbers = domain.LineNumbers }

  let fromDto (dto: Dto.ExtractLine) =
    ExtractLineClass
      ( EachLineAsUniqueValue=dto.EachLineAsUniqueValue,
        IncludeLineBreak=dto.IncludeLineBreak,
        LineNumbers=dto.LineNumbers )

type ExtractLineConverter() =
  inherit RuleObjectConverter<ExtractLineClass, IExtractLine, Dto.ExtractLine>()
  override _.toDto _mc domain = domain |> ExtractLine.toDto
  override _.fromDto _mc dto = dto |> ExtractLine.fromDto
