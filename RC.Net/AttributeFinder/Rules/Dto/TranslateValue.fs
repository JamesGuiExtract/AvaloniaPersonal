namespace Extract.AttributeFinder.Rules.Dto

type TranslateFieldType =
| Name = 0
| Type = 1
| Value = 2

type TranslationPair = {
  From: string
  To: string
}

type TranslateValue = {
  TranslateFieldType: TranslateFieldType
  TranslationStringPairs: TranslationPair list
  IsCaseSensitive: bool
}