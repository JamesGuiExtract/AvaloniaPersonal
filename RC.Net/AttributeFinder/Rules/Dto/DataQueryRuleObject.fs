namespace Extract.AttributeFinder.Rules.Dto

type DataQueryRuleObject = {
  DataConnectionString: string
  DataProviderName: string
  DataSourceName: string
  OpenSqlCompactReadOnly: bool
  Query: string
  UseFAMDBConnection: bool
  UseSpecifiedDBConnection: bool
}