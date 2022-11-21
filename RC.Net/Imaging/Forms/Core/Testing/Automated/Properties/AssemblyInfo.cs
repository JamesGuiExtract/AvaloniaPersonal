using NUnit.Framework;
using System;
using System.Runtime.InteropServices;
using System.Threading;

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("252551b6-da99-4e80-b7a3-d72321cb435b")]

// Mark as CLS compliant
[assembly: CLSCompliant(true)]

// Some tests (e.g., the ones that use a file browser dialog) need to be run in a STA.
// Since the tests in this assembly share the image viewer form, run all the tests in an STA thread
// to prevent the STA-requiring tests from failing
// https://extract.atlassian.net/browse/ISSUE-17418
[assembly: Apartment(ApartmentState.STA)]
