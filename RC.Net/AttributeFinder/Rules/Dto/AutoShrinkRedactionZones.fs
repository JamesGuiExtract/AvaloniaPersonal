namespace Extract.AttributeFinder.Rules.Dto

type AutoShrinkRedactionZones = {
  AttributeSelector: ObjectWithType
  AutoExpandBeforeAutoShrink: bool
  MaxPixelsToExpand: single
}