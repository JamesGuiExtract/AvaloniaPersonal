module Utils

module LM =
  open System
  open System.IO
  open System.Reflection

  // License Extract Systems stuff
  let private create (o: string) = Activator.CreateInstance(Type.GetTypeFromProgID(o))
  let license()=
      let lm = create "UCLIDCOMLM.UCLIDComponentLM"
      let commonComponentsDir =
          let aupdir = Environment.GetEnvironmentVariable("ALLUSERSPROFILE")
          Path.Combine(aupdir, @"Application Data\Extract Systems\LicenseFiles")
      seq {
        yield! Directory.GetFiles(commonComponentsDir, "*.lic")
        yield! Directory.GetFiles(__SOURCE_DIRECTORY__, "*.lic")
      }
      |> Seq.choose
          (fun licFile ->
            let pwdFile = Path.ChangeExtension(licFile, "pwd")
            if File.Exists(pwdFile) then Some(licFile,pwdFile)
            else None
          )
      |> Seq.iter
          (fun (licFile,pwdFile) ->
            let pwdKeys =
              ([|','; ' '; '\r'; '\n'|], StringSplitOptions.RemoveEmptyEntries)
                |> (File.ReadAllText pwdFile).Split
                |> Array.map System.Int32.Parse
            lm.GetType().InvokeMember("InitializeFromFile", BindingFlags.InvokeMethod, null, lm, [| licFile; pwdKeys.[0]; pwdKeys.[1]; pwdKeys.[2]; pwdKeys.[3] |]) |> ignore
          )

LM.license()

module Regex =
  open System.Text.RegularExpressions

  let replace pat rep inp =
    Regex.Replace(input=inp, pattern=pat, replacement=rep)
