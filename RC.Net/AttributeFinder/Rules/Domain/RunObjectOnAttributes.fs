namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFOUTPUTHANDLERSLib
open UCLID_AFCORELib

module TypeOfObject =
  let (|Modifier|OutputHandler|Splitter|Unknown|) guid =
    if guid = typeof<IAttributeModifyingRule>.GUID then Modifier
    elif guid = typeof<IOutputHandler>.GUID then OutputHandler
    elif guid = typeof<IAttributeSplitter>.GUID then Splitter
    else Unknown

  let toDto = function
  | Modifier -> Dto.TypeOfObject.Modifier
  | OutputHandler -> Dto.TypeOfObject.OutputHandler
  | Splitter -> Dto.TypeOfObject.Splitter
  | other -> failwithf "Not a valid TypeOfObject GUID! %A" other

  let fromDto = function
  | Dto.TypeOfObject.Modifier -> typeof<IAttributeModifyingRule>.GUID
  | Dto.TypeOfObject.OutputHandler -> typeof<IOutputHandler>.GUID
  | Dto.TypeOfObject.Splitter -> typeof<IAttributeSplitter>.GUID
  | other -> failwithf "Not a valid TypeOfObject! %A" other


module RunObjectOnAttributes =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (mc: IMasterRuleObjectConverter) (domain: IRunObjectOnQuery) =
    let guid = ref System.Guid.Empty
    let ruleObject = domain.GetObjectAndIID guid
    { Dto.RunObjectOnAttributes.AttributeQuery = domain.AttributeQuery
      AttributeSelector = domain.AttributeSelector |> ObjectWithType.toDto mc
      UseAttributeSelector = domain.UseAttributeSelector
      Type = !guid |> TypeOfObject.toDto
      Object = ruleObject |> ObjectWithType.toDto mc }

  let fromDto (mc: IMasterRuleObjectConverter) (dto: Dto.RunObjectOnAttributes) =
    let domain =
      RunObjectOnQueryClass
        ( AttributeQuery=dto.AttributeQuery,
          UseAttributeSelector=dto.UseAttributeSelector )

    if dto.AttributeSelector.Object <> null
    then domain.AttributeSelector <- dto.AttributeSelector |> ObjectWithType.fromDto mc

    let ruleObject = dto.Object |> ObjectWithType.fromDto mc
    let guid = dto.Type |> TypeOfObject.fromDto
    domain.SetObjectAndIID (guid, ruleObject)

    domain

type RunObjectOnAttributesConverter() =
  inherit RuleObjectConverter<RunObjectOnQueryClass, IRunObjectOnQuery, Dto.RunObjectOnAttributes>()
  override _.toDto mc domain = domain |> RunObjectOnAttributes.toDto mc
  override _.fromDto mc dto = dto |> RunObjectOnAttributes.fromDto mc
