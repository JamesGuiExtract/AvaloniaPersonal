namespace Extract.AttributeFinder.Rules.Dto

type Replace = {
  Pattern: string
  Replacement: string
}

type ReplaceStrings = {
  Replacements: Replace list
  AsRegularExpr: bool
  IsCaseSensitive: bool
}