using DynamicData;
using Extract;
using Extract.AttributeFinder;
using Extract.Utilities;
using ExtractDataExplorer.Models;
using ReactiveUI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;

using TaskMessage = Extract.Utilities.Union<ExtractDataExplorer.Models.LoadRequest, ExtractDataExplorer.Models.FilterRequest>;

namespace ExtractDataExplorer.Services
{
    /// <summary>
    /// Service dedicated to dealing with finicky COM objects that need to be kept on a single thread
    /// </summary>
    internal class ComProcessor : IAttributeFilter, IDisposable
    {
        readonly IAFUtilityFactory _afutilityFactory;

        // Info about the currently applied filter
        string? _attributeFilterQuery;
        AttributeQueryType _attributeFilterQueryType;
        bool _startXPathAtElement;
        HashSet<int>? _attributeFilterPages;
        HashSet<Guid>? _attributeIDsMatchingFilter;

        // Misc backing fields
        readonly Subject<Union<bool, Exception>> _attributesLoadedSubject = new();
        readonly BehaviorSubject<IAttributeFilter> _attributeFilterSubject;
        readonly BlockingCollection<TaskMessage> _tasks = new();
        readonly SourceCache<AttributeModel, Guid> _attributeModelSource = new(x => x.ID);

        // COM thread maintenance
        readonly Thread _comObjectThread;
        readonly CancellationTokenSource _stopComObjectThread;
        CancellationToken _stopComObjectThreadToken;
        readonly ManualResetEventSlim _isComObjectThreadStopped = new(false);

        bool _isDisposing;
        bool _isDisposed;

        /// <summary>
        /// Request that the COM thread load a VOA file
        /// </summary>
        /// <remarks>This will trigger a value to be emitted from the <see cref="AttributeModels"/>
        /// and <see cref="AttributesLoaded"/> streams</remarks>
        public void AddLoadRequest(LoadRequest request)
        {
            _tasks.Add(new(request));
        }

        /// <summary>
        /// Request that the COM thread update the filter
        /// </summary>
        /// <remarks>This will trigger a value to be emitted from the <see cref="AttributeFilter"/> stream</remarks>
        public void AddFilterRequest(FilterRequest request)
        {
            _tasks.Add(new(request));
        }

        /// <summary>
        /// Remove the current filter
        /// </summary>
        /// <remarks>This will trigger a value to be emitted from the <see cref="AttributeFilter"/> stream</remarks>
        public void RemoveAttributeFilter()
        {
            _attributeIDsMatchingFilter = null;
            _attributeFilterSubject.OnNext(this);
        }

        /// <summary>
        /// Emits a value whenever the cache of AttributeModels is updated (i.e., when a VOA file is loaded)
        /// </summary>
        public IObservableCache<AttributeModel, Guid> AttributeModels => _attributeModelSource.AsObservableCache();

        /// <summary>
        /// Emits a value whenever a VOA file is loaded, after a new filter is calculated, if applicable
        /// </summary>
        public IObservable<Union<bool, Exception>> AttributesLoaded => _attributesLoadedSubject;

        /// <summary>
        /// Contains the current filter and emits a value whenever the filter changes
        /// </summary>
        public IObservable<IAttributeFilter> AttributeFilter => _attributeFilterSubject;

        #region IAttributeFilter

        /// <inheritdoc/>
        public Func<Node<AttributeModel, Guid>, bool> FilterPredicate { get; }

        /// <inheritdoc/>
        public Func<Node<AttributeModel, Guid>, bool> ExpandPredicate { get; }

        /// <inheritdoc/>
        public bool IsFilterEmpty => _attributeIDsMatchingFilter is null;

        #endregion IAttributeFilter

        /// <summary>
        /// Create an instance
        /// </summary>
        public ComProcessor(IAFUtilityFactory afutilityFactory)
        {
            _afutilityFactory = afutilityFactory;

            FilterPredicate = node => node.ShouldKeep(_attributeIDsMatchingFilter);
            ExpandPredicate = node => node.ShouldExpand(_attributeIDsMatchingFilter);
            _attributeFilterSubject = new(this);

            _stopComObjectThread = new CancellationTokenSource();
            _stopComObjectThreadToken = _stopComObjectThread.Token;

            _comObjectThread = new Thread(ComObjectThreadStart);
            _comObjectThread.SetApartmentState(ApartmentState.STA);
            _comObjectThread.Start();
        }

        // Run by a dedicated thread for processing COM objects (Attributes and SpatialStrings)
        private void ComObjectThreadStart()
        {
            try
            {
                IAFUtility afutil = _afutilityFactory.Create();
                IAttribute? tree = null;

                // Use the blocking collection to keep this thread alive so that all
                // COM operations take place on a single, dedicated thread
                while (!_stopComObjectThreadToken.IsCancellationRequested)
                {
                    try
                    {
                        _tasks.Take(_stopComObjectThreadToken).Match(
                            loadRequest => tree = LoadAttributeModels(loadRequest, afutil),
                            filterRequest => FilterAttributeTree(filterRequest, tree, afutil));
                    }
                    catch (OperationCanceledException)
                    {
                        // Operation canceled exception used to stop the tasks.Take operation
                    }
                }
            }
            catch (Exception ex) when (!_isDisposing)
            {
                ex.ExtractLog("ELI53936");
            }
            finally
            {
                _isComObjectThreadStopped.Set();
            }
        }

        // Load a VOA file and create models that can be used by any thread
        private IAttribute? LoadAttributeModels(LoadRequest loadRequest, IAFUtility afutil)
        {
            IAttribute? tree = null;
            try
            {
                tree = LoadAttributesFromFile(loadRequest.Path, afutil);

                if (_attributeIDsMatchingFilter is not null)
                {
                    _attributeIDsMatchingFilter = GetAttributeIDsMatchingFilter(tree, afutil);
                }

                IList<AttributeModel>? attributes = BuildAttributeModelsFromAttributeHierarchy(tree);

                if (attributes is null)
                {
                    _attributeModelSource.Clear();
                    _attributesLoadedSubject.OnNext(new Union<bool, Exception>(false));
                }
                else
                {
                    _attributeModelSource.Edit(cache =>
                    {
                        var noLongerInCache = cache.Keys.Except(attributes.Select(x => x.ID)).ToList();
                        cache.RemoveKeys(noLongerInCache);
                        cache.AddOrUpdate(attributes);
                    });

                    _attributesLoadedSubject.OnNext(new Union<bool, Exception>(true));
                }
            }
            catch (Exception ex) when (!_isDisposing)
            {
                _attributesLoadedSubject.OnNext(new Union<bool, Exception>(ex.AsExtract("ELI53933")));
            }

            return tree;
        }

        // Load a VOA file
        private static IAttribute? LoadAttributesFromFile(string? path, IAFUtility afutil)
        {
            if (path is null)
            {
                return null;
            }

            IAttribute tree = new AttributeClass();

            try
            {
                tree.SubAttributes = afutil.GetAttributesFromFile(path);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI53942", "Could not load attributes from file", ex);
            }

            EnsureInstanceIDsAreUnique(tree);

            return tree;
        }

        // Check that the GUIDs for the attributes are distinct, update if necessary
        private static void EnsureInstanceIDsAreUnique(IAttribute tree)
        {
            HashSet<Guid> knownIDs = new();

            foreach (var identifiable in tree.EnumerateDepthFirst().Cast<IIdentifiableObject>())
            {
                while (!knownIDs.Add(identifiable.InstanceGUID))
                {
                    ((IAttribute)identifiable).SetGUID(Guid.NewGuid());
                }
            }
        }

        // Create models that can be used by any thread from the COM objects that are basically restricted to the thread they were created on
        private static IList<AttributeModel>? BuildAttributeModelsFromAttributeHierarchy(IAttribute? tree)
        {
            if (tree is null)
            {
                return null;
            }

            static IEnumerable<AttributeModel> BuildAttributeModelsFromAttributeHierarchy(IAttribute tree, Guid? parentID, int idx)
            {
                Guid id = ((IIdentifiableObject)tree).InstanceGUID;

                yield return new AttributeModel(tree, id, parentID, idx);

                var children = tree.SubAttributes
                    .ToIEnumerable<IAttribute>()
                    .Select((subTree, childIdx) => (subTree, childIdx));

                foreach (var (subTree, childIdx) in children)
                {
                    foreach (var node in BuildAttributeModelsFromAttributeHierarchy(subTree, id, childIdx))
                    {
                        yield return node;
                    }
                }
            }

            return BuildAttributeModelsFromAttributeHierarchy(tree, null, 0).Skip(1).ToList();
        }

        // Calculate the page range and update the set of attribute GUIDs that match the specified filter
        private void FilterAttributeTree(FilterRequest filterRequest, IAttribute? tree, IAFUtility afutil)
        {
            try
            {
                _attributeFilterQuery = filterRequest.Query;
                _attributeFilterQueryType = filterRequest.QueryType;
                _startXPathAtElement = filterRequest.StartAtElement;
                string? pageFilter = filterRequest.IsPageFilterEnabled
                    ? filterRequest.PageRange
                    : null;

                if (tree is null || string.IsNullOrWhiteSpace(pageFilter))
                {
                    _attributeFilterPages = null;
                }
                // Allow special case of 0 as page filter (TODO: Use a more generic numeric range parser?)
                else if (pageFilter!.Trim().Equals("0", StringComparison.Ordinal))
                {
                    _attributeFilterPages = new HashSet<int>() { 0 };
                }
                else
                {
                    UtilityMethods.ValidatePageNumbers(pageFilter);
                    var pageCount = tree.EnumerateDepthFirst()
                        .Select(attribute =>
                            attribute.Value.HasSpatialInfo()
                            ? attribute.Value.GetFirstPageNumber()
                            : 0)
                        .Max();

                    _attributeFilterPages = new(UtilityMethods.GetPageNumbersFromString(pageFilter, pageCount, false));
                }

                _attributeIDsMatchingFilter = GetAttributeIDsMatchingFilter(tree, afutil);
                _attributeFilterSubject.OnNext(this);
            }
            catch (Exception ex) when (!_isDisposing)
            {
                RxApp.DefaultExceptionHandler.OnNext(ex);
            }
        }

        // Update the set of attribute GUIDs that match the specified filter
        private HashSet<Guid>? GetAttributeIDsMatchingFilter(IAttribute? tree, IAFUtility afutil)
        {
            if (tree is null || _attributeFilterPages is null && string.IsNullOrWhiteSpace(_attributeFilterQuery))
            {
                return null;
            }

            HashSet<Guid>? filtered = null;

            if (!string.IsNullOrWhiteSpace(_attributeFilterQuery))
            {
                if (_attributeFilterQueryType == AttributeQueryType.AFQuery)
                {
                    filtered = afutil.QueryAttributes(tree.SubAttributes, _attributeFilterQuery, false)
                        .ToIEnumerable<IIdentifiableObject>()
                        .Select(attribute => attribute.InstanceGUID)
                        .ToHashSet();
                }
                else
                {
                    XPathContext context = new(tree.SubAttributes);

                    string startAtQuery = _startXPathAtElement ? "/*" : "/";
                    XPathContext.XPathIterator startAt = context.GetIterator(startAtQuery);
                    startAt.MoveNext();

                    filtered = context.FindAllOfType<IIdentifiableObject>(_attributeFilterQuery, startAt)
                        .Select(attribute => attribute.InstanceGUID)
                        .ToHashSet();
                }
            }

            if (_attributeFilterPages is not null)
            {
                var filteredByPage = tree.EnumerateDepthFirst()
                    .Where(attribute =>
                    {
                        int page =
                            attribute.Value.HasSpatialInfo()
                            ? attribute.Value.GetFirstPageNumber()
                            : 0;
                        return _attributeFilterPages.Contains(page);
                    })
                    .Cast<IIdentifiableObject>()
                    .Select(attribute => attribute.InstanceGUID)
                    .ToHashSet();

                if (filtered is not null)
                {
                    filtered.IntersectWith(filteredByPage);
                }
                else
                {
                    filtered = filteredByPage;
                }
            }

            return filtered;
        }

        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                _isDisposing = true;

                // Free unmanaged resources (stop the COM object processing thread)
                _stopComObjectThread.Cancel();

                // Let the thread stop gracefully, if it can do so in a second or less
                _isComObjectThreadStopped.Wait(1000);

                // Stop the thread before it can attempt to use the objects that are about to be disposed of
                _comObjectThread.Abort();

                // Dispose managed state (managed objects)
                if (disposing)
                {
                    _stopComObjectThread.Dispose();
                    _isComObjectThreadStopped.Dispose();
                    _attributesLoadedSubject.Dispose();
                    _attributeFilterSubject.Dispose();
                    _attributeModelSource.Dispose();
                    _tasks.Dispose();
                }

                _isDisposed = true;
            }
        }

        // Override finalizer because 'Dispose(bool disposing)' has code to free unmanaged resources
        ~ComProcessor()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
