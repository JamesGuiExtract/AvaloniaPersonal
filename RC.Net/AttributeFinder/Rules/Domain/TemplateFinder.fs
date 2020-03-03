namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules

module TemplateFinder =
  type TemplateFinderClass = TemplateFinder
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: ITemplateFinder): Dto.TemplateFinder =
    { TemplateLibrary = domain.TemplateLibrary
      RedactionPredictorOptions = domain.RedactionPredictorOptions }

  let fromDto (dto: Dto.TemplateFinder) =
    TemplateFinderClass
      ( TemplateLibrary=dto.TemplateLibrary,
        RedactionPredictorOptions=dto.RedactionPredictorOptions )

type TemplateFinderConverter() =
  inherit RuleObjectConverter<TemplateFinder, ITemplateFinder, Dto.TemplateFinder>()
  override _.toDto _mc domain = domain |> TemplateFinder.toDto
  override _.fromDto _mc dto = dto |> TemplateFinder.fromDto
