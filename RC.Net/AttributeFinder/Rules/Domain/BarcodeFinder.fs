namespace Extract.AttributeFinder.Rules.Domain

open Extract.Utilities
open Extract.AttributeFinder.Rules

module BarType =
  let toDto = function
  | 0 -> Dto.BarType.BAR_EAN
  | 1 -> Dto.BarType.BAR_EAN_SUPPL
  | 2 -> Dto.BarType.BAR_UPC_A
  | 3 -> Dto.BarType.BAR_UPC_E
  | 4 -> Dto.BarType.BAR_ITF
  | 5 -> Dto.BarType.BAR_ITF_CDT
  | 6 -> Dto.BarType.BAR_C39
  | 7 -> Dto.BarType.BAR_C39_CDT
  | 8 -> Dto.BarType.BAR_C39_SST
  | 9 -> Dto.BarType.BAR_C39_EXT
  | 10 -> Dto.BarType.BAR_C128
  | 11 -> Dto.BarType.BAR_C128_CDT
  | 12 -> Dto.BarType.BAR_CB
  | 13 -> Dto.BarType.BAR_CB_NO_SST
  | 14 -> Dto.BarType.BAR_POSTNET
  | 15 -> Dto.BarType.BAR_A2of5
  | 16 -> Dto.BarType.BAR_UCC128
  | 17 -> Dto.BarType.BAR_2of5
  | 18 -> Dto.BarType.BAR_C93
  | 19 -> Dto.BarType.BAR_PATCH
  | 20 -> Dto.BarType.BAR_PDF417
  | 21 -> Dto.BarType.BAR_PLANET
  | 22 -> Dto.BarType.BAR_C32
  | 23 -> Dto.BarType.BAR_DMATRIX
  | 24 -> Dto.BarType.BAR_C39_NSS
  | 25 -> Dto.BarType.BAR_4STATE
  | 26 -> Dto.BarType.BAR_QR
  | 27 -> Dto.BarType.BAR_MAT25
  | 28 -> Dto.BarType.BAR_4STATE_DK1
  | 29 -> Dto.BarType.BAR_AZTEC
  | 30 -> Dto.BarType.BAR_CODE11
  | 31 -> Dto.BarType.BAR_ITAPOST25
  | 32 -> Dto.BarType.BAR_MSI
  | 33 -> Dto.BarType.BAR_BOOKLAND
  | 34 -> Dto.BarType.BAR_ITF14
  | 35 -> Dto.BarType.BAR_EAN14
  | 36 -> Dto.BarType.BAR_SSCC18
  | 37 -> Dto.BarType.BAR_DATABAR_LTD
  | 38 -> Dto.BarType.BAR_DATABAR_EXP
  | 39 -> Dto.BarType.BAR_4STATE_USPS
  | 40 -> Dto.BarType.BAR_4STATE_AUSPOST
  | 41 -> Dto.BarType.BAR_SIZE
  | other -> failwithf "Not a valid EBarType! %A" other

  let fromDto = function
  | Dto.BarType.BAR_EAN -> 0
  | Dto.BarType.BAR_EAN_SUPPL -> 1
  | Dto.BarType.BAR_UPC_A -> 2
  | Dto.BarType.BAR_UPC_E -> 3
  | Dto.BarType.BAR_ITF -> 4
  | Dto.BarType.BAR_ITF_CDT -> 5
  | Dto.BarType.BAR_C39 -> 6
  | Dto.BarType.BAR_C39_CDT -> 7
  | Dto.BarType.BAR_C39_SST -> 8
  | Dto.BarType.BAR_C39_EXT -> 9
  | Dto.BarType.BAR_C128 -> 10
  | Dto.BarType.BAR_C128_CDT -> 11
  | Dto.BarType.BAR_CB -> 12
  | Dto.BarType.BAR_CB_NO_SST -> 13
  | Dto.BarType.BAR_POSTNET -> 14
  | Dto.BarType.BAR_A2of5 -> 15
  | Dto.BarType.BAR_UCC128 -> 16
  | Dto.BarType.BAR_2of5 -> 17
  | Dto.BarType.BAR_C93 -> 18
  | Dto.BarType.BAR_PATCH -> 19
  | Dto.BarType.BAR_PDF417 -> 20
  | Dto.BarType.BAR_PLANET -> 21
  | Dto.BarType.BAR_C32 -> 22
  | Dto.BarType.BAR_DMATRIX -> 23
  | Dto.BarType.BAR_C39_NSS -> 24
  | Dto.BarType.BAR_4STATE -> 25
  | Dto.BarType.BAR_QR -> 26
  | Dto.BarType.BAR_MAT25 -> 27
  | Dto.BarType.BAR_4STATE_DK1 -> 28
  | Dto.BarType.BAR_AZTEC -> 29
  | Dto.BarType.BAR_CODE11 -> 30
  | Dto.BarType.BAR_ITAPOST25 -> 31
  | Dto.BarType.BAR_MSI -> 32
  | Dto.BarType.BAR_BOOKLAND -> 33
  | Dto.BarType.BAR_ITF14 -> 34
  | Dto.BarType.BAR_EAN14 -> 35
  | Dto.BarType.BAR_SSCC18 -> 36
  | Dto.BarType.BAR_DATABAR_LTD -> 37
  | Dto.BarType.BAR_DATABAR_EXP -> 38
  | Dto.BarType.BAR_4STATE_USPS -> 39
  | Dto.BarType.BAR_4STATE_AUSPOST -> 40
  | Dto.BarType.BAR_SIZE -> 41
  | other -> failwithf "Not a valid BarType! %A" other


module BarcodeFinder =
  type BarcodeFinderClass = BarcodeFinder
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IBarcodeFinder): Dto.BarcodeFinder =
    { Types = domain.Types |> VariantVector.toList |> List.map BarType.toDto
      InheritOCRParameters = domain.InheritOCRParameters }

  let fromDto (dto: Dto.BarcodeFinder) =
    BarcodeFinderClass
      ( Types=(dto.Types |> Seq.map BarType.fromDto).ToVariantVector(),
        InheritOCRParameters=dto.InheritOCRParameters )

type BarcodeFinderAttributesConverter() =
  inherit RuleObjectConverter<BarcodeFinder, IBarcodeFinder, Dto.BarcodeFinder>()
  override _.toDto _mc domain = domain |> BarcodeFinder.toDto
  override _.fromDto _mc dto = dto |> BarcodeFinder.fromDto