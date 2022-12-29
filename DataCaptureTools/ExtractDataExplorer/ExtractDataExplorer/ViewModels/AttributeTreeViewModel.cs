using DynamicData;
using ExtractDataExplorer.Models;
using ExtractDataExplorer.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace ExtractDataExplorer.ViewModels
{
    /// <summary>
    /// An attribute tree view model
    /// </summary>
    public sealed class AttributeTreeViewModel : ViewModelBase, IDisposable
    {
        readonly ReadOnlyObservableCollection<AttributeTreeViewModel> _branches;

        readonly CompositeDisposable _disposables = new();
        bool _isDisposed;

        /// <summary>
        /// The original order of this node in relation to its siblings
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Child view models
        /// </summary>
        public ReadOnlyObservableCollection<AttributeTreeViewModel> Branches => _branches;

        /// <summary>
        /// Whether or not the branches should be shown
        /// </summary>
        [Reactive] public bool IsExpanded { get; set; }

        /// <summary>
        /// The string version of the element
        /// </summary>
        public string StringRepresentation { get; }

        /// <summary>
        /// A string representation of the branches
        /// </summary>
        /// <remarks>Use to show a preview of the child attributes when the node is collapsed
        /// but not when it is expanded, since it would be mostly redundant in that case</remarks>
        [ObservableAsProperty] public string? StringRepresentationExtension { get; }

        /// <summary>
        /// Create an instance of a tree node view model
        /// </summary>
        public AttributeTreeViewModel(Node<AttributeModel, Guid> node, IObservable<IAttributeFilter> attributeFilter)
        {
            _ = node ?? throw new ArgumentNullException(nameof(node));

            Index = node.Item.Index;

            node.Children.Connect()
                .CreateViewModelsFromAttributeTrees(attributeFilter, out _branches)
                .DisposeWith(_disposables);

            StringRepresentation = node.GetStringRepresentation();

            this.WhenAnyValue(x => x.IsExpanded)
                .Select(isExpanded => node.GetStringRepresentationExtension(isExpanded))
                .ToPropertyEx(this, x => x.StringRepresentationExtension);

            attributeFilter
                .Select(filter => filter.ExpandPredicate(node))
                .BindTo(this, x => x.IsExpanded)
                .DisposeWith(_disposables);
        }

        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _disposables.Dispose();
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
