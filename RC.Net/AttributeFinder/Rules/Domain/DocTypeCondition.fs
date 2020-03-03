namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open Extract.Utilities
open UCLID_AFCONDITIONSLib
open UCLID_AFUTILSLib

module DocumentConfidenceLevel =
  let toDto = function
  | EDocumentConfidenceLevel.kZeroLevel -> Dto.DocumentConfidenceLevel.ZeroLevel
  | EDocumentConfidenceLevel.kMaybeLevel -> Dto.DocumentConfidenceLevel.MaybeLevel
  | EDocumentConfidenceLevel.kProbableLevel -> Dto.DocumentConfidenceLevel.ProbableLevel
  | EDocumentConfidenceLevel.kSureLevel -> Dto.DocumentConfidenceLevel.SureLevel
  | other -> failwithf "Not a valid EDocumentConfidenceLevel! %A" other

  let fromDto = function
  | Dto.DocumentConfidenceLevel.ZeroLevel -> EDocumentConfidenceLevel.kZeroLevel
  | Dto.DocumentConfidenceLevel.MaybeLevel -> EDocumentConfidenceLevel.kMaybeLevel
  | Dto.DocumentConfidenceLevel.ProbableLevel -> EDocumentConfidenceLevel.kProbableLevel
  | Dto.DocumentConfidenceLevel.SureLevel -> EDocumentConfidenceLevel.kSureLevel
  | other -> failwithf "Not a valid DocumentConfidenceLevel! %A" other

module DocTypeCondition =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IDocTypeCondition): Dto.DocTypeCondition =
    { AllowTypes = domain.AllowTypes
      DocumentClassifiersPath = domain.DocumentClassifiersPath
      Category = domain.Category
      Types = domain.Types |> VariantVector.toList
      MinConfidence = domain.MinConfidence |> DocumentConfidenceLevel.toDto }

  let fromDto (dto: Dto.DocTypeCondition) =
    let domain =
      DocTypeConditionClass
        ( AllowTypes=dto.AllowTypes,
          DocumentClassifiersPath=dto.DocumentClassifiersPath,
          Types=dto.Types.ToVariantVector(),
          MinConfidence=(dto.MinConfidence |> DocumentConfidenceLevel.fromDto) )

    if dto.Category |> String.length > 0
    then domain.Category <- dto.Category

    domain

type DocTypeConditionConverter() =
  inherit RuleObjectConverter<DocTypeConditionClass, IDocTypeCondition, Dto.DocTypeCondition>()
  override _.toDto _mc domain = domain |> DocTypeCondition.toDto
  override _.fromDto _mc dto = dto |> DocTypeCondition.fromDto
