namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFCORELib
open UCLID_AFOUTPUTHANDLERSLib

type RemoveInvalidEntriesConverter() =
  inherit RuleObjectConverter<RemoveInvalidEntriesClass, IOutputHandler, Dto.RemoveInvalidEntries>()
  override _.toDto _mc _domain = Dto.RemoveInvalidEntries.RemoveInvalidEntries
  override _.fromDto _mc _dto = RemoveInvalidEntriesClass ()
