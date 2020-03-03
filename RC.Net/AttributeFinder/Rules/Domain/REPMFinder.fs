namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFVALUEFINDERSLib

module PMReturnMatchType =
  let toDto = function
  | EPMReturnMatchType.kReturnFirstMatch -> Dto.PMReturnMatchType.ReturnFirstMatch
  | EPMReturnMatchType.kReturnBestMatch -> Dto.PMReturnMatchType.ReturnBestMatch
  | EPMReturnMatchType.kReturnAllMatches -> Dto.PMReturnMatchType.ReturnAllMatches
  | EPMReturnMatchType.kReturnFirstOrBest -> Dto.PMReturnMatchType.ReturnFirstOrBest
  | other -> failwithf "Not a valid EPMReturnMatchType! %A" other

  let fromDto = function
  | Dto.PMReturnMatchType.ReturnFirstMatch -> EPMReturnMatchType.kReturnFirstMatch
  | Dto.PMReturnMatchType.ReturnBestMatch -> EPMReturnMatchType.kReturnBestMatch
  | Dto.PMReturnMatchType.ReturnAllMatches -> EPMReturnMatchType.kReturnAllMatches
  | Dto.PMReturnMatchType.ReturnFirstOrBest -> EPMReturnMatchType.kReturnFirstOrBest
  | other -> failwithf "Not a valid PMReturnMatchType! %A" other

module REPMFinder =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (mc: IMasterRuleObjectConverter) (domain: IREPMFinder): Dto.REPMFinder =
    { RulesFileName = domain.RulesFileName
      IgnoreInvalidTags = domain.IgnoreInvalidTags
      CaseSensitive = domain.CaseSensitive
      StoreRuleWorked = domain.StoreRuleWorked
      RuleWorkedName = domain.RuleWorkedName
      DataScorer = domain.DataScorer |> ObjectWithDescription.toDto mc
      MinScoreToConsiderAsMatch = domain.MinScoreToConsiderAsMatch
      ReturnMatchType = domain.ReturnMatchType |> PMReturnMatchType.toDto
      MinFirstToConsiderAsMatch = domain.MinFirstToConsiderAsMatch
      OnlyCreateOneAttributePerGroup = domain.OnlyCreateOneAttributePerGroup }

  let fromDto (mc: IMasterRuleObjectConverter) (dto: Dto.REPMFinder) =
    REPMFinderClass
      ( RulesFileName=dto.RulesFileName,
        IgnoreInvalidTags=dto.IgnoreInvalidTags,
        CaseSensitive=dto.CaseSensitive,
        StoreRuleWorked=dto.StoreRuleWorked,
        RuleWorkedName=dto.RuleWorkedName,
        DataScorer=(dto.DataScorer |> ObjectWithDescription.fromDto mc),
        MinScoreToConsiderAsMatch=dto.MinScoreToConsiderAsMatch,
        ReturnMatchType=(dto.ReturnMatchType |> PMReturnMatchType.fromDto),
        MinFirstToConsiderAsMatch=dto.MinFirstToConsiderAsMatch,
        OnlyCreateOneAttributePerGroup=dto.OnlyCreateOneAttributePerGroup )

type REPMFinderConverter() =
  inherit RuleObjectConverter<REPMFinderClass, IREPMFinder, Dto.REPMFinder>()
  override _.toDto mc domain = domain |> REPMFinder.toDto mc
  override _.fromDto mc dto = dto |> REPMFinder.fromDto mc
