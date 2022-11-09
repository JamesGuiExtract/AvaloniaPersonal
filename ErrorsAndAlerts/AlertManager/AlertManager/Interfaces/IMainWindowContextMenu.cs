using Avalonia.Controls;
using System;
using System.Collections.Generic;

namespace AvaloniaDashboard.Interfaces
{
    public interface IMainWindowContextMenu
    {
        //use this as the context menu for the textblock
        public TextBlock ContextMenuTextBlock(List<MenuItem> listOfMenuItems, string textBlockContent)
        {
            return new TextBlock();
        }

        //use this as the context menu for the button
        public Button ContextMenuButton(List<MenuItem> listOfMenuItems, string textBlockContent)
        {
            return new Button();
        }

        public MenuItem CreateMenuItem()
        {
            return new MenuItem();
        }

        public MenuItem CreateMenuItem(string menuHeader)
        {
            return new MenuItem();
        }

        //guess i can't pass a Delegate directly
        public MenuItem CreateMenuItem(string menuHeader, Action menuClickFunction)
        {
            return new MenuItem();
        }


        //note if i want to be fancy and add a seperator, could make a list of list of menu items and add a seperator between each one, don't really care though
    }
}
