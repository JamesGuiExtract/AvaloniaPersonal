namespace Extract.Utilities.FSharp

open System.Runtime.CompilerServices

// Copied from Jared Parsons' blog
// https://blogs.msdn.microsoft.com/jaredpar/2010/07/27/converting-system-funct1-tn-to-fsharpfuncttresult/
// Type and parameter names are in line with naming conventions given here: https://fsharpforfunandprofit.com/posts/naming-conventions/
[<Extension>]
type public FSharpFuncUtil = 

    [<Extension>] 
    static member ToFSharpFunc<'a,'b> (func:System.Converter<'a,'b>) = fun x -> func.Invoke(x)

    [<Extension>] 
    static member ToFSharpFunc<'a,'b> (func:System.Func<'a,'b>) = fun x -> func.Invoke(x)

    [<Extension>] 
    static member ToFSharpFunc<'a,'b,'c> (func:System.Func<'a,'b,'c>) = fun x y -> func.Invoke(x,y)

    [<Extension>] 
    static member ToFSharpFunc<'a,'b,'c,'d> (func:System.Func<'a,'b,'c,'d>) = fun x y z -> func.Invoke(x,y,z)

    static member Create<'a,'b> (func:System.Func<'a,'b>) = FSharpFuncUtil.ToFSharpFunc func

    static member Create<'a,'b,'c> (func:System.Func<'a,'b,'c>) = FSharpFuncUtil.ToFSharpFunc func

    static member Create<'a,'b,'c,'d> (func:System.Func<'a,'b,'c,'d>) = FSharpFuncUtil.ToFSharpFunc func

[<Extension>]
type public FSharpListUtil = 

    [<Extension>] 
    static member ToFSharpList<'a> (lst:System.Collections.Generic.IEnumerable<'a>) = lst |> List.ofSeq
