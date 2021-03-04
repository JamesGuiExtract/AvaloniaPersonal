module Extract.Utilities.FSharp.AFUtils

open Extract.AttributeFinder
open System
open UCLID_AFCORELib
open UCLID_AFUTILSLib
open UCLID_COMUTILSLib
open UCLID_RASTERANDOCRMGMTLib

module Seq =
  let ofUV<'t> (uv : IIUnknownVector) =
    seq { for i in 0 .. (uv.Size()-1) -> uv.At(i)} |> Seq.cast<'t>

  let ofVOA = ofUV<IAttribute>

  let toUV (x : 'a seq when 'a :> obj) =
    let uv = IUnknownVectorClass()
    x |> Seq.iter uv.PushBack
    uv
(************************************************************************************************************************)

module AFDoc =
  // Active pattern for getting first page, ignoring __EMPTYPAGE__
  let (|FirstAvailablePage|_|) (text: SpatialString) =
    if text.HasSpatialInfo () && not <| text.String.Equals("___EMPTYPAGE___", StringComparison.Ordinal)
    then
      let firstPageNumber = text.GetFirstPageNumber ()
      Some firstPageNumber
    else None
(************************************************************************************************************************)

module SpatialString =
  let getAvgConf (value: SpatialString) =
    let min = ref 0 in let max = ref 0 in let avg = ref 0
    value.GetCharConfidence (min, max, avg)
    !avg
(************************************************************************************************************************)

let loadVoa (afUtil: AFUtilityClass) voaPath =
  let voa = afUtil.GetAttributesFromFile voaPath
  MemoryManagerExtensionMethods.ReportMemoryUsage voa
  voa
(************************************************************************************************************************)
