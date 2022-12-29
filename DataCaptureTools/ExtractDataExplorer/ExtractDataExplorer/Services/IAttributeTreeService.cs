using DynamicData;
using ExtractDataExplorer.Models;
using System;
using System.Threading.Tasks;

namespace ExtractDataExplorer.Services
{
    public interface IAttributeTreeService : IDisposable
    {
        /// <summary>
        /// Emits a value whenever the cache of AttributeModels is updated (i.e., when a VOA file is loaded)
        /// </summary>
        IObservableCache<AttributeModel, Guid> AttributeModels { get; }

        /// <summary>
        /// Emits a value whenever a VOA file is loaded, after a new filter is calculated, if applicable
        /// </summary>
        IObservable<bool> AttributesLoaded { get; }

        /// <summary>
        /// Contains the current filter and emits a value whenever the filter changes
        /// </summary>
        IObservable<IAttributeFilter> AttributeFilter { get; }

        /// <summary>
        /// Load and convert attributes to models
        /// </summary>
        Task LoadAttributesAsync(string? path);

        /// <summary>
        /// Update the current filter
        /// </summary>
        Task<bool> ApplyAttributeFilterAsync(FilterRequest filterRequest);

        /// <summary>
        /// Remove the current filter
        /// </summary>
        void RemoveAttributeFilter();
    }
}
