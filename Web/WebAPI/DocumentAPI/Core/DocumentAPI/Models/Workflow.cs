﻿using UCLID_FILEPROCESSINGLib;
using System;
using static DocumentAPI.Utils;

namespace DocumentAPI.Models
{
    /// <summary>
    /// Workflow data model
    /// </summary>
    public class Workflow
    {
        private int _id;
        private string _name;
        private EWorkflowType _type;
        private string _description;
        private string _startAction;
        private string _endAction;
        private string _postWorkflowAction;
        private string _documentFolder;
        private string _outputAttributeSet;
        private string _outputFileMetadataField;
        private string _databaseServerName;
        private string _databaseName;

        /// <summary>
        /// The Id of the Workflow, used to get by Id.
        /// </summary>
        public int Id
        {
            get
            {
                Contract.Assert(_id > 0, "attempt to get Id, bad value: {0}", _id);
                return _id;
            }
            private set
            {
                Contract.Assert(value > 0, "attempt to set bad value for Id: {0}", value);
                _id = value;
            }
        }

        /// <summary>
        /// Name of the workflow
        /// </summary>
        public string Name
        {
            get
            {
                Contract.Assert(!String.IsNullOrEmpty(_name), "Attempt to get: {0}, bad value: {1}", GetMethodName(), _name);
                return _name;
            }
            private set
            {
                Contract.Assert(!String.IsNullOrEmpty(value), "Attempt to set: {0}, to bad value: {1}", GetMethodName(), value);
                _name = value;
            }
        }

        /// <summary>
        /// the workflow Type value
        /// </summary>
        public EWorkflowType Type
        {
            get
            {
                int iType = (int)_type;
                Contract.Assert(iType >= 0 && iType < 4, "attempt to get Type, bad value: {0}", iType);
                return _type;
            }
            private set
            {
                int iType = (int)value;
                Contract.Assert(iType >= 0 && iType < 4, "attempt to set Type to bad value: {0}", iType);

                _type = value;
            }
        }

        /// <summary>
        /// description of the workflow
        /// </summary>
        public string Description
        {
            get
            {
                return _description;
            }
            private set
            {
                _description = value;
            }
        }

        /// <summary>
        /// the entry action for the workflow
        /// </summary>
        public string StartAction
        {
            get
            {
                return _startAction;
            }
            private set
            {
                _startAction = value;
            }
        }

        /// <summary>
        /// The exit action for the workflow
        /// </summary>
        public string EndAction
        {
            get
            {
                return _endAction;
            }
            private set
            {
                _endAction = value;
            }
        }

        /// <summary>
        /// the run-after-results action for the workflow
        /// </summary>
        public string PostWorkflowAction
        {
            get
            {
                return _postWorkflowAction;
            }
            private set
            {
                _postWorkflowAction = value;
            }
        }

        /// <summary>
        /// The workflow document folder name
        /// </summary>
        public string DocumentFolder
        {
            get
            {
                return _documentFolder;
            }
            private set
            {
                Contract.Assert(!String.IsNullOrEmpty(value), "DocumentFolder is a required value and cannot be empty");
                _documentFolder = value;
            }
        }

        /// <summary>
        /// The workflow attribute set name
        /// </summary>
        public string OutputAttributeSet
        {
            get
            {
                return _outputAttributeSet;
            }
            private set
            {
                Contract.Assert(!String.IsNullOrEmpty(value), "OutputAttributeSet is a required value and cannot be empty");
                _outputAttributeSet = value;
            }
        }

        /// <summary>
        /// The name used to retrieve the per-fileID meta-data field (which contains the path(s) of the result file(s))
        /// Note that this field is OPTIONAL.
        /// </summary>
        public string OutputFileMetadataField
        {
            get
            {
                return _outputFileMetadataField;
            }
            private set
            {
                _outputFileMetadataField = value;
            }
        }

        /// <summary>
        /// database server name
        /// </summary>
        public string DatabaseServerName
        {
            get
            {
                Contract.Assert(!String.IsNullOrEmpty(_databaseServerName), "DatabaseServerName is empty");
                return _databaseServerName;
            }
            private set
            {
                Contract.Assert(!String.IsNullOrEmpty(value), "Attempt to set DatabaseServerName to an empty value");
                _databaseServerName = value;
            }
        }

        /// <summary>
        /// database name
        /// </summary>
        public string DatabaseName
        {
            get
            {
                Contract.Assert(!String.IsNullOrEmpty(_databaseName), "DatabaseName is empty");
                return _databaseName;
            }
            private set
            {
                Contract.Assert(!String.IsNullOrEmpty(value), "Attempt to set DatabaseName to an empty value");
                _databaseName = value;
            }
        }

        /// <summary>
        /// CTOR
        /// </summary>
        /// <param name="wd">instance of a WorkflowDefinition</param>
        /// <param name="databaseServerName">database server name</param>
        /// <param name="databaseName">database name</param>
        public Workflow(UCLID_FILEPROCESSINGLib.WorkflowDefinition wd, string databaseServerName, string databaseName)
        {
            Id = wd.ID;
            Name = wd.Name;
            Type = wd.Type;
            Description = wd.Description;
            StartAction = wd.StartAction;
            EndAction = wd.EndAction;
            PostWorkflowAction = wd.PostWorkflowAction;
            DocumentFolder = wd.DocumentFolder;
            OutputAttributeSet = wd.OutputAttributeSet;
            OutputFileMetadataField = wd.OutputFileMetadataField;

            DatabaseServerName = databaseServerName;
            DatabaseName = databaseName;
        }
    }

    /// <summary>
    /// State of the workflow - running, stopped, or error
    /// </summary>
    public enum WorkflowState
    {
        /// <summary>
        /// running
        /// </summary>
        Running = 1,

        /// <summary>
        /// stopped
        /// </summary>
        Stopped,

        /// <summary>
        /// error
        /// </summary>
        Error
    }

    /// <summary>
    /// overall status information of a workflow
    /// </summary>
    public class WorkflowStatus
    {
        /// <summary>
        /// error information, when Error.ErrorOccurred == true
        /// </summary>
        public ErrorInfo Error { get; set; }

        /// <summary>
        /// number of documents processing
        /// </summary>
        public uint NumberProcessing { get; set; }

        /// <summary>
        /// number of documents done processing
        /// </summary>
        public uint NumberDone { get; set; }

        /// <summary>
        /// number of documents that have failed
        /// </summary>
        public uint NumberFailed { get; set; }

        /// <summary>
        /// number of documents that have been ignored
        /// </summary>
        public uint NumberIgnored { get; set; }

        /// <summary>
        /// the state of the specified workflow
        /// </summary>
        public string State { get; set; }
    }
}