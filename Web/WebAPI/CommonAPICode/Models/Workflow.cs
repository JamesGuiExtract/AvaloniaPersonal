using UCLID_FILEPROCESSINGLib;
using System;
using static WebAPI.Utils;
using System.Collections.Generic;

namespace WebAPI.Models
{
    /// <summary>
    /// Describes a workflow configuration.
    /// </summary>
    public class Workflow
    {
        private int _id;
        private string _name;
        private string _databaseServerName;
        private string _databaseName;

        /// <summary>
        /// The ID of the Workflow, used to get-by-ID
        /// </summary>
        public int Id
        {
            get
            {
                return _id;
            }
            private set
            {
                HTTPError.Assert("ELI46379", value > 0, "Invalid workflow ID", ("WorkflowID", value, false));
                _id = value;
            }
        }

        /// <summary>
        /// The name of the workflow
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            private set
            {
                HTTPError.Assert("ELI46381", !String.IsNullOrWhiteSpace(value),
                    "Invalid workflow name", ("WorkflowName", value, false));
                _name = value;
            }
        }

        /// <summary>
        /// The workflow type
        /// </summary>
        public EWorkflowType Type { get; set; }

        /// <summary>
        /// Description of the workflow
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The entry action for the workflow
        /// </summary>
        public string StartAction { get; set; }

        /// <summary>
        /// The verify action
        /// </summary>
        public string EditAction { get; set; }

        /// <summary>
        /// The action to be queued after verification
        /// </summary>
        public string PostEditAction { get; set; }

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
                HTTPError.Assert("ELI46382", !String.IsNullOrWhiteSpace(value),
                    "Attempt to set DatabaseServerName to an empty value");
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
                HTTPError.Assert("ELI46383", !String.IsNullOrWhiteSpace(value),
                    "Attempt to set DatabaseName to an empty value");
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
            EditAction = wd.EditAction;
            PostEditAction = wd.PostEditAction;
            EndAction = wd.EndAction;
            PostWorkflowAction = wd.PostWorkflowAction;
            DocumentFolder = wd.DocumentFolder;
            OutputAttributeSet = wd.OutputAttributeSet;
            OutputFileMetadataField = wd.OutputFileMetadataField;

            DatabaseServerName = databaseServerName;
            DatabaseName = databaseName;
        }
    }
}
