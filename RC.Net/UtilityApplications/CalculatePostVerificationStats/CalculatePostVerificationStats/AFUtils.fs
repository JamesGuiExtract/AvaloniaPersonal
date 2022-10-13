module CalculatePostVerificationStats.AFUtils

open UCLID_AFCORELib
open UCLID_COMUTILSLib

module Seq =
    let ofUV<'t> (uv: IUnknownVector) =
        seq { for i in 0 .. (uv.Size() - 1) -> uv.At(i) :?> 't }

    let ofVOA = ofUV<IAttribute>

    let toUV (x: 'a seq when 'a :> obj) =
        let uv = IUnknownVectorClass()
        x |> Seq.iter (fun li -> uv.PushBack li)
        uv :> IUnknownVector

    let ofVV<'t> (vv: IVariantVector) =
        if vv = null then
            Seq.empty
        else
            seq { for i in 0 .. (vv.Size - 1) -> vv.[i] :?> 't }

module List =
    let ofUV<'t> = Seq.ofUV<'t> >> Seq.toList<'t>
    let ofVOA: IUnknownVector -> IAttribute list = Seq.ofVOA >> Seq.toList<IAttribute>
