namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_REDACTIONCUSTOMCOMPONENTSLib

module SSNFinder =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: ISSNFinder) =
    let (subattributeName, spatialSubattribute, clearIfNoneFound) = domain.GetOptions()
    { Dto.SSNFinder.SubattributeName = subattributeName
      SpatialSubattribute = spatialSubattribute
      ClearIfNoneFound = clearIfNoneFound }

  let fromDto (dto: Dto.SSNFinder) =
    let domain = SSNFinderClass ()
    domain.SetOptions (dto.SubattributeName, dto.SpatialSubattribute, dto.ClearIfNoneFound)
    domain

type SSNFinderConverter() =
  inherit RuleObjectConverter<SSNFinderClass, ISSNFinder, Dto.SSNFinder>()
  override _.toDto _mc domain = domain |> SSNFinder.toDto
  override _.fromDto _mc dto = dto |> SSNFinder.fromDto
