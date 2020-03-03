namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFCORELib
open UCLID_AFOUTPUTHANDLERSLib

type SpatiallySortAttributesConverter() =
  inherit RuleObjectConverter<SpatiallySortAttributesClass, IOutputHandler, Dto.SpatiallySortAttributes>()
  override _.toDto _mc _domain = Dto.SpatiallySortAttributes.SpatiallySortAttributes
  override _.fromDto _mc _dto = SpatiallySortAttributesClass ()
