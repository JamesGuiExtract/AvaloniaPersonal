namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_GENERALIVLib
open UCLID_INPUTFUNNELLib

type ShortInputValidatorConverter() =
  inherit RuleObjectConverter<ShortInputValidatorClass, IInputValidator, Dto.ShortInputValidator>()
  override _.toDto _mc _domain = Dto.ShortInputValidator.ShortInputValidator
  override _.fromDto _mc _dto = ShortInputValidatorClass ()
