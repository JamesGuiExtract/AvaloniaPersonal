namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFCORELib
open UCLID_AFPREPROCESSORSLib

type RemoveSpatialInfoConverter() =
  inherit RuleObjectConverter<RemoveSpatialInfoClass, IDocumentPreprocessor, Dto.RemoveSpatialInfo>()
  override _.toDto _mc _domain = Dto.RemoveSpatialInfo.RemoveSpatialInfo
  override _.fromDto _mc _dto = RemoveSpatialInfoClass ()
