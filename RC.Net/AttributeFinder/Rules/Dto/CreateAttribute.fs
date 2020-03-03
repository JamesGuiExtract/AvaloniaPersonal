namespace Extract.AttributeFinder.Rules.Dto

type AttributeNameAndTypeAndValue = {
  Name: string
  NameContainsXPath: bool
  DoNotCreateIfNameIsEmpty: bool
  TypeOfAttribute: string
  TypeContainsXPath: bool
  DoNotCreateIfTypeIsEmpty: bool
  Value: string
  ValueContainsXPath: bool
  DoNotCreateIfValueIsEmpty: bool
}

type CreateAttribute = {
  Root: string
  SubAttributesToCreate: AttributeNameAndTypeAndValue list
}