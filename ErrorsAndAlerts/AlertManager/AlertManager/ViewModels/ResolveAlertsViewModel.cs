using AlertManager.Interfaces;
using AlertManager.Models;
using AlertManager.Models.AllDataClasses;
using AlertManager.Models.AllEnums;
using AlertManager.Services;
using AlertManager.Views;
using Extract.ErrorHandling;
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

        private ResolveAlertsView ThisWindow = new();
        private IAlertResolutionLogger ResolutionLogger;

        #endregion fields

        #region setters and getters for bindings
        [Reactive]
        public List<AlertsObject>? AlertList { get; set; }

        [Reactive]
        public AlertsObject ThisObject { get; set; }  = new AlertsObject();


        #endregion setters and getters for bindings

        public void RefreshScreen(AlertsObject newObject)
        {
            try
            {
                if(newObject == null)
                {
                    throw new ExtractException("ELI53867", "Issue with refreshing screen, object to refresh to is null or invalid");
                }
                ThisObject = newObject;

            }
            catch(Exception e)
            {
                ExtractException ex = new("ELI53868", "Issue refreshing screen", e);
                throw ex;
            }
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
            try
            {
                if(alertResolutionLogger == null)
                {
                    alertResolutionLogger = new AlertResolutionLogger();
                }

                if(alertObjectToDisplay == null)
                {
                    alertObjectToDisplay = new AlertsObject();
                }

                ThisObject = alertObjectToDisplay;
                RefreshScreen(alertObjectToDisplay);
                ThisObject.Resolution.AlertId = ThisObject.AlertId;
                ThisObject.Resolution.ResolutionTime = DateTime.Now;
                ResolutionLogger = alertResolutionLogger!;
            }
            catch(Exception e)
            {
                ExtractException ex = new("ELI53869", "Issue with initializing values", e);
                throw ex;
            }
        }

        public ResolveAlertsViewModel(AlertsObject alertObjectToDisplay, ResolveAlertsView thisWindow, IAlertResolutionLogger? alertResolutionLogger)
        {
            try
            {
                if (ThisObject == null)
                {
                    throw new Exception("ThisObject is null");
                }

                RefreshScreen(alertObjectToDisplay);
                ThisObject.Resolution.AlertId = ThisObject.AlertId;
                ThisObject.Resolution.ResolutionTime = DateTime.Now;
                ResolutionLogger = alertResolutionLogger!;
                this.ThisWindow = thisWindow;
            }
            catch(Exception e)
            {
                ExtractException ex = new("ELI53870", "Issue with initializing values", e);
                throw ex;
            }
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