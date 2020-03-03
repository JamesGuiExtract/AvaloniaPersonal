namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFCORELib
open UCLID_AFUTILSLib

type MERSHandlerConverter() =
  inherit RuleObjectConverter<MERSHandlerClass, IAttributeModifyingRule, Dto.MERSHandler>()
  override _.toDto _mc _domain = Dto.MERSHandler.MERSHandler
  override _.fromDto _mc _dto = MERSHandlerClass ()
