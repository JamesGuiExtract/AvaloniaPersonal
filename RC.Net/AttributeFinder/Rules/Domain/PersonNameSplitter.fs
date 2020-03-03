namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFCORELib
open UCLID_AFSPLITTERSLib

type PersonNameSplitterConverter() =
  inherit RuleObjectConverter<PersonNameSplitterClass, IAttributeSplitter, Dto.PersonNameSplitter>()
  override _.toDto _mc _domain = Dto.PersonNameSplitter.PersonNameSplitter
  override _.fromDto _mc _dto = PersonNameSplitterClass ()
