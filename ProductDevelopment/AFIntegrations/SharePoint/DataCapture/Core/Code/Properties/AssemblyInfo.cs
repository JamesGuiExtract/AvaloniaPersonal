using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Extract.SharePoint.DataCapture")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Extract Systems")]
[assembly: AssemblyProduct("Extract.SharePoint.DataCapture")]
[assembly: AssemblyCopyright("Copyright © Extract Systems, LLC 2019")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Mark as not CLS compliant (since most SP items are not compliant)
[assembly: CLSCompliant(false)]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("38fa3088-8446-46eb-8da4-982c8d5e70ae")]

// Typically you should avoid having a namespace with very few types (less than 5),
// but we are using the namespaces to organize the features/event receivers
// and webparts for ID Shield/FlexIndex, there aren't really any classes in
// these places, most of it is configuration and asp definitions, but the namespaces
// keep it organized.
[module: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes",
    Scope="namespace", Target="Extract.SharePoint.DataCapture")]
[module: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes",
    Scope="namespace", Target="Extract.SharePoint.DataCapture.Features")]
[module: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes",
    Scope="namespace", Target="Extract.SharePoint.DataCapture.Layouts")]
[module: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes",
    Scope="namespace", Target="Extract.SharePoint.DataCapture.Administration")]
[module: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes",
    Scope="namespace", Target="Extract.SharePoint.DataCapture.Administration.Layouts")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
