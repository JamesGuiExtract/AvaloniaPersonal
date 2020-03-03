namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFOUTPUTHANDLERSLib

module ReformatPersonNames =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IReformatPersonNames): Dto.ReformatPersonNames =
    { PersonAttributeQuery = domain.PersonAttributeQuery
      ReformatPersonSubAttributes = domain.ReformatPersonSubAttributes
      FormatString = domain.FormatString }

  let fromDto (dto: Dto.ReformatPersonNames) =
    ReformatPersonNamesClass
      ( PersonAttributeQuery=dto.PersonAttributeQuery,
        ReformatPersonSubAttributes=dto.ReformatPersonSubAttributes,
        FormatString=dto.FormatString )

type ReformatPersonNamesConverter() =
  inherit RuleObjectConverter<ReformatPersonNamesClass, IReformatPersonNames, Dto.ReformatPersonNames>()
  override _.toDto _mc domain = domain |> ReformatPersonNames.toDto
  override _.fromDto _mc dto = dto |> ReformatPersonNames.fromDto
