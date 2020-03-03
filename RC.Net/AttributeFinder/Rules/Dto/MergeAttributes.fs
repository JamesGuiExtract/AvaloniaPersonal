namespace Extract.AttributeFinder.Rules.Dto

type FieldMergeMode =
| SpecifyField = 0
| CombineField = 1
| PreserveField = 2
| SelectField = 3

type MergeAttributes = {
  AttributeQuery: string
  CreateMergedRegion: bool
  NameMergeMode: FieldMergeMode
  NameMergePriority: string list
  OverlapPercent: double
  PreserveAsSubAttributes: bool
  PreserveType: bool
  SpecifiedName: string
  SpecifiedType: string
  SpecifiedValue: string
  TreatNameListAsRegex: bool
  TreatTypeListAsRegex: bool
  TreatValueListAsRegex: bool
  TypeFromName: bool
  TypeMergeMode: FieldMergeMode
  TypeMergePriority: string list
  ValueMergeMode: FieldMergeMode
  ValueMergePriority: string list
}