﻿using AlertManager.Services;
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

namespace AlertManager.ViewModels
{
    public class ResolveFilesViewModel : ReactiveObject
    {
        #region fields
        public IDBService dbService;

        [Reactive]
        public AlertsObject thisAlert { get; set; }

        [Reactive]
        public ObservableCollection<FileObject> ListOfFiles { get; set; } = new();

        //README: temperary, everything will be added to alertsobject later
        //TODO remove once alerts contains this information
        private string dataBaseName = "Demo_Web", dataBaseServer = "BLUEJAY";
        private int actionId = 23;

        private List<int> listOfFileIds = new List<int> { 1, 2, 3 };

        [Reactive]
        public int ActionSelection
        {
            get; set;
        } = 3;

        #endregion fields

        /// <summary>
        /// Constructor that initalizes values thisAlert and dbService, sets up the values to be used in the view
        /// </summary>
        /// <param name="alertObject">Alert Object that holds information to display and manipulate</param>
        /// <param name="dbService">DB service that retrieves files and sets files in the associated database</param>
        public ResolveFilesViewModel(AlertsObject alertObject, IDBService? dbService)
        {
            thisAlert = alertObject;

            this.dbService = (dbService == null) ? new DBService(new FileProcessingDB()) : dbService;

            SetupDBInformation();

            GetFilesFromEvents();

        }
        
        /// <summary>
        /// Constructor that intialzes a registered dbservice if one is not registered
        /// </summary>
        /// <param name="alertObject"></param>
        public ResolveFilesViewModel(AlertsObject alertObject) : this(alertObject,
            Locator.Current.GetService<IDBService>()
            )
        {

        }

        /// <summary>
        /// Will take the information of the database name, server, actionid from the events in the alertobject
        /// Sets fields above with said values
        /// </summary>
        public void SetupDBInformation()
        {
            //TODO set up database information from the events db information, should be in events or alert info

        }

        
        /// <summary>
        /// Gets files associated to the alert from the attached events, sets the field listoffiles above with the info
        /// </summary>
        public void GetFilesFromEvents()
        {

            try
            {
                //TODO impliment data from events from https://extract.atlassian.net/browse/ISSUE-18956
                GetFilesFromDB(listOfFileIds);
            }
            catch (Exception e)
            {
                RxApp.DefaultExceptionHandler.OnNext(e);
            }
        }

        /// <summary>
        /// Gets a list of fileobjects from a associated list of id's sets field listOfFiles to said values
        /// </summary>
        /// <param name="listOfFileIds"></param>
        public void GetFilesFromDB(List<int> listOfFileIds)
        {

            List<FileObject> newFiles = dbService.GetFileObjects(
                    listOfFileIds,
                    this.dataBaseName,
                    this.dataBaseServer,
                    this.actionId);

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
            try
            {
                EActionStatus actionStatus = GetStatusFromCombo();

                List<FileObject> newList = new List<FileObject>(ListOfFiles);
                foreach (FileObject file in newList)
                {
                    dbService.SetFileStatus(
                        file.FileId,
                        actionStatus,
                        dataBaseName,
                        dataBaseServer,
                        actionId);
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