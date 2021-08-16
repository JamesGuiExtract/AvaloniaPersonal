namespace Extract.AttributeFinder.Rules.Domain

open Extract
open System
open UCLID_AFCORELib
open UCLID_COMUTILSLib

module RuleObjectConverter =

  // Collect every IRuleObjectConverter in this assembly
  let private converters =
    let assembly = typeof<IRuleObjectConverter>.Assembly
    assembly.GetTypes ()
    |> Seq.filter (fun typ ->
      not typ.IsInterface &&
      not typ.IsAbstract &&
      typeof<IRuleObjectConverter>.IsAssignableFrom typ
      )
    |> Seq.map (fun typ -> Activator.CreateInstance typ :?> IRuleObjectConverter)
    |> Seq.toList

  // Does not have a Legacy attribute applied to the type
  let private isCurrentConverter converter = 
      converter.GetType().GetCustomAttributes(typeof<LegacyAttribute>, false)
      |> Seq.isEmpty

  let private currentVersionConverters = 
    converters |> List.where isCurrentConverter

  // Collect all supported ICategorizedComponent descriptions to use for mapping domain object to converters
  // (runtime type tests aren't always reliable with COM rule objects)
  let private descriptionToConverter =
    currentVersionConverters
    |> Seq.choose (fun converter ->
      let domainType = converter.convertsDomain
      match Activator.CreateInstance domainType with
      | :? ICategorizedComponent as c -> Some (c.GetComponentDescription (), converter)
      | _ -> None
      )
    |> dict

  // Map Domain types to DTO types so that the DTO type doesn't need to be supplied to deserialize
  let private domainTypeToDtoType =
    currentVersionConverters
    |> Seq.map (fun converter -> converter.convertsDomain, converter.convertsDto)
    |> dict

  let private dtoTypeToConverter =
    converters
    |> Seq.map (fun converter -> converter.convertsDto, converter)
    |> dict

  let private dtoTypeNameToType =
    converters
    |> Seq.map (fun converter ->
      let dtoType = converter.convertsDto
      dtoType.Name, dtoType
    )
    |> dict

  let private getConverterForDomainObject (domain: obj) =
    match domain with
    | :? IRuleSet -> Ok (RuleSetConverter () :> IRuleObjectConverter)
    | :? IObjectWithDescription -> Ok (ObjectWithDescriptionConverter () :> IRuleObjectConverter)
    | :? ICategorizedComponent as c ->
      let description = c.GetComponentDescription ()
      match descriptionToConverter.TryGetValue description with
      | true, converter -> Ok converter
      | false, _ -> Error description
    | _ -> Error (domain.GetType().Name)

  let private getConverterForDtoObject (dto: obj) =
    match dtoTypeToConverter.TryGetValue (dto.GetType ()) with
    | true, converter -> Some converter
    | false, _ -> None

  type MasterRuleObjectConverter () =
    interface IMasterRuleObjectConverter with
      member this.ToDto domain =
        if domain = null then None
        else
          let converter =
            match getConverterForDomainObject domain with
            | Ok converter -> converter
            | Error description -> failwithf "Rule object, %s, is not supported!" description
          let typeName =
            let typ = converter.convertsDto
            typ.Name
          (domain |> converter.toDto this, typeName)
          |> Some

      member this.FromDto dto =
        if dto = null then None
        else
          match getConverterForDtoObject dto with
          | Some converter ->
            dto
            |> converter.fromDto this
            |> Some
          | None -> failwithf "Type, %s, not supported!" (dto.GetType ()).Name

  let private mc = MasterRuleObjectConverter ()
  let private imc = mc :> IMasterRuleObjectConverter

  /// <summary>
  /// Convert rule object from domain to data transfer object (a type built for serializing)
  /// </summary>
  /// <param name="ruleObject">The domain object to be converted</param>
  let ConvertToDto (ruleObject: obj) =
    try
      match ruleObject |> imc.ToDto with
      | None -> null
      | Some (dto, _) -> dto
    with e ->
      raise (ExtractException.AsExtractException ("ELI49665", e))

  /// <summary>
  /// Convert rule object from data transfer object to domain object
  /// </summary>
  /// <param name="dto">The data transfer object to be converted</param>
  let ConvertFromDto (dto: obj) =
    try
      match dto |> imc.FromDto with
      | None -> null
      | Some domain -> domain
    with e ->
      raise (ExtractException.AsExtractException ("ELI49666", e))

  /// <summary>
  /// Lookup the short name of a DTO type name and return the type
  /// </summary>
  /// <param name="typeName">The short name of the data transfer type</param>
  /// <param name="dtoType">The type of the data transfer object associated with this name</param>
  let TryGetDtoTypeFromTypeName (typeName, dtoType: outref<Type>) =
    dtoTypeNameToType.TryGetValue (typeName, &dtoType)

  /// <summary>
  /// Lookup the domain class type and return the corresponding data transfer object type
  /// </summary>
  /// <param name="typ">The type of the domain object</param>
  /// <param name="dtoType">The type of the data transfer object associated with the supplied domain type</param>
  let TryGetDtoTypeFromDomainType (typ, dtoType: outref<Type>) =
    domainTypeToDtoType.TryGetValue (typ, &dtoType)
