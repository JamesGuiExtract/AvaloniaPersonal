namespace Extract.FileActionManager.Utilities.FAMServiceManager

open System

type ShowHideDetails =
| ShowAll
| ShowSelected
| HideAll
| Unknown

type Model =
  { Services: FAMService list
    ServicesToRestart: Set<string>
    NewServiceName: string
    NewServiceDisplayName: string
    LastRefresh: DateTimeOffset
    StatusMsg: string
    ShowDetails: ShowHideDetails
    SelectedService: string option
    Dialog: NamePasswordDialog.Model option }

[<RequireQualifiedAccess>]
module Model =

  let empty =
    { Services = []
      ServicesToRestart = Set.empty
      NewServiceName = ""
      NewServiceDisplayName = ""
      LastRefresh = DateTimeOffset.MinValue
      StatusMsg = ""
      ShowDetails = HideAll
      SelectedService = None
      Dialog = None }

  let mapService name f model =
    { model with Services = model.Services |> List.map (fun s -> if s.Name = name then f s else s) }

  let isUpdatePending model =
    not (model.ServicesToRestart |> Set.isEmpty
      && model.Services |> List.filter FAMService.isPendingState |> List.isEmpty)

  let getServicesToRestart model =
    let stoppedServices =
      model.Services
      |> Seq.filter (fun s -> match s.State with Stopped -> true | _ -> false)
      |> Seq.map (fun s -> s.Name)
      |> Set.ofSeq
    model.ServicesToRestart |> Set.intersect stoppedServices
