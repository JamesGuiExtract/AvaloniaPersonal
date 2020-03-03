namespace Extract.AttributeFinder.Rules.Dto

type XMLOutputFormat =
| XMLOriginal = 0
| XMLSchema = 1

type OutputToXML = {
  FileName: string
  Format: XMLOutputFormat
  NamedAttributes: bool
  UseSchemaName: bool
  SchemaName: string
  ValueAsFullText: bool
  RemoveEmptyNodes: bool
  RemoveSpatialInfo: bool
}