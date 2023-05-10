using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace Extract.Utilities.WPF
{
    /// <summary>
    /// Behavior to make it possilbe to bind the SelectedItem of a TreeView to a ViewModel
    /// </summary>
    /// <example>
    /// Add the following namespace attributes:
    /// xmlns:i="http://schemas.microsoft.com/xaml/behaviors" 
    /// xmlns:wpf="clr-namespace:Extract.Utilities.WPF;assembly=Extract.Utilities.WPF"
    ///
    /// Add the following in the TreeView markup:
    /// <i:Interaction.Behaviors>
    ///   <wpf:TreeViewSelectedItemBehavior SelectedItem="{Binding VMProp, Mode=TwoWay}"/>
    /// </i:Interaction.Behaviors>
    /// </example>
    public class TreeViewSelectedItemBehavior : Behavior<TreeView>
    {
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object),
                typeof(TreeViewSelectedItemBehavior),
                new FrameworkPropertyMetadata(
                    defaultValue: null,
                    flags: FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    propertyChangedCallback: OnSelectedItemChanged));

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null)
            {
                AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
            }
        }

        // This updates the bound property when the tree selection is changed
        // (when the user clicks on an item, e.g.)
        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectedItem = e.NewValue;
        }

        // This takes care of selecting an item in the tree when the bound property is changed
        // (when the property on the view model is programmatically set, e.g.)
        private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is TreeViewSelectedItemBehavior behavior && behavior.AssociatedObject is TreeView tree)
            {
                if (tree.ItemContainerGenerator.ContainerFromItem(e.OldValue) is TreeViewItem oldItem)
                {
                    oldItem.IsSelected = false;
                }

                if (e.NewValue != null &&
                    tree.ItemContainerGenerator.ContainerFromItem(e.NewValue) is TreeViewItem newItem)
                {
                    newItem.IsSelected = true;
                }
            }
        }
    }
}
