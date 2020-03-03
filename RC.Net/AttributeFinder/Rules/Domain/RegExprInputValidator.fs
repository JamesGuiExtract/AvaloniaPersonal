namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_REGEXPRIVLib

module RegExprInputValidator =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IRegExprInputValidator): Dto.RegExprInputValidator =
    { Pattern = domain.Pattern
      IgnoreCase = domain.IgnoreCase
      InputType = domain.GetInputType () }

  let fromDto (dto: Dto.RegExprInputValidator) =
    let domain =
      RegExprInputValidatorClass
        ( Pattern=dto.Pattern,
          IgnoreCase=dto.IgnoreCase )
    domain.SetInputType dto.InputType
    domain

type RegExprInputValidatorConverter() =
  inherit RuleObjectConverter<RegExprInputValidatorClass, IRegExprInputValidator, Dto.RegExprInputValidator>()
  override _.toDto _mc domain = domain |> RegExprInputValidator.toDto
  override _.fromDto _mc dto = dto |> RegExprInputValidator.fromDto
