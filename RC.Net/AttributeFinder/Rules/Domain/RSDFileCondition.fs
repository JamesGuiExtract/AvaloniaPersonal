namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFCONDITIONSLib

module RSDFileCondition =
  let toDto (domain: IRSDFileCondition): Dto.RSDFileCondition =
    { RSDFileName = domain.RSDFileName }

  let fromDto (dto: Dto.RSDFileCondition) =
    RSDFileConditionClass
      ( RSDFileName=dto.RSDFileName )

type RSDFileConditionConverter() =
  inherit RuleObjectConverter<RSDFileConditionClass, IRSDFileCondition, Dto.RSDFileCondition>()
  override _.toDto _mc domain = domain |> RSDFileCondition.toDto
  override _.fromDto _mc dto = dto |> RSDFileCondition.fromDto
