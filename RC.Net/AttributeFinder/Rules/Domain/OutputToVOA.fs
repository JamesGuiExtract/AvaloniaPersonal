namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFOUTPUTHANDLERSLib

module OutputToVOA =
  let toDto (domain: IOutputToVOA): Dto.OutputToVOA =
    { FileName = domain.FileName }

  let fromDto (dto: Dto.OutputToVOA) =
    OutputToVOAClass
      ( FileName=dto.FileName )

type OutputToVOAConverter() =
  inherit RuleObjectConverter<OutputToVOAClass, IOutputToVOA, Dto.OutputToVOA>()
  override _.toDto _mc domain = domain |> OutputToVOA.toDto
  override _.fromDto _mc dto = dto |> OutputToVOA.fromDto
