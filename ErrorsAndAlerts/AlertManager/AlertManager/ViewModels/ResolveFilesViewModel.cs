using AlertManager.Services;
using System;
using System.Collections.Generic;
using UCLID_FILEPROCESSINGLib;
using Extract.ErrorHandling;
using AlertManager.Models.AllDataClasses;
using AlertManager.Interfaces;
using Splat;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Configuration;
using Extract.ErrorsAndAlerts.ElasticDTOs;

namespace AlertManager.ViewModels
{
    public class ResolveFilesViewModel : ViewModelBase
    {
        private readonly IDBService _dbService;

        [Reactive]
        public AlertsObject ThisAlert { get; set; }

        [Reactive]
        public ObservableCollection<FileObject> ListOfFiles { get; set; } = new();

        private string? _databaseServer = ConfigurationManager.AppSettings["DatabaseServer"];
        private string? _databaseName = ConfigurationManager.AppSettings["DatabaseName"];

        private int _actionId = 23;

        private IList<int> listOfFileIds = new List<int>() { 1, 2, 3 };

        //Should probably be renamed to "StatusSelection" and use an enum
        [Reactive]
        public int ActionSelection { get; set; } = 3;

        /// <summary>
        /// Constructor that initalizes values thisAlert and dbService, sets up the values to be used in the view
        /// </summary>
        /// <param name="alertObject">Alert Object that holds information to display and manipulate</param>
        /// <param name="dbService">DB service that retrieves files and sets files in the associated database</param>
        public ResolveFilesViewModel(AlertsObject alertObject, IDBService dbService)
        {
            ThisAlert = alertObject;

            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));

            //https://extract.atlassian.net/browse/ISSUE-19088
            //SetupDBInformation(); TODO its done, but we dont' have dedicated servers or dbs so will need to create this

            GetFilesFromEvents();

        }

        /// <summary>
        /// Will take the information of the database name, server, actionid from the events in the alertobject
        /// Sets fields above with said values
        /// </summary>
        public void SetupDBInformation()
        {
            try
            {
                if (ThisAlert == null)
                {
                    throw new Exception("no alert object");
                }

                if (ThisAlert.AssociatedEvents == null || ThisAlert.AssociatedEvents[0] == null)
                {
                    throw new Exception("issue with associated exceptions, null value");
                }
                //TODO: in the future change this to be on alerts itself... or each file associated with each event
                _databaseName = ThisAlert.AssociatedEvents[0].Context.DatabaseName;
                _databaseServer = ThisAlert.AssociatedEvents[0].Context.DatabaseServer;
                _actionId = ThisAlert.AssociatedEvents[0].Context.ActionID;
            }
            catch (Exception e)
            {
                RxApp.DefaultExceptionHandler.OnNext(e.AsExtractException("ELI54201"));
            }
        }


        /// <summary>
        /// Gets files associated to the alert from the attached events, sets the field listoffiles above with the info
        /// </summary>
        public void GetFilesFromEvents()
        {

            try
            {
                if (ThisAlert == null || ThisAlert.AssociatedEvents == null)
                {
                    throw new Exception("no alert");
                }

                foreach (EventDto @event in ThisAlert.AssociatedEvents)
                {
                    //TODO listOfFileIds.Add(@event.ContextType.FileID);
                }

                GetFilesFromDB(listOfFileIds);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI54276");
            }
        }

        /// <summary>
        /// Gets a list of fileobjects from a associated list of id's sets field listOfFiles to said values
        /// </summary>
        /// <param name="listOfFileIds"></param>
        public void GetFilesFromDB(IList<int> listOfFileIds)
        {
            if (String.IsNullOrEmpty(_databaseServer) || String.IsNullOrEmpty(_databaseName))
            {
                return;
            }

            List<FileObject> newFiles = _dbService.GetFileObjects(
                    listOfFileIds,
                    this._databaseName,
                    this._databaseServer,
                    this._actionId);

            ListOfFiles = new ObservableCollection<FileObject>();

            foreach (FileObject fileObject in newFiles)
            {
                ListOfFiles.Add(fileObject);
            };

        }

        /// <summary>
        /// Sets the file status in the database
        /// Will set the files based on the ListOfFiles field above
        /// Sets the status based on user selection of a bound combobox
        /// Resets the fileds above with new values from database for associated file Id's
        /// </summary>
        public void SetFileStatus()
        {
            if (String.IsNullOrEmpty(_databaseServer) || String.IsNullOrEmpty(_databaseName))
            {
                return;
            }

            try
            {
                EActionStatus actionStatus = GetStatusFromCombo();

                List<FileObject> newList = new List<FileObject>(ListOfFiles);
                foreach (FileObject file in newList)
                {
                    _dbService.SetFileStatus(
                        file.FileId,
                        actionStatus,
                        _databaseName,
                        _databaseServer,
                        _actionId);
                }

                GetFilesFromDB(listOfFileIds);
            }
            catch (Exception e)
            {
                throw e.AsExtractException("ELI54022");
            }
        }

        /// <summary>
        /// Returns a EAction status based on user selection of a combobox
        /// </summary>
        /// <returns></returns>
        private EActionStatus GetStatusFromCombo()
        {
            try
            {
                switch (ActionSelection)
                {
                    case 0:
                        return EActionStatus.kActionUnattempted;
                    case 1:
                        return EActionStatus.kActionPending;
                    case 2:
                        return EActionStatus.kActionProcessing;
                    case 3:
                        return EActionStatus.kActionCompleted;
                    case 4:
                        return EActionStatus.kActionFailed;
                    case 5:
                        return EActionStatus.kActionSkipped;
                    default:
                        throw new Exception("Issue with retrieving action status from combobox");

                }
            }
            catch (Exception e)
            {
                e.AsExtractException("ELI54023").Log();
                return EActionStatus.kActionUnattempted;
            }
        }

    }
}