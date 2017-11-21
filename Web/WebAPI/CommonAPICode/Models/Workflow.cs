using UCLID_FILEPROCESSINGLib;
using System;
using static WebAPI.Utils;

namespace WebAPI.Models
{
    /// <summary>
    /// Workflow data model
    /// </summary>
    public class Workflow
    {
        private int _id;
        private string _name;
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
                Contract.Assert(!String.IsNullOrWhiteSpace(_name), "Attempt to get: {0}, bad value: {1}", GetMethodName(), _name);
                return _name;
            }
            private set
            {
                Contract.Assert(!String.IsNullOrWhiteSpace(value), "Attempt to set: {0}, to bad value: {1}", GetMethodName(), value);
                _name = value;
            }
        }

        /// <summary>
        /// the workflow Type value
        /// </summary>
        public EWorkflowType Type { get; set; }

        /// <summary>
        /// description of the workflow
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// the entry action for the workflow
        /// </summary>
        public string StartAction { get; set; }

        /// <summary>
        /// Gets or sets the post verify action.
        /// </summary>
        public string VerifyAction { get; set; }

        /// <summary>
        /// The action to be queued after verification
        /// </summary>
        public string PostVerifyAction { get; set; }

        /// <summary>
        /// The exit action for the workflow
        /// </summary>
        public string EndAction { get; set; }

        /// <summary>
        /// the run-after-results action for the workflow
        /// </summary>
        public string PostWorkflowAction { get; set; }

        /// <summary>
        /// The workflow document folder name - this value is optional
        /// </summary>
        public string DocumentFolder { get; set; }

        /// <summary>
        /// The workflow attribute set name
        /// </summary>
        public string OutputAttributeSet { get; set; }

        /// <summary>
        /// The name used to retrieve the per-fileID meta-data field (which contains the path(s) of the result file(s))
        /// Note that this field is OPTIONAL.
        /// </summary>
        public string OutputFileMetadataField { get; set; }

        /// <summary>
        /// database server name
        /// </summary>
        public string DatabaseServerName
        {
            get
            {
                return _databaseServerName;
            }
            private set
            {
                Contract.Assert(!String.IsNullOrWhiteSpace(value), "Attempt to set DatabaseServerName to an empty value");
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
                return _databaseName;
            }
            private set
            {
                Contract.Assert(!String.IsNullOrWhiteSpace(value), "Attempt to set DatabaseName to an empty value");
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
            VerifyAction = wd.VerifyAction;
            PostVerifyAction = wd.PostVerifyAction;
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
    /// overall status information of a workflow
    /// </summary>
    public class WorkflowStatus : IResultData
    {
        /// <summary>
        /// error information, when Error.ErrorOccurred == true
        /// </summary>
        public ErrorInfo Error { get; set; }

        /// <summary>
        /// number of documents processing
        /// </summary>
        public int NumberProcessing { get; set; }

        /// <summary>
        /// number of documents done processing
        /// </summary>
        public int NumberDone { get; set; }

        /// <summary>
        /// number of documents that have failed
        /// </summary>
        public int NumberFailed { get; set; }

        /// <summary>
        /// number of document submitted but that are no longer progressing through the workflow.
        /// </summary>
        public int NumberIncomplete { get; set; }
    }
}
