namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFSPLITTERSLib

module EntityAliasChoice =
  let toDto = function
  | EEntityAliasChoice.kIgnoreLaterEntities -> Dto.EntityAliasChoice.IgnoreLaterEntities
  | EEntityAliasChoice.kLaterEntitiesAsAttributes -> Dto.EntityAliasChoice.LaterEntitiesAsAttributes
  | EEntityAliasChoice.kLaterEntitiesAsSubattributes -> Dto.EntityAliasChoice.LaterEntitiesAsSubattributes
  | other -> failwithf "Not a valid EEntityAliasChoice! %A" other

  let fromDto = function
  | Dto.EntityAliasChoice.IgnoreLaterEntities -> EEntityAliasChoice.kIgnoreLaterEntities
  | Dto.EntityAliasChoice.LaterEntitiesAsAttributes -> EEntityAliasChoice.kLaterEntitiesAsAttributes
  | Dto.EntityAliasChoice.LaterEntitiesAsSubattributes -> EEntityAliasChoice.kLaterEntitiesAsSubattributes
  | other -> failwithf "Not a valid EntityAliasChoice! %A" other

module EntityNameSplitter =
  let toDto (domain: IEntityNameSplitter): Dto.EntityNameSplitter =
    { EntityAliasChoice = domain.EntityAliasChoice |> EntityAliasChoice.toDto }

  let fromDto (dto: Dto.EntityNameSplitter) =
    EntityNameSplitterClass
      ( EntityAliasChoice=(dto.EntityAliasChoice |> EntityAliasChoice.fromDto) )

type EntityNameSplitterConverter() =
  inherit RuleObjectConverter<EntityNameSplitterClass, IEntityNameSplitter, Dto.EntityNameSplitter>()
  override _.toDto _mc domain = domain |> EntityNameSplitter.toDto
  override _.fromDto _mc dto = dto |> EntityNameSplitter.fromDto
