namespace Extract.AttributeFinder.Rules.Dto

type EntityAliasChoice =
| IgnoreLaterEntities = 0
| LaterEntitiesAsAttributes = 1
| LaterEntitiesAsSubattributes = 2

type EntityNameSplitter = {
  EntityAliasChoice: EntityAliasChoice
}