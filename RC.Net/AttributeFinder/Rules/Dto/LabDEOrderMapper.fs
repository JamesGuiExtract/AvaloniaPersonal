namespace Extract.AttributeFinder.Rules.Dto

type LabDEOrderMapper = {
  DatabaseFileName: string
  EliminateDuplicateTestSubAttributes: bool
  RequireMandatoryTests: bool
  RequirementsAreOptional: bool
  UseFilledRequirement: bool
  UseOutstandingOrders: bool
  SkipSecondPass: bool
  AddESNamesAttribute: bool
  AddESTestCodesAttribute: bool
  SetFuzzyType: bool
}
