namespace Extract.AttributeFinder.Rules.Dto

type DataQueryRuleObject = {
  DataConnectionString: string
  DataProviderName: string
  DataSourceName: string
  Query: string
  UseFAMDBConnection: bool
  UseSpecifiedDBConnection: bool
}