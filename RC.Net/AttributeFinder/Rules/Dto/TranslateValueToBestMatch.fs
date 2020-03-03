namespace Extract.AttributeFinder.Rules.Dto

type NoGoodMatchAction =
| DoNothing = 0
| ClearValue = 1
| RemoveAttribute = 2
| SetTypeToUntranslated = 3

type TranslateValueToBestMatch = {
  AttributeSelector: ObjectWithType
  SourceListPath: string
  SynonymMapPath: string
  MinimumMatchScore: double
  UnableToTranslateAction: NoGoodMatchAction
  CreateBestMatchScoreSubAttribute: bool
}