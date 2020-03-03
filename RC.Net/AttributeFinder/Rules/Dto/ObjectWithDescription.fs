namespace Extract.AttributeFinder.Rules.Dto

type ObjectWithType = {
  Type: string
  Object: obj
}

type ObjectWithDescription = {
  Type: string
  Description: string
  Enabled: bool
  Object: obj
}