namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFSELECTORSLib

module QueryBasedAS =
  let toDto (domain: IQueryBasedAS): Dto.QueryBasedAS =
    { QueryText = domain.QueryText }

  let fromDto (dto: Dto.QueryBasedAS) =
    QueryBasedASClass
      ( QueryText=dto.QueryText )

type QueryBasedASConverter() =
  inherit RuleObjectConverter<QueryBasedASClass, IQueryBasedAS, Dto.QueryBasedAS>()
  override _.toDto _mc domain = domain |> QueryBasedAS.toDto
  override _.fromDto _mc dto = dto |> QueryBasedAS.fromDto
