namespace Extract.AttributeFinder.Rules.Dto

type NamedEntityRecognizer =
| None = 0
| OpenNLP = 1
| Stanford = 2

type OpenNlpTokenizer =
| None = 0
| WhiteSpaceTokenizer = 1
| SimpleTokenizer = 2
| LearnableTokenizer = 3

type NERFinder = {
  NameFinderType: NamedEntityRecognizer
  SplitIntoSentences: bool
  SentenceDetectorPath: string
  TokenizerType: OpenNlpTokenizer
  TokenizerPath: string
  NameFinderPath: string
  EntityTypes: string
  OutputConfidenceSubAttribute: bool
  ApplyLogFunctionToConfidence: bool
  LogBase: double
  LogSteepness: double
  LogXValueOfMiddle: double
  ConvertConfidenceToPercent: bool
}