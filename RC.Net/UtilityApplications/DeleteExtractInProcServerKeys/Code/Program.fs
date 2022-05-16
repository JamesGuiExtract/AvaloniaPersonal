open System
open Microsoft.Win32

[<EntryPoint>]
let main argv =
    let clsidKey = Registry.ClassesRoot.OpenSubKey("WOW6432Node\CLSID")

    clsidKey.GetSubKeyNames()
    |> Seq.iter (fun keyName ->
        let subKey = clsidKey.OpenSubKey(keyName)

        try
            let inProcServerKey = subKey.OpenSubKey("InprocServer32", true)

            if inProcServerKey |> isNull |> not then
                let assembly = inProcServerKey.GetValue("Assembly")
                let codebase = inProcServerKey.GetValue("CodeBase")

                if (not (isNull assembly))
                   && Convert.ToString(assembly).StartsWith("Extract.")
                   || (not (isNull codebase))
                      && Convert
                          .ToString(codebase)
                          .StartsWith("file:///C:/Program Files (x86)/Extract Systems/CommonComponents") then
                    printfn "%O" subKey
                    printfn "    Assembly: %O" assembly

                    inProcServerKey.GetSubKeyNames()
                    |> Seq.iter (fun versionKey ->
                        printfn "    Version Key: %O" versionKey
                        inProcServerKey.DeleteSubKey(versionKey))
        with
        | _ -> ())

    0
