namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules

type ValueGenerator =
| NoGen
| LiteralGen of string
| FromTagGen of string
| FromAttributesGen of Dto.ObjectWithType

module StringValueGenerator =
  open Extract.AttributeFinder.Rules.Dto

  let toDto tagName delimiter generator =
    let dto =
        { Dto.StringValueGenerator.TagName = tagName
          Generator = ValueGeneratorType.None
          SpecifiedValue = ""
          ValuesFromTag = ""
          AttributeSelector = ObjectWithType.empty
          Delimiter = delimiter }

    match generator with
    | NoGen -> dto
    | LiteralGen specifiedValue ->
        { dto with Generator = ValueGeneratorType.Literal
                   SpecifiedValue = specifiedValue }
    | FromTagGen tagName ->
        { dto with Generator = ValueGeneratorType.FromTag
                   ValuesFromTag = tagName }
    | FromAttributesGen selector ->
        { dto with Generator = ValueGeneratorType.FromAttributes
                   AttributeSelector = selector }

module ObjectValueGenerator =
  open Extract.AttributeFinder.Rules.Dto

  let toDto tagName generator =
    let dto =
        { Dto.ObjectValueGenerator.TagName = tagName
          Generator = ValueGeneratorType.None
          SpecifiedValue = ""
          AttributeSelector = ObjectWithType.empty }

    match generator with
    | NoGen -> dto
    | LiteralGen specifiedValue ->
        { dto with Generator = ValueGeneratorType.Literal
                   SpecifiedValue = specifiedValue }
    | FromTagGen _ -> failwith "Setting object tag from document tag is not valid!"
    | FromAttributesGen selector ->
        { dto with Generator = ValueGeneratorType.FromAttributes
                   AttributeSelector = selector }

module SetDocumentTags =
  type SetDocumentTagsClass = SetDocumentTags
  open Extract.AttributeFinder.Rules.Dto

  let toDto (mc: IMasterRuleObjectConverter) (domain: ISetDocumentTags): Dto.SetDocumentTags =
    { SetStringTag =
        let toDto = StringValueGenerator.toDto domain.StringTagName domain.Delimiter
        if domain.SetStringTag
        then
          match domain.UseSpecifiedValueForStringTag, domain.UseTagValueForStringTag, domain.UseSelectedAttributesForStringTagValue with
          | true, false, false -> LiteralGen domain.SpecifiedValueForStringTag |> toDto
          | false, true, false -> FromTagGen domain.TagNameForStringTagValue |> toDto
          | false, false, true -> FromAttributesGen (domain.StringTagAttributeSelector |> ObjectWithType.toDto mc) |> toDto
          | _, _, _ ->
            failwithf
              "Set string tag configuration is not valid! UseSpecifiedValueForStringTag: %b, UseTagValueForStringTag: %b, UseSelectedAttributesForStringTagValue: %b"
              domain.UseSpecifiedValueForStringTag domain.UseTagValueForStringTag domain.UseSelectedAttributesForStringTagValue
        else NoGen |> toDto
      SetObjectTag =
        let toDto = ObjectValueGenerator.toDto domain.ObjectTagName
        if domain.SetObjectTag
        then
          match domain.UseSpecifiedValueForObjectTag, domain.UseSelectedAttributesForObjectTagValue with
          | true, false -> LiteralGen domain.SpecifiedValueForObjectTag |> toDto
          | false, true -> FromAttributesGen (domain.ObjectTagAttributeSelector |> ObjectWithType.toDto mc) |> toDto
          | _, _ ->
            failwithf
              "Set object tag configuration is not valid! UseSpecifiedValueForObjectTag: %b, UseSelectedAttributesForObjectTagValue: %b"
              domain.UseSpecifiedValueForObjectTag domain.UseSelectedAttributesForObjectTagValue
        else NoGen |> toDto
      NoTagsIfEmpty = domain.NoTagsIfEmpty
      GenerateSourceAttributesWithRSDFile = domain.GenerateSourceAttributesWithRSDFile
      SourceAttributeRSDFile = domain.SourceAttributeRSDFile }

  let fromDto (mc: IMasterRuleObjectConverter) (dto: Dto.SetDocumentTags) =
    SetDocumentTags
      ( SetStringTag=(dto.SetStringTag.Generator <> Dto.ValueGeneratorType.None),
        StringTagName=dto.SetStringTag.TagName,
        Delimiter=dto.SetStringTag.Delimiter,
        UseSpecifiedValueForStringTag=(dto.SetStringTag.Generator = Dto.ValueGeneratorType.Literal),
        SpecifiedValueForStringTag=dto.SetStringTag.SpecifiedValue,
        UseTagValueForStringTag=(dto.SetStringTag.Generator = Dto.ValueGeneratorType.FromTag),
        TagNameForStringTagValue=dto.SetStringTag.ValuesFromTag,
        UseSelectedAttributesForStringTagValue=(dto.SetStringTag.Generator = Dto.ValueGeneratorType.FromAttributes),
        StringTagAttributeSelector=(dto.SetStringTag.AttributeSelector |> ObjectWithType.fromDto mc),

        SetObjectTag=(dto.SetObjectTag.Generator <> Dto.ValueGeneratorType.None),
        ObjectTagName=dto.SetObjectTag.TagName,
        UseSpecifiedValueForObjectTag=(dto.SetObjectTag.Generator = Dto.ValueGeneratorType.Literal),
        SpecifiedValueForObjectTag=dto.SetObjectTag.SpecifiedValue,
        UseSelectedAttributesForObjectTagValue=(dto.SetObjectTag.Generator = Dto.ValueGeneratorType.FromAttributes),
        ObjectTagAttributeSelector=(dto.SetObjectTag.AttributeSelector |> ObjectWithType.fromDto mc),

        NoTagsIfEmpty=dto.NoTagsIfEmpty,
        GenerateSourceAttributesWithRSDFile=dto.GenerateSourceAttributesWithRSDFile,
        SourceAttributeRSDFile=dto.SourceAttributeRSDFile )

type SetDocumentTagsConverter() =
  inherit RuleObjectConverter<SetDocumentTags, ISetDocumentTags, Dto.SetDocumentTags>()
  override _.toDto mc domain = domain |> SetDocumentTags.toDto mc
  override _.fromDto mc dto = dto |> SetDocumentTags.fromDto mc
