namespace Extract.AttributeFinder.Rules.Domain

module AttributeRules =
  let toDto mc domain =
    domain |> IUnknownVector.toDto (AttributeRule.toDto mc)
  let fromDto mc dto =
    dto |> IUnknownVector.fromDto (AttributeRule.fromDto mc)

module AttributeFindInfo =
  open Extract.AttributeFinder.Rules
  open Extract.AttributeFinder.Rules.Dto
  open UCLID_AFCORELib

  let toDto mc (domain: IAttributeFindInfo): Dto.AttributeFindInfo =
    { AttributeRules = domain.AttributeRules |> AttributeRules.toDto mc
      AttributeSplitter = domain.AttributeSplitter |> ObjectWithDescription.toDto mc
      IgnoreAttributeSplitterErrors = domain.IgnoreAttributeSplitterErrors
      InputValidator = domain.InputValidator |> ObjectWithDescription.toDto mc
      StopSearchingWhenValueFound = domain.StopSearchingWhenValueFound }

  let fromDto mc (dto: Dto.AttributeFindInfo) =
    AttributeFindInfoClass
      ( AttributeRules=(dto.AttributeRules |> AttributeRules.fromDto mc),
        AttributeSplitter=(dto.AttributeSplitter |> ObjectWithDescription.fromDto mc),
        IgnoreAttributeSplitterErrors=dto.IgnoreAttributeSplitterErrors,
        InputValidator=(dto.InputValidator |> ObjectWithDescription.fromDto mc),
        StopSearchingWhenValueFound=dto.StopSearchingWhenValueFound )
