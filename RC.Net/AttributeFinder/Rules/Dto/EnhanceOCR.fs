namespace Extract.AttributeFinder.Rules.Dto

type FilterPackage =
| Low = 0
| Medium = 1
| High = 2
| HalftoneSpeckled = 4
| AliasedDiffuse = 5
| LinesSmudged = 6
| Custom = 7

type EnhanceOCR = {
  ConfidenceCriteria: int
  FilterPackage: FilterPackage
  CustomFilterPackage: string
  PreferredFormatRegexFile: string
  CharsToIgnore: string
  OutputFilteredImages: bool
}
