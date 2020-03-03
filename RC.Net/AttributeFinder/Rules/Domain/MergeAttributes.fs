namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open Extract.Utilities
open UCLID_AFOUTPUTHANDLERSLib
open UCLID_AFUTILSLib

module FieldMergeMode =
  let toDto = function
  | EFieldMergeMode.kSpecifyField -> Dto.FieldMergeMode.SpecifyField
  | EFieldMergeMode.kCombineField -> Dto.FieldMergeMode.CombineField
  | EFieldMergeMode.kPreserveField -> Dto.FieldMergeMode.PreserveField
  | EFieldMergeMode.kSelectField -> Dto.FieldMergeMode.SelectField
  | other -> failwithf "Not a valid EFieldMergeMode! %A" other

  let fromDto = function
  | Dto.FieldMergeMode.SpecifyField -> EFieldMergeMode.kSpecifyField
  | Dto.FieldMergeMode.CombineField -> EFieldMergeMode.kCombineField
  | Dto.FieldMergeMode.PreserveField -> EFieldMergeMode.kPreserveField
  | Dto.FieldMergeMode.SelectField -> EFieldMergeMode.kSelectField
  | other -> failwithf "Not a valid EFieldMergeMode! %A" other

module MergeAttributes =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IMergeAttributes): Dto.MergeAttributes =
    { AttributeQuery = domain.AttributeQuery
      CreateMergedRegion = domain.CreateMergedRegion
      NameMergeMode = domain.NameMergeMode |> FieldMergeMode.toDto
      NameMergePriority = domain.NameMergePriority |> VariantVector.toList
      OverlapPercent = domain.OverlapPercent
      PreserveAsSubAttributes = domain.PreserveAsSubAttributes
      PreserveType = domain.PreserveType
      SpecifiedName = domain.SpecifiedName
      SpecifiedType = domain.SpecifiedType
      SpecifiedValue = domain.SpecifiedValue
      TreatNameListAsRegex = domain.TreatNameListAsRegex
      TreatTypeListAsRegex = domain.TreatTypeListAsRegex
      TreatValueListAsRegex = domain.TreatValueListAsRegex
      TypeFromName = domain.TypeFromName
      TypeMergeMode = domain.TypeMergeMode |> FieldMergeMode.toDto
      TypeMergePriority = domain.TypeMergePriority |> VariantVector.toList
      ValueMergeMode = domain.ValueMergeMode |> FieldMergeMode.toDto
      ValueMergePriority = domain.ValueMergePriority |> VariantVector.toList }

  let fromDto (dto: Dto.MergeAttributes) =
    MergeAttributesClass
      ( AttributeQuery=dto.AttributeQuery,
        CreateMergedRegion=dto.CreateMergedRegion,
        NameMergeMode=(dto.NameMergeMode |> FieldMergeMode.fromDto),
        NameMergePriority=dto.NameMergePriority.ToVariantVector(),
        OverlapPercent=dto.OverlapPercent,
        PreserveAsSubAttributes=dto.PreserveAsSubAttributes,
        PreserveType=dto.PreserveType,
        SpecifiedName=dto.SpecifiedName,
        SpecifiedType=dto.SpecifiedType,
        SpecifiedValue=dto.SpecifiedValue,
        TreatNameListAsRegex=dto.TreatNameListAsRegex,
        TreatTypeListAsRegex=dto.TreatTypeListAsRegex,
        TreatValueListAsRegex=dto.TreatValueListAsRegex,
        TypeFromName=dto.TypeFromName,
        TypeMergeMode=(dto.TypeMergeMode |> FieldMergeMode.fromDto),
        TypeMergePriority=dto.TypeMergePriority.ToVariantVector(),
        ValueMergeMode=(dto.ValueMergeMode |> FieldMergeMode.fromDto),
        ValueMergePriority=dto.ValueMergePriority.ToVariantVector() )

type MergeAttributesConverter() =
  inherit RuleObjectConverter<MergeAttributesClass, IMergeAttributes, Dto.MergeAttributes>()
  override _.toDto _mc domain = domain |> MergeAttributes.toDto
  override _.fromDto _mc dto = dto |> MergeAttributes.fromDto
