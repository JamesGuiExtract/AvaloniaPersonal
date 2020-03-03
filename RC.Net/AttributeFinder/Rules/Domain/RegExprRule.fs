namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFVALUEFINDERSLib
open System.IO

module RegExprRule =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IRegExprRule) =

    // the domain object will throw an exception for missing dat files if they are literal paths
    // this takes care of junk paths that were left in the domain object but aren't being used
    let useRegexFileName =
      if domain.IsRegExpFromFile <> 0 then true
      else
        let fname = domain.RegExpFileName
        if String.length fname = 0 then false
        else
          if domain.RegExpFileName.Contains("<") || (try File.Exists domain.RegExpFileName with _ -> false)
          then true
          else
            printfn "Skipping regex filename: %s" fname
            false

    { Dto.RegExprRule.IsRegExpFromFile = domain.IsRegExpFromFile <> 0
      RegExpFileName = if useRegexFileName then domain.RegExpFileName else ""
      Pattern = domain.Pattern
      IsCaseSensitive = domain.IsCaseSensitive
      FirstMatchOnly = domain.FirstMatchOnly
      CreateSubAttributesFromNamedMatches = domain.CreateSubAttributesFromNamedMatches
      OnlyCreateOneSubAttributePerGroup = domain.OnlyCreateOneSubAttributePerGroup }

  let fromDto (dto: Dto.RegExprRule) =
    let domain =
      RegExprRuleClass
        ( IsRegExpFromFile=(if dto.IsRegExpFromFile then -1 else 0),
          IsCaseSensitive=dto.IsCaseSensitive,
          FirstMatchOnly=dto.FirstMatchOnly,
          CreateSubAttributesFromNamedMatches=dto.CreateSubAttributesFromNamedMatches,
          OnlyCreateOneSubAttributePerGroup=dto.OnlyCreateOneSubAttributePerGroup )

    if dto.Pattern |> String.length > 0
    then domain.Pattern <- dto.Pattern

    if dto.RegExpFileName |> String.length > 0
    then domain.RegExpFileName <- dto.RegExpFileName

    domain

type RegExprRuleConverter() =
  inherit RuleObjectConverter<RegExprRuleClass, IRegExprRule, Dto.RegExprRule>()
  override _.toDto _mc domain = domain |> RegExprRule.toDto
  override _.fromDto _mc dto = dto |> RegExprRule.fromDto
