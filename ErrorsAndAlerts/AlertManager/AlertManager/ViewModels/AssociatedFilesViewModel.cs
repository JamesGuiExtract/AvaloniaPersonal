﻿using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Models.AllEnums;
using Extract.ErrorHandling;
using Extract.ErrorsAndAlerts.ElasticDTOs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Reactive;
using System.Threading.Tasks;
using UCLID_FILEPROCESSINGLib;

namespace AlertManager.ViewModels
{
    public class AssociatedFilesViewModel : ViewModelBase
    {

        private readonly IDBService _dbService;

        [Reactive]
        public AlertsObject ThisAlert { get; set; }

        [Reactive]
        public ObservableCollection<FileObjectViewModel> ListOfFiles { get; set; } = new();

        private string? _databaseServer = ConfigurationManager.AppSettings["DatabaseServer"];
        private string? _databaseName = ConfigurationManager.AppSettings["DatabaseName"];

        private int _actionId = 23;

        private IList<int> _listOfFileIds = new List<int>() { 1, 2, 3 };

        [Reactive]
        public KeyValuePair<EActionStatus, string> StatusSelection { get; set; }

        public IEnumerable<KeyValuePair<EActionStatus, string>> FutureFileStatuses 
         => Constants.ActionStatusToDescription;

        public ReactiveCommand<Unit, Unit> SetFileStatus { get; private set; }

        /// <summary>
        /// Constructor that initalizes values thisAlert and dbService, sets up the values to be used in the view
        /// </summary>
        /// <param name="alertObject">Alert Object that holds information to display and manipulate</param>
        /// <param name="dbService">DB service that retrieves files and sets files in the associated database</param>
        public AssociatedFilesViewModel(AlertsObject alertObject, IDBService dbService)
        {
            ThisAlert = alertObject;

            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));

            SetFileStatus = ReactiveCommand.Create(SetFileStatusImpl);

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
                    //TODO _listOfFileIds.Add(@event.ContextType.FileID);
                }

                GetFilesFromDB(_listOfFileIds);
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
                throw new("Null database fields");
            }
            
            List<FileObject> newFiles = _dbService.GetFileObjects(
                    listOfFileIds,
                    this._databaseName,
                    this._databaseServer,
                    this._actionId);

            UpdateFileList(newFiles);
        }

        private EActionStatus ActionStatusFromIndividualFile(FileObjectViewModel fileToSet)
        {
            try
            {
                return fileToSet.SelectedFileStatus.Key;
            }
            catch(Exception e)
            {
                throw e.AsExtractException("ELI55305");
            }
        }

        public void SetFileStatusImpl(FileObjectViewModel fileToSet)
        {
            if (String.IsNullOrEmpty(_databaseServer) || String.IsNullOrEmpty(_databaseName))
            {
                throw new("Database settings are empty.");
            }

            try
            {
                EActionStatus actionStatus = ActionStatusFromIndividualFile(fileToSet);

                if(fileToSet.FileObject == null)
                {
                    throw new Exception("issue with fileobject retrieval");
                }
                _dbService.SetFileStatus(
                    fileToSet.FileObject.FileId,
                    actionStatus,
                    _databaseName,
                    _databaseServer,
                    _actionId);

                List<FileObject> newFiles = _dbService.GetFileObjects(
                        _listOfFileIds,
                        this._databaseName,
                        this._databaseServer,
                        this._actionId);

                UpdateFileList(newFiles);
            }
            catch (Exception e)
            {
                throw e.AsExtractException("ELI54022");
            }
        }

        private void UpdateFileList(List<FileObject> newFiles)
        {

            ListOfFiles.Clear();

            foreach (FileObject fileObject in newFiles)
            {
                FileObjectViewModel fileViewModel = new FileObjectViewModel(fileObject);
                fileViewModel.SetIndividualStatuses =
                    ReactiveCommand.Create<FileObject>(_ => SetFileStatusImpl(fileViewModel));
                ListOfFiles.Add(fileViewModel);
            }

        }

        /// <summary>
        /// Sets the file status in the database
        /// Will set the files based on the ListOfFiles field above
        /// Sets the status based on user selection of a bound combobox
        /// Resets the fileds above with new values from database for associated file Id's
        /// </summary>
        public void SetFileStatusImpl()
        {
            if (String.IsNullOrEmpty(_databaseServer) || String.IsNullOrEmpty(_databaseName))
            {
                throw new("Database settings are empty.");
            }

            try
            {
                EActionStatus actionStatus = GetStatusFromCombo();

                List<FileObjectViewModel> newList = new List<FileObjectViewModel>(ListOfFiles);
                foreach (FileObjectViewModel file in newList)
                {
                    if(file.FileObject == null) 
                    { 
                        throw new Exception("issue retrieving files");  
                    }

                    _dbService.SetFileStatus(
                        file.FileObject.FileId,
                        actionStatus,
                        _databaseName,
                        _databaseServer,
                        _actionId);
                }

                GetFilesFromDB(_listOfFileIds);
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
                return StatusSelection.Key;
            }
            catch (Exception e)
            {
                e.AsExtractException("ELI54023").Log();
                return EActionStatus.kActionUnattempted;
            }
        }
    }
}