namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules

module DataQueryRuleObject =
  type DataQueryRuleObjectClass = DataQueryRuleObject
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IDataQueryRuleObject): Dto.DataQueryRuleObject =
    { DataConnectionString = domain.DataConnectionString
      DataProviderName = domain.DataProviderName
      DataSourceName = domain.DataSourceName
      Query = domain.Query
      UseFAMDBConnection = domain.UseFAMDBConnection
      UseSpecifiedDBConnection = domain.UseSpecifiedDBConnection }

  let fromDto (dto: Dto.DataQueryRuleObject) =
    new DataQueryRuleObjectClass
      ( DataConnectionString=dto.DataConnectionString,
        DataProviderName=dto.DataProviderName,
        DataSourceName=dto.DataSourceName,
        Query=dto.Query,
        UseFAMDBConnection=dto.UseFAMDBConnection,
        UseSpecifiedDBConnection=dto.UseSpecifiedDBConnection )

type DataQueryRuleObjectConverter() =
  inherit RuleObjectConverter<DataQueryRuleObject, IDataQueryRuleObject, Dto.DataQueryRuleObject>()
  override _.toDto _mc domain = domain |> DataQueryRuleObject.toDto
  override _.fromDto _mc dto = dto |> DataQueryRuleObject.fromDto


