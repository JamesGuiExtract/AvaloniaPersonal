namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules

module MoveOrCopyAttributes =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IMoveCopyAttributes): Dto.MoveOrCopyAttributes =
    { SourceAttributeTreeXPath = domain.SourceAttributeTreeXPath
      DestinationAttributeTreeXPath = domain.DestinationAttributeTreeXPath
      CopyAttributes = domain.CopyAttributes }

  let fromDto (dto: Dto.MoveOrCopyAttributes) =
    MoveCopyAttributes
      ( SourceAttributeTreeXPath=dto.SourceAttributeTreeXPath,
        DestinationAttributeTreeXPath=dto.DestinationAttributeTreeXPath,
        CopyAttributes=dto.CopyAttributes )

type MoveOrCopyAttributesConverter() =
  inherit RuleObjectConverter<MoveCopyAttributes, IMoveCopyAttributes, Dto.MoveOrCopyAttributes>()
  override _.toDto _mc domain = domain |> MoveOrCopyAttributes.toDto
  override _.fromDto _mc dto = dto |> MoveOrCopyAttributes.fromDto
