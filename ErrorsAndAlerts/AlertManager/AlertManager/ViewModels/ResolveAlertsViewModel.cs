using AvaloniaDashboard.Models;
using AvaloniaDashboard.Models.AllDataClasses;
using AvaloniaDashboard.Models.AllEnums;
using AvaloniaDashboard.Views;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;

namespace AvaloniaDashboard.ViewModels
{
    public class ResolveAlertsViewModel : ReactiveObject
    {
        #region fields
        //fields

        private ResolveAlertsView ThisWindow;

        #endregion fields

        #region setters and getters for bindings
        [Reactive]
        public List<AlertsObject>? AlertList { get; set; }

        [Reactive]
        public AlertsObject? ThisObject { get; set; } 

        [Reactive]
        public int? ErrorId { get; set; }

        [Reactive]
        public int? AlertId { get; set; } 

        [Reactive]
        public string? ActionType { get; set; } 

        [Reactive]
        public string? AlertType { get; set; } 

        [Reactive]
        public string? Configuration { get; set; } 

        [Reactive]
        public DateTime? ActivationTime { get; set; } 

        [Reactive]
        public string? UserFound { get; set; } 

        [Reactive]
        public string? MachineFoundError { get; set; } 

        [Reactive]
        public string? ResolutionType { get; set; }

        [Reactive]
        public DateTime? ResolutionTime { get; set; } 

        [Reactive]
        public TypeOfResolutionAlerts? TypeOfResolution { get; set; } 

        [Reactive]
        public string? AlertHistory { get; set; }
        #endregion setters and getters for bindings

        public void RefreshScreen(AlertsObject newObject)
        {
            ThisObject = newObject;
            ActionType = ThisObject.action_Type;
            ErrorId = ThisObject.issue_Id;
            AlertType = ThisObject.alert_Type;
            Configuration = ThisObject.configuration;
            ActivationTime = ThisObject.activation_Time;
            UserFound = ThisObject.user_Found;
            MachineFoundError = ThisObject.machine_Found_Error;
            ResolutionType = ThisObject.resolution_Type;
            ResolutionTime = ThisObject.resolution_Time;
            TypeOfResolution = ThisObject.type_Of_Resolution;
            AlertHistory = ThisObject.alert_History;
        }

        #region Constructors
        //These constructors use splat/reactive UI for dependency inversion
        public ResolveAlertsViewModel() : this(new AlertsObject(), new ResolveAlertsView())
        {

        }
        public ResolveAlertsViewModel(AlertsObject alertObjectToDisplay) : this(alertObjectToDisplay, new ResolveAlertsView())
        {

        }

        public ResolveAlertsViewModel(AlertsObject alertObjectToDisplay, ResolveAlertsView thisWindow)
        {
            RefreshScreen(alertObjectToDisplay);
            this.ThisWindow = thisWindow;
        }

        #endregion Constructors

        private void CloseWindow()
        {
            ThisWindow.Close("Refresh");
        }
    }
}