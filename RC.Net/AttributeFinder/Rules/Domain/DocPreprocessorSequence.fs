namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFPREPROCESSORSLib
open UCLID_COMUTILSLib

module DocPreprocessorSequence =
  let toDto (mc: IMasterRuleObjectConverter) (domain: IMultipleObjectHolder): Dto.DocPreprocessorSequence =
    { ObjectsVector = domain.ObjectsVector |> ObjectWithDescriptionVector.toDto mc }

  let fromDto (mc: IMasterRuleObjectConverter) (dto: Dto.DocPreprocessorSequence) =
    DocPreprocessorSequenceClass
      ( ObjectsVector=(dto.ObjectsVector |> ObjectWithDescriptionVector.fromDto mc) )

type DocPreprocessorSequenceConverter() =
  inherit RuleObjectConverter<DocPreprocessorSequenceClass, IMultipleObjectHolder, Dto.DocPreprocessorSequence>()
  override _.toDto mc domain = domain |> DocPreprocessorSequence.toDto mc
  override _.fromDto mc dto = dto |> DocPreprocessorSequence.fromDto mc
