namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_GENERALIVLib

module DoubleInputValidator =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IDoubleInputValidator): Dto.DoubleInputValidator =
    { HasMin = domain.HasMin
      Min = domain.Min
      HasMax = domain.HasMax
      Max = domain.Max
      ZeroAllowed = domain.ZeroAllowed
      NegativeAllowed = domain.NegativeAllowed
      IncludeMinInRange = domain.IncludeMinInRange
      IncludeMaxInRange = domain.IncludeMaxInRange }

  let fromDto (dto: Dto.DoubleInputValidator) =
    DoubleInputValidatorClass
      ( HasMin=dto.HasMin,
        Min=dto.Min,
        HasMax=dto.HasMax,
        Max=dto.Max,
        ZeroAllowed=dto.ZeroAllowed,
        NegativeAllowed=dto.NegativeAllowed,
        IncludeMinInRange=dto.IncludeMinInRange,
        IncludeMaxInRange=dto.IncludeMaxInRange )

type DoubleInputValidatorConverter() =
  inherit RuleObjectConverter<DoubleInputValidatorClass, IDoubleInputValidator, Dto.DoubleInputValidator>()
  override _.toDto _mc domain = domain |> DoubleInputValidator.toDto
  override _.fromDto _mc dto = dto |> DoubleInputValidator.fromDto
