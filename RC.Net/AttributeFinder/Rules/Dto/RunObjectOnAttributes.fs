namespace Extract.AttributeFinder.Rules.Dto

type TypeOfObject =
| Modifier = 0
| OutputHandler = 1
| Splitter = 2

type RunObjectOnAttributes = {
  AttributeQuery: string
  AttributeSelector: ObjectWithType
  UseAttributeSelector: bool
  Type: TypeOfObject
  Object: ObjectWithType
}