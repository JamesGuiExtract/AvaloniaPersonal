namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_GENERALIVLib

module IntegerInputValidator =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IIntegerInputValidator): Dto.IntegerInputValidator =
    { HasMin = domain.HasMin
      Min = domain.Min
      HasMax = domain.HasMax
      Max = domain.Max
      ZeroAllowed = domain.ZeroAllowed
      NegativeAllowed = domain.NegativeAllowed
      IncludeMinInRange = domain.IncludeMinInRange
      IncludeMaxInRange = domain.IncludeMaxInRange }

  let fromDto (dto: Dto.IntegerInputValidator) =
    IntegerInputValidatorClass
      ( HasMin=dto.HasMin,
        Min=dto.Min,
        HasMax=dto.HasMax,
        Max=dto.Max,
        ZeroAllowed=dto.ZeroAllowed,
        NegativeAllowed=dto.NegativeAllowed,
        IncludeMinInRange=dto.IncludeMinInRange,
        IncludeMaxInRange=dto.IncludeMaxInRange )

type IntegerInputValidatorConverter() =
  inherit RuleObjectConverter<IntegerInputValidatorClass, IIntegerInputValidator, Dto.IntegerInputValidator>()
  override _.toDto _mc domain = domain |> IntegerInputValidator.toDto
  override _.fromDto _mc dto = dto |> IntegerInputValidator.fromDto
