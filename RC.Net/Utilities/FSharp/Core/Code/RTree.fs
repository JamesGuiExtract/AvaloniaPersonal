namespace Extract.Utilities.FSharp.RTree

open System

open Extract.Utilities.FSharp.UnitsOfMeasure
open System.Collections.Generic
open Extract.Utilities.FSharp.Utils

module Envelope =
    type TEnvelope =
        { p0: float
          p1: float
          x0: float
          x1: float
          y0: float
          y1: float }

    // Use max short for max width or height so that area calculations can still be performed
    // (if Double.MaxValue is used then calculations could overflow)
    let maxValue = float Int16.MaxValue

    let rangesIntersect a b a' b' : bool = a' <= b && a <= b'

    let intersects e e' : bool =
        // For two envelopes to intersect, all of their ranges intersect
        rangesIntersect e.p0 e.p1 e'.p0 e'.p1
        && rangesIntersect e.x0 e.x1 e'.x0 e'.x1
        && rangesIntersect e.y0 e.y1 e'.y0 e'.y1

    let add (e: TEnvelope) (e': TEnvelope) : TEnvelope =
        { p0 = min e.p0 e'.p0
          p1 = max e.p1 e'.p1
          x0 = min e.x0 e'.x0
          x1 = max e.x1 e'.x1
          y0 = min e.y0 e'.y0
          y1 = max e.y1 e'.y1 }

    let addMany (envelopes: #seq<TEnvelope>) : TEnvelope =
        if envelopes |> Seq.isEmpty then
            raise (ArgumentException "Can't add zero envelopes")
        else
            envelopes |> Seq.reduce add

    let area (e: TEnvelope) : float =
        (e.p1 - e.p0 + 1.)
        * (e.x1 - e.x0 + 1.)
        * (e.y1 - e.y0 + 1.)

    let within (e: TEnvelope) (e': TEnvelope) : bool =
        e.p0 <= e'.p0
        && e.p1 >= e'.p1
        && e.x0 <= e'.x0
        && e.x1 >= e'.x1
        && e.y0 <= e'.y0
        && e.y1 >= e'.y1

    let empty: TEnvelope =
        { p0 = 0.
          p1 = 0.
          x0 = 0.
          x1 = 0.
          y0 = 0.
          y1 = 0. }

    let isEmpty: TEnvelope -> bool =
        function
        | e when
            e.p0 = 0. && e.p1 = 0.
            || e.x0 = 0. && e.x1 = 0.
            || e.y0 = 0. && e.y1 = 0.
            ->
            true
        | _ -> false

    let intersect (e: TEnvelope) (e': TEnvelope) : TEnvelope =
        let intersecting =
            max e.p0 e'.p0, min e.p1 e'.p1, max e.x0 e'.x0, min e.x1 e'.x1, max e.y0 e'.y0, min e.y1 e'.y1

        let p0, p1, x0, x1, y0, y1 = intersecting

        if p0 <= p1 && x0 < x1 && y0 < y1 then
            { p0 = p0
              p1 = p1
              x0 = x0
              x1 = x1
              y0 = y0
              y1 = y1 }
        else
            empty

module RTree =
    type TRTree<'a> =
        | Node of (Envelope.TEnvelope * TRTree<'a>) list
        | Leaf of (Envelope.TEnvelope * 'a) list
        | Empty

    let maxNodeLoad = 8
    let minEntries = 2
    let emptyNode = (Envelope.empty, Empty)

    let enlargementNeeded e e' : float =
        (Envelope.add e e' |> Envelope.area)
        - Envelope.area e

    let rec partitionByMinEnlargement e =
        function
        | (e', _) as n :: [] -> n, [], enlargementNeeded e e'
        | (e', _) as n :: ns ->
            let enlargement = enlargementNeeded e e'
            let min, maxs, enlargement' = partitionByMinEnlargement e ns

            if enlargement < enlargement' then
                n, min :: maxs, enlargement
            else
                min, n :: maxs, enlargement'
        | [] -> raise (ArgumentException "cannot partition an empty node")

    // cross product
    let pairsOfList xs =
        xs
        |> List.collect (fun x -> xs |> List.map (fun y -> (x, y)))

    // This is Guttman's quadradic splitting algorithm
    let splitPickSeeds ns =
        let pairs = pairsOfList ns

        let cost (e0, _) (e1, _) =
            (Envelope.area (Envelope.add e0 e1))
            - (Envelope.area e0)
            - (Envelope.area e1)

        let rec maxCost =
            function
            | [ (n, n') ] -> cost n n', (n, n')
            | (n, n') as pair :: ns ->
                let maxCost', pair' = maxCost ns
                let cost = cost n n'

                if cost > maxCost' then
                    cost, pair
                else
                    maxCost', pair'
            | [] -> raise (ArgumentException "can't compute split on empty list")

        let (_, groups) = maxCost pairs in
        groups

    let splitPickNext e0 e1 ns =
        let diff (e, _) =
            abs (
                (enlargementNeeded e0 e)
                - (enlargementNeeded e1 e)
            )

        let rec maxDifference =
            function
            | [ n ] -> diff n, n
            | n :: ns ->
                let diff', n' = maxDifference ns
                let diff = diff n

                if diff > diff' then
                    diff, n
                else
                    diff', n'
            | [] -> raise (ArgumentException "can't compute max diff on empty list")

        let (_, n) = maxDifference ns in
        n

    let splitNodes ns =
        let rec partition xs xsEnvelope ys ysEnvelope =
            function
            | [] -> (xs, xsEnvelope), (ys, ysEnvelope)
            | rest ->
                let (e, _) as n = splitPickNext xsEnvelope ysEnvelope rest
                let rest' = List.filter ((<>) n) rest
                let enlargementX = enlargementNeeded e xsEnvelope
                let enlargementY = enlargementNeeded e ysEnvelope

                if enlargementX < enlargementY then
                    partition (n :: xs) (Envelope.add xsEnvelope e) ys ysEnvelope rest'
                else
                    partition xs xsEnvelope (n :: ys) (Envelope.add ysEnvelope e) rest'

        let (((e0, _) as n0), ((e1, _) as n1)) = splitPickSeeds ns
        partition [ n0 ] e0 [ n1 ] e1 (List.filter (fun n -> n <> n0 && n <> n1) ns)

    let envelopeOfNodes ns =
        Envelope.addMany (List.map (fun (e, _) -> e) ns)

    let rec insert' elem e =
        function
        | Node ns ->
            let (_, min), maxs, _ = partitionByMinEnlargement e ns

            match insert' elem e min with
            | min', (_, Empty) ->
                let ns' = min' :: maxs
                let e' = envelopeOfNodes ns'
                (e', Node ns'), emptyNode
            | min', min'' when (List.length maxs + 2) < maxNodeLoad ->
                let ns' = min' :: min'' :: maxs
                let e' = envelopeOfNodes ns'
                (e', Node ns'), emptyNode
            | min', min'' ->
                let (a, envelopeA), (b, envelopeB) = splitNodes (min' :: min'' :: maxs)
                (envelopeA, Node a), (envelopeB, Node b)
        | Leaf es ->
            let es' = (e, elem) :: es

            if List.length es' > maxNodeLoad then
                let (a, envelopeA), (b, envelopeB) = splitNodes es'
                (envelopeA, Leaf a), (envelopeB, Leaf b)
            else
                (envelopeOfNodes es', Leaf es'), emptyNode
        | Empty -> (e, Leaf [ e, elem ]), emptyNode

    let insert t elem e =
        match insert' elem e t with
        | (_, a), (_, Empty) -> a
        | a, b -> Node [ a; b ] // root split


    //--------------------------------------------------------------------------------
    // Bulk load
    // OMT: Overlap Minimizing Top-down algorithm
    // http://ceur-ws.org/Vol-74/files/FORUM_18.pdf
    //--------------------------------------------------------------------------------

    // Primary sort: sort by minimum page and minimum x coordinate
    // tuple comparison would work here but it is much slower for some reason
    let inline private getMinPX (e: Envelope.TEnvelope, _) = e.p0, e.x0

    let inline private compareMinPX a b =
        let ap, ax = getMinPX a
        let bp, bx = getMinPX b

        if ap < bp then -1
        elif ap > bp then 1
        elif ax < bx then -1
        elif ax > bx then 1
        else 0

    // IComparer that sorts by minimum page and minimum x coordinate
    type CompareMinPX<'a>() =
        interface IComparer<Envelope.TEnvelope * 'a> with
            member x.Compare(a, b) = compareMinPX a b

    // Secondary sort: sort by minimum y coordinate
    let inline private getMinY (e: Envelope.TEnvelope, _) = e.y0

    let inline private compareMinY e e' =
        let y = getMinY e
        let y' = getMinY e'

        if y < y' then -1
        elif y' < y then 1
        else 0

    // LOGm(n) where n is the number of nodes in the tree and m is the maximum children per node
    let private getDepth numNodes =
        int (ceil ((log (float numNodes)) / (log (float maxNodeLoad))))

    // Calculate the envelope for a tree
    let private getEnvelope =
        function
        | Node ns -> ns |> List.map fst |> Envelope.addMany
        | Leaf es -> es |> List.map fst |> Envelope.addMany
        | Empty -> Envelope.empty

    // Build a subtree
    let rec private buildNodes height maxEntries (data: ResizeArray<Envelope.TEnvelope * 'a>) =
        let num = data.Count

        if num <= maxEntries then
            if height = 1 then
                Leaf(data |> Seq.toList)
            else
                let envelope = data |> Seq.map fst |> Envelope.addMany
                Node [ (envelope, (buildNodes (height - 1) maxNodeLoad data)) ]
        else
            data.Sort(CompareMinPX())
            let nodeSize = (num + maxEntries - 1) / maxEntries

            let subSortLength = nodeSize * int (ceil (sqrt (float maxEntries)))

            let children =
                data
                |> Seq.groupsOfAtMost subSortLength
                |> Seq.collect (fun verticalSlice ->
                    verticalSlice
                    |> Seq.sortWith compareMinY
                    |> Seq.groupsOfAtMost nodeSize
                    |> Seq.map (fun subSeq -> buildNodes (height - 1) maxNodeLoad (ResizeArray subSeq)))

            Node(
                children
                |> Seq.map (fun t -> getEnvelope t, t)
                |> Seq.toList
            )

    // Build the tree
    let private buildTree (data: ResizeArray<Envelope.TEnvelope * 'a>) =
        let treeHeight = getDepth data.Count

        let rootMaxEntries =
            int (
                ceil (
                    (float data.Count)
                    / pown (float maxNodeLoad) (treeHeight - 1)
                )
            )

        buildNodes treeHeight rootMaxEntries data

    /// Build an r-tree using the OMT bulk-loading algorithm
    let bulkLoad (data: seq<Envelope.TEnvelope * 'a>) =
        let data = ResizeArray(data)

        if data.Count < minEntries then
            Seq.fold (fun t (e, elem) -> insert t elem e) Empty data
        else
            buildTree data

    //--------------------------------------------------------------------------------
    // Query
    //--------------------------------------------------------------------------------
    let filterIntersecting e =
        List.filter (fun (e', _) -> Envelope.intersects e e')

    let rec finde t e =
        match t with
        | Node ns ->
            let intersecting = filterIntersecting e ns

            let found = List.map (fun (_, n) -> finde n e) intersecting

            List.concat found
        | Leaf es -> filterIntersecting e es
        | Empty -> []

    let findContained t e =
        finde t e
        |> List.filter (fun (e', _) -> e' |> Envelope.within e)
        |> List.map snd

    /// Find values and envelopes where the area of the envelope intersection is at least minOverlap
    /// of the envelope of the item in the tree
    let findeWithMinOverlap t (minOverlap: float<percent>) e =
        finde t e
        |> List.filter (fun (e', _) ->
            let area' = e' |> Envelope.area
            let reqOverlap = (float minOverlap / 100.) * area'
            let overlap = Envelope.intersect e e' |> Envelope.area
            overlap >= reqOverlap)

    // get a tuple of long-dimension * short-dimension of an envelope
    let private getDims (e: Envelope.TEnvelope) =
        let w = e.x1 - e.x0
        let h = e.y1 - e.y0
        if w > h then w, h else h, w

    /// Find values and envelopes where the dimensions of the envelope intersection are at least the specified
    /// minimum percents of the dimensions of the envelope of the item in the tree
    let findeWithMinOverlaps t (minLongDimOverlap: float<percent>) (minShortDimOverlap: float<percent>) e =
        finde t e
        |> List.filter (fun (e', _) ->
            let longDim', shortDim' = getDims e'
            let reqLongOverlap = (float minLongDimOverlap / 100.) * longDim'
            let reqShortOverlap = (float minShortDimOverlap / 100.) * shortDim'
            let intersect = Envelope.intersect e e'
            let longOverlap, shortOverlap = getDims intersect

            longOverlap >= reqLongOverlap
            && shortOverlap >= reqShortOverlap)

    type OverlapAndMaxSizeSpec =
        { minLongDimOverlap: float<percent>
          maxLongDimFactor: float<percent>
          minShortDimOverlap: float<percent>
          maxShortDimFactor: float<percent> }

    /// Find values and envelopes where the dimensions of the envelope intersection are at least the specified
    /// minimum percents of the dimensions of the envelope of the item in the tree
    /// and where the dimensions of the envelope to search for are no more than the specified percents of the
    /// combined covered areas
    let findeWithMinOverlapsAndMaxSizes t (spec: OverlapAndMaxSizeSpec) e =
        let overlapping =
            findeWithMinOverlaps t spec.minLongDimOverlap spec.minShortDimOverlap e

        if overlapping |> List.isEmpty then
            List.empty
        else
            let longDim, shortDim = getDims e
            let coveredArea = overlapping |> List.map fst |> Envelope.addMany
            let longDim', shortDim' = getDims coveredArea

            if longDim'
               <= (longDim * (float spec.maxLongDimFactor / 100.))
               && shortDim'
                  <= (shortDim * (float spec.maxShortDimFactor / 100.)) then
                overlapping
            else
                List.empty

    /// Find values where the area of the envelope intersection is at least minOverlap
    /// of the envelope of the item in the tree
    let findWithMinOverlap t (minOverlap: float<percent>) e =
        findeWithMinOverlap t minOverlap e |> List.map snd

    /// Has a value where the area of the envelope intersection is at least minOverlap
    /// of the envelope of the item in the tree
    let hasMatchWithMinOverlap t (minOverlap: float<percent>) e =
        e
        |> findeWithMinOverlap t minOverlap
        |> (not << List.isEmpty)

    /// Has a value where the dimensions of the envelope intersection are at least the specified
    /// minimum percents of the dimensions of the envelope of the item in the tree
    let hasMatchWithMinOverlaps t (minLongDimOverlap: float<percent>) (minShortDimOverlap: float<percent>) e =
        e
        |> findeWithMinOverlaps t minLongDimOverlap minShortDimOverlap
        |> (not << List.isEmpty)

    /// Has a value where the dimensions of the envelope intersection are at least the specified
    /// minimum percents of the dimensions of the envelope of the item in the tree
    /// and where the dimensions of the envelope to search for are no more than the specified percents of the
    /// combined covered areas
    let HasMatchWithMinOverlapsAndMaxSizes t (spec: OverlapAndMaxSizeSpec) e =
        e
        |> findeWithMinOverlapsAndMaxSizes t spec
        |> (not << List.isEmpty)


    let find t e = finde t e |> List.map snd

    let hasMatchContained t e =
        e |> findContained t |> (not << List.isEmpty)

    let hasMatch t e = e |> finde t |> (not << List.isEmpty)

    let rec size =
        function
        | Node ns ->
            let subSizes = List.map (fun (_, n) -> size n) ns
            List.sum subSizes
        | Leaf es -> List.length es
        | Empty -> 0
