using DynamicData;
using ExtractDataExplorer.Models;
using ExtractDataExplorer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace ExtractDataExplorer.Services
{
    /// <summary>
    /// Helper methods used by <see cref="IAttributeFilter"/>
    /// </summary>
    internal static class AttributeTreeExtensionMethods
    {
        /// <summary>
        /// Whether a node has a child that matches one of the specified IDs
        /// </summary>
        public static bool HasMatchingChild(this Node<AttributeModel, Guid> node, HashSet<Guid> idsToMatch)
        {
            bool IsMatchOrHasMatchingChild(Node<AttributeModel, Guid> node)
            {
                return idsToMatch.Contains(node.Key)
                    || node.Children.Items.Any(node => IsMatchOrHasMatchingChild(node));
            }

            return node.Children.Items.Any(IsMatchOrHasMatchingChild);
        }

        /// <summary>
        /// Whether a node has a parent that matches one of the specified IDs
        /// </summary>
        public static bool HasMatchingParent(this Node<AttributeModel, Guid> node, HashSet<Guid> idsToMatch)
        {
            bool IsMatchOrHasMatchingParent(Node<AttributeModel, Guid> node)
            {
                return idsToMatch.Contains(node.Key)
                    || node.Parent.HasValue && IsMatchOrHasMatchingParent(node.Parent.Value);
            }

            return node.Parent.HasValue && IsMatchOrHasMatchingParent(node.Parent.Value);
        }

        /// <summary>
        /// Whether a node should be retained based on the specified IDs
        /// </summary>
        /// <remarks>
        /// Generally this means "true if the node's ID is in the set or one of the ancestors' or children's ID is in the set"
        /// but if the set is null then this method will always return true.
        /// </remarks>
        public static bool ShouldKeep(this Node<AttributeModel, Guid> node, HashSet<Guid>? attributeIDsMatchingFilter)
        {
            return attributeIDsMatchingFilter is not HashSet<Guid> idsToMatch
                || idsToMatch.Contains(node.Key)
                || node.HasMatchingParent(idsToMatch)
                || node.HasMatchingChild(idsToMatch);
        }

        /// <summary>
        /// Whether a node should be expanded based on the specified IDs
        /// </summary>
        /// <remarks>
        /// Generally this means "true if one of the ancestors' ID is in the set"
        /// but if the set is null then this method will always return false.
        /// </remarks>
        public static bool ShouldExpand(this Node<AttributeModel, Guid> node, HashSet<Guid>? attributeIDsMatchingFilter)
        {
            return attributeIDsMatchingFilter is HashSet<Guid> idsToMatch
                && node.HasMatchingChild(idsToMatch);
        }

        /// <summary>
        /// Get a string that represents this node and its children
        /// </summary>
        public static string GetExtendedStringRepresentation(this Node<AttributeModel, Guid> node)
        {
            if (node.Children.Count == 0)
            {
                return node.GetStringRepresentation();
            }

            return $"{node.GetStringRepresentation()} {node.GetStringRepresentationExtension(false)}";
        }

        /// <summary>
        /// Get a string that represents this node
        /// </summary>
        public static string GetStringRepresentation(this Node<AttributeModel, Guid> node)
        {
            return $"{node.Item.Name}|{node.Item.Value?.Replace("\r", "\\r").Replace("\n", "\\n")}|{node.Item.Type}|{node.Item.Page}";
        }

        /// <summary>
        /// Get a string that represents this node's children, unless it is expanded in which case return the empty string
        /// </summary>
        public static string GetStringRepresentationExtension(this Node<AttributeModel, Guid> node, bool isExpanded)
        {
            if (isExpanded || node.Children.Count == 0)
            {
                return "";
            }

            return new StringBuilder()
                .Append('(')
                .Append(string.Join(Environment.NewLine,
                    node.Children.Items.Select(childNode =>
                        string.Join(Environment.NewLine,
                            childNode.GetExtendedStringRepresentation()
                            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(line => ".." + line))
                    )))
                .Append(')')
                .ToString();
        }

        /// <summary>
        /// Create a derived collection by filtering the tree and transforming the models into view models
        /// </summary>
        /// <param name="attributes">The tree of attribute models</param>
        /// <param name="attributeFilter">The filter to determine which nodes to keep and which to expand</param>
        /// <param name="viewModels">The resulting, derived collection</param>
        /// <returns>An <see cref="IDisposable"/> to be used to stop the source and the filter subscriptions</returns>
        public static IDisposable CreateViewModelsFromAttributeTrees(
            this IObservable<IChangeSet<Node<AttributeModel, Guid>, Guid>> attributes,
            IObservable<IAttributeFilter> attributeFilter,
            out ReadOnlyObservableCollection<AttributeTreeViewModel> viewModels)
        {
            return attributes
                .Filter(attributeFilter.Select(x => x.FilterPredicate))
                .Transform(node => new AttributeTreeViewModel(node, attributeFilter))
                .SortBy(attribute => attribute.Index)
                .Bind(out viewModels)
                .DisposeMany()
                .Subscribe();
        }
    }
}
