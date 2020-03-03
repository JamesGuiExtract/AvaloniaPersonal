namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_COMUTILSLib
open Extract

module ObjectWithType =
  open Extract.AttributeFinder.Rules.Dto

  let empty: Dto.ObjectWithType = 
    { Type = null
      Object = null }

  let toDto (mc: IMasterRuleObjectConverter) (domain: obj): Dto.ObjectWithType =
    match domain |> mc.ToDto with
    | Some (dto, typeName) ->
      { Type = typeName
        Object = dto }
    | None -> empty

  let fromDto (mc: IMasterRuleObjectConverter) (dto: ObjectWithType) =
    try
      match dto.Object |> mc.FromDto with
      | Some domain -> domain :?> 't
      | None -> null
    with
    | e -> raise (ExtractException.AsExtractException ("ELI49674", e))

type ObjectWithTypeConverter() =
  inherit RuleObjectConverter<obj, obj, Dto.ObjectWithType>()
  override _.toDto mc domain = domain |> ObjectWithType.toDto mc
  override _.fromDto mc dto = dto |> ObjectWithType.fromDto mc
(**************************************************************************************************)

module ObjectWithDescription =
  open Extract.AttributeFinder.Rules.Dto

  let empty: Dto.ObjectWithDescription = 
    { Type = null
      Description = null
      Enabled = false
      Object = null }

  let toDto mc (domain: IObjectWithDescription): Dto.ObjectWithDescription =
    match domain with
    | null -> empty
    | _ ->
      let dto = ObjectWithType.toDto mc domain.Object
      { Type = dto.Type
        Description = domain.Description
        Enabled = domain.Enabled
        Object = dto.Object }

  let fromDto mc (dto: Dto.ObjectWithDescription) =
    let domain = ObjectWithType.fromDto mc { ObjectWithType.Type = dto.Type; Object = dto.Object }

    ObjectWithDescriptionClass
      ( Description=dto.Description,
        Enabled=dto.Enabled,
        Object=domain )

type ObjectWithDescriptionConverter() =
  inherit RuleObjectConverter<ObjectWithDescriptionClass, IObjectWithDescription, Dto.ObjectWithDescription>()
  override _.toDto mc domain = domain |> ObjectWithDescription.toDto mc
  override _.fromDto mc dto = dto |> ObjectWithDescription.fromDto mc
(**************************************************************************************************)

module VariantVector =
  let toSeq (vv : IVariantVector) =
    if vv = null then Seq.empty
    else
      seq { for i in 0 .. (vv.Size-1) -> downcast vv.[i] }

  let toList vv = vv |> toSeq |> Seq.toList
(**************************************************************************************************)

module IUnknownVector =
  let toSeq (uv : IIUnknownVector) =
    if uv = null then Seq.empty
    else
      seq { for i in 0 .. (uv.Size()-1) -> downcast uv.At(i)}

  let toDto f (domain: IIUnknownVector) =
    domain
    |> toSeq
    |> Seq.map f
    |> Seq.toList

  let fromDto f (dto : 'a seq when 'a :> obj) =
    let uv = IUnknownVectorClass()
    dto |> Seq.map f |> Seq.iter uv.PushBack
    uv
(**************************************************************************************************)

module ObjectWithDescriptionVector =
  let toDto mc domain =
    domain |> IUnknownVector.toDto (ObjectWithDescription.toDto mc)

  let fromDto mc dto =
    dto |> IUnknownVector.fromDto (ObjectWithDescription.fromDto mc)
(**************************************************************************************************)
