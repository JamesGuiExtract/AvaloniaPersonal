namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open Extract.Utilities
open UCLID_AFVALUEFINDERSLib

module RuleRefiningType =
  let toDto = function
  | ERuleRefiningType.kNoRefiningType -> Dto.RuleRefiningType.NoRefiningType
  | ERuleRefiningType.kUptoXWords -> Dto.RuleRefiningType.UptoXWords
  | ERuleRefiningType.kClueLine -> Dto.RuleRefiningType.ClueLine
  | ERuleRefiningType.kUptoXLines -> Dto.RuleRefiningType.UptoXLines
  | ERuleRefiningType.kClueToString -> Dto.RuleRefiningType.ClueToString
  | other -> failwithf "Not a valid ERuleRefiningType! %A" other

  let fromDto = function
  | Dto.RuleRefiningType.NoRefiningType -> ERuleRefiningType.kNoRefiningType
  | Dto.RuleRefiningType.UptoXWords -> ERuleRefiningType.kUptoXWords
  | Dto.RuleRefiningType.ClueLine -> ERuleRefiningType.kClueLine
  | Dto.RuleRefiningType.UptoXLines -> ERuleRefiningType.kUptoXLines
  | Dto.RuleRefiningType.ClueToString -> ERuleRefiningType.kClueToString
  | other -> failwithf "Not a valid RuleRefiningType! %A" other

module ValueAfterClue =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IValueAfterClue) =
    let refiningType = domain.RefiningType |> RuleRefiningType.toDto
    let clueToString = domain.GetClueToString ()
    let numOfLines = ref 0
    let includeClueLine = ref false
    domain.GetUptoXLines(numOfLines, includeClueLine)
    let numOfWords = ref 0
    let punctuations = ref ""
    let stopAtNewLine = ref false
    let stopChars = ref ""
    domain.GetUptoXWords(numOfWords, punctuations, stopAtNewLine, stopChars)

    { Dto.ValueAfterClue.Clues = domain.Clues |> VariantVector.toList
      IsCaseSensitive = domain.IsCaseSensitive
      ClueAsRegExpr = domain.ClueAsRegExpr
      RefiningType = refiningType
      ClueToString = clueToString
      ClueToStringAsRegExpr = domain.ClueToStringAsRegExpr
      NumOfLines = !numOfLines
      IncludeClueLine = !includeClueLine
      NumOfWords = !numOfWords
      Punctuations = !punctuations
      StopAtNewLine = !stopAtNewLine
      StopChars = !stopChars }

  let fromDto (dto: Dto.ValueAfterClue) =
    let domain =
      ValueAfterClueClass
        ( Clues=dto.Clues.ToVariantVector(),
          IsCaseSensitive=dto.IsCaseSensitive,
          ClueAsRegExpr=dto.ClueAsRegExpr,
          ClueToStringAsRegExpr=dto.ClueToStringAsRegExpr )

    // Set all properties so that the binary stream will match when doing round-trip testing
    domain.SetClueToString dto.ClueToString
    domain.SetUptoXLines (dto.NumOfLines, dto.IncludeClueLine)
    domain.SetUptoXWords(dto.NumOfWords, dto.Punctuations, dto.StopAtNewLine, dto.StopChars)

    // Set actual type to be used
    match dto.RefiningType with
    | RuleRefiningType.NoRefiningType ->
        domain.SetNoRefiningType ()
    | RuleRefiningType.ClueToString ->
        domain.SetClueToString dto.ClueToString
    | RuleRefiningType.UptoXLines ->
        domain.SetUptoXLines (dto.NumOfLines, dto.IncludeClueLine)
    | RuleRefiningType.UptoXWords ->
        domain.SetUptoXWords(dto.NumOfWords, dto.Punctuations, dto.StopAtNewLine, dto.StopChars)
    | RuleRefiningType.ClueLine -> domain.SetClueLineType ()
    | other -> failwithf "Not a valid RuleRefiningType! %A" other

    domain

type ValueAfterClueConverter() =
  inherit RuleObjectConverter<ValueAfterClueClass, IValueAfterClue, Dto.ValueAfterClue>()
  override _.toDto _mc domain = domain |> ValueAfterClue.toDto
  override _.fromDto _mc dto = dto |> ValueAfterClue.fromDto
