namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open System.Collections

module AttributeNameAndTypeAndValue =
  type AttributeNameAndTypeAndValueClass = AttributeNameAndTypeAndValue
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: AttributeNameAndTypeAndValueClass): Dto.AttributeNameAndTypeAndValue =
    { Name = domain.Name
      NameContainsXPath = domain.NameContainsXPath
      DoNotCreateIfNameIsEmpty = domain.DoNotCreateIfNameIsEmpty
      TypeOfAttribute = domain.TypeOfAttribute
      TypeContainsXPath = domain.TypeContainsXPath
      DoNotCreateIfTypeIsEmpty = domain.DoNotCreateIfTypeIsEmpty
      Value = domain.Value
      ValueContainsXPath = domain.ValueContainsXPath
      DoNotCreateIfValueIsEmpty = domain.DoNotCreateIfValueIsEmpty }

  let fromDto (dto: Dto.AttributeNameAndTypeAndValue) =
    AttributeNameAndTypeAndValueClass
      ( Name=dto.Name,
        NameContainsXPath=dto.NameContainsXPath,
        DoNotCreateIfNameIsEmpty=dto.DoNotCreateIfNameIsEmpty,
        TypeOfAttribute=dto.TypeOfAttribute,
        TypeContainsXPath=dto.TypeContainsXPath,
        DoNotCreateIfTypeIsEmpty=dto.DoNotCreateIfTypeIsEmpty,
        Value=dto.Value,
        ValueContainsXPath=dto.ValueContainsXPath,
        DoNotCreateIfValueIsEmpty=dto.DoNotCreateIfValueIsEmpty )

module CreateAttribute =
  type CreateAttributeClass = CreateAttribute
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: CreateAttributeClass): Dto.CreateAttribute =
    { Root = domain.Root
      SubAttributesToCreate = domain.SubAttributesToCreate
                              |> Seq.map AttributeNameAndTypeAndValue.toDto
                              |> Seq.toList }

  let fromDto (dto: Dto.CreateAttribute) =
    CreateAttributeClass
      ( Root=dto.Root,
        SubAttributesToCreate=(dto.SubAttributesToCreate
                               |> Seq.map AttributeNameAndTypeAndValue.fromDto
                               |> Generic.List) )

type CreateAttributeConverter() =
  inherit RuleObjectConverter<CreateAttribute, CreateAttribute, Dto.CreateAttribute>()
  override _.toDto _mc domain = domain |> CreateAttribute.toDto
  override _.fromDto _mc dto = dto |> CreateAttribute.fromDto
