namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open Extract.Utilities
open UCLID_AFVALUEFINDERSLib

module FindFromRSD =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IFindFromRSD) =
    { FindFromRSD.AttributeNames = domain.AttributeNames |> VariantVector.toSeq |> Seq.toList
      RSDFileName = domain.RSDFileName }

  let fromDto (dto: Dto.FindFromRSD) =
    FindFromRSDClass
      ( AttributeNames=(dto.AttributeNames.ToVariantVector ()),
        RSDFileName=dto.RSDFileName )

type FindFromRSDConverter() =
  inherit RuleObjectConverter<FindFromRSDClass, IFindFromRSD, Dto.FindFromRSD>()
  override _.toDto _mc domain = domain |> FindFromRSD.toDto
  override _.fromDto _mc dto = dto |> FindFromRSD.fromDto
