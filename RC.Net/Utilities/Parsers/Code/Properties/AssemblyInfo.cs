using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("b5a7fc0e-25ab-4ab5-9de4-dccc2c36caf0")]

// Mark as not CLS compliant since COM pointers are passed in methods
[assembly: CLSCompliant(false)]

[module: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes",
    Scope = "namespace", Target = "Extract.Utilities.Parsers")]