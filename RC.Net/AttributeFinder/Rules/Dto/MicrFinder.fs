namespace Extract.AttributeFinder.Rules.Dto

type MicrFinderV2 = {
  FilterCharsWhenSplitting: bool
  FilterRegex: string
  HighConfidenceThreshold: int
  InheritOCRParameters: bool
  LowConfidenceThreshold: int
  MicrSplitterRegex: string
  ReturnUnrecognizedCharacters: bool
  SplitAccountNumber: bool
  SplitAmount: bool
  SplitCheckNumber: bool
  SplitRoutingNumber: bool
  UseLowConfidenceThreshold: bool
}

type MicrFinderV1 = {
  SplitRoutingNumber: bool
  SplitAccountNumber: bool
  SplitCheckNumber: bool
  SplitAmount: bool
  Rotate0: bool
  Rotate90: bool
  Rotate180: bool
  Rotate270: bool
}