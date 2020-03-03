namespace Extract.AttributeFinder.Rules.Dto

type IntegerInputValidator = {
  HasMin: bool
  Min: int
  HasMax: bool
  Max: int
  ZeroAllowed: bool
  NegativeAllowed: bool
  IncludeMinInRange: bool
  IncludeMaxInRange: bool
}
