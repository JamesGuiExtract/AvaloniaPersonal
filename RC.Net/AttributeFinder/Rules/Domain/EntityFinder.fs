namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFCORELib
open UCLID_AFUTILSLib

type EntityFinderConverter() =
  inherit RuleObjectConverter<EntityFinderClass, IAttributeModifyingRule, Dto.EntityFinder>()
  override _.toDto _mc _domain = Dto.EntityFinder.EntityFinder
  override _.fromDto _mc _dto = EntityFinderClass ()
