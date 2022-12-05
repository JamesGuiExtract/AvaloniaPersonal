using AlertManager.Interfaces;
using AlertManager.Models;
using AlertManager.Models.AllDataClasses;
using AlertManager.Models.AllEnums;
using AlertManager.Views;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;

namespace AlertManager.ViewModels
{
    public class ResolveAlertsViewModel : ReactiveObject
    {
        #region fields
        //fields

        private ResolveAlertsView ThisWindow;
        private IAlertResolutionLogger ResolutionLogger;

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
        public string? AlertName { get; set; }

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
            ActionType = ThisObject.ActionType;
            ErrorId = ThisObject.IssueId;
            AlertType = ThisObject.AlertType;
            Configuration = ThisObject.Configuration;
            ActivationTime = ThisObject.ActivationTime;
            UserFound = ThisObject.UserFound;
            MachineFoundError = ThisObject.MachineFoundError;
            ResolutionType = ThisObject.Resolution.ResolutionComment;
            ResolutionTime = ThisObject.Resolution.ResolutionTime;
            TypeOfResolution = ThisObject.Resolution.ResolutionType;
            AlertHistory = ThisObject.AlertHistory;
        }

        #region Constructors
        //These constructors use splat/reactive UI for dependency inversion
        public ResolveAlertsViewModel() : this(new AlertsObject(), new ResolveAlertsView(), Locator.Current.GetService<IAlertResolutionLogger>())
        {

        }
        public ResolveAlertsViewModel(AlertsObject alertObjectToDisplay) : this(alertObjectToDisplay, new ResolveAlertsView(), Locator.Current.GetService<IAlertResolutionLogger>())
        {

        }
        public ResolveAlertsViewModel(AlertsObject alertObjectToDisplay, ResolveAlertsView thisWindow) : this(alertObjectToDisplay, thisWindow, Locator.Current.GetService<IAlertResolutionLogger>())
        {

        }

        /// <summary>
        /// used for testing only
        /// </summary>
        /// <param name="alertObjectToDisplay"></param>
        /// <param name="alertResolutionLogger"></param>
        public ResolveAlertsViewModel(AlertsObject alertObjectToDisplay, IAlertResolutionLogger? alertResolutionLogger)
        {
            RefreshScreen(alertObjectToDisplay);
            ThisObject.Resolution.AlertId = ThisObject.AlertId;
            ThisObject.Resolution.ResolutionTime = DateTime.Now;
            ResolutionLogger = alertResolutionLogger!;
        }

        public ResolveAlertsViewModel(AlertsObject alertObjectToDisplay, ResolveAlertsView thisWindow, IAlertResolutionLogger? alertResolutionLogger)
        {
            RefreshScreen(alertObjectToDisplay);
            ThisObject.Resolution.AlertId = ThisObject.AlertId;
            ThisObject.Resolution.ResolutionTime = DateTime.Now;
            ResolutionLogger = alertResolutionLogger!;
            this.ThisWindow = thisWindow;
        }

        #endregion Constructors

        private void CloseWindow()
        {
            ThisWindow.Close("Refresh");
        }

        private void CommitResolution()
        {
            ResolutionLogger.LogResolution(ThisObject!);
            ThisWindow.Close("Refresh");
        }
    }
}