using Extract.AttributeFinder;
using Extract.SqlDatabase;
using Extract.Utilities;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace Extract.ETL
{
    public abstract class MachineLearningService : DatabaseService
    {
        #region Constants

        /// <summary>
        /// Query to get all MLData associated with a name
        /// </summary>
        static readonly string _GET_ALL_MLDATA_FOR_NAME =
            @"SELECT * FROM MLData WHERE MLModelID IN
                (SELECT ID FROM MLModel WHERE Name = @Name)";

        static readonly string _MARK_ALL_DATA_FOR_DELETION =
            @"UPDATE d
                SET CanBeDeleted = 'True'
                FROM MLData d
                JOIN MLModel ON d.MLModelID = MLModel.ID
                WHERE Name = @Name
                AND CanBeDeleted = 'False'";

        #endregion Constants

        #region Fields

        string _modelName;

        #endregion Fields

        #region Properties

        /// <summary>
        /// The name used to group training/testing data in FAMDB (table MLModel)
        /// </summary>
        [DataMember]
        public string ModelName
        {
            get
            {
                return _modelName;
            }
            set
            {
                if (value != _modelName)
                {
                    _modelName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the DatabaseServer of the <see cref="Container"/> if non-null, else returns base implementation
        /// </summary>
        public override string DatabaseServer
        {
            get
            {
                if (Container is DatabaseService parent)
                {
                    return parent.DatabaseServer;
                }
                else
                {
                    return base.DatabaseServer;
                }
            }
        }

        /// <summary>
        /// Gets the DatabaseName of the <see cref="Container"/> if non-null, else returns base implementation
        /// </summary>
        public override string DatabaseName
        {
            get
            {
                if (Container is DatabaseService parent)
                {
                    return parent.DatabaseName;
                }
                else
                {
                    return base.DatabaseName;
                }
            }
        }

        /// <summary>
        /// Used by the <see cref="ITrainingCoordinator"/> to save statuses for the contained
        /// instances
        /// </summary>
        [DataMember]
        public Guid Guid { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Whether enabled. Serialized to simplify keeping track of this information when
        /// contained in a <see cref="ITrainingCoordinator"/>.
        /// </summary>
        [DataMember]
        public override bool Enabled
        {
            get => base.Enabled;
            set => base.Enabled = value;
        }

        /// <summary>
        /// The instance that contains this service
        /// </summary>
        public ITrainingCoordinator Container { get; set; }

        /// <summary>
        /// Used as a namespace to prefix MLModel.Names in the database
        /// </summary>
        /// <remarks>This is empty if this instance is not contained in a <see cref="ITrainingCoordinator"/></remarks>
        public string ModelNamePrefix => string.IsNullOrEmpty(Container?.ProjectName)
            ? ""
            : Container.ProjectName + "::";

        /// <summary>
        /// The directory where referenced file's paths are based
        /// </summary>
        /// <remarks>This is null if this instance is not contained in a <see cref="ITrainingCoordinator"/></remarks>
        public string RootDir => Container?.RootDir;

        /// <summary>
        /// The <see cref="ModelName"/> prefixed by the <see cref="ModelNamePrefix"/>
        /// </summary>
        /// <remarks>The setter will update the <see cref="ModelName"/> by trimming the prefix
        /// from the new value and using what's left</remarks>
        public string QualifiedModelName
        {
            get
            {
                return string.IsNullOrEmpty(ModelName)
                    ? ""
                    : ModelNamePrefix + ModelName;
            }
            set
            {
                try
                {
                    if (string.IsNullOrEmpty(ModelNamePrefix)
                        || !value.StartsWith(ModelNamePrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        ModelName = value;
                    }
                    else
                    {
                        ModelName = value.Remove(0, ModelNamePrefix.Length);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI45809");
                }
            }
        }

        /// <summary>
        /// Action for the service to use to append text to a log
        /// </summary>
        public Action<string> Log { get; set; }

        #endregion Properties

        #region Abstract Properties

        /// <summary>
        /// The last AttributeSetForFile.ID or MLData.ID record processed by the service
        /// </summary>
        public abstract long LastIDProcessed { get; set; }

        #endregion Abstract Properties

        #region Public Methods

        /// <summary>
        /// Clones with a new Guid
        /// </summary>
        public MachineLearningService Duplicate()
        {
            var clone = (MachineLearningService)Clone();
            clone.Guid = Guid.NewGuid();
            return clone;
        }

        /// <summary>
        /// Sets CanBeDeleted to true for all ML Data for this model name
        /// </summary>
        public void MarkAllDataForDeletion()
        {
            try
            {
                using var connection = new ExtractRoleConnection(DatabaseServer, DatabaseName);
                connection.Open();


                using var cmd = connection.CreateCommand();

                cmd.CommandText = _MARK_ALL_DATA_FOR_DELETION;
                cmd.Parameters.AddWithValue("@Name", QualifiedModelName);

                // Set the timeout so that it waits indefinitely
                cmd.CommandTimeout = 0;

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    var ue = new ExtractException("ELI45970", "Failed to mark data for deletion", ex);
                    ue.AddDebugData("Database Server", DatabaseServer, false);
                    ue.AddDebugData("Database Name", DatabaseName, false);
                    throw ue;
                }

            }
            catch (Exception ex)
            {
                var ue = ex.AsExtract("ELI45969");
                ue.AddDebugData("Database Server", DatabaseServer, false);
                ue.AddDebugData("Database Name", DatabaseName, false);
                throw ue;
            }   
        }

        #endregion Public Methods

        #region Abstract Methods

        /// <summary>
        /// When overridden by a derived class, will return the current status
        /// </summary>
        public abstract DatabaseServiceStatus Status { get; }

        /// <summary>
        /// When overridden by a derived class, will return the count of new MLData records
        /// since last processing occurred
        /// </summary>
        public abstract int CalculateUnprocessedRecordCount();

        /// <summary>
        /// When overridden by a derived class, will change an answer, e.g., a doctype, in
        /// the configured LearningMachine's Encoder and will also update the MLData stored in
        /// the DB
        /// </summary>
        /// <param name="oldAnswer">The answer to be changed (must exist in the LearningMachine)</param>
        /// <param name="newAnswer">The new answer to change to (must not exist in the LearningMachine)</param>
        /// <param name="silent">Whether to display exceptions and messages</param>
        public abstract bool ChangeAnswer(string oldAnswer, string newAnswer, bool silent);

        /// <summary>
        /// When overridden by a derived class, will update the properties of this object using the provided <see cref="DatabaseServiceStatus"/>
        /// </summary>
        public abstract void UpdateFromStatus(DatabaseServiceStatus status);

        #endregion Abstract Methods

        #region Protected Methods

        protected bool ChangeAnswer(string oldAnswer, string newAnswer, string lmPath, bool silent)
        {
            try
            {
                // Prevent unneeded work but allow a change in case
                if (string.Equals(oldAnswer, newAnswer, StringComparison.Ordinal))
                {
                    var ex = new ExtractException("ELI45991", "Both values are the same");
                    if (silent)
                    {
                        ex.Log();
                    }
                    else
                    {
                        ex.Display();
                    }
                    return false;
                }

                // Change in the data encoder
                bool encoderUpdated = false;
                try
                {
                    using (var lm = LearningMachine.Load(lmPath))
                    {
                        if (lm.Encoder.AnswerNameToCode.ContainsKey(oldAnswer))
                        {
                            lm.Encoder.ChangeAnswer(oldAnswer, newAnswer);
                            lm.Save(lmPath);
                            encoderUpdated = true;
                        }
                        else if (!silent)
                        {
                            string message = UtilityMethods.FormatCurrent($"Answer \"{oldAnswer}\" doesn't exist in",
                                $" \"{lmPath}\"");
                            UtilityMethods.ShowMessageBox(message, "Nothing to change", false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!silent)
                    {
                        ex.ExtractDisplay("ELI45990");
                    }
                    return false;
                }

                // Change ML Data in the DB
                int rowsChanged = 0;

                using var connection = new ExtractRoleConnection(DatabaseServer, DatabaseName);
                connection.Open();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = _GET_ALL_MLDATA_FOR_NAME;
                cmd.Parameters.AddWithValue("@Name", QualifiedModelName);
                using var adapter = new SqlDataAdapter(cmd.BaseSqlCommand);
                using var dt = new DataTable { Locale = CultureInfo.CurrentCulture };

                var builder = new SqlCommandBuilder();
                builder.DataAdapter = adapter;
                adapter.Fill(dt);
                foreach (DataRow row in dt.Rows)
                {
                    string csv = row.Field<string>("Data");
                    using (var sr = new StringReader(csv))
                    using (var csvReader = new TextFieldParser(sr) { Delimiters = new[] { "," } })
                    {
                        var fields = csvReader.ReadFields();
                        string answer = fields[0];
                        if (string.Equals(answer, oldAnswer, StringComparison.OrdinalIgnoreCase))
                        {
                            fields[0] = newAnswer;
                            row["Data"] = string.Join(",", fields.Select(f => f.QuoteIfNeeded("\"", ",")));
                        }
                    }
                }
                rowsChanged = adapter.Update(dt);

                bool changedAnything = encoderUpdated || rowsChanged > 0;

                if (silent)
                {
                    return changedAnything;
                }
                else if (!changedAnything)
                {
                    string message = UtilityMethods.FormatCurrent($"Attempted to change \"{oldAnswer}\" to \"{newAnswer}\"",
                        $" but nothing was changed");
                    UtilityMethods.ShowMessageBox(message, "Failure", true);
                }
                else
                {
                    string message = UtilityMethods.FormatCurrent($"Changed \"{oldAnswer}\" to \"{newAnswer}\"");
                    if (encoderUpdated)
                    {
                        message += UtilityMethods.FormatCurrent($"\r\n\r\nUpdated \"{lmPath}\"");
                    }
                    else
                    {
                        message += UtilityMethods.FormatCurrent($"\r\n\r\nDid not update \"{lmPath}\"");
                    }
                    message += UtilityMethods.FormatCurrent($"\r\n\r\nUpdated {rowsChanged} DB records");
                    UtilityMethods.ShowMessageBox(message, "Success", false);
                }

                return changedAnything;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45846");
            }
        }

        /// <summary>
        /// Invokes the <see cref="Log"/> action if one is defined
        /// </summary>
        /// <param name="value">The string parameter for the <see cref="Log"/> action</param>
        protected void AppendToLog(string value)
        {
            Log?.Invoke(value);
        }

        #endregion Protected Methods
    }
}
