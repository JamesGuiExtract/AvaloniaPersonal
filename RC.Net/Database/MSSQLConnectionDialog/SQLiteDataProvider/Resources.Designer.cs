﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.Data.ConnectionUI {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microsoft.Data.ConnectionUI.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SQLite Data Provider.
        /// </summary>
        internal static string DataProvider_SQLite {
            get {
                return ResourceManager.GetString("DataProvider_SQLite", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use this data provider to connect to a SQLite database file..
        /// </summary>
        internal static string DataProvider_SQLite_Description {
            get {
                return ResourceManager.GetString("DataProvider_SQLite_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to sqlite.
        /// </summary>
        internal static string SQLiteConnectionUIControl_BrowseFileDefaultExt {
            get {
                return ResourceManager.GetString("SQLiteConnectionUIControl_BrowseFileDefaultExt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SQLite Databases (*.db;*.sqlite;*.sqlite3)|*.db;*.sqlite;*.sqlite3|All Files (*.*)|*.*||.
        /// </summary>
        internal static string SQLiteConnectionUIControl_BrowseFileFilter {
            get {
                return ResourceManager.GetString("SQLiteConnectionUIControl_BrowseFileFilter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Select SQLite.
        /// </summary>
        internal static string SQLiteConnectionUIControl_BrowseFileTitle {
            get {
                return ResourceManager.GetString("SQLiteConnectionUIControl_BrowseFileTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The connection properties object must be of type SQLiteConnectionProperties..
        /// </summary>
        internal static string SQLiteConnectionUIControl_InvalidConnectionProperties {
            get {
                return ResourceManager.GetString("SQLiteConnectionUIControl_InvalidConnectionProperties", resourceCulture);
            }
        }
    }
}
