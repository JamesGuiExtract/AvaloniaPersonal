namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFCORELib
open UCLID_AFOUTPUTHANDLERSLib

type SelectUsingMajorityConverter() =
  inherit RuleObjectConverter<SelectUsingMajorityClass, IOutputHandler, Dto.SelectUsingMajority>()
  override _.toDto _mc _domain = Dto.SelectUsingMajority.SelectUsingMajority
  override _.fromDto _mc _dto = SelectUsingMajorityClass ()
