using AttributeDbMgrComponentsLib;
using Extract.Utilities;
using Extract.Web.ApiConfiguration.Models;
using Extract.Web.ApiConfiguration.Models.Dto;
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
    public class ConfigurationDatabaseService : IConfigurationDatabaseService, IObservableConfigurationDatabaseService
    {
        readonly CompositeDisposable _disposables = new();
        readonly IFileProcessingDB _fileProcessingDB;
        readonly IAttributeDBMgr _attributeDBMgr;
        readonly Subject<Unit> _refreshCache;
        static readonly DataTransferObjectSerializer _serializer = new(typeof(ICommonWebConfiguration).Assembly);

        /// <inheritdoc/>
        public IObservable<IList<Workflow>> Workflows { get; }

        /// <inheritdoc/>
        public IObservable<IList<WorkflowAction>> WorkflowActions { get; }

        /// <inheritdoc/>
        public IObservable<IList<string>> AttributeSetNames { get; }

        /// <inheritdoc/>
        public IObservable<IList<ICommonWebConfiguration>> Configurations { get; }

        /// <inheritdoc/>
        public IObservable<IList<string>> MetadataFieldNames { get; }

        /// <inheritdoc/>
        public IObservable<IList<IRedactionWebConfiguration>> RedactionWebConfigurations { get; }

        /// <inheritdoc/>
        public IObservable<IList<IDocumentApiWebConfiguration>> DocumentAPIWebConfigurations { get; }

        /// <inheritdoc/>
        IList<Workflow> IConfigurationDatabaseService.Workflows => Workflows.FirstAsync().GetAwaiter().GetResult();

        /// <inheritdoc/>
        IList<WorkflowAction> IConfigurationDatabaseService.WorkflowActions => WorkflowActions.FirstAsync().GetAwaiter().GetResult();

        /// <inheritdoc/>
        IList<string> IConfigurationDatabaseService.AttributeSetNames => AttributeSetNames.FirstAsync().GetAwaiter().GetResult();

        /// <inheritdoc/>
        IList<ICommonWebConfiguration> IConfigurationDatabaseService.Configurations => Configurations.FirstAsync().GetAwaiter().GetResult();

        /// <inheritdoc/>
        IList<string> IConfigurationDatabaseService.MetadataFieldNames => MetadataFieldNames.FirstAsync().GetAwaiter().GetResult();

        /// <inheritdoc/>
        IList<IRedactionWebConfiguration> IConfigurationDatabaseService.RedactionWebConfigurations => RedactionWebConfigurations.FirstAsync().GetAwaiter().GetResult();

        /// <inheritdoc/>
        IList<IDocumentApiWebConfiguration> IConfigurationDatabaseService.DocumentAPIWebConfigurations => DocumentAPIWebConfigurations.FirstAsync().GetAwaiter().GetResult();

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

            this._refreshCache = new();

            // Setup observables
            Workflows = _refreshCache
                .Select(_ => GetWorkflows())
                .Replay(1).RefCount();
            Workflows.Subscribe().DisposeWith(_disposables);

            WorkflowActions = Workflows.Select(workflows => workflows.SelectMany(GetWorkflowActions).ToList())
                .Replay(1)
                .RefCount();
            WorkflowActions.Subscribe().DisposeWith(_disposables);

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

            MetadataFieldNames = _refreshCache
                .Select(_ => GetMetadataFieldNames())
                .Replay(1)
                .RefCount();
            MetadataFieldNames.Subscribe().DisposeWith(_disposables);

            RedactionWebConfigurations = Configurations.Select(configs => configs.OfType<IRedactionWebConfiguration>().ToList())
            .Replay(1)
            .RefCount();
            RedactionWebConfigurations.Subscribe().DisposeWith(_disposables);

            DocumentAPIWebConfigurations = Configurations.Select(configs => configs.OfType<IDocumentApiWebConfiguration>().ToList())
            .Replay(1)
            .RefCount();
            DocumentAPIWebConfigurations.Subscribe().DisposeWith(_disposables);

            // Start the streams
            RefreshCache();
        }

        /// <inheritdoc/>
        public void RefreshCache()
        {
            _refreshCache.OnNext(Unit.Default);
        }

        IList<WorkflowAction> GetWorkflowActions(Workflow workflow)
        {
            return _fileProcessingDB
                .GetWorkflowActions(workflow.WorkflowID)
                .ToIEnumerable<IVariantVector>()
                .Select(actionProperties => (string)actionProperties[1])
                .Select(actionName => new WorkflowAction(workflow.WorkflowName, actionName))
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
                .Select(nameToSettings => Deserialize(nameToSettings.Value))
                .ToList();
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

        // Create an ICommonWebConfiguration from the JSON version of its DTO wrapped in a DataTransferObjectWithType
        public ICommonWebConfiguration Deserialize(string json)
        {
            DataTransferObjectWithType dto = _serializer.Deserialize(json);
            return dto.CreateDomainObject() as ICommonWebConfiguration;
        }


        // Create a JSON version of a domain object
        public static string Serialize(IDomainObject domain)
        {
            _ = domain ?? throw new ArgumentNullException(nameof(domain));

            return _serializer.Serialize(domain.CreateDataTransferObject());
        }
    }
}
