namespace Extract.AttributeFinder.Rules.Dto

type MoveAttributeLevel =
| NoMove = 0
| MoveToRoot = 1
| MoveToParent = 2

type OverwriteAttributeName =
| DoNotOverwrite = 0
| OverwriteWithRootOrParentName = 1
| OverwriteWithSpecifiedName = 2

type MoveAndModifyAttributes = {
  AddAttributeNameToType: bool
  AddRootOrParentAttributeType: bool
  AddSpecifiedAttributeType: bool
  AttributeQuery: string
  DeleteRootOrParentIfAllChildrenMoved: bool
  MoveAttributeLevel: MoveAttributeLevel
  OverwriteAttributeName: OverwriteAttributeName
  RetainAttributeType: bool
  SpecifiedAttributeName: string
  SpecifiedAttributeType: string
}