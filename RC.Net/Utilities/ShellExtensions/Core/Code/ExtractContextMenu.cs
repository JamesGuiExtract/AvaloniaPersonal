using Extract.Interfaces;
using LogicNP.EZShellExtensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel;

// Note:
// This code is loaded into the process space of any process making use of Window's shell. On 64bit
// OS's, this means this code needs to run as 64bit as well as 32bit. Therefore this assembly is
// compiled as any CPU and cannot have any dependencies on 32bit Extract assemblies.
namespace Extract.Utilities.ShellExtensions
{
    /// <summary>
    /// Provides a hook to allow shell context menu items to be displayed for Extract software.
    /// This class will look for <see cref="MenuDefinition"/>s in the <see cref="FileReceiver"/>s
    /// on the <see cref="FileReceiver.WcfAddress"/> WCF endpoint. If there are any, the defined
    /// menu items will be displayed in the context menu.
    /// </summary>
    [Guid("948154A0-91A0-4C4B-9E86-1D2105F92244")]
    [ComVisible(true)]
    [TargetExtension(SpecialProgIDTargets.AllFiles, false)]
    [TargetExtension(SpecialProgIDTargets.AllFolders, false)]
    public class ExtractContextMenu : ContextMenuExtension
    {
        #region Constants

        /// <summary>
        /// If the context menu is being owner-drawn, the size the icon should be.
        /// </summary>
        static readonly Size _ICON_SIZE = new Size(16, 16);

        #endregion Constants

        #region Fields

        /// <summary>
        /// Maps parent menu names to the items within the menu. If the menu name is
        /// <see cref="String.Empty"/>, the menu items will be added directly to the main context
        /// menu.
        /// </summary>
        Dictionary<string, List<MenuDefinition>> _menuHierarchy =
            new Dictionary<string, List<MenuDefinition>>();

        /// <summary>
        /// For the ID of each menu being displayed, the files that qualified for its
        /// <see cref="FileFilter"/>.
        /// </summary>
        Dictionary<int, IEnumerable<string>> _qualifiedFiles =
            new Dictionary<int, IEnumerable<string>>();

        /// <summary>
        /// Caches menu item bitmaps so they don't need to be re-loaded from disk every time the
        /// context menu is displayed.
        /// </summary>
        static Dictionary<string, Bitmap> _bitmapCache = new Dictionary<string, Bitmap>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractContextMenu"/> class.
        /// </summary>
        public ExtractContextMenu()
        {
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Called after the context menu extension has been initialized.
        /// </summary>
        /// <returns>
        /// Ignored.
        /// </returns>
        protected override bool OnInitialize()
        {
            ChannelFactory<IWcfFileReceiverManager> channelFactory = null;

            try
            {
                // Attempt to connect to FileReceiver.WcfAddress
                channelFactory = new ChannelFactory<IWcfFileReceiverManager>(
                    new NetNamedPipeBinding(), new EndpointAddress(FileReceiver.WcfAddress));

                IWcfFileReceiverManager fileReceiverManager = channelFactory.CreateChannel();

                if (fileReceiverManager != null)
                {
                    // If we were able to connect, query for the IDs of the active FileReceivers.
                    IEnumerable<int> fileReceiverIds = fileReceiverManager.GetFileReceiverIds();
                    foreach (int id in fileReceiverIds)
                    {
                        // Test TargetFiles against the FileFilter for each receiver; we will add
                        // context menu options for any receivers that have matching files.
                        FileFilter fileFilter = fileReceiverManager.GetFileFilter(id);

                        IEnumerable<string> qualifiedFiles = TargetFiles
                            .Where(fileName =>
                                fileFilter != null && fileFilter.FileMatchesFilter(fileName));

                        if (qualifiedFiles.Any())
                        {
                            // There are qualifying files; Add the MenuDefinition to _menuHierarchy.
                            MenuDefinition menuItem = fileReceiverManager.GetMenuDefinition(id);
                            // string.Empty will represent menu options that should be added at the
                            // top level.
                            string parentMenuText =
                                string.IsNullOrWhiteSpace(menuItem.ParentMenuItemName)
                                    ? string.Empty : menuItem.ParentMenuItemName;

                            List<MenuDefinition> subMenuItems;
                            if (!_menuHierarchy.TryGetValue(parentMenuText, out subMenuItems))
                            {
                                subMenuItems = new List<MenuDefinition>();
                                _menuHierarchy[parentMenuText] = subMenuItems;
                            }

                            subMenuItems.Add(menuItem);
                            
                            // Store the qualifiedFiles so they don't need to be re-calculated when
                            // the option is selected.
                            _qualifiedFiles[id] = qualifiedFiles;
                        }
                    }

                    // _qualifiedFiles will have more than zero entries if we found a menu to be
                    // displayed.
                    return _qualifiedFiles.Count() > 0;
                }

                return false;
            }
            finally
            {
                if (channelFactory != null)
                {
                    channelFactory.Close();
                }
            }
        }

        /// <summary>
        /// Called when the menu items for your context menu extension should be added to the menu.
        /// </summary>
        /// <param name="e">The data for the method.</param>
        protected override void OnGetMenuItems(GetMenuitemsEventArgs e)
        {
            // Add a separator before our menu option(s) to distinguish them from other context menu
            // options.
            ShellMenuItem separator = e.Menu.AddItem("");
            separator.Separator = true;

            // For each key in _menuHierarchy, add a top-level menu option.
            foreach (KeyValuePair<string, List<MenuDefinition>> topLevelMenuItem in _menuHierarchy)
            {
                ShellMenu parentMenu;
                if (string.IsNullOrWhiteSpace(topLevelMenuItem.Key))
                {
                    // If the key is empty, this is a menu item, not a sub-menu. Add it to the
                    // context menu itself
                    parentMenu = e.Menu;
                }
                else
                {
                    // Add a new sub-Menu to which the topLevelMenuItem.Value's will be added.
                    ShellMenuItem menuItem = e.Menu.AddItem(topLevelMenuItem.Key);
                    menuItem.HasSubMenu = true;
                    parentMenu = menuItem.SubMenu;

                    ApplyIcon(topLevelMenuItem.Value.First().ParentIconFileName, menuItem);
                }

                foreach (MenuDefinition menuDefinition in topLevelMenuItem.Value)
                {
                    ShellMenuItem menuItem = parentMenu.AddItem(
                        menuDefinition.MenuItemName,
                        menuDefinition.FileReceiverId.ToString(CultureInfo.InvariantCulture), "");

                    ApplyIcon(menuDefinition.IconFileName, menuItem);
                }
            }

            // Add a separator after our menu option(s) to distinguish them from other context menu
            // options.
            separator = e.Menu.AddItem("");
            separator.Separator = true;
        }

        /// <summary>
        /// Called when a contextmenu item is selected by the user.
        /// </summary>
        /// <param name="e">The data for the method. The  MenuItem property specifies the menu item
        /// which is to be drawn. The  Verb property identifies the menu item.</param>
        /// <returns>
        /// <see langword="true"/> if the extension successfully executes the menu item;
        /// <see langword="false"/> otherwise. The default implementation returns false. Note that
        /// the OS may call this method for items which are not added by your extension, so this
        /// method should return true only if the menu item is supported by your extension and is
        /// successfully executed.
        /// </returns>
        protected override bool OnExecuteMenuItem(ExecuteItemEventArgs e)
        {
            ChannelFactory<IWcfFileReceiverManager> channelFactory = null;

            try
            {
                // Attempt to connect to FileReceiver.WcfAddress
                channelFactory = new ChannelFactory<IWcfFileReceiverManager>(
                    new NetNamedPipeBinding(), new EndpointAddress(FileReceiver.WcfAddress));

                IWcfFileReceiverManager fileReceiverManager = channelFactory.CreateChannel();

                if (fileReceiverManager != null)
                {
                    // Interpreting the Verb as a stringized ID, attempt to supply the qualified
                    // files to that ID.
                    int id;
                    if (int.TryParse(e.MenuItem.Verb, out id))
                    {
                        return fileReceiverManager.SupplyFiles(id, _qualifiedFiles[id].ToArray());
                    }
                }

                return false;
            }
            finally
            {
                if (channelFactory != null)
                {
                    channelFactory.Close();
                }
            }
        }

        #endregion Overrides

        #region Private Members

        /// <summary>
        /// Registers the specified <see paramref="type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to register.</param>
        [ComRegisterFunction]
        static void Register(Type type)
        {
            ContextMenuExtension.RegisterExtension(type);
        }

        /// <summary>
        /// Unregisters the specified <see paramref="type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to un-register.</param>
        [ComUnregisterFunction]
        static void Unregister(Type type)
        {
            ContextMenuExtension.UnRegisterExtension(type);
        }

        /// <summary>
        /// If the specified <see paramref="iconFileName"/> is valid, makes the menu owner-drawn
        /// and assigns this icon to be drawn next to the menu item.
        /// </summary>
        /// <param name="iconFileName">The fileName of the icon to display.</param>
        /// <param name="menuItem">The <see cref="ShellMenuItem"/> for which the icon should be
        /// displayed.</param>
        static void ApplyIcon(string iconFileName, ShellMenuItem menuItem)
        {
            try
            {
                // If the specified icon file exists, use it.
                if (File.Exists(iconFileName))
                {
                    Bitmap bitmap;

                    if (!_bitmapCache.TryGetValue(iconFileName, out bitmap))
                    {
                        // If an .ico file, load the file as an icon
                        if (iconFileName.EndsWith(".ico", StringComparison.OrdinalIgnoreCase))
                        {
                            using (FileStream stream = File.Open(iconFileName, FileMode.Open))
                            using (Icon icon = new Icon(stream, _ICON_SIZE))
                            {
                                bitmap = icon.ToBitmap();
                                _bitmapCache[iconFileName] = bitmap;
                            }
                        }
                        // If not an .ico file, load the icon associated with the file.
                        else
                        {
                            using (Icon icon = Icon.ExtractAssociatedIcon(iconFileName))
                            {
                                if (icon.Size == _ICON_SIZE)
                                {
                                    bitmap = icon.ToBitmap();
                                }
                                else
                                {
                                    // ExtractAssociatedIcon can only retrieve 32x32 sized icons.
                                    // Without using p/invoke for SHGetFileInfo, it appears the
                                    // only way around this is to scale the image (the results may
                                    // not be great.
                                    using (Bitmap unsizedBitmap = icon.ToBitmap())
                                    {
                                        bitmap = new Bitmap(_ICON_SIZE.Width, _ICON_SIZE.Height);
                                        using (Graphics g = Graphics.FromImage(bitmap))
                                        {
                                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                            g.SmoothingMode = SmoothingMode.HighQuality;
                                            g.DrawImage(unsizedBitmap, 0, 0, _ICON_SIZE.Width, _ICON_SIZE.Height);
                                        }
                                    }
                                }

                                _bitmapCache[iconFileName] = bitmap;
                            }
                        }
                    }

                    menuItem.SetBitmap(bitmap);
                }
            }
            // If the icon can't be set, we can still display the menu item; Ignore any exceptions.
            catch { }
        }

        #endregion Private Members
    }
}
