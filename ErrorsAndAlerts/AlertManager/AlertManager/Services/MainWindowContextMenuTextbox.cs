using Avalonia.Controls;
using AvaloniaDashboard.Interfaces;
using System;
using System.Collections.Generic;

namespace AvaloniaDashboard.Services
{
    public class MainWindowContextMenuTextbox : IMainWindowContextMenu
    {
        //TODO important, figure out how the hell to add to items in Avolonia, works in wpf but wtf
        public TextBlock ContextMenuTextBlock(List<MenuItem> listOfMenuItems, string textBlockContent)
        {
            TextBlock returnBlock = new TextBlock();

            try
            {
                returnBlock.Text = textBlockContent;

                ContextMenu contextMenu = new ContextMenu()
                {
                    Items = new[]
                    {
                        new MenuItem(){ Header = "testing" }
                    }
                };

                returnBlock.ContextMenu = contextMenu;

                return returnBlock;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

            return returnBlock;
        }

        /// <summary>
        /// context menu for the button
        /// </summary>
        /// <param name="listOfMenuItems"> List of type MenuItem</param>
        /// <param name="textBlockContent"> string of what willb e in the textBlock</param>
        /// <returns></returns>
        public Button ContextMenuButton(List<MenuItem> listOfMenuItems, string textBlockContent)
        {
            Button returnButton = new Button();

            try
            {
                returnButton.Content = textBlockContent;

                ContextMenu contextMenu = new ContextMenu()
                {
                    Items = new[]
                    {
                        listOfMenuItems
                    }
                };

                returnButton.ContextMenu = contextMenu;

                return returnButton;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

            return returnButton;
        }

        /// <summary>
        /// REturns a new MenuItem, if no parameters are entered
        /// </summary>
        /// <returns></returns>
        public MenuItem CreateMenuItem()
        {
            return new MenuItem();
        }

        /// <summary>
        /// Overloaded method, returns a menuItem with the string menuHeader as header
        /// </summary>
        /// <param name="menuHeader"></param>
        /// <returns>menuItem</returns>
        public MenuItem CreateMenuItem(string menuHeader)
        {
            MenuItem menuItem = new MenuItem();
            menuItem.Header = menuHeader;
            return menuItem;
        }

        /// <summary>
        /// Overloaded method, returns a menuItem with the string menuHeader as header, 
        /// and the action menuClickFunction as the action
        /// </summary>
        /// <param name="menuHeader">The header of the returned MenuItem</param>
        /// <param name="menuClickFunction"> Function that will trigger onclick with menuItem </param>
        /// <returns>MenuItem</returns>
        public MenuItem CreateMenuItem(string menuHeader, Action menuClickFunction)
        {
            MenuItem menuItem = new MenuItem();
            menuItem.Header = menuHeader;
            menuItem.Click += delegate
            {
                menuClickFunction();
            };
            return menuItem;
        }

    }

}
