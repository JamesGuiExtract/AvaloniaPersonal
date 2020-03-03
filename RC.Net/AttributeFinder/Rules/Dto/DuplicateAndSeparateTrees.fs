namespace Extract.AttributeFinder.Rules.Dto

type DuplicateAndSeparateTrees = {
  AttributeSelector: ObjectWithType
  DividingAttributeName: string
  OutputHandler: ObjectWithType
  RunOutputHandler: bool
}