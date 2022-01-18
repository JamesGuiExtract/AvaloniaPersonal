using NUnit.Framework;
using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Threading;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Extract.Imaging.Forms.Test")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Extract Systems")]
[assembly: AssemblyProduct("Extract.Imaging.Forms.Test")]
[assembly: AssemblyCopyright("Copyright © Extract Systems 2022")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("252551b6-da99-4e80-b7a3-d72321cb435b")]

// Mark as CLS compliant
[assembly: CLSCompliant(true)]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: NeutralResourcesLanguageAttribute("en-US")]

// Some tests (e.g., the ones that use a file browser dialog) need to be run in a STA.
// Since the tests in this assembly share the image viewer form, run all the tests in an STA thread
// to prevent the STA-requiring tests from failing
// https://extract.atlassian.net/browse/ISSUE-17418
[assembly: Apartment(ApartmentState.STA)]
