namespace Extract.AttributeFinder.Rules.Dto

type DoubleInputValidator = {
  HasMin: bool
  Min: double
  HasMax: bool
  Max: double
  ZeroAllowed: bool
  NegativeAllowed: bool
  IncludeMinInRange: bool
  IncludeMaxInRange: bool
}
