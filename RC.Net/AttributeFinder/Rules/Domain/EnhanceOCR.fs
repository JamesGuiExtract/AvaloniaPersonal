namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFVALUEMODIFIERSLib

module FilterPackage =
  let toDto = function
  | EFilterPackage.kLow -> Dto.FilterPackage.Low
  | EFilterPackage.kMedium -> Dto.FilterPackage.Medium
  | EFilterPackage.kHigh -> Dto.FilterPackage.High
  | EFilterPackage.kHalftoneSpeckled -> Dto.FilterPackage.HalftoneSpeckled
  | EFilterPackage.kAliasedDiffuse -> Dto.FilterPackage.AliasedDiffuse
  | EFilterPackage.kLinesSmudged -> Dto.FilterPackage.LinesSmudged
  | EFilterPackage.kCustom -> Dto.FilterPackage.Custom
  | other -> failwithf "Not a valid EFilterPackage! %A" other

  let fromDto = function
  | Dto.FilterPackage.Low -> EFilterPackage.kLow
  | Dto.FilterPackage.Medium -> EFilterPackage.kMedium
  | Dto.FilterPackage.High -> EFilterPackage.kHigh
  | Dto.FilterPackage.HalftoneSpeckled -> EFilterPackage.kHalftoneSpeckled
  | Dto.FilterPackage.AliasedDiffuse -> EFilterPackage.kAliasedDiffuse
  | Dto.FilterPackage.LinesSmudged -> EFilterPackage.kLinesSmudged
  | Dto.FilterPackage.Custom -> EFilterPackage.kCustom
  | other -> failwithf "Not a valid FilterPackage! %A" other

module EnhanceOCR =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IEnhanceOCR): Dto.EnhanceOCR =
    { ConfidenceCriteria = domain.ConfidenceCriteria
      FilterPackage = domain.FilterPackage |> FilterPackage.toDto
      CustomFilterPackage = domain.CustomFilterPackage
      PreferredFormatRegexFile = domain.PreferredFormatRegexFile
      CharsToIgnore = domain.CharsToIgnore
      OutputFilteredImages = domain.OutputFilteredImages }

  let fromDto (dto: Dto.EnhanceOCR) =
    EnhanceOCRClass
      ( ConfidenceCriteria=dto.ConfidenceCriteria,
        FilterPackage=(dto.FilterPackage |> FilterPackage.fromDto),
        CustomFilterPackage=dto.CustomFilterPackage,
        PreferredFormatRegexFile=dto.PreferredFormatRegexFile,
        CharsToIgnore=dto.CharsToIgnore,
        OutputFilteredImages=dto.OutputFilteredImages )

type EnhanceOCRConverter() =
  inherit RuleObjectConverter<EnhanceOCRClass, IEnhanceOCR, Dto.EnhanceOCR>()
  override _.toDto _mc domain = domain |> EnhanceOCR.toDto
  override _.fromDto _mc dto = dto |> EnhanceOCR.fromDto
