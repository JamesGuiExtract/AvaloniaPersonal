namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFVALUEMODIFIERSLib
open UCLID_COMUTILSLib

module TranslateFieldType =
  let toDto = function
  | ETranslateFieldType.kTranslateName -> Dto.TranslateFieldType.Name
  | ETranslateFieldType.kTranslateType -> Dto.TranslateFieldType.Type
  | ETranslateFieldType.kTranslateValue -> Dto.TranslateFieldType.Value
  | other -> failwithf "Not a valid ETranslateFieldType! %A" other

  let fromDto = function
  | Dto.TranslateFieldType.Name -> ETranslateFieldType.kTranslateName
  | Dto.TranslateFieldType.Type -> ETranslateFieldType.kTranslateType
  | Dto.TranslateFieldType.Value -> ETranslateFieldType.kTranslateValue
  | other -> failwithf "Not a valid TranslateFieldType! %A" other

module TranslationPair =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IStringPair): Dto.TranslationPair =
    { From = domain.StringKey
      To = domain.StringValue }

  let fromDto (dto: Dto.TranslationPair) =
    StringPairClass
      ( StringKey=dto.From,
        StringValue=dto.To )

module TranslateValue =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: ITranslateValue): Dto.TranslateValue =
    { TranslateFieldType = domain.TranslateFieldType |> TranslateFieldType.toDto
      TranslationStringPairs = domain.TranslationStringPairs |> IUnknownVector.toDto TranslationPair.toDto
      IsCaseSensitive = domain.IsCaseSensitive }

  let fromDto (dto: Dto.TranslateValue) =
    TranslateValueClass
      ( TranslateFieldType=(dto.TranslateFieldType |> TranslateFieldType.fromDto),
        TranslationStringPairs=(dto.TranslationStringPairs |> IUnknownVector.fromDto TranslationPair.fromDto),
        IsCaseSensitive=dto.IsCaseSensitive )

type TranslateValueConverter() =
  inherit RuleObjectConverter<TranslateValueClass, ITranslateValue, Dto.TranslateValue>()
  override _.toDto _mc domain = domain |> TranslateValue.toDto
  override _.fromDto _mc dto = dto |> TranslateValue.fromDto
