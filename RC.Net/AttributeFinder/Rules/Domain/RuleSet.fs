namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open Extract.Utilities
open UCLID_AFCORELib
open UCLID_RASTERANDOCRMGMTLib
open UCLID_COMUTILSLib
open System.Collections.Generic

module RuleSetRunMode =
    let toDto =
        function
        | ERuleSetRunMode.kPassInputVOAToOutput -> Dto.RuleSetRunMode.PassInputVOAToOutput
        | ERuleSetRunMode.kRunPerDocument -> Dto.RuleSetRunMode.RunPerDocument
        | ERuleSetRunMode.kRunPerPage -> Dto.RuleSetRunMode.RunPerPage
        | ERuleSetRunMode.kRunPerPaginationDocument -> Dto.RuleSetRunMode.RunPerPaginationDocument
        | other -> failwithf "Not a valid ERuleSetRunMode! %A" other

    let fromDto =
        function
        | Dto.RuleSetRunMode.PassInputVOAToOutput -> ERuleSetRunMode.kPassInputVOAToOutput
        | Dto.RuleSetRunMode.RunPerDocument -> ERuleSetRunMode.kRunPerDocument
        | Dto.RuleSetRunMode.RunPerPage -> ERuleSetRunMode.kRunPerPage
        | Dto.RuleSetRunMode.RunPerPaginationDocument -> ERuleSetRunMode.kRunPerPaginationDocument
        | other -> failwithf "Not a valid RuleSetRunMode! %A" other


module RuleExecutionCounters =
    open Extract.AttributeFinder.Rules.Dto

    let toDto (domain: IRuleSet) =
        let builtInCounters =
            seq {
                { ID = 1
                  Name = "FLEX Index - Indexing (By Document)"
                  ByPage = false
                  Enabled = domain.UseDocsIndexingCounter }

                { ID = 2
                  Name = "FLEX Index - Pagination (By Document)"
                  ByPage = false
                  Enabled = domain.UsePaginationCounter }

                { ID = 3
                  Name = "FLEX Index - Redaction (By Page)"
                  ByPage = true
                  Enabled = domain.UsePagesRedactionCounter }

                { ID = 4
                  Name = "FLEX Index - Redaction (By Document)"
                  ByPage = false
                  Enabled = domain.UseDocsRedactionCounter }

                { ID = 5
                  Name = "FLEX Index - Indexing (By Page)"
                  ByPage = true
                  Enabled = domain.UsePagesIndexingCounter }
            }
            |> Seq.filter (fun c -> c.Enabled)

        let customCounters =
            domain.CustomCounters
            |> IUnknownVector.toSeq
            |> Seq.map (fun counterProperties ->
                let x: obj list = counterProperties |> VariantVector.toList

                { ID = downcast x.[0]
                  Name = downcast x.[1]
                  ByPage = downcast x.[2]
                  Enabled = downcast x.[3] })

        seq {
            yield! builtInCounters
            yield! customCounters
        }
        |> Seq.toList

    let setFromDto (target: IRuleSet) (dto: Counter list) =
        let customCounters = IUnknownVectorClass()

        dto
        |> Seq.iter (fun counter ->
            match counter.ID with
            | 1 -> target.UseDocsIndexingCounter <- counter.Enabled
            | 2 -> target.UsePaginationCounter <- counter.Enabled
            | 3 -> target.UsePagesRedactionCounter <- counter.Enabled
            | 4 -> target.UseDocsRedactionCounter <- counter.Enabled
            | 5 -> target.UsePagesIndexingCounter <- counter.Enabled
            | _ ->
                customCounters.PushBack(
                    [ box counter.ID
                      box counter.Name
                      box counter.ByPage
                      box counter.Enabled ]
                        .ToVariantVector()
                ))

        if customCounters.Size() > 0 then
            target.CustomCounters <- customCounters


module OCRParameters =
    // Newtonsoft deserializes integers to int64 when the exact type isn't known (e.g., deserializing to obj)
    // Store as int64 in the DTO so that the result of domain->dto = json->dto
    let changeIntToLong (o: obj) =
        match o with
        | :? int32 as i -> i |> int64 |> box
        | _ -> o

    let changeLongToInt (o: obj) =
        match o with
        | :? int64 as i -> i |> int32 |> box
        | _ -> o

    let toDto (domain: IOCRParameters) =
        domain :?> IVariantVector
        |> VariantVector.toSeq
        |> Seq.map (fun (vp: IVariantPair) ->
            KeyValuePair<_, _>(vp.VariantKey |> changeIntToLong, vp.VariantValue |> changeIntToLong))
        |> Seq.toList

    let fromDto (dto: KeyValuePair<_, _> list) =
        dto
        |> Seq.map (fun kv ->
            VariantPairClass(VariantKey = (kv.Key |> changeLongToInt), VariantValue = (kv.Value |> changeLongToInt)))
        |> (fun x -> x.ToVariantVector())
        :?> IOCRParameters

module AttributeNameToInfoMap =
    let toDto mc (domain: IStrToObjectMap) =
        seq { for i in 0 .. (domain.Size - 1) -> domain.GetKeyValue i }
        |> Seq.map (fun (k, v) -> k, (downcast v |> AttributeFindInfo.toDto mc))
        |> Map.ofSeq

    let fromDto mc (dto: IDictionary<string, Dto.AttributeFindInfo>) =
        let domain = StrToObjectMapClass()

        dto
        |> Seq.iter (fun kv -> domain.Set(kv.Key, (kv.Value |> AttributeFindInfo.fromDto mc)))

        domain :?> 't

module RuleSet =
    open Extract.AttributeFinder.Rules.Dto

    let toDto mc (domain: IRuleSet) : Dto.RuleSet =
        let irun = domain :?> IRunMode

        { SavedWithSoftwareVersion = Dto.RuleSet.version
          Comments = domain.Comments
          Counters = domain |> RuleExecutionCounters.toDto
          FKBVersion = domain.FKBVersion
          ForInternalUseOnly = domain.ForInternalUseOnly
          IsSwipingRule = domain.IsSwipingRule
          OCRParameters =
            ((domain :?> IHasOCRParameters).OCRParameters
             |> OCRParameters.toDto)
          RunMode = irun.RunMode |> RuleSetRunMode.toDto
          InsertAttributesUnderParent = irun.InsertAttributesUnderParent
          InsertParentName = irun.InsertParentName
          InsertParentValue = irun.InsertParentValue
          DeepCopyInput = irun.DeepCopyInput
          GlobalDocPreprocessor =
            domain.GlobalDocPreprocessor
            |> ObjectWithDescription.toDto mc
          IgnorePreprocessorErrors = domain.IgnorePreprocessorErrors
          AttributeNameToInfoMap =
            domain.AttributeNameToInfoMap
            |> AttributeNameToInfoMap.toDto mc
          GlobalOutputHandler =
            domain.GlobalOutputHandler
            |> ObjectWithDescription.toDto mc
          IgnoreOutputHandlerErrors = domain.IgnoreOutputHandlerErrors }

    let fromDto mc (dto: Dto.RuleSet) =
        let domain =
            RuleSetClass(
                Comments = dto.Comments,
                FKBVersion = dto.FKBVersion,
                ForInternalUseOnly = dto.ForInternalUseOnly,
                IsSwipingRule = dto.IsSwipingRule,
                OCRParameters = (dto.OCRParameters |> OCRParameters.fromDto),
                RunMode = (dto.RunMode |> RuleSetRunMode.fromDto),
                InsertAttributesUnderParent = dto.InsertAttributesUnderParent,
                InsertParentName = dto.InsertParentName,
                InsertParentValue = dto.InsertParentValue,
                DeepCopyInput = dto.DeepCopyInput,
                GlobalDocPreprocessor =
                    (dto.GlobalDocPreprocessor
                     |> ObjectWithDescription.fromDto mc),
                IgnorePreprocessorErrors = dto.IgnorePreprocessorErrors,
                AttributeNameToInfoMap =
                    (dto.AttributeNameToInfoMap
                     |> AttributeNameToInfoMap.fromDto mc),
                GlobalOutputHandler =
                    (dto.GlobalOutputHandler
                     |> ObjectWithDescription.fromDto mc),
                IgnoreOutputHandlerErrors = dto.IgnoreOutputHandlerErrors
            )

        RuleExecutionCounters.setFromDto domain dto.Counters
        domain

type RuleSetConverter() =
    inherit RuleObjectConverter<RuleSetClass, IRuleSet, Dto.RuleSet>()
    override _.toDto mc domain = domain |> RuleSet.toDto mc
    override _.fromDto mc dto = dto |> RuleSet.fromDto mc
