﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Extract.ReportViewer.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.7.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Normal")]
        public global::System.Windows.Forms.FormWindowState OpenReportState {
            get {
                return ((global::System.Windows.Forms.FormWindowState)(this["OpenReportState"]));
            }
            set {
                this["OpenReportState"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("300, 300")]
        public global::System.Drawing.Size OpenReportSize {
            get {
                return ((global::System.Drawing.Size)(this["OpenReportSize"]));
            }
            set {
                this["OpenReportSize"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0, 0")]
        public global::System.Drawing.Point OpenReportLocation {
            get {
                return ((global::System.Drawing.Point)(this["OpenReportLocation"]));
            }
            set {
                this["OpenReportLocation"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Normal")]
        public global::System.Windows.Forms.FormWindowState ReportViewerState {
            get {
                return ((global::System.Windows.Forms.FormWindowState)(this["ReportViewerState"]));
            }
            set {
                this["ReportViewerState"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("300, 300")]
        public global::System.Drawing.Size ReportViewerSize {
            get {
                return ((global::System.Drawing.Size)(this["ReportViewerSize"]));
            }
            set {
                this["ReportViewerSize"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0, 0")]
        public global::System.Drawing.Point ReportViewerLocation {
            get {
                return ((global::System.Drawing.Point)(this["ReportViewerLocation"]));
            }
            set {
                this["ReportViewerLocation"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int OpenReportSplitterDistance {
            get {
                return ((int)(this["OpenReportSplitterDistance"]));
            }
            set {
                this["OpenReportSplitterDistance"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool OpenReportUsePersistedSettings {
            get {
                return ((bool)(this["OpenReportUsePersistedSettings"]));
            }
            set {
                this["OpenReportUsePersistedSettings"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool ReportViewerUsePersistedSettings {
            get {
                return ((bool)(this["ReportViewerUsePersistedSettings"]));
            }
            set {
                this["ReportViewerUsePersistedSettings"] = value;
            }
        }
    }
}
