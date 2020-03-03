namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_GENERALIVLib
open UCLID_INPUTFUNNELLib

type DateInputValidatorConverter() =
  inherit RuleObjectConverter<DateInputValidatorClass, IInputValidator, Dto.DateInputValidator>()
  override _.toDto _mc _domain = Dto.DateInputValidator.DateInputValidator
  override _.fromDto _mc _dto = DateInputValidatorClass ()
