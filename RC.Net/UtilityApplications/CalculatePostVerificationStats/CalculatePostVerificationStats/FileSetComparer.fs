module CalculatePostVerificationStats.FileSetComparer

open System
open System.Collections.Generic

open UCLID_AFCORELib
open UCLID_COMUTILSLib

open AFUtils
open Database

type PerFileResult =
    { missed: ResizeArray<IAttribute>
      falsePositives: ResizeArray<IAttribute>
      expected: int
      flagged: bool }

type NumericResult =
    { files: int
      expected: int
      missed: int
      correct: int
      falsePositives: int
      flagged: int }
    static member empty =
        { files = 0
          expected = 0
          missed = 0
          correct = 0
          falsePositives = 0
          flagged = 0 }

    member x.flaggedPercent = (float x.flagged * 100. / float x.files)
    member x.recall = (float x.correct / float x.expected)

    member x.precision =
        (float x.correct
         / (float x.falsePositives + float x.correct))

type OverallResult =
    { rulesSummary: NumericResult
      rulesTable: string list list
      postVerifySummary: NumericResult
      postVerifyTable: string list list }
    static member empty =
        { rulesSummary = NumericResult.empty
          rulesTable = []
          postVerifySummary = NumericResult.empty
          postVerifyTable = [] }

    member x.totalFiles = x.rulesSummary.files
    member x.expected = x.rulesSummary.expected
    member x.flaggedForReview = x.rulesSummary.flagged
    member x.flaggedForReviewPercent = x.rulesSummary.flaggedPercent

    override x.ToString() =
        [ sprintf "Total Files: %d" x.totalFiles
          sprintf "Flagged for review: %d (%.1f%%)" x.flaggedForReview x.flaggedForReviewPercent
          sprintf "Expected: %d" x.expected
          sprintf "Missed: %d" x.rulesSummary.missed
          sprintf "Recall: %.3f" x.rulesSummary.recall
          sprintf "Precision: %.3f" x.rulesSummary.precision
          sprintf "Post-Verify Precision: %.3f" x.postVerifySummary.precision
          sprintf "Post-Verify Recall: %.3f" x.postVerifySummary.recall ]
        |> String.concat Environment.NewLine

type IFileSetComparer =
    abstract member CompareRedactions: images: IList<string> -> OverallResult

type FileSetComparer(db: IAttributeSource, dataQuery: string, flaggedQuery: string, parallelism: int) =

    let compareRedactions (images: IList<string>) =
        let results = Array.zeroCreate images.Count

        let opts =
            System.Threading.Tasks.ParallelOptions(MaxDegreeOfParallelism = parallelism)

        System.Threading.Tasks.Parallel.For(
            0,
            images.Count,
            opts,
            (fun i ->
                let image = images.[i]

                let exp =
                    db.getExpAttributes image
                    |> Option.map fst
                    |> Option.defaultWith (fun () ->
                        eprintfn "No expected data for %s" image
                        IUnknownVectorClass() :> IUnknownVector)

                let fnd =
                    db.getFndAttributes image
                    |> Option.map fst
                    |> Option.defaultWith (fun () ->
                        eprintfn "No found data for %s" image
                        IUnknownVectorClass() :> IUnknownVector)

                let comparer = Comparer.Comparer(image, exp, fnd, dataQuery, flaggedQuery)

                results.[i] <-
                    { missed = comparer.Missed
                      falsePositives = comparer.FalsePositives
                      expected = comparer.TotalExpected
                      flagged = comparer.IsFlaggedForVerification })
        )
        |> ignore

        results

    let combineResults (results: #seq<PerFileResult>) =
        let missed, falsePositives =
            results
            |> Seq.map (fun perFileResults -> perFileResults.missed, perFileResults.falsePositives)
            |> Seq.toList
            |> List.unzip

        let falsePositives = falsePositives |> Seq.collect id |> Seq.toUV

        let missed = missed |> Seq.collect id |> Seq.toUV
        missed, falsePositives

    let aggregate (results: #seq<NumericResult>) =
        results
        |> Seq.reduce (fun x y ->
            { x with
                files = x.files + y.files
                expected = x.expected + y.expected
                correct = x.correct + y.correct
                missed = x.missed + y.missed
                falsePositives = x.falsePositives + y.falsePositives
                flagged = x.flagged + y.flagged })

    let getNumericResult (perFileResult: PerFileResult) =
        { files = 1
          missed = perFileResult.missed.Count
          correct =
            (perFileResult.expected
             - perFileResult.missed.Count)
          falsePositives = perFileResult.falsePositives.Count
          expected = perFileResult.expected
          flagged = if perFileResult.flagged then 1 else 0 }

    // TODO: make these names more consistent...
    let header =
        [ "Expecteds"
          "Correct"
          "Miss"
          "False Positive"
          "Flagged" ]

    let getRow (perFileResult: NumericResult) =
        [ perFileResult.expected
          perFileResult.correct
          perFileResult.missed
          perFileResult.falsePositives
          perFileResult.flagged ]
        |> List.map (sprintf "%d")

    let convertToPostVerificationResult (result: NumericResult) =
        { result with
            missed =
                if result.flagged = 1 then
                    0
                else
                    result.missed
            correct =
                if result.flagged = 1 then
                    result.expected
                else
                    result.correct
            falsePositives =
                if result.flagged = 1 then
                    0
                else
                    result.falsePositives }

    let getPostVerificationTable (numericResults: NumericResult list) =
        let postVerificationResults =
            numericResults
            |> Seq.map convertToPostVerificationResult
            |> Seq.toList

        let table =
            header
            :: (postVerificationResults |> List.map getRow)

        let aggregate = postVerificationResults |> aggregate

        table, aggregate

    let getRulesAccuracyTable (numericResults: NumericResult list) =
        let numericResults = numericResults |> Seq.toList
        let table = header :: (numericResults |> List.map getRow)
        let aggregate = numericResults |> aggregate

        table, aggregate

    interface IFileSetComparer with
        member _.CompareRedactions(imageFiles) =
            let perFileResults =
                compareRedactions imageFiles
                |> Seq.map getNumericResult
                |> Seq.toList

            let rulesTable, rulesSummary = perFileResults |> getRulesAccuracyTable
            let verifyTable, verifySummary = perFileResults |> getPostVerificationTable

            { rulesSummary = rulesSummary
              rulesTable = rulesTable
              postVerifySummary = verifySummary
              postVerifyTable = verifyTable }
