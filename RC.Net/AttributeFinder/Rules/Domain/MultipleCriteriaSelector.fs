namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open Extract.Utilities
open UCLID_COMUTILSLib

module CombinationType =
  let toDto selectExclusively =
    if selectExclusively
    then Dto.CombinationType.Chain
    else Dto.CombinationType.Union

  let fromDto = function
  | Dto.CombinationType.Chain -> true
  | Dto.CombinationType.Union -> false
  | other -> failwithf "Not a valid CombinationType! %A" other

module SelectType =
  let toDto negated =
    if negated
    then Dto.SelectType.NonMatching
    else Dto.SelectType.Matching

  let fromDto = function
  | Dto.SelectType.NonMatching -> true
  | Dto.SelectType.Matching -> false
  | other -> failwithf "Not a valid SelectType! %A" other

module CriteriaSelectors =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (mc: IMasterRuleObjectConverter) (selectors: IIUnknownVector) (negatedSelectors: bool array) =
    selectors
    |> IUnknownVector.toSeq
    |> Seq.zip negatedSelectors
    |> Seq.map (fun (selectType, selector: IObjectWithDescription) ->
      { Dto.Selector.Select = selectType |> SelectType.toDto
        With = selector |> ObjectWithDescription.toDto mc }
    )
    |> Seq.toList

  let fromDto (mc: IMasterRuleObjectConverter) (selectors: Dto.Selector list) =
    selectors
    |> List.map (fun selector ->
      selector.With |> ObjectWithDescription.fromDto mc,
      selector.Select |> SelectType.fromDto
    )
    |> List.unzip
    |> (fun (selectors, negatedSelectors) ->
      selectors.ToIUnknownVector (), negatedSelectors
    )

module MultipleCriteriaSelector =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (mc: IMasterRuleObjectConverter) (domain: IMultipleCriteriaSelector) =
    { Selectors = (domain.Selectors, domain.NegatedSelectors) ||> CriteriaSelectors.toDto mc
      CombineBy = domain.SelectExclusively |> CombinationType.toDto }

  let fromDto (mc: IMasterRuleObjectConverter) (dto: Dto.MultipleCriteriaSelector) =
    let (selectors, negatedSelectors) = dto.Selectors |> CriteriaSelectors.fromDto mc
    MultipleCriteriaSelector
      ( Selectors=selectors,
        NegatedSelectors=(negatedSelectors |> List.toArray),
        SelectExclusively=(dto.CombineBy |> CombinationType.fromDto) )

type MultipleCriteriaSelectorConverter() =
  inherit RuleObjectConverter<MultipleCriteriaSelector, IMultipleCriteriaSelector, Dto.MultipleCriteriaSelector>()
  override _.toDto mc domain = domain |> MultipleCriteriaSelector.toDto mc
  override _.fromDto mc dto = dto |> MultipleCriteriaSelector.fromDto mc
