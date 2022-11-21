namespace Extract.CalculatePostVerificationStats.Test

open NUnit.Framework
open Swensen.Unquote
open System
open System.IO

open Extract.Testing.Utilities
open Extract.Utilities
open UCLID_COMUTILSLib

open CalculatePostVerificationStats
open AFUtils

// Automated Tests for the cmdline application
[<Category("CalculatePostVerificationStats"); Category("Automated")>]
type ``Test App``() =

    let imageFiles = [ "one.tif"; "two.tif"; "three.tif" ]

    // Track the use of mocked dependencies
    let getFilesCompleteForActionsWasCalled = ref false
    let getFirstFilesCompleteForActionsWasCalled = ref None
    let dbInfoForDocumentSource = ref None
    let dbInfoForComparer = ref None
    let dataQueryForComparer = ref None
    let flaggedQueryForComparer = ref None
    let parallelismForComparer = ref None
    let imagesForComparer = ref None

    // Setup some mocked dependencies to test that the app is parsing the arguments and calling the proper functions
    let documentSourceMock =
        { new Database.IDocumentSource with
            member _.getFilesCompleteForActions() =
                getFilesCompleteForActionsWasCalled.Value <- true
                imageFiles |> ResizeArray<_>

            member _.getFirstFilesCompleteForActions(limit) =
                getFirstFilesCompleteForActionsWasCalled.Value <- Some limit
                imageFiles |> Seq.truncate limit |> ResizeArray<_> }

    let attributeSourceMock =
        { new Database.IAttributeSource with
            member _.getExpAttributes(_) =
                Some(IUnknownVectorClass(), DateTime.Now)

            member _.getFndAttributes(_) =
                Some(IUnknownVectorClass(), DateTime.Now) }


    let documentSourceFactory info =
        dbInfoForDocumentSource.Value <- Some info
        documentSourceMock

    let attributeSourceFactory info =
        dbInfoForComparer.Value <- Some info
        attributeSourceMock


    let fileSetComparerMock =
        { new FileSetComparer.IFileSetComparer with
            member _.CompareRedactions(images) =
                imagesForComparer.Value <- images |> Seq.toList |> Some

                { FileSetComparer.OverallResult.empty with
                    rulesSummary = { FileSetComparer.NumericResult.empty with files = images.Count }
                    rulesTable = [ [ "Fake"; "Table" ] ] } }

    let comparerFactory info dataQuery flaggedQuery parallelism =
        dbInfoForComparer.Value <- Some info
        dataQueryForComparer.Value <- Some dataQuery
        flaggedQueryForComparer.Value <- Some flaggedQuery
        parallelismForComparer.Value <- Some parallelism
        fileSetComparerMock

    [<OneTimeSetUp>]
    static member Setup() = GeneralMethods.TestSetup()

    [<OneTimeTearDown>]
    static member Teardown() = ()

    [<SetUp>]
    member _.PerTestSetup() =
        // Reset the vars that track the use of mocked dependencies
        getFilesCompleteForActionsWasCalled.Value <- false
        getFirstFilesCompleteForActionsWasCalled.Value <- None
        dbInfoForDocumentSource.Value <- None
        dbInfoForComparer.Value <- None
        dataQueryForComparer.Value <- None
        flaggedQueryForComparer.Value <- None
        parallelismForComparer.Value <- None
        imagesForComparer.Value <- None

    [<Test>]
    member _.``Confirm that required and default arguments are used``() =
        // Arrange
        let args =
            [| "--database"
               "MyServer"
               "MyDB"
               "--input-action"
               "A20_Verify"
               "--expected-attribute-set"
               "ExpAttr"
               "--found-attribute-set"
               "FndAttr" |]

        // Act
        let output, exitCode = App.run documentSourceFactory comparerFactory None args

        printfn "%s" output

        // Assert
        test <@ exitCode = 0 @>
        test <@ dbInfoForComparer.Value = dbInfoForDocumentSource.Value @>
        test <@ getFilesCompleteForActionsWasCalled.Value = true @>
        test <@ getFirstFilesCompleteForActionsWasCalled.Value = None @>

        test
            <@ dbInfoForComparer.Value = Some
                                             { Server = "MyServer"
                                               Name = "MyDB"
                                               VerifyActions = [ "A20_Verify" ]
                                               ExpAttributeSet = "ExpAttr"
                                               FndAttributeSet = "FndAttr" } @>

        test <@ dataQueryForComparer.Value = Some "/*/HCData|/*/MCData|/*/LCData|/*/Manual" @>
        test <@ flaggedQueryForComparer.Value = Some "/*/HCData|/*/MCData|/*/LCData|/*/Clues" @>
        test <@ parallelismForComparer.Value = Some 8 @>
        test <@ imagesForComparer.Value = Some imageFiles @>

    [<Test>]
    member _.``Confirm that non-default arguments are used``() =
        // Arrange
        let args =
            [| "--database"
               "MyServer"
               "MyDB"
               "--input-action"
               "A20_Verify"
               "--expected-attribute-set"
               "ExpAttr"
               "--found-attribute-set"
               "FndAttr"
               "--flagged-query"
               "/*/CustomFlaggedName"
               "--data-query"
               "/*/CustomDataName"
               "--parallel-degree"
               "1"
               "--limit-to-first"
               "2" |]

        // Act
        let output, exitCode = App.run documentSourceFactory comparerFactory None args

        printfn "%s" output

        // Assert
        test <@ exitCode = 0 @>
        test <@ dbInfoForComparer.Value = dbInfoForDocumentSource.Value @>
        test <@ getFilesCompleteForActionsWasCalled.Value = false @>
        test <@ getFirstFilesCompleteForActionsWasCalled.Value = Some 2 @>

        test
            <@ dbInfoForComparer.Value = Some
                                             { Server = "MyServer"
                                               Name = "MyDB"
                                               VerifyActions = [ "A20_Verify" ]
                                               ExpAttributeSet = "ExpAttr"
                                               FndAttributeSet = "FndAttr" } @>

        test <@ dataQueryForComparer.Value = Some "/*/CustomDataName" @>
        test <@ flaggedQueryForComparer.Value = Some "/*/CustomFlaggedName" @>
        test <@ parallelismForComparer.Value = Some 1 @>
        test <@ imagesForComparer.Value = Some(imageFiles |> List.truncate 2) @>

    [<Test>]
    member _.``Confirm contents of output and output file``() =
        // Arrange
        use outputFile = new TemporaryFile(false)

        let expectedOutput =
            """Total Files: 3
Flagged for review: 0 (0.0%)
Expected: 0
Missed: 0
Recall: NaN
Precision: NaN
Post-Verify Precision: NaN
Post-Verify Recall: NaN
Total seconds: 0"""

        let expectedCsv = "Fake, Table\r\n"

        let args =
            [| "--database"
               "MyServer"
               "MyDB"
               "--input-action"
               "A20_Verify"
               "--expected-attribute-set"
               "ExpAttr"
               "--found-attribute-set"
               "FndAttr"
               "--output-file"
               outputFile.FileName |]

        // Act
        let output, exitCode = App.run documentSourceFactory comparerFactory None args

        printfn "%s" output

        // Assert
        test <@ exitCode = 0 @>
        test <@ output = expectedOutput @>
        test <@ File.ReadAllText(outputFile.FileName) = expectedCsv @>
