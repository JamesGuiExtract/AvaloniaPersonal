namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules

module DuplicateAndSeparateTrees =
  type DuplicateAndSeparateTreesClass = DuplicateAndSeparateTrees
  open Extract.AttributeFinder.Rules.Dto

  let toDto (mc: IMasterRuleObjectConverter) (domain: IDuplicateAndSeparateTrees): Dto.DuplicateAndSeparateTrees =
    { AttributeSelector = domain.AttributeSelector |> ObjectWithType.toDto mc
      DividingAttributeName = domain.DividingAttributeName
      OutputHandler = domain.OutputHandler |> ObjectWithType.toDto mc
      RunOutputHandler = domain.RunOutputHandler }

  let fromDto (mc: IMasterRuleObjectConverter) (dto: Dto.DuplicateAndSeparateTrees) =
    DuplicateAndSeparateTrees
      ( AttributeSelector=(dto.AttributeSelector |> ObjectWithType.fromDto mc),
        DividingAttributeName=dto.DividingAttributeName,
        OutputHandler=(dto.OutputHandler |> ObjectWithType.fromDto mc),
        RunOutputHandler=dto.RunOutputHandler )

type DuplicateAndSeparateAttributeTreesConverter() =
  inherit RuleObjectConverter<DuplicateAndSeparateTrees, IDuplicateAndSeparateTrees, Dto.DuplicateAndSeparateTrees>()
  override _.toDto mc domain = domain |> DuplicateAndSeparateTrees.toDto mc
  override _.fromDto mc dto = dto |> DuplicateAndSeparateTrees.fromDto mc
