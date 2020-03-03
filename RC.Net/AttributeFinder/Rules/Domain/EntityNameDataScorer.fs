namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFCORELib
open UCLID_AFDATASCORERSLib

type EntityNameDataScorerConverter() =
  inherit RuleObjectConverter<EntityNameDataScorerClass, IDataScorer, Dto.EntityNameDataScorer>()
  override _.toDto _mc _domain = Dto.EntityNameDataScorer.EntityNameDataScorer
  override _.fromDto _mc _dto = EntityNameDataScorerClass ()
