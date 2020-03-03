namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules

module RSDDataScorer =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IRSDDataScorer): Dto.RSDDataScorer =
    { RSDFileName = domain.RSDFileName
      ScoreExpression = domain.ScoreExpression }

  let fromDto (dto: Dto.RSDDataScorer) =
    RSDDataScorer
      ( RSDFileName=dto.RSDFileName,
        ScoreExpression=dto.ScoreExpression )

type RSDDataScorerConverter() =
  inherit RuleObjectConverter<RSDDataScorer, IRSDDataScorer, Dto.RSDDataScorer>()
  override _.toDto _mc domain = domain |> RSDDataScorer.toDto
  override _.fromDto _mc dto = dto |> RSDDataScorer.fromDto
