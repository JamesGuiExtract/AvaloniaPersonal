namespace Extract.AttributeFinder.Rules.Dto

type AttributeFindInfo = {
  AttributeRules: AttributeRule list
  AttributeSplitter: ObjectWithDescription
  IgnoreAttributeSplitterErrors: bool
  InputValidator: ObjectWithDescription
  StopSearchingWhenValueFound: bool
}