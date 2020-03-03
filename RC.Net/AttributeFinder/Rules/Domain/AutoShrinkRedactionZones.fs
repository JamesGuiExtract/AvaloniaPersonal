namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules

module AutoShrinkRedactionZones =
  type AutoShrinkRedactionZonesClass = AutoShrinkRedactionZones
  open Extract.AttributeFinder.Rules.Dto

  let toDto (mc: IMasterRuleObjectConverter) (domain: AutoShrinkRedactionZonesClass): Dto.AutoShrinkRedactionZones =
    { AttributeSelector = domain.AttributeSelector |> ObjectWithType.toDto mc
      AutoExpandBeforeAutoShrink = domain.AutoExpandBeforeAutoShrink
      MaxPixelsToExpand = domain.MaxPixelsToExpand }

  let fromDto (mc: IMasterRuleObjectConverter) (dto: Dto.AutoShrinkRedactionZones) =
    AutoShrinkRedactionZonesClass
      ( AttributeSelector=(dto.AttributeSelector |> ObjectWithType.fromDto mc),
        AutoExpandBeforeAutoShrink=dto.AutoExpandBeforeAutoShrink,
        MaxPixelsToExpand=dto.MaxPixelsToExpand )

type AutoShrinkRedactionZonesConverter() =
  inherit RuleObjectConverter<AutoShrinkRedactionZones, AutoShrinkRedactionZones, Dto.AutoShrinkRedactionZones>()
  override _.toDto mc domain = domain |> AutoShrinkRedactionZones.toDto mc
  override _.fromDto mc dto = dto |> AutoShrinkRedactionZones.fromDto mc
