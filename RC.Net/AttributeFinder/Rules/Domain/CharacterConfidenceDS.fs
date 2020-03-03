namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFDATASCORERSLib

module CharacterConfidenceDS =
  let toDto (domain: ICharacterConfidenceDS): Dto.CharacterConfidenceDS =
    { AggregateFunction = domain.AggregateFunction |> AggregateFunction.toDto }

  let fromDto (dto: Dto.CharacterConfidenceDS) =
    CharacterConfidenceDSClass
      ( AggregateFunction=(dto.AggregateFunction |> AggregateFunction.fromDto) )

type CharacterConfidenceDSConverter() =
  inherit RuleObjectConverter<CharacterConfidenceDSClass, ICharacterConfidenceDS, Dto.CharacterConfidenceDS>()
  override _.toDto _mc domain = domain |> CharacterConfidenceDS.toDto
  override _.fromDto _mc dto = dto |> CharacterConfidenceDS.fromDto
