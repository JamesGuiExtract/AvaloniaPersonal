namespace Extract.FileActionManager.Utilities.FAMServiceManager

open Elmish.WPF

[<RequireQualifiedAccess>]
module NamePasswordDialog =
  type Model =
    { ServiceName: string
      StartName: string
      Password: string
      PasswordCheck: string }

  let passwordsMatch m =
    m.Password = m.PasswordCheck

  type Msg =
    | SetStartName of string
    | SetPassword of string
    | SetPasswordCheck of string
    | Submit of string * string * string
    | Cancel

  let init =
    { ServiceName = ""
      StartName = ""
      Password = ""
      PasswordCheck = "" }

  let update msg (m: Model) =
    match msg with
    | SetStartName name -> { m with StartName = name }
    | SetPassword pwd -> { m with Password = pwd }
    | SetPasswordCheck pwd -> { m with PasswordCheck = pwd }
    | Submit _ -> m  // handled by parent
    | Cancel -> m  // handled by parent

  let bindings () : Binding<Model, Msg> list = [
    "ServiceName" |> Binding.oneWay (fun m -> m.ServiceName)
    "StartName" |> Binding.twoWay ((fun m -> m.StartName), SetStartName)
    "Password" |> Binding.twoWay ((fun m -> m.Password), SetPassword)
    "ConfirmPassword" |> Binding.twoWay ((fun m -> m.PasswordCheck), SetPasswordCheck)
    "PasswordsDoNotMatch" |> Binding.oneWay (not << passwordsMatch)
    "Ok" |> Binding.cmdIf (fun (m: Model) ->
      if m |> passwordsMatch then
        Some <| Submit (m.ServiceName, m.StartName, m.Password)
      else None)
    "Cancel" |> Binding.cmd Cancel
  ]

  let designVm = ViewModel.designInstance init (bindings ())
