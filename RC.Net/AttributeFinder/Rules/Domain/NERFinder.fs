namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules

module NamedEntityRecognizer =
  let toDto = function
  | NamedEntityRecognizer.None -> Dto.NamedEntityRecognizer.None
  | NamedEntityRecognizer.OpenNLP -> Dto.NamedEntityRecognizer.OpenNLP
  | NamedEntityRecognizer.Stanford -> Dto.NamedEntityRecognizer.Stanford
  | other -> failwithf "Not a valid NamedEntityRecognizer! %A" other

  let fromDto = function
  | Dto.NamedEntityRecognizer.None -> NamedEntityRecognizer.None
  | Dto.NamedEntityRecognizer.OpenNLP -> NamedEntityRecognizer.OpenNLP
  | Dto.NamedEntityRecognizer.Stanford -> NamedEntityRecognizer.Stanford
  | other -> failwithf "Not a valid NamedEntityRecognizer! %A" other

module OpenNlpTokenizer =
  let toDto = function
  | OpenNlpTokenizer.None -> Dto.OpenNlpTokenizer.None
  | OpenNlpTokenizer.WhiteSpaceTokenizer -> Dto.OpenNlpTokenizer.WhiteSpaceTokenizer
  | OpenNlpTokenizer.SimpleTokenizer -> Dto.OpenNlpTokenizer.SimpleTokenizer
  | OpenNlpTokenizer.LearnableTokenizer -> Dto.OpenNlpTokenizer.LearnableTokenizer
  | other -> failwithf "Not a valid OpenNlpTokenizer! %A" other

  let fromDto = function
  | Dto.OpenNlpTokenizer.None -> OpenNlpTokenizer.None
  | Dto.OpenNlpTokenizer.WhiteSpaceTokenizer -> OpenNlpTokenizer.WhiteSpaceTokenizer
  | Dto.OpenNlpTokenizer.SimpleTokenizer -> OpenNlpTokenizer.SimpleTokenizer
  | Dto.OpenNlpTokenizer.LearnableTokenizer -> OpenNlpTokenizer.LearnableTokenizer
  | other -> failwithf "Not a valid OpenNlpTokenizer! %A" other

module NERFinder =
  type NERFinderClass = NERFinder
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: INERFinder): Dto.NERFinder =
    { NameFinderType = domain.NameFinderType |> NamedEntityRecognizer.toDto
      SplitIntoSentences = domain.SplitIntoSentences
      SentenceDetectorPath = domain.SentenceDetectorPath
      TokenizerType = domain.TokenizerType |> OpenNlpTokenizer.toDto
      TokenizerPath = domain.TokenizerPath
      NameFinderPath = domain.NameFinderPath
      EntityTypes = domain.EntityTypes
      OutputConfidenceSubAttribute = domain.OutputConfidenceSubAttribute
      ApplyLogFunctionToConfidence = domain.ApplyLogFunctionToConfidence
      LogBase = domain.LogBase
      LogSteepness = domain.LogSteepness
      LogXValueOfMiddle = domain.LogXValueOfMiddle
      ConvertConfidenceToPercent = domain.ConvertConfidenceToPercent }

  let fromDto (dto: Dto.NERFinder) =
    NERFinderClass
      ( NameFinderType=(dto.NameFinderType |> NamedEntityRecognizer.fromDto),
        SplitIntoSentences=dto.SplitIntoSentences,
        SentenceDetectorPath=dto.SentenceDetectorPath,
        TokenizerType=(dto.TokenizerType |> OpenNlpTokenizer.fromDto),
        TokenizerPath=dto.TokenizerPath,
        NameFinderPath=dto.NameFinderPath,
        EntityTypes=dto.EntityTypes,
        OutputConfidenceSubAttribute=dto.OutputConfidenceSubAttribute,
        ApplyLogFunctionToConfidence=dto.ApplyLogFunctionToConfidence,
        LogBase=dto.LogBase,
        LogSteepness=dto.LogSteepness,
        LogXValueOfMiddle=dto.LogXValueOfMiddle,
        ConvertConfidenceToPercent=dto.ConvertConfidenceToPercent )

type NERFinderConverter() =
  inherit RuleObjectConverter<NERFinder, INERFinder, Dto.NERFinder>()
  override _.toDto _mc domain = domain |> NERFinder.toDto
  override _.fromDto _mc dto = dto |> NERFinder.fromDto
