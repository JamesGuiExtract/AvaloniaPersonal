using AttributeDbMgrComponentsLib;
using Extract.SqlDatabase;
using Extract.Utilities;
using Extract.Web.ApiConfiguration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Web.ApiConfiguration.Services
{
    public sealed class ConfigurationDatabaseService : IConfigurationDatabaseService, IObservableConfigurationDatabaseService
    {
        readonly CompositeDisposable _disposables = new();
        readonly IFileProcessingDB _fileProcessingDB;
        readonly IAttributeDBMgr _attributeDBMgr;
        readonly Subject<Unit> _refreshCache;
        readonly DataTransferObjectSerializer _serializer = new(typeof(ICommonWebConfiguration).Assembly);

        /// <inheritdoc/>
        public IObservable<IList<Workflow>> Workflows { get; }

        /// <inheritdoc/>
        public IObservable<IList<WorkflowAction>> WorkflowActions { get; }

        /// <inheritdoc/>
        public IObservable<IList<WorkflowAction>> MainSequenceWorkflowActions { get; }

        /// <inheritdoc/>
        public IObservable<IList<WorkflowAction>> NonMainSequenceWorkflowActions { get; }

        /// <inheritdoc/>
        public IObservable<IList<string>> AttributeSetNames { get; }

        /// <inheritdoc/>
        public IObservable<IList<ICommonWebConfiguration>> Configurations { get; }

        /// <inheritdoc/>
        public IObservable<IList<ConfigurationForEditing>> ConfigurationsForEditing { get; }

        /// <inheritdoc/>
        public IObservable<IList<string>> MetadataFieldNames { get; }

        /// <inheritdoc/>
        public IObservable<decimal> VerificationSessionTimeoutMinutes { get; }

        IObservable<IList<IRedactionWebConfiguration>> RedactionWebConfigurations { get; }

        IObservable<IList<IDocumentApiWebConfiguration>> DocumentAPIWebConfigurations { get; }

        #region IConfigurationDatabaseService

        /// <inheritdoc/>
        IList<Workflow> IConfigurationDatabaseService.Workflows => Workflows
            .FirstAsync()
            .GetAwaiter()
            .GetResult();

        /// <inheritdoc/>
        IList<WorkflowAction> IConfigurationDatabaseService.WorkflowActions => WorkflowActions
            .FirstAsync()
            .GetAwaiter()
            .GetResult();

        /// <inheritdoc/>
        IList<string> IConfigurationDatabaseService.AttributeSetNames => AttributeSetNames
            .FirstAsync()
            .GetAwaiter()
            .GetResult();

        /// <inheritdoc/>
        IList<ICommonWebConfiguration> IConfigurationDatabaseService.Configurations => Configurations
            .FirstAsync()
            .GetAwaiter()
            .GetResult();

        /// <inheritdoc/>
        IList<IRedactionWebConfiguration> IConfigurationDatabaseService.RedactionWebConfigurations => RedactionWebConfigurations
            .FirstAsync()
            .GetAwaiter()
            .GetResult();

        /// <inheritdoc/>
        IList<IDocumentApiWebConfiguration> IConfigurationDatabaseService.DocumentAPIWebConfigurations => DocumentAPIWebConfigurations
            .FirstAsync()
            .GetAwaiter()
            .GetResult();

        /// <inheritdoc/>
        IList<string> IConfigurationDatabaseService.MetadataFieldNames => MetadataFieldNames
            .FirstAsync()
            .GetAwaiter()
            .GetResult();

        #endregion IConfigurationDatabaseService

        /// <summary>
        /// Create an instance and immediately query the database
        /// </summary>
        /// <param name="fileProcessingDB">The database API to use</param>
        public ConfigurationDatabaseService(IFileProcessingDB fileProcessingDB)
        {
            _fileProcessingDB = fileProcessingDB;
            _attributeDBMgr = new AttributeDBMgrClass
            {
                FAMDB = (FileProcessingDB)fileProcessingDB
            };

            _refreshCache = new();

            // Setup observables
            Workflows = _refreshCache
                .Select(_ => GetWorkflows())
                .Replay(1).RefCount();
            Workflows.Subscribe().DisposeWith(_disposables);

            WorkflowActions = Workflows.Select(workflows => workflows.SelectMany(GetWorkflowActions).ToList())
                .Replay(1)
                .RefCount();
            WorkflowActions.Subscribe().DisposeWith(_disposables);

            MainSequenceWorkflowActions = WorkflowActions
                .Select(actions => actions.Where(a => a.IsMainSequence).ToList())
                .Replay(1)
                .RefCount();
            MainSequenceWorkflowActions.Subscribe().DisposeWith(_disposables);

            NonMainSequenceWorkflowActions = WorkflowActions
                .Select(actions => actions.Where(a => !a.IsMainSequence).ToList())
                .Replay(1)
                .RefCount();
            NonMainSequenceWorkflowActions.Subscribe().DisposeWith(_disposables);

            AttributeSetNames = _refreshCache
                .Select(_ => GetAttributeSetNames())
                .Replay(1)
                .RefCount();
            AttributeSetNames.Subscribe().DisposeWith(_disposables);

            Configurations = _refreshCache
                .Select(_ => GetConfigurations())
                .Replay(1)
                .RefCount();
            Configurations.Subscribe().DisposeWith(_disposables);

            ConfigurationsForEditing = _refreshCache
                .Select(_ => GetConfigurationsForEditing())
                .Replay(1)
                .RefCount();
            ConfigurationsForEditing.Subscribe().DisposeWith(_disposables);

            MetadataFieldNames = _refreshCache
                .Select(_ => GetMetadataFieldNames())
                .Replay(1)
                .RefCount();
            MetadataFieldNames.Subscribe().DisposeWith(_disposables);

            RedactionWebConfigurations = Configurations
                .Select(configs => configs.OfType<IRedactionWebConfiguration>().ToList())
                .Replay(1)
                .RefCount();
            RedactionWebConfigurations.Subscribe().DisposeWith(_disposables);

            DocumentAPIWebConfigurations = Configurations
                .Select(configs => configs.OfType<IDocumentApiWebConfiguration>().ToList())
                .Replay(1)
                .RefCount();
            DocumentAPIWebConfigurations.Subscribe().DisposeWith(_disposables);

            VerificationSessionTimeoutMinutes = _refreshCache
                .Select(_ => GetVerificationSessionTimeoutMinutes())
                .Replay(1)
                .RefCount();
            VerificationSessionTimeoutMinutes.Subscribe().DisposeWith(_disposables);

            // Start the streams
            RefreshCache();
        }

        /// <inheritdoc/>
        public void RefreshCache()
        {
            _refreshCache.OnNext(Unit.Default);
        }

        /// <inheritdoc/>
        public void SaveConfiguration(ICommonWebConfiguration config)
        {
            try
            {
                _ = config ?? throw new ArgumentNullException(nameof(config));

                Guid configID = config.ID ?? throw new ExtractException("ELI53762", "Unexpected null configuration ID");

                IDomainObject domainObject = config as IDomainObject;
                ExtractException.Assert("ELI53761", "Configuration object cannot be serialized",
                    domainObject is not null);

                string json = Serialize(domainObject);

                using ExtractRoleConnection roleConnection = new(_fileProcessingDB.DatabaseServer, _fileProcessingDB.DatabaseName);
                roleConnection.Open();
                using var cmd = roleConnection.CreateCommand();
                cmd.CommandText =
                    "UPDATE [dbo].[WebAPIConfiguration] SET [Settings]=@Settings, [Name]=@Name WHERE [Guid]=@ConfigID\r\n" +
                    "IF @@ROWCOUNT = 0 INSERT INTO [dbo].[WebAPIConfiguration] ([Guid], [Name], [Settings]) VALUES (@ConfigID, @Name, @Settings)";
                cmd.Parameters.AddWithValue("@ConfigID", configID);
                cmd.Parameters.AddWithValue("@Name", config.ConfigurationName);
                cmd.Parameters.AddWithValue("@Settings", json);
                cmd.ExecuteNonQuery();

                RefreshCache();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53760");
            }
        }

        /// <inheritdoc/>
        public void DeleteConfiguration(Guid configID)
        {
            try
            {
                using ExtractRoleConnection roleConnection = new(_fileProcessingDB.DatabaseServer, _fileProcessingDB.DatabaseName);
                roleConnection.Open();
                using var cmd = roleConnection.CreateCommand();
                cmd.CommandText = "DELETE FROM [dbo].[WebAPIConfiguration] WHERE [Guid] = @ConfigID";
                cmd.Parameters.AddWithValue("@ConfigID", configID);
                cmd.ExecuteNonQuery();

                RefreshCache();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53763");
            }
        }

        #region Private Methods

        IList<WorkflowAction> GetWorkflowActions(Workflow workflow)
        {
            return _fileProcessingDB
                .GetWorkflowActions(workflow.WorkflowID)
                .ToIEnumerable<IVariantVector>()
                .Select(actionProperties =>
                {
                    string actionName = (string)actionProperties[1];
                    bool isMainSequence = (short)actionProperties[2] != 0;
                    return new WorkflowAction(workflow.WorkflowName, actionName, isMainSequence);
                })
                .ToList();
        }

        IList<string> GetAttributeSetNames()
        {
            return _attributeDBMgr.GetAllAttributeSetNames().GetKeys().ToIEnumerable<string>().ToList();
        }

        IList<ICommonWebConfiguration> GetConfigurations()
        {
            return _fileProcessingDB.GetWebAPIConfigurations()
                .ComToDictionary()
                .Select(nameToSettings =>
                {
                    ICommonWebConfiguration config = Deserialize(nameToSettings.Value);

                    if (config is not null)
                    {
                        // The name in the database table should override whatever is in the json
                        config.ConfigurationName = nameToSettings.Key;
                    }

                    return config;
                })
                .Where(config => config is not null)
                .ToList();
        }

        // Get the configurations with the ID property set to facilitate editing the configurations
        // Preserve the Name column and the ConfigurationName from the Settings column so that if
        // they are different it will be considered an edit to be saved (sync the two sources)
        IList<ConfigurationForEditing> GetConfigurationsForEditing()
        {
            List<ConfigurationForEditing> nameToConfig = new();

            using ExtractRoleConnection roleConnection = new(_fileProcessingDB.DatabaseServer, _fileProcessingDB.DatabaseName);
            roleConnection.Open();
            using var cmd = roleConnection.CreateCommand();
            cmd.CommandText = "SELECT [Guid], [Name], [Settings] FROM [dbo].[WebAPIConfiguration]";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var id = reader.GetGuid(0);
                var name = reader.GetString(1);
                var settings = reader.GetString(2);
                var config = Deserialize(settings);

                // Deserialize will log an exception and return null if unable to deserialize the settings
                if (config is not null)
                {
                    config.ID = id;
                    nameToConfig.Add(new(name, config));
                }
            }

            return nameToConfig;
        }

        IList<Workflow> GetWorkflows()
        {
            return _fileProcessingDB
                .GetWorkflows()
                .ComToDictionary()
                .Select(nameToID =>
                {
                    if (int.TryParse(nameToID.Value, out int id))
                    {
                        return new Workflow(nameToID.Key, id);
                    }
                    return null;
                })
                .Where(wf => wf is not null)
                .ToList();
        }

        IList<string> GetMetadataFieldNames()
        {
            return _fileProcessingDB.GetMetadataFieldNames().ToIEnumerable<string>().ToList();
        }

        decimal GetVerificationSessionTimeoutMinutes()
        {
            string timeoutValue = _fileProcessingDB.GetDBInfoSetting("VerificationSessionTimeout", false);
            if (decimal.TryParse(timeoutValue, out decimal timeoutSeconds))
            {
                return timeoutSeconds / 60;
            }

            return 0;
        }

        // Create an ICommonWebConfiguration from the JSON version of its DTO wrapped in a DataTransferObjectWithType
        ICommonWebConfiguration Deserialize(string json)
        {
            DataTransferObjectWithType dto = _serializer.Deserialize(json);
            if (dto is null)
            {
                var uex = new ExtractException("ELI53764", "Could not deserialize web API configuration object from settings!");
                uex.AddDebugData("Settings", json);

                return null;
            }

            return dto.CreateDomainObject() as ICommonWebConfiguration;
        }

        // Create a JSON version of a domain object
        string Serialize(IDomainObject domain)
        {
            _ = domain ?? throw new ArgumentNullException(nameof(domain));

            return _serializer.Serialize(domain.CreateDataTransferObject());
        }

        #endregion Private Methods
    }
}
