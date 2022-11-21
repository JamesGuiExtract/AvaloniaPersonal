namespace Extract.CalculatePostVerificationStats.Test

open NUnit.Framework
open Swensen.Unquote

open Extract.Testing.Utilities
open UCLID_AFCORELib
open UCLID_COMUTILSLib
open UCLID_AFVALUEFINDERSLib
open UCLID_RASTERANDOCRMGMTLib

open CalculatePostVerificationStats
open Comparer
open AFUtils

[<AutoOpen>]
module TestUtils =
    /// Create attributes for regex matches
    let findWithRegex ussFilePath pattern =
        let doc = AFDocumentClass()
        doc.Text.LoadFrom(ussFilePath, false) |> ignore

        let regexRule = RegExprRuleClass(Pattern = pattern)
        regexRule.ParseText(doc, null)

    /// Update the name on the attributes and return the mutated collection
    let setAttributeName name (attributes: IUnknownVector) =
        for a in (attributes |> Seq.ofVOA) do
            a.Name <- name

        attributes

    /// Update the page number on the attributes and return the mutated collection
    /// Throws an exception if the current page number doesn't match the fromPage
    let changeAttributePageOrFail fromPage toPage (attributes: IUnknownVector) =
        for a in attributes |> Seq.ofVOA do
            if a.Value.GetFirstPageNumber() <> fromPage then
                failwithf "Unexpected page number! %d" fromPage

            a.Value.UpdatePageNumber(toPage)

        attributes

    /// Convert the attributes to hybrid mode without any OCR rotation and return the mutated collection
    let convertToHybridWithoutRotation (attributes: IUnknownVector) =
        for a in attributes |> Seq.ofVOA do
            let value = a.Value
            let pageInfos = LongToObjectMapClass()

            for pageNumber in value.SpatialPageInfos.GetKeys() |> Seq.ofVV do
                let orig = value.SpatialPageInfos.GetValue(pageNumber) :?> ISpatialPageInfo

                let width, height =
                    match orig.Orientation with
                    | EOrientation.kRotRight
                    | EOrientation.kRotLeft -> orig.Height, orig.Width
                    | _ -> orig.Width, orig.Height

                let info = SpatialPageInfoClass()
                info.Initialize(width, height, EOrientation.kRotNone, 0.)
                pageInfos.Set(pageNumber, info)

            value.CreateHybridString(value.GetOriginalImageRasterZones(), value.String, value.SourceDocName, pageInfos)

        attributes

// Automated Tests for the Comparer class
[<Category("CalculatePostVerificationStats"); Category("Automated")>]
type ``Test Comparer``() =

    static let DATA_QUERY = "/*/HCData|/*/MCData|/*/LCData|/*/Manual"
    static let FLAGGED_QUERY = "/*/HCData|/*/MCData|/*/LCData|/*/Clues"

    static let EXAMPLE05_WITH_ROTATED_PAGE_TIF_FILE = "Resources.Example05_WithRotatedPage.tif"

    static let testFiles = new TestFileManager<``Test Comparer``>()

    static let getUssPath imageResourceName =
        let sourceDocName = testFiles.GetFile(imageResourceName)
        testFiles.GetFile(imageResourceName + ".uss", sourceDocName + ".uss")

    [<OneTimeSetUp>]
    static member Setup() = GeneralMethods.TestSetup()

    [<OneTimeTearDown>]
    static member Teardown() = testFiles.Dispose()

    [<Test>]
    member _.``Comparing empty vectors is valid``() =
        // Arrange
        let expVOA = IUnknownVectorClass()
        let fndVOA = IUnknownVectorClass()

        // Act
        let comparer = Comparer("", expVOA, fndVOA, DATA_QUERY, FLAGGED_QUERY)

        // Assert
        test <@ comparer.TotalExpected = 0 @>
        test <@ comparer.FalsePositives.Count = 0 @>
        test <@ comparer.Missed.Count = 0 @>
        test <@ comparer.IsFlaggedForVerification = false @>

    [<Test>]
    member _.``Only the expected that match the data query are counted``() =
        // Arrange
        let expVOA =
            [ AttributeClass(Name = "HCData")
              AttributeClass(Name = "MCData")
              AttributeClass(Name = "LCData")
              AttributeClass(Name = "Clues")
              AttributeClass(Name = "OtherName") ]
            |> Seq.toUV

        let fndVOA = IUnknownVectorClass()

        // Act
        let comparer = Comparer("", expVOA, fndVOA, DATA_QUERY, FLAGGED_QUERY)

        // Assert
        test <@ comparer.TotalExpected = 3 @>
        test <@ comparer.Missed.Count = 3 @>
        test <@ comparer.IsFlaggedForVerification = false @>

    [<Test>]
    member _.``Only the found that match the data query are counted``() =
        // Arrange
        let expVOA = IUnknownVectorClass()

        let fndVOA =
            [ AttributeClass(Name = "HCData")
              AttributeClass(Name = "MCData")
              AttributeClass(Name = "LCData")
              AttributeClass(Name = "Clues")
              AttributeClass(Name = "OtherName") ]
            |> Seq.toUV

        // Act
        let comparer = Comparer("", expVOA, fndVOA, DATA_QUERY, FLAGGED_QUERY)

        // Assert
        test <@ comparer.TotalExpected = 0 @>
        test <@ comparer.FalsePositives.Count = 3 @>
        test <@ comparer.Missed.Count = 0 @>
        test <@ comparer.IsFlaggedForVerification = true @>

    [<Test>]
    member _.``When nothing is found, all expected are missed``() =
        // Arrange
        let expVOA =
            [ AttributeClass(Name = "HCData")
              AttributeClass(Name = "MCData") ]
            |> Seq.toUV

        let fndVOA = IUnknownVectorClass()

        // Act
        let comparer = Comparer("", expVOA, fndVOA, DATA_QUERY, FLAGGED_QUERY)

        // Assert
        test <@ comparer.TotalExpected = expVOA.Size() @>
        test <@ comparer.Missed.Count = expVOA.Size() @>
        test <@ comparer.IsFlaggedForVerification = false @>

    [<Test>]
    member _.``Considered flagged by attribute name when it is in the flagged query``() =
        // Arrange
        let flaggedQuery = "/*/FlagForVerification"
        let expVOA = IUnknownVectorClass()

        let fndVOA =
            [ AttributeClass(Name = "FlagForVerification") ]
            |> Seq.toUV

        // Act
        let comparer = Comparer("", expVOA, fndVOA, DATA_QUERY, flaggedQuery)

        // Assert
        test <@ comparer.IsFlaggedForVerification = true @>

    [<Test>]
    member _.``Not considered flagged by attribute name that isn't in the flagged query``() =
        // Arrange
        let flaggedQuery = "/*/FlagForVerification"
        let expVOA = IUnknownVectorClass()

        let fndVOA =
            [ AttributeClass(Name = "HCData")
              AttributeClass(Name = "MCData")
              AttributeClass(Name = "LCData")
              AttributeClass(Name = "Clues") ]
            |> Seq.toUV

        // Act
        let comparer = Comparer("", expVOA, fndVOA, DATA_QUERY, flaggedQuery)

        // Assert
        test <@ comparer.IsFlaggedForVerification = false @>

    [<Test>]
    member _.``Exact spatial match``() =
        // Arrange
        let sourceUSS = getUssPath EXAMPLE05_WITH_ROTATED_PAGE_TIF_FILE
        let pattern = """FOR\sA\sVALUABLE\sCONSIDERATION"""

        let expVOA =
            findWithRegex sourceUSS pattern
            |> setAttributeName "Manual"

        let fndVOA =
            findWithRegex sourceUSS pattern
            |> setAttributeName "HCData"

        // Act
        let comparer = Comparer("", expVOA, fndVOA, DATA_QUERY, FLAGGED_QUERY)

        // Assert
        test <@ comparer.TotalExpected = 1 @>
        test <@ comparer.FalsePositives.Count = 0 @>
        test <@ comparer.Missed.Count = 0 @>
        test <@ comparer.IsFlaggedForVerification = true @>

    [<Test>]
    member _.``80% spatial match is not a Missed redaction``() =
        // Arrange
        let sourceUSS = getUssPath EXAMPLE05_WITH_ROTATED_PAGE_TIF_FILE

        let expVOA =
            findWithRegex sourceUSS """FOR\sA\sVALUABLE\sCONSIDERATION"""
            |> setAttributeName "Manual"

        let fndVOA =
            findWithRegex sourceUSS """FOR\sA\sVALUABLE\sCONSIDER(?=ATION)"""
            |> setAttributeName "HCData"

        // Act
        let comparer = Comparer("", expVOA, fndVOA, DATA_QUERY, FLAGGED_QUERY)

        // Assert
        test <@ comparer.TotalExpected = 1 @>
        test <@ comparer.FalsePositives.Count = 0 @>
        test <@ comparer.Missed.Count = 0 @>
        test <@ comparer.IsFlaggedForVerification = true @>

    [<Test>]
    member _.``Not quite 80% spatial match is a Missed redaction``() =
        // Arrange
        let sourceUSS = getUssPath EXAMPLE05_WITH_ROTATED_PAGE_TIF_FILE

        let expVOA =
            findWithRegex sourceUSS """FOR\sA\sVALUABLE\sCONSIDERATION"""
            |> setAttributeName "Manual"

        let fndVOA =
            findWithRegex sourceUSS """FOR\sA\sVALUABLE\sCONSIDE(?=RATION)"""
            |> setAttributeName "HCData"

        // Act
        let comparer = Comparer("", expVOA, fndVOA, DATA_QUERY, FLAGGED_QUERY)

        // Assert
        test <@ comparer.TotalExpected = 1 @>
        test <@ comparer.FalsePositives.Count = 0 @>
        test <@ comparer.Missed.Count = 1 @>
        test <@ comparer.IsFlaggedForVerification = true @>

    [<Test>]
    member _.``If at least one line of a found item overlaps one line of an expected item by 10% it is not a FalsePositive``
        ()
        =
        // Arrange
        let sourceUSS = getUssPath EXAMPLE05_WITH_ROTATED_PAGE_TIF_FILE

        let expVOA =
            findWithRegex
                sourceUSS
                ("""This\sconveyance\sis\ssolely\sbetween\sspouses\sand\sestablishes\sthe\ssole\sand\sseparate\sproperty\sof\sa\sspouse\sand\sis\s+"""
                 + """EXEMPT\sfrom\sthe\simposition\sof\sthe\sDocu""")
            |> setAttributeName "Manual"

        let fndVOA =
            findWithRegex
                sourceUSS
                """Documentary\sTransfer\sTax\spursuant\sto\s11911\sof\sthe\sRevenue\sand\sTaxation\s+Code"""
            |> setAttributeName "HCData"

        // Act
        let comparer = Comparer("", expVOA, fndVOA, DATA_QUERY, FLAGGED_QUERY)

        // Assert
        test <@ comparer.TotalExpected = 1 @>
        test <@ comparer.FalsePositives.Count = 0 @>
        test <@ comparer.Missed.Count = 1 @>
        test <@ comparer.IsFlaggedForVerification = true @>

    [<Test>]
    member _.``If no line of a found item overlaps a line of an expected item by at least 10% it is a FalsePositive``
        ()
        =
        // Arrange
        let sourceUSS = getUssPath EXAMPLE05_WITH_ROTATED_PAGE_TIF_FILE

        let expVOA =
            findWithRegex
                sourceUSS
                ("""This\sconveyance\sis\ssolely\sbetween\sspouses\sand\sestablishes\sthe\ssole\sand\sseparate\sproperty\sof\sa\sspouse\sand\sis\s+"""
                 + """EXEMPT\sfrom\sthe\simposition\sof\sthe\sDoc""")
            |> setAttributeName "Manual"

        let fndVOA =
            findWithRegex
                sourceUSS
                """Documentary\sTransfer\sTax\spursuant\sto\s11911\sof\sthe\sRevenue\sand\sTaxation\s+Code"""
            |> setAttributeName "HCData"

        // Act
        let comparer = Comparer("", expVOA, fndVOA, DATA_QUERY, FLAGGED_QUERY)

        // Assert
        test <@ comparer.TotalExpected = 1 @>
        test <@ comparer.FalsePositives.Count = 1 @>
        test <@ comparer.Missed.Count = 1 @>
        test <@ comparer.IsFlaggedForVerification = true @>

    [<Test>]
    member _.``Exact spatial match on a different page is a Missed and a FalsePositive``() =
        // Arrange
        let sourceUSS = getUssPath EXAMPLE05_WITH_ROTATED_PAGE_TIF_FILE
        let pattern = """FOR\sA\sVALUABLE\sCONSIDERATION"""

        let expVOA =
            findWithRegex sourceUSS pattern
            |> setAttributeName "Manual"

        // Change the page from 1 to 3
        let fndVOA =
            findWithRegex sourceUSS pattern
            |> setAttributeName "HCData"
            |> changeAttributePageOrFail 1 3

        // Act
        let comparer = Comparer("", expVOA, fndVOA, DATA_QUERY, FLAGGED_QUERY)

        // Assert
        test <@ comparer.TotalExpected = 1 @>
        test <@ comparer.FalsePositives.Count = 1 @>
        test <@ comparer.Missed.Count = 1 @>
        test <@ comparer.IsFlaggedForVerification = true @>

    [<Test>]
    member _.``Exact spatial match on a rotated page is not a Missed redaction``() =
        // Arrange
        let sourceUSS = getUssPath EXAMPLE05_WITH_ROTATED_PAGE_TIF_FILE
        let pattern = """1004-1075450"""

        let expVOA =
            findWithRegex sourceUSS pattern
            |> setAttributeName "Manual"

        let fndVOA =
            findWithRegex sourceUSS pattern
            |> setAttributeName "HCData"

        // Act
        let comparer = Comparer("", expVOA, fndVOA, DATA_QUERY, FLAGGED_QUERY)

        // Assert
        test <@ comparer.TotalExpected = 3 @>
        test <@ comparer.FalsePositives.Count = 0 @>
        test <@ comparer.Missed.Count = 0 @>
        test <@ comparer.IsFlaggedForVerification = true @>

    [<Test>]
    member _.``Hybrid redaction without rotation information matches normal found redaction``() =
        // Arrange
        let sourceUSS = getUssPath EXAMPLE05_WITH_ROTATED_PAGE_TIF_FILE
        let pattern = """1004-1075450"""

        let expVOA =
            findWithRegex sourceUSS pattern
            |> convertToHybridWithoutRotation
            |> setAttributeName "Manual"

        let fndVOA =
            findWithRegex sourceUSS pattern
            |> setAttributeName "HCData"

        // Act
        let comparer = Comparer("", expVOA, fndVOA, DATA_QUERY, FLAGGED_QUERY)

        // Assert
        test <@ comparer.TotalExpected = 3 @>
        test <@ comparer.FalsePositives.Count = 0 @>
        test <@ comparer.Missed.Count = 0 @>
        test <@ comparer.IsFlaggedForVerification = true @>
