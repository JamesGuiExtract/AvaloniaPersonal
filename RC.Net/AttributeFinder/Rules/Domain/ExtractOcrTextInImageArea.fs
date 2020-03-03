namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_RASTERANDOCRMGMTLib

module SpatialEntity =
  let toDto = function
  | ESpatialEntity.kNoEntity -> Dto.SpatialEntity.NoEntity
  | ESpatialEntity.kCharacter -> Dto.SpatialEntity.Character
  | ESpatialEntity.kWord -> Dto.SpatialEntity.Word
  | ESpatialEntity.kLine -> Dto.SpatialEntity.Line
  | other -> failwithf "Not a valid ESpatialEntity! %A" other

  let fromDto = function
  | Dto.SpatialEntity.NoEntity -> ESpatialEntity.kNoEntity
  | Dto.SpatialEntity.Character -> ESpatialEntity.kCharacter
  | Dto.SpatialEntity.Word -> ESpatialEntity.kWord
  | Dto.SpatialEntity.Line -> ESpatialEntity.kLine
  | other -> failwithf "Not a valid SpatialEntity! %A" other

module ExtractOcrTextInImageArea =
  type ExtractOcrTextInImageAreaClass = ExtractOcrTextInImageArea
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IExtractOcrTextInImageArea): Dto.ExtractOcrTextInImageArea =
    { IncludeTextOnBoundary = domain.IncludeTextOnBoundary
      SpatialEntityType = domain.SpatialEntityType |> SpatialEntity.toDto
      UseOriginalDocumentOcr = domain.UseOriginalDocumentOcr
      UseOverallBounds = domain.UseOverallBounds }

  let fromDto (dto: Dto.ExtractOcrTextInImageArea) =
    ExtractOcrTextInImageAreaClass
      ( IncludeTextOnBoundary=dto.IncludeTextOnBoundary,
        SpatialEntityType=(dto.SpatialEntityType |> SpatialEntity.fromDto),
        UseOriginalDocumentOcr=dto.UseOriginalDocumentOcr,
        UseOverallBounds=dto.UseOverallBounds )

type ExtractOcrTextInImageAreaConverter() =
  inherit RuleObjectConverter<ExtractOcrTextInImageArea, IExtractOcrTextInImageArea, Dto.ExtractOcrTextInImageArea>()
  override _.toDto _mc domain = domain |> ExtractOcrTextInImageArea.toDto
  override _.fromDto _mc dto = dto |> ExtractOcrTextInImageArea.fromDto
