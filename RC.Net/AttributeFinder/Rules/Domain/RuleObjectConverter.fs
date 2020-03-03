namespace Extract.AttributeFinder.Rules.Domain

open Extract
open System
open System.Reflection
open UCLID_AFCORELib
open UCLID_COMUTILSLib
open Extract.AttributeFinder.Rules

module RuleObjectConverter =

  // Collect every IRuleObjectConverter in this assembly
  let private converters =
    let assembly = Assembly.GetExecutingAssembly ()
    assembly.GetTypes ()
    |> Seq.filter (fun typ ->
      not typ.IsInterface &&
      not typ.IsAbstract &&
      typeof<IRuleObjectConverter>.IsAssignableFrom typ
      )
    |> Seq.map (fun typ -> Activator.CreateInstance typ :?> IRuleObjectConverter)
    |> Seq.toList

  // Collect all supported ICategorizedComponent descriptions to use for mapping domain object to converters
  // (runtime type tests aren't always reliable with COM rule objects)
  let private descriptionToConverter =
    converters
    |> Seq.choose (fun converter ->
      let domainType = converter.convertsDomain
      match Activator.CreateInstance domainType with
      | :? ICategorizedComponent as c -> Some (c.GetComponentDescription (), converter)
      | _ -> None
      )
    |> dict

  // Map Domain types to DTO types so that the DTO type doesn't need to be supplied to deserialize
  let private domainTypeToDtoType =
    converters
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
    | :? IRuleSet -> Some (RuleSetConverter () :> IRuleObjectConverter)
    | :? IObjectWithDescription -> Some (ObjectWithDescriptionConverter () :> IRuleObjectConverter)
    | :? ICategorizedComponent as c ->
      match descriptionToConverter.TryGetValue (c.GetComponentDescription ()) with
      | true, converter -> Some converter
      | false, _ -> None
    | _ -> None

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
            | Some converter -> converter
            | None -> failwithf "Type, %s, not supported!" (domain.GetType ()).Name
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
  /// <param name="domainType">The type of the domain object</param>
  /// <param name="dtoType">The type of the data transfer object associated with the supplied domain type</param>
  let TryGetDtoTypeFromDomainType (typ, dtoType: outref<Type>) =
    domainTypeToDtoType.TryGetValue (typ, &dtoType)
