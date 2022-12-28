using Extract.ErrorHandling;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace ExtractDataExplorer.ViewModels
{
    /// <summary>
    /// Interface for a hierarchy of 'attribute' view models (leaves or nodes of a rose tree)
    /// </summary>
    public interface IAttributeTreeViewModel
    {
        /// <summary>
        /// The <see cref="AttributeViewModel"/> node/leaf
        /// </summary>
        AttributeViewModel Element { get; }

        /// <summary>
        /// Child view models
        /// </summary>
        IList<IAttributeTreeViewModel> Branches { get; }

        /// <summary>
        /// Whether or not the branches should be shown
        /// </summary>
        bool IsExpanded { get; set; }

        /// <summary>
        /// The string version of the element
        /// </summary>
        string StringRepresentation { get; }

        /// <summary>
        /// Either a string representation of the branches (when <see cref="IsExpanded"/> is <c>false</c>)
        /// or an empty string (when <see cref="IsExpanded"/> is <c>true</c>)
        /// </summary>
        /// <remarks>The idea is to show a preview of the child attributes when the node is collapsed
        /// but not when it is expanded, since it would be redundant noise in that case</remarks>
        string? StringRepresentationExtension { get; }
    }

    /// <summary>
    /// Helper for creating nodes/leaves
    /// </summary>
    public static class AttributeTreeViewModel
    {
        /// <summary>
        /// Create a node or leaf as appropriate
        /// </summary>
        public static IAttributeTreeViewModel Create(
            AttributeViewModel value,
            params IAttributeTreeViewModel[] branches)
        {
            if (branches is not null && branches.Length > 0)
            {
                return new AttributeTreeNodeViewModel(value, branches);
            }
            else
            {
                return new AttributeTreeLeafViewModel(value);
            }
        }
    }

    /// <summary>
    /// A leaf view model (no branches/sub-attributes)
    /// </summary>
    public sealed class AttributeTreeLeafViewModel : ViewModelBase, IAttributeTreeViewModel
    {
        /// <inheritdoc/>
        public AttributeViewModel Element { get; }

        /// <inheritdoc/>
        public IList<IAttributeTreeViewModel> Branches => Array.Empty<IAttributeTreeViewModel>();

        /// <inheritdoc/>
        public bool IsExpanded { get; set; }

        /// <inheritdoc/>
        public string StringRepresentation { get; }

        /// <inheritdoc/>
        public string StringRepresentationExtension { get; } = "";

        /// <inheritdoc/>
        public override string ToString()
        {
            return Element?.ToString() ?? "";
        }

        public AttributeTreeLeafViewModel(AttributeViewModel value)
        {
            value.AssertNotNull("ELI53925", "Value cannot be null!");

            Element = value;
            StringRepresentation = Element.ToString();
        }
    }

    /// <summary>
    /// A node view model (has branches/sub-attributes)
    /// </summary>
    public sealed class AttributeTreeNodeViewModel : ViewModelBase, IAttributeTreeViewModel
    {
        /// <inheritdoc/>
        public AttributeViewModel Element { get; }

        /// <inheritdoc/>
        public IList<IAttributeTreeViewModel> Branches { get; }

        /// <inheritdoc/>
        [Reactive] public bool IsExpanded { get; set; }

        /// <inheritdoc/>
        public string StringRepresentation { get; }

        /// <inheritdoc/>
        [ObservableAsProperty] public string? StringRepresentationExtension { get; }

        public AttributeTreeNodeViewModel (
            AttributeViewModel value,
            IList<IAttributeTreeViewModel> branches)
        {
            value.AssertNotNull("ELI53923", "Value cannot be null!");
            branches.AssertNotNull("ELI53924", "Branches cannot be null!");

            Element = value;
            Branches = branches;

            StringRepresentation = Element.ToString();

            this.WhenAnyValue(x => x.IsExpanded)
                .Select(CreateStringRepresentationExtension)
                .ToPropertyEx(this, x => x.StringRepresentationExtension);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (IsExpanded)
            {
                return StringRepresentation;
            }

            return $"{StringRepresentation} {StringRepresentationExtension}";
        }

        private string CreateStringRepresentationExtension(bool isExpanded)
        {
            if (isExpanded)
            {
                return "";
            }

            string branchesString = Branches is null
                ? ""
                : string.Join(Environment.NewLine,
                    Branches.Select(x =>
                        string.Join(Environment.NewLine,
                        x.ToString()
                        .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(line => ".." + line))
                    ));

            return $"({branchesString})";
        }
    }
}
