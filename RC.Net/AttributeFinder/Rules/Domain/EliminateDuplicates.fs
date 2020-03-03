namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFCORELib
open UCLID_AFOUTPUTHANDLERSLib

type EliminateDuplicatesConverter() =
  inherit RuleObjectConverter<EliminateDuplicatesClass, IOutputHandler, Dto.EliminateDuplicates>()
  override _.toDto _mc _domain = Dto.EliminateDuplicates.EliminateDuplicates
  override _.fromDto _mc _dto = EliminateDuplicatesClass ()
