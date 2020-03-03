namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules

module NoGoodMatchAction =
  let toDto = function
  | NoGoodMatchAction.DoNothing -> Dto.NoGoodMatchAction.DoNothing
  | NoGoodMatchAction.ClearValue -> Dto.NoGoodMatchAction.ClearValue
  | NoGoodMatchAction.RemoveAttribute -> Dto.NoGoodMatchAction.RemoveAttribute
  | NoGoodMatchAction.SetTypeToUntranslated -> Dto.NoGoodMatchAction.SetTypeToUntranslated
  | other -> failwithf "Not a valid NoGoodMatchAction! %A" other

  let fromDto = function
  | Dto.NoGoodMatchAction.DoNothing -> NoGoodMatchAction.DoNothing
  | Dto.NoGoodMatchAction.ClearValue -> NoGoodMatchAction.ClearValue
  | Dto.NoGoodMatchAction.RemoveAttribute -> NoGoodMatchAction.RemoveAttribute
  | Dto.NoGoodMatchAction.SetTypeToUntranslated -> NoGoodMatchAction.SetTypeToUntranslated
  | other -> failwithf "Not a valid NoGoodMatchAction! %A" other

module TranslateValueToBestMatch =
  type TranslateValueToBestMatchClass = TranslateValueToBestMatch
  open Extract.AttributeFinder.Rules.Dto

  let toDto (mc: IMasterRuleObjectConverter) (domain: ITranslateValueToBestMatch): Dto.TranslateValueToBestMatch =
    { AttributeSelector = domain.AttributeSelector |> ObjectWithType.toDto mc
      SourceListPath = domain.SourceListPath
      SynonymMapPath = domain.SynonymMapPath
      MinimumMatchScore = domain.MinimumMatchScore
      UnableToTranslateAction = domain.UnableToTranslateAction |> NoGoodMatchAction.toDto
      CreateBestMatchScoreSubAttribute = domain.CreateBestMatchScoreSubAttribute }

  let fromDto (mc: IMasterRuleObjectConverter) (dto: Dto.TranslateValueToBestMatch) =
    TranslateValueToBestMatchClass
      ( AttributeSelector=(dto.AttributeSelector |> ObjectWithType.fromDto mc),
        SourceListPath=dto.SourceListPath,
        SynonymMapPath=dto.SynonymMapPath,
        MinimumMatchScore=dto.MinimumMatchScore,
        UnableToTranslateAction=(dto.UnableToTranslateAction |> NoGoodMatchAction.fromDto),
        CreateBestMatchScoreSubAttribute=dto.CreateBestMatchScoreSubAttribute )

type TranslateValueToBestMatchConverter() =
  inherit RuleObjectConverter<TranslateValueToBestMatch, ITranslateValueToBestMatch, Dto.TranslateValueToBestMatch>()
  override _.toDto mc domain = domain |> TranslateValueToBestMatch.toDto mc
  override _.fromDto mc dto = dto |> TranslateValueToBestMatch.fromDto mc
