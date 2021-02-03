namespace Extract.AttributeFinder.Rules.Dto

open System.Runtime.InteropServices

[<ComVisible(true)>]
[<Guid("293BFA29-6293-465E-9242-7E5C51A7A252")>]
type MicrEngineType =
| None = 0
| Kofax = 1
| GdPicture = 2

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
  SearchAllPages: bool
  EngineType: MicrEngineType
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