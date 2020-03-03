namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFOUTPUTHANDLERSLib

module ModifyAttributeValueOH =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IModifyAttributeValueOH): Dto.ModifyAttributeValueOH =
    { AttributeQuery = domain.AttributeQuery
      CreateSubAttribute = domain.CreateSubAttribute
      AttributeName = domain.AttributeName
      SetAttributeName = domain.SetAttributeName
      AttributeType = domain.AttributeType
      SetAttributeType = domain.SetAttributeType
      AttributeValue = domain.AttributeValue
      SetAttributeValue = domain.SetAttributeValue }

  let fromDto (dto: Dto.ModifyAttributeValueOH) =
    let domain =
      ModifyAttributeValueOHClass
        ( AttributeQuery=dto.AttributeQuery,
          CreateSubAttribute=dto.CreateSubAttribute,
          SetAttributeName=dto.SetAttributeName,
          AttributeType=dto.AttributeType,
          SetAttributeType=dto.SetAttributeType,
          AttributeValue=dto.AttributeValue,
          SetAttributeValue=dto.SetAttributeValue )

    if dto.SetAttributeName || dto.AttributeName |> String.length > 0
    then domain.AttributeName <- dto.AttributeName

    domain

type ModifyAttributeValueOHConverter() =
  inherit RuleObjectConverter<ModifyAttributeValueOHClass, IModifyAttributeValueOH, Dto.ModifyAttributeValueOH>()
  override _.toDto _mc domain = domain |> ModifyAttributeValueOH.toDto
  override _.fromDto _mc dto = dto |> ModifyAttributeValueOH.fromDto
