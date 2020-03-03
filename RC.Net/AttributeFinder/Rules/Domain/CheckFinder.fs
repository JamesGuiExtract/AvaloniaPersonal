namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFCORELib
open UCLID_AFVALUEFINDERSLib

type CheckFinderConverter() =
  inherit RuleObjectConverter<CheckFinderClass, IAttributeFindingRule, Dto.CheckFinder>()
  override _.toDto _mc _domain = Dto.CheckFinder.CheckFinder
  override _.fromDto _mc _dto = CheckFinderClass ()
