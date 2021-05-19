using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// This code was obtained from: https://stackoverflow.com/questions/3350187/wpf-c-rearrange-items-in-listbox-via-drag-and-drop
    /// The code has been slightly modified to support interfaces
    /// </summary>
    public class DragAndDropListBox<T> : ListBox
        where T : class
    {
        private Point _dragStartPoint;
        private Assembly executingAssembly;

        private P FindVisualParent<P>(DependencyObject child)
            where P : DependencyObject
        {
            var parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null)
                return null;

            P parent = parentObject as P;
            if (parent != null)
                return parent;

            return FindVisualParent<P>(parentObject);
        }

        public DragAndDropListBox(Assembly executingAssembly)
        {
            this.executingAssembly = executingAssembly;
            this.PreviewMouseMove += ListBox_PreviewMouseMove;

            var style = new Style(typeof(ListBoxItem));

            style.Setters.Add(new Setter(ListBoxItem.AllowDropProperty, true));

            style.Setters.Add(
                new EventSetter(
                    ListBoxItem.PreviewMouseLeftButtonDownEvent,
                    new MouseButtonEventHandler(ListBoxItem_PreviewMouseLeftButtonDown)));

            style.Setters.Add(
                    new EventSetter(
                        ListBoxItem.DropEvent,
                        new DragEventHandler(ListBoxItem_Drop)));

            this.ItemContainerStyle = style;
        }

        private void ListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point point = e.GetPosition(null);
            Vector diff = _dragStartPoint - point;
            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                var lbi = FindVisualParent<ListBoxItem>(((DependencyObject)e.OriginalSource));
                if (lbi != null)
                {
                    DragDrop.DoDragDrop(lbi, lbi.DataContext, DragDropEffects.Move);
                }
            }
        }

        private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
        }

        private void ListBoxItem_Drop(object sender, DragEventArgs e)
        {
            if (sender is ListBoxItem _sender)
            {
                T source = null;
                var implemenations = UtilityMethods.AllTypesThatImplementInterface(typeof(T), this.executingAssembly);
                foreach (var implemenation in implemenations)
                {

                    source = e.Data.GetData(implemenation) as T;
                    if (source != null)
                    {
                        break;
                    }
                }

                var target = _sender.DataContext as T;

                int sourceIndex = this.Items.IndexOf(source);
                int targetIndex = this.Items.IndexOf(target);

                Move(source, sourceIndex, targetIndex);
            }
        }

        private void Move(T source, int sourceIndex, int targetIndex)
        {
            if (sourceIndex < targetIndex)
            {
                var items = this.DataContext as IList<T>;
                if (items != null)
                {
                    items.Insert(targetIndex + 1, source);
                    items.RemoveAt(sourceIndex);
                }
            }
            else
            {
                var items = this.DataContext as IList<T>;
                if (items != null)
                {
                    int removeIndex = sourceIndex + 1;
                    if (items.Count + 1 > removeIndex)
                    {
                        items.Insert(targetIndex, source);
                        items.RemoveAt(removeIndex);
                    }
                }
            }
        }
    }
}