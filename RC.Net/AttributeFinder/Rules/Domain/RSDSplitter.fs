namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFSPLITTERSLib

module RSDSplitter =
  let toDto (domain: IRSDSplitter): Dto.RSDSplitter =
    { RSDFileName = domain.RSDFileName }

  let fromDto (dto: Dto.RSDSplitter) =
    RSDSplitterClass
      ( RSDFileName=dto.RSDFileName )

type RSDSplitterConverter() =
  inherit RuleObjectConverter<RSDSplitterClass, IRSDSplitter, Dto.RSDSplitter>()
  override _.toDto _mc domain = domain |> RSDSplitter.toDto
  override _.fromDto _mc dto = dto |> RSDSplitter.fromDto
