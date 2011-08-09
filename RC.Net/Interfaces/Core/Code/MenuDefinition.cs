using System.Runtime.Serialization;

// This assembly is reserved for the definition of interfaces and helper classes for those
// interfaces. To ensure these interfaces are accessible from all projects without circular
// dependency issues and to allow the assemblies definitions to be used in both 32 and 64 bit code,
// This assembly should have no dependencies on any other Extract projects.
namespace Extract.Interfaces
{
    /// <summary>
    /// A helper class for <see cref="FileReceiver"/> which defines a menu option that should supply
    /// files to the <see cref="FileReceiver"/>. This class is constructed by an out-of-process
    /// channel creator, stored in the service's <see cref="IWcfFileReceiverManager"/> channel, but is to be used within an endpoint's
    /// process space. 
    /// </summary>
    [DataContract]
    public class MenuDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MenuDefinition"/> class.
        /// </summary>
        /// <param name="menuItemName">Name of the menu item.</param>
        public MenuDefinition(string menuItemName)
        {
            MenuItemName = menuItemName;
        }

        /// <summary>
        /// Gets the ID of the FileReceiver this menu item is associated with.
        /// </summary>
        [DataMember]
        public int FileReceiverId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets text of the menu option.
        /// </summary>
        [DataMember]
        public string MenuItemName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the name of the sub-menu this item should be beneath. If <see langword="null"/> or
        /// empty, the item will be added to the root of the context menu.
        /// </summary>
        [DataMember]
        public string ParentMenuItemName
        {
            get;
            set;
        }

        /// <summary>
        /// If specified, this icon will be drawn in front of the menu item text.
        /// </summary>
        [DataMember]
        public string IconFileName
        {
            get;
            set;
        }

        /// <summary>
        /// If specified, this icon will be drawn in front of the parent sub-menu. If there are
        /// multiple menu items in the sub-menu, the ParentIconFileName property of the first will
        /// dictated the icon used by the sub-menu.
        /// </summary>
        [DataMember]
        public string ParentIconFileName
        {
            get;
            set;
        }
    }
}
