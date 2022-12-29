using DynamicData;
using Extract;
using Extract.Utilities;
using ExtractDataExplorer.Models;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace ExtractDataExplorer.Services
{
    public sealed class AttributeTreeService : IAttributeTreeService
    {
        readonly ComProcessor _comProcessor;

        bool _isDisposed;

        /// <inheritdoc/>
        public IObservableCache<AttributeModel, Guid> AttributeModels => _comProcessor.AttributeModels;

        /// <inheritdoc/>
        public IObservable<bool> AttributesLoaded => _comProcessor.AttributesLoaded
            .Select(result => result.Match(isLoaded => isLoaded, error =>
            {
                RxApp.DefaultExceptionHandler.OnNext(error);
                return false;
            }));

        /// <inheritdoc/>
        public IObservable<IAttributeFilter> AttributeFilter => _comProcessor.AttributeFilter;

        public AttributeTreeService(IAFUtilityFactory afutilityFactory)
        {
            _comProcessor = new ComProcessor(afutilityFactory);
        }

        /// <inheritdoc/>
        public async Task LoadAttributesAsync(string? path)
        {
            try
            {
                // Setup a task source that will complete when the attribute loader thread finishes an operation
                TaskCompletionSource<Union<bool, Exception>> isLoadedTaskSource = new();
                using var cleanup = _comProcessor.AttributesLoaded.Subscribe(
                    onNext: isLoadedTaskSource.SetResult,
                    onError: isLoadedTaskSource.SetException);

                _comProcessor.AddLoadRequest(new(path));

                await isLoadedTaskSource.Task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53931");
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ApplyAttributeFilterAsync(FilterRequest filterRequest)
        {
            try
            {
                // Setup a task source that will complete when the attribute loader thread finishes an operation
                TaskCompletionSource<bool> filterChangedTaskSource = new();

                // Skip the current value (BehaviorSubject always has a value on subscribe)
                using var cleanup = _comProcessor.AttributeFilter.Skip(1).Subscribe(
                    onNext: filter => filterChangedTaskSource.SetResult(!filter.IsFilterEmpty),
                    onError: filterChangedTaskSource.SetException);

                _comProcessor.AddFilterRequest(filterRequest);

                return await filterChangedTaskSource.Task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53932");
            }
        }

        /// <inheritdoc/>
        public void RemoveAttributeFilter()
        {
            _comProcessor.RemoveAttributeFilter();
        }

        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                // Dispose managed state (managed objects)
                if (disposing)
                {
                    _comProcessor.Dispose();
                }

                _isDisposed = true;
            }
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
