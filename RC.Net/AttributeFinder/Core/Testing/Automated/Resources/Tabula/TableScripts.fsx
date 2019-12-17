#I @"C:\Engineering\Binaries\Debug"
#I @"C:\Program Files (x86)\Extract Systems\CommonComponents"
#r "Extract.AttributeFinder.Tabula.dll"
#r "Extract.Imaging.dll"
#r "Tabula.IKVM.exe"
#r "IKVM.OpenJDK.Core.dll"
#r "Interop.UCLID_AFCORELib.dll"
#r "Interop.UCLID_COMUTILSLib.dll"
#r "Interop.UCLID_RASTERANDOCRMGMTLib.dll"

open System
open UCLID_AFCORELib
open UCLID_RASTERANDOCRMGMTLib
open System.Collections.Generic

type MaybeBuilder() =
  member this.Bind(m, f) = Option.bind f m
  member this.Return(x) = Some x
  member this.Zero() = None

let maybe = new MaybeBuilder()

let memoize f =
  let cache = Dictionary<_, _>()
  fun x ->
    match cache.TryGetValue x with
    | found, res when found -> res
    | _ ->
      let res = f x
      cache.[x] <- res
      res

(*
  __  __           _       _         
 |  \/  |         | |     | |        
 | \  / | ___   __| |_   _| | ___    
 | |\/| |/ _ \ / _` | | | | |/ _ \   
 | |  | | (_) | (_| | |_| | |  __/   
 |_|  |_|\___/ \__,_|\__,_|_|\___|   
  ______ ______ ______ ______ ______ 
 |______|______|______|______|______|
*)
module Seq =
  open UCLID_COMUTILSLib
  open System.Collections

  let ofUV<'t> (uv : IIUnknownVector) =
    seq { for i in 0 .. (uv.Size()-1) -> uv.At(i)} |> Seq.cast<'t>

  let ofVOA = ofUV<IAttribute>

  let toGenericList<'t> (x : 't seq) =
    Generic.List<'t>(x)

  let toUV (x : 'a seq when 'a :> obj) =
      let uv = IUnknownVectorClass()
      x |> Seq.iter (fun li -> uv.PushBack li)
      uv

(*
  __  __           _       _         
 |  \/  |         | |     | |        
 | \  / | ___   __| |_   _| | ___    
 | |\/| |/ _ \ / _` | | | | |/ _ \   
 | |  | | (_) | (_| | |_| | |  __/   
 |_|  |_|\___/ \__,_|\__,_|_|\___|   
  ______ ______ ______ ______ ______ 
 |______|______|______|______|______|
*)
module Regex =
  open System.Text.RegularExpressions

  let replace pat rep inp =
    Regex.Replace(input=inp, pattern=pat, replacement=rep)

  let isMatch pat inp =
    Regex.IsMatch(input=inp, pattern=pat)

  let countMatches pat inp =
    Regex.Matches(input=inp, pattern=pat).Count

(*
  __  __           _       _         
 |  \/  |         | |     | |        
 | \  / | ___   __| |_   _| | ___    
 | |\/| |/ _ \ / _` | | | | |/ _ \   
 | |  | | (_) | (_| | |_| | |  __/   
 |_|  |_|\___/ \__,_|\__,_|_|\___|   
  ______ ______ ______ ______ ______ 
 |______|______|______|______|______|
*)
module AFDoc =
  open Extract.AttributeFinder.Tabula

  // Active pattern for getting first page, ignoring __EMPTYPAGE__
  let private (|Empty|FirstAvailablePage|) (text: SpatialString) =
    if text.HasSpatialInfo () && not <| text.String.Equals("___EMPTYPAGE___", StringComparison.Ordinal)
    then
      let firstPageNumber = text.GetFirstPageNumber ()
      FirstAvailablePage firstPageNumber
    else Empty

  let getFirstAvailablePage (doc: AFDocument): SpatialString option =
    let text = doc.Text
    match text with
    | FirstAvailablePage pageNumber -> Some (text.GetSpecifiedPages (pageNumber, pageNumber))
    | Empty -> None

 (*__  __         _      _     
  |  \/  |___  __| |_  _| |___ 
  | |\/| / _ \/ _` | || | / -_)
  |_|  |_\___/\__,_|\_,_|_\___|
   ___ ___ ___ ___ ___         
  |___|___|___|___|___|*)
  module TableFeatures =
    let (|Money|Number|Alpha|AlphaNumeric|NumericAlpha|Garbage|) (value: string) =
      let value = value.Trim ()
      let numAlpha = value |> Regex.countMatches """(?inx)[a-z]"""
      let numNumber = value |> Regex.countMatches """(?inx)\d"""
      let numPunct = value |> Regex.countMatches """(?inx)[\W-[\s]]"""

      if numNumber > numAlpha && numNumber >= numPunct
      then
        if value |> Regex.isMatch """(?inx)[$€]""" then Money
        elif numAlpha = 0 then Number
        else NumericAlpha
      elif numAlpha > numPunct
      then
        if numNumber = 0 then Alpha
        else AlphaNumeric
      else Garbage

    let getCellsWithFeatures =
      let getClassFromString =
        memoize (fun value ->
          match value with
          | Money -> "Money"
          | Number -> "Number"
          | Alpha -> "Alpha"
          | AlphaNumeric -> "AlphaNumeric"
          | NumericAlpha -> "NumericAlpha"
          | Garbage -> "Garbage"
        )
      let getClass maybeSpatialString =
        maybe {
          let! (s : SpatialString) = maybeSpatialString
          return getClassFromString s.String
        }
      let getVal maybeSpatialString =
        maybe {
          let! (s : SpatialString) = maybeSpatialString
          return s.String.Trim () |> Regex.replace """\s+""" " "
        }
      
      let stringToSpatialString sourceDocName value =
        let res = SpatialStringClass ()
        res.CreateNonSpatialString (value, sourceDocName)
        res

      let getFeatures sourceDocName (table: SpatialString[][]) row col =
        let toSpatialString = stringToSpatialString sourceDocName
        let thisCell = Some table.[row].[col]
        // Skip getting header features if the current cell is that header
        let hasPrevCellInRow = col > 0
        let hasPrevCellInCol = row > 0
        let rowHeader = if hasPrevCellInRow then Some table.[row].[0] else None
        let colHeader = if hasPrevCellInCol then Some table.[0].[col] else None
        let stubHeader = if hasPrevCellInCol && hasPrevCellInRow then Some table.[0].[0] else None
        let prevCellInRow = if hasPrevCellInRow then Some table.[row].[col-1] else None
        let prevCellInCol = if hasPrevCellInCol then Some table.[row-1].[col] else None
        seq {
          yield ("RowHeader", rowHeader |> getVal, "Feature+Tokenize")
          yield ("ColHeader", colHeader |> getVal, "Feature+Tokenize")
          yield ("StubHeader", stubHeader |> getVal, "Feature+Tokenize")
          yield ("CellClass", thisCell |> getClass, "Feature")
          yield ("PrevCellInRowClass", prevCellInRow |> getClass, "Feature")
          yield ("PrevCellInColClass", prevCellInCol |> getClass, "Feature")
        }
        // Remove None values
        |> Seq.choose (fun (name, value, typ) -> match value with | Some v -> Some (name, v, typ) | _ -> None)
        // Convert to attributes
        |> Seq.map (fun (name, value, typ) -> AttributeClass(Name = name, Value = (value |> toSpatialString), Type = typ))

      // Get feature subattributes for each cell
      let getCellsWithFeatures_Internal sourceDocName (table: SpatialString[][]) =
        table |> Array.mapi (fun rowIdx row ->
          row |> Array.mapi (fun colIdx cell ->
            let features = getFeatures sourceDocName table rowIdx colIdx |> Seq.toUV
            AttributeClass (Name = "Cell", Value = cell, SubAttributes = features)
        ))
        |> Seq.concat

      getCellsWithFeatures_Internal
  // End of TableFeatures module

  let private tableCellsToAttributesWithFeatures sourceDocName tables =
    tables
    |> Seq.map (fun table -> TableFeatures.getCellsWithFeatures sourceDocName table)
    |> Seq.concat
    |> Seq.toUV

  let getCellsWithFeaturesForAllPages (doc: AFDocument): AFDocument =
    let sourceDocName = doc.Text.SourceDocName
    use tableFinder = TabulaUtils.CreateTabulaUtility();
    let tables = tableFinder.GetTablesOnSpecifiedPages sourceDocName
    let cellsWithFeatures = tables |> Seq.concat |> tableCellsToAttributesWithFeatures sourceDocName
    doc.Attribute.SubAttributes <- cellsWithFeatures
    doc

  let getCellsWithFeaturesForFirstAvailablePage (doc: AFDocument): AFDocument =
    let text = doc.Text
    match text with
    | Empty -> doc.Attribute.SubAttributes.Clear ()
    | FirstAvailablePage pageNumber ->
      use tableFinder = TabulaUtils.CreateTabulaUtility()
      let sourceDocName = text.SourceDocName
      let tables = tableFinder.GetTablesOnSpecifiedPages (sourceDocName, pageNumbers = [pageNumber])
      let cellsWithFeatures = tables |> Seq.concat |> tableCellsToAttributesWithFeatures sourceDocName
      doc.Attribute.SubAttributes <- cellsWithFeatures
    doc

