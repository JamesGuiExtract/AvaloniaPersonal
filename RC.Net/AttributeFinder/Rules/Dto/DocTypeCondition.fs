namespace Extract.AttributeFinder.Rules.Dto

type DocumentConfidenceLevel =
| ZeroLevel = 0
| MaybeLevel = 1
| ProbableLevel = 2
| SureLevel = 3
 
type DocTypeCondition = {
  AllowTypes: bool
  DocumentClassifiersPath: string
  Category: string
  Types: string list
  MinConfidence: DocumentConfidenceLevel
}