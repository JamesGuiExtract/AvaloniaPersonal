namespace Extract.AttributeFinder.Rules.Dto

type ValueGeneratorType =
| None = 0
| Literal = 1
| FromTag = 2
| FromAttributes = 3

type StringValueGenerator = {
  TagName: string
  Generator: ValueGeneratorType
  SpecifiedValue: string
  ValuesFromTag: string
  AttributeSelector: ObjectWithType
  Delimiter: string
}

type ObjectValueGenerator = {
  TagName: string
  Generator: ValueGeneratorType
  SpecifiedValue: string
  AttributeSelector: ObjectWithType
}

type SetDocumentTags = {
  SetStringTag: StringValueGenerator
  SetObjectTag: ObjectValueGenerator
  NoTagsIfEmpty: bool
  GenerateSourceAttributesWithRSDFile: bool
  SourceAttributeRSDFile: string
}
