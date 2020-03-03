namespace Extract.AttributeFinder.Rules.Dto

type FilterCharacters =
| Alpha = 1
| Numeral = 2
| Period = 4
| Hyphen = 8
| Underscore = 16
| Comma = 32
| ForwardSlash = 64
| Custom = 128

type OCRArea = {
  Filter: FilterCharacters list
  CustomFilterCharacters: string
  DetectHandwriting: bool
  ReturnUnrecognized: bool
  ClearIfNoneFound: bool
}