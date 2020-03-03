namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_GENERALIVLib
open UCLID_INPUTFUNNELLib

type FloatInputValidatorConverter() =
  inherit RuleObjectConverter<FloatInputValidatorClass, IInputValidator, Dto.FloatInputValidator>()
  override _.toDto _mc _domain = Dto.FloatInputValidator.FloatInputValidator
  override _.fromDto _mc _dto = FloatInputValidatorClass ()
