namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFUTILSLib

module DocumentClassifier =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IDocumentClassifier): Dto.DocumentClassifier =
    { IndustryCategoryName = domain.IndustryCategoryName
      ReRunClassifier = domain.ReRunClassifier }

  let fromDto (dto: Dto.DocumentClassifier) =
    DocumentClassifierClass
      ( IndustryCategoryName=dto.IndustryCategoryName,
        ReRunClassifier=dto.ReRunClassifier )

type DocumentClassifierConverter() =
  inherit RuleObjectConverter<DocumentClassifierClass, IDocumentClassifier, Dto.DocumentClassifier>()
  override _.toDto _mc domain = domain |> DocumentClassifier.toDto
  override _.fromDto _mc dto = dto |> DocumentClassifier.fromDto
