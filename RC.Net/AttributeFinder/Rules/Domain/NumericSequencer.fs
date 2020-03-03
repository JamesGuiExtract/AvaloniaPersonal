namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules

module NumericSequencer =
  type NumericSequencerClass = NumericSequencer
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: INumericSequencer): Dto.NumericSequencer =
    { ExpandSequence = domain.ExpandSequence
      Sort = domain.Sort
      AscendingSortOrder = domain.AscendingSortOrder
      EliminateDuplicates = domain.EliminateDuplicates }

  let fromDto (dto: Dto.NumericSequencer) =
    NumericSequencerClass
      ( ExpandSequence=dto.ExpandSequence,
        Sort=dto.Sort,
        AscendingSortOrder=dto.AscendingSortOrder,
        EliminateDuplicates=dto.EliminateDuplicates )

type NumericSequencerConverter() =
  inherit RuleObjectConverter<NumericSequencer, INumericSequencer, Dto.NumericSequencer>()
  override _.toDto _mc domain = domain |> NumericSequencer.toDto
  override _.fromDto _mc dto = dto |> NumericSequencer.fromDto
