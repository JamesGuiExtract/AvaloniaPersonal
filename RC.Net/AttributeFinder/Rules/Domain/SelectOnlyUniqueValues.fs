namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFCORELib
open UCLID_AFOUTPUTHANDLERSLib

type SelectOnlyUniqueValuesConverter() =
  inherit RuleObjectConverter<SelectOnlyUniqueValuesClass, IOutputHandler, Dto.SelectOnlyUniqueValues>()
  override _.toDto _mc _domain = Dto.SelectOnlyUniqueValues.SelectOnlyUniqueValues
  override _.fromDto _mc _dto = SelectOnlyUniqueValuesClass ()
