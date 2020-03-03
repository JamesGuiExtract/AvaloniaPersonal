namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open Extract.LabResultsCustomComponents

module LabDEOrderMapper =
  type LabDEOrderMapperClass = LabDEOrderMapper
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: ILabDEOrderMapper) =
    { LabDEOrderMapper.DatabaseFileName = domain.DatabaseFileName
      EliminateDuplicateTestSubAttributes = domain.EliminateDuplicateTestSubAttributes
      RequireMandatoryTests = domain.RequireMandatoryTests
      RequirementsAreOptional = domain.RequirementsAreOptional
      UseFilledRequirement = domain.UseFilledRequirement
      UseOutstandingOrders = domain.UseOutstandingOrders
      SkipSecondPass = domain.SkipSecondPass
      AddESNamesAttribute = domain.AddESNamesAttribute
      AddESTestCodesAttribute = domain.AddESTestCodesAttribute
      SetFuzzyType = domain.SetFuzzyType }

  let fromDto (dto: Dto.LabDEOrderMapper) =
    new LabDEOrderMapperClass
      ( DatabaseFileName=dto.DatabaseFileName,
        EliminateDuplicateTestSubAttributes=dto.EliminateDuplicateTestSubAttributes,
        RequireMandatoryTests=dto.RequireMandatoryTests,
        RequirementsAreOptional=dto.RequirementsAreOptional,
        UseFilledRequirement=dto.UseFilledRequirement,
        UseOutstandingOrders=dto.UseOutstandingOrders,
        SkipSecondPass=dto.SkipSecondPass,
        AddESNamesAttribute=dto.AddESNamesAttribute,
        AddESTestCodesAttribute=dto.AddESTestCodesAttribute,
        SetFuzzyType=dto.SetFuzzyType )

type LabDEOrderMapperConverter() =
  inherit RuleObjectConverter<LabDEOrderMapper, ILabDEOrderMapper, Dto.LabDEOrderMapper>()
  override _.toDto _mc domain = domain |> LabDEOrderMapper.toDto
  override _.fromDto _mc dto = dto |> LabDEOrderMapper.fromDto
