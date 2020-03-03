namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFOUTPUTHANDLERSLib
open UCLID_COMUTILSLib

module OutputHandlerSequence =
  let toDto (mc: IMasterRuleObjectConverter) (domain: IMultipleObjectHolder): Dto.OutputHandlerSequence =
    { ObjectsVector = domain.ObjectsVector |> ObjectWithDescriptionVector.toDto mc }

  let fromDto (mc: IMasterRuleObjectConverter) (dto: Dto.OutputHandlerSequence) =
    OutputHandlerSequenceClass
      ( ObjectsVector=(dto.ObjectsVector |> ObjectWithDescriptionVector.fromDto mc) )

type OutputHandlerSequenceConverter() =
  inherit RuleObjectConverter<OutputHandlerSequenceClass, IMultipleObjectHolder, Dto.OutputHandlerSequence>()
  override _.toDto mc domain = domain |> OutputHandlerSequence.toDto mc
  override _.fromDto mc dto = dto |> OutputHandlerSequence.fromDto mc
