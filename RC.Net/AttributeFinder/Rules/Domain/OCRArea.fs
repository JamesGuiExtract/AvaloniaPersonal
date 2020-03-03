namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFVALUEMODIFIERSLib
open UCLID_RASTERANDOCRMGMTLib
open System

module FilterCharacters =
  module Flag =
    let toDto = function
    | EFilterCharacters.kNoFilter -> None
    | EFilterCharacters.kAlphaFilter -> Some Dto.FilterCharacters.Alpha
    | EFilterCharacters.kNumeralFilter -> Some Dto.FilterCharacters.Numeral
    | EFilterCharacters.kPeriodFilter -> Some Dto.FilterCharacters.Period
    | EFilterCharacters.kHyphenFilter -> Some Dto.FilterCharacters.Hyphen
    | EFilterCharacters.kUnderscoreFilter -> Some Dto.FilterCharacters.Underscore
    | EFilterCharacters.kCommaFilter -> Some Dto.FilterCharacters.Comma
    | EFilterCharacters.kForwardSlashFilter -> Some Dto.FilterCharacters.ForwardSlash
    | EFilterCharacters.kCustomFilter -> Some Dto.FilterCharacters.Custom
    | other -> failwithf "Not a valid EFilterCharacters! %A" other

    let fromDto = function
    | Dto.FilterCharacters.Alpha -> EFilterCharacters.kAlphaFilter
    | Dto.FilterCharacters.Numeral -> EFilterCharacters.kNumeralFilter
    | Dto.FilterCharacters.Period -> EFilterCharacters.kPeriodFilter
    | Dto.FilterCharacters.Hyphen -> EFilterCharacters.kHyphenFilter
    | Dto.FilterCharacters.Underscore -> EFilterCharacters.kUnderscoreFilter
    | Dto.FilterCharacters.Comma -> EFilterCharacters.kCommaFilter
    | Dto.FilterCharacters.ForwardSlash -> EFilterCharacters.kForwardSlashFilter
    | Dto.FilterCharacters.Custom -> EFilterCharacters.kCustomFilter
    | other -> failwithf "Not a valid FilterCharacters! %A" other

  let toDto (flags: EFilterCharacters) =
    Enum.GetValues(typeof<EFilterCharacters>)
    |> Seq.cast
    |> Seq.filter flags.HasFlag
    |> Seq.choose Flag.toDto
    |> Seq.toList

  let fromDto = function
    | [] -> EFilterCharacters.kNoFilter
    | flags -> flags |> Seq.map Flag.fromDto |> Seq.reduce (|||)

module OCRArea =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IOCRArea) =
    let (filter, customFilter, detectHandwriting, returnUnrecognized, clearIfNoneFound) = domain.GetOptions ()
    { Dto.OCRArea.Filter = filter |> FilterCharacters.toDto
      CustomFilterCharacters = customFilter
      DetectHandwriting = detectHandwriting
      ReturnUnrecognized = returnUnrecognized
      ClearIfNoneFound = clearIfNoneFound }

  let fromDto (dto: Dto.OCRArea) =
    let filter = dto.Filter |> FilterCharacters.fromDto
    let domain = OCRAreaClass ()
    domain.SetOptions
      ( filter,
        dto.CustomFilterCharacters,
        dto.DetectHandwriting,
        dto.ReturnUnrecognized,
        dto.ClearIfNoneFound )
    domain
type OCRAreaConverter() =
  inherit RuleObjectConverter<OCRAreaClass, IOCRArea, Dto.OCRArea>()
  override _.toDto _mc domain = domain |> OCRArea.toDto
  override _.fromDto _mc dto = dto |> OCRArea.fromDto
