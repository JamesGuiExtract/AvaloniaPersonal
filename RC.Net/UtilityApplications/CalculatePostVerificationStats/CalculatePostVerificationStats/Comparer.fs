module CalculatePostVerificationStats.Comparer

open Extract.AttributeFinder
open Extract.Utilities.FSharp.RTree
open Extract.Utilities.FSharp.UnitsOfMeasure
open Extract.Utilities.FSharp.Utils
open UCLID_AFCORELib
open UCLID_COMUTILSLib
open UCLID_RASTERANDOCRMGMTLib

open AFUtils

//------------------------------------------------------------------------------------------------------
let private getRasterZones (attr: #IAttribute) : IRasterZone list =
    let value = attr.Value

    let res =
        if value.HasSpatialInfo() then
            let zones = value.GetOriginalImageRasterZones()
            zones |> List.ofUV<IRasterZone>
        else
            []

    res
//------------------------------------------------------------------------------------------------------

let private getEnvelopes (zones: IRasterZone list) : Envelope.TEnvelope list =
    zones
    |> List.map (fun zone ->
        let p = float zone.PageNumber

        let (x0, y0, x1, y1) = zone.GetRectangularBounds(null).GetBounds()

        { p0 = p
          p1 = p
          x0 = float x0
          x1 = float x1
          y0 = float y0
          y1 = float y1 })
//------------------------------------------------------------------------------------------------------

let private buildRedactionLookup (redactions: IUnknownVector) (xpathQueryForData: string) : IAttribute array =
    let context = XPathContext(redactions)

    context.FindAllOfType<IAttribute>(xpathQueryForData)
    |> Seq.toArray

let private buildZonesLookup (redactions: IAttribute array) : IRasterZone list array =
    redactions |> Array.map getRasterZones

let private buildEnvelopesLookup (redactions: IRasterZone list array) : Envelope.TEnvelope list array =
    redactions |> Array.map getEnvelopes

let private buildRTree (redactions: Envelope.TEnvelope list array) : int RTree.TRTree =
    let data =
        redactions
        |> Seq.indexed
        |> Seq.collect (fun (ident, envelopes) ->
            envelopes
            |> Seq.map (fun envelope -> envelope, ident))

    RTree.bulkLoad data

type RedactionLookup
    (
        sourceDocName: string,
        redactions: IUnknownVector,
        xpathQueryForData: string,
        ?xpathQueryForFlagged: string
    ) =
    do AttributeMethods.UpdateSourceDocNameOfAttributes(redactions, sourceDocName)

    let redactionLookup = buildRedactionLookup redactions xpathQueryForData

    let zonesLookup = buildZonesLookup redactionLookup
    let envelopesLookup = buildEnvelopesLookup zonesLookup
    let envelopeRTree = buildRTree envelopesLookup

    let isFlagged =
        xpathQueryForFlagged
        |> Option.map (fun q ->
            let context = XPathContext(redactions)

            context.FindAllOfType<IAttribute>(q)
            |> Seq.isEmpty
            |> not)
        |> Option.defaultValue false

    member _.RedactionLookup: IAttribute array = redactionLookup
    member _.ZonesLookup: IRasterZone list array = zonesLookup
    member _.EnvelopesLookup: Envelope.TEnvelope list array = envelopesLookup
    member _.EnvelopeRTree: int RTree.TRTree = envelopeRTree
    member _.IsFlagged: bool = isFlagged

// Get the index pairs of overlapping redactions, using the overall bounds
let private getOverlapping (lookup1: RedactionLookup) (lookup2: RedactionLookup) : (int * int) list =
    lookup1.EnvelopesLookup
    |> Seq.indexed
    |> Seq.collect (fun (ident1, envelopes) ->
        envelopes
        |> Seq.collect (RTree.find lookup2.EnvelopeRTree)
        |> Seq.map (fun ident2 -> ident1, ident2))
    |> Seq.distinct
    |> Seq.toList

// Get the total area of a redaction by summing the area of its zones
let private getArea (zones: IRasterZone list) =
    zones |> Seq.sumBy (fun zone -> float zone.Area)

// Whether the total overlap by zones of the needsCover redaction and the coverWith redaction overlap by the specified minOverlap percent
// (of the area of the needsCover redaction)
let private isCoveredBy (needsCover: IRasterZone list) (minOverlap: float<percent>) (coverWith: IRasterZone list) =
    let totalOverlap =
        needsCover
        |> List.sumBy (fun zone1 ->
            coverWith
            |> List.sumBy (fun zone2 -> zone1.GetAreaOverlappingWith(zone2 :?> RasterZone)))

    let overlapPercent =
        totalOverlap / (getArea needsCover)
        * 100.<percent>

    overlapPercent >= minOverlap

// Get the indexes of redactions that are covered by the specified minOverlap percent
let private getCoveredBy (needsCover: RedactionLookup) (minOverlap: float<percent>) (coverWith: RedactionLookup) =
    let overlapping = getOverlapping needsCover coverWith

    overlapping
    |> List.filter (fun (needsCoverIdent, coverWithIdent) ->
        isCoveredBy needsCover.ZonesLookup.[needsCoverIdent] minOverlap coverWith.ZonesLookup.[coverWithIdent])
    |> List.map fst

// Whether at least one zone of the needsCover redaction overlaps a zone of the coverWith redaction by the specified minOverlap percent (of the smaller zone)
let private hasOverlappingZone
    (needsCover: IRasterZone list)
    (minOverlap: float<percent>)
    (coverWith: IRasterZone list)
    =
    needsCover
    |> Seq.filter (fun zone1 ->
        let area1 = zone1.Area

        coverWith
        |> Seq.filter (fun zone2 ->
            let minArea = float (min area1 zone2.Area)
            let overlap = float (zone1.GetAreaOverlappingWith(zone2 :?> RasterZone))
            let overlapPercent = overlap / minArea * 100.<percent>
            overlapPercent >= minOverlap)
        |> (not << Seq.isEmpty))
    |> (not << Seq.isEmpty)


// Get the indexes of needsCover redactions where at least one zone of the needsCover redaction overlaps
// a zone of a coverWith redaction by the specified minOverlap percent (of the smaller zone)
// This is the same false positive criteria as used by the legacy RedactionTester and the RedactionAccuracy ETL
let private getOverlappingByZone
    (needsCover: RedactionLookup)
    (minOverlap: float<percent>)
    (coverWith: RedactionLookup)
    =
    let overlapping = getOverlapping needsCover coverWith

    overlapping
    |> List.filter (fun (needsCoverIdent, coverWithIdent) ->
        hasOverlappingZone needsCover.ZonesLookup.[needsCoverIdent] minOverlap coverWith.ZonesLookup.[coverWithIdent])
    |> List.map fst

type Comparer
    (
        sourceDocName: string,
        expectedRedactions: IUnknownVector,
        foundRedactions: IUnknownVector,
        xpathQueryForData: string,
        xpathQueryForFlagged: string
    ) =
    let expectedLookup =
        RedactionLookup(sourceDocName, expectedRedactions, xpathQueryForData)

    let foundLookup =
        RedactionLookup(sourceDocName, foundRedactions, xpathQueryForData, xpathQueryForFlagged)

    let correctlyRedactedIdents =
        getCoveredBy expectedLookup 80.<percent> foundLookup
        |> Set

    let missed =
        expectedLookup.RedactionLookup
        |> Seq.indexed
        |> Seq.choose (fun (ident, redaction) ->
            if correctlyRedactedIdents |> Set.contains ident then
                None
            else
                Some redaction)
        |> ResizeArray

    let notFalsePositives =
        getOverlappingByZone foundLookup 10.<percent> expectedLookup
        |> Set

    let falsePositives =
        foundLookup.RedactionLookup
        |> Seq.indexed
        |> Seq.choose (fun (ident, redaction) ->
            if notFalsePositives |> Set.contains ident then
                None
            else
                Some redaction)
        |> ResizeArray

    member _.Missed = missed
    member _.FalsePositives = falsePositives
    member _.TotalExpected = expectedLookup.RedactionLookup.Length
    member _.IsFlaggedForVerification = foundLookup.IsFlagged
