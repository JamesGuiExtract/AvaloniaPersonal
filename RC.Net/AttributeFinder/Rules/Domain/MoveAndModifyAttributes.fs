namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFOUTPUTHANDLERSLib

module MoveAttributeLevel =
  let toDto = function
  | EMoveAttributeLevel.kNoMove -> Dto.MoveAttributeLevel.NoMove
  | EMoveAttributeLevel.kMoveToRoot -> Dto.MoveAttributeLevel.MoveToRoot
  | EMoveAttributeLevel.kMoveToParent -> Dto.MoveAttributeLevel.MoveToParent
  | other -> failwithf "Not a valid EMoveAttributeLevel! %A" other

  let fromDto = function
  | Dto.MoveAttributeLevel.NoMove -> EMoveAttributeLevel.kNoMove
  | Dto.MoveAttributeLevel.MoveToRoot -> EMoveAttributeLevel.kMoveToRoot
  | Dto.MoveAttributeLevel.MoveToParent -> EMoveAttributeLevel.kMoveToParent
  | other -> failwithf "Not a valid MoveAttributeLevel! %A" other

module OverwriteAttributeName =
  let toDto = function
  | EOverwriteAttributeName.kDoNotOverwrite -> Dto.OverwriteAttributeName.DoNotOverwrite
  | EOverwriteAttributeName.kOverwriteWithRootOrParentName -> Dto.OverwriteAttributeName.OverwriteWithRootOrParentName
  | EOverwriteAttributeName.kOverwriteWithSpecifiedName -> Dto.OverwriteAttributeName.OverwriteWithSpecifiedName
  | other -> failwithf "Not a valid EOverwriteAttributeName! %A" other

  let fromDto = function
  | Dto.OverwriteAttributeName.DoNotOverwrite -> EOverwriteAttributeName.kDoNotOverwrite
  | Dto.OverwriteAttributeName.OverwriteWithRootOrParentName -> EOverwriteAttributeName.kOverwriteWithRootOrParentName
  | Dto.OverwriteAttributeName.OverwriteWithSpecifiedName -> EOverwriteAttributeName.kOverwriteWithSpecifiedName
  | other -> failwithf "Not a valid OverwriteAttributeName! %A" other

module MoveAndModifyAttributes =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IMoveAndModifyAttributes): Dto.MoveAndModifyAttributes =
    { AddAttributeNameToType = domain.AddAttributeNameToType
      AddRootOrParentAttributeType = domain.AddRootOrParentAttributeType
      AddSpecifiedAttributeType = domain.AddSpecifiedAttributeType
      AttributeQuery = domain.AttributeQuery
      DeleteRootOrParentIfAllChildrenMoved = domain.DeleteRootOrParentIfAllChildrenMoved
      MoveAttributeLevel = domain.MoveAttributeLevel |> MoveAttributeLevel.toDto
      OverwriteAttributeName = domain.OverwriteAttributeName |> OverwriteAttributeName.toDto
      RetainAttributeType = domain.RetainAttributeType
      SpecifiedAttributeName = domain.SpecifiedAttributeName
      SpecifiedAttributeType = domain.SpecifiedAttributeType }

  let fromDto (dto: Dto.MoveAndModifyAttributes) =
    let domain =
      MoveAndModifyAttributesClass
        ( AddAttributeNameToType=dto.AddAttributeNameToType,
          AddRootOrParentAttributeType=dto.AddRootOrParentAttributeType,
          AddSpecifiedAttributeType=dto.AddSpecifiedAttributeType,
          AttributeQuery=dto.AttributeQuery,
          DeleteRootOrParentIfAllChildrenMoved=dto.DeleteRootOrParentIfAllChildrenMoved,
          MoveAttributeLevel=(dto.MoveAttributeLevel |> MoveAttributeLevel.fromDto),
          OverwriteAttributeName=(dto.OverwriteAttributeName |> OverwriteAttributeName.fromDto),
          RetainAttributeType=dto.RetainAttributeType )

    if dto.SpecifiedAttributeName |> String.length > 0
    then domain.SpecifiedAttributeName <- dto.SpecifiedAttributeName

    if dto.SpecifiedAttributeType |> String.length > 0
    then domain.SpecifiedAttributeType <- dto.SpecifiedAttributeType

    domain

type MoveAndModifyAttributesConverter() =
  inherit RuleObjectConverter<MoveAndModifyAttributesClass, IMoveAndModifyAttributes, Dto.MoveAndModifyAttributes>()
  override _.toDto _mc domain = domain |> MoveAndModifyAttributes.toDto
  override _.fromDto _mc dto = dto |> MoveAndModifyAttributes.fromDto
