using DynamicData;
using ExtractDataExplorer.Models;
using System;

namespace ExtractDataExplorer.Services
{
    public interface IAttributeFilter
    {
        /// <summary>
        /// Function to determine whether or not a tree node should be kept
        /// </summary>
        Func<Node<AttributeModel, Guid>, bool> FilterPredicate { get; }

        /// <summary>
        /// Function to determine whether or not a tree node should be expanded
        /// </summary>
        Func<Node<AttributeModel, Guid>, bool> ExpandPredicate { get; }

        /// <summary>
        /// Whether or not the filter is empty
        /// </summary>
        /// <remarks>
        /// When the filte is empty, every node will match the <see cref="FilterPredicate"/>
        /// and no node will match the <see cref="ExpandPredicate"/>
        /// </remarks>
        bool IsFilterEmpty { get; }
    }
}
