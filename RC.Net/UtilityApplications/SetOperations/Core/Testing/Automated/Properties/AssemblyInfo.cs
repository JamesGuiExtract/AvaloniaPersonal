using System;
using System.Runtime.InteropServices;

// This is a testing assembly and as such uses NUnit value array attributes
// to generate multiple test cases using the same code. The value attribute
// makes each method that uses it Non-CLS compliant. Mark the whole assembly
// as such to quiet FxCop and prevent having to add attributes to each testing method.
[assembly: CLSCompliant(false)]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("2a0bfc38-001c-469d-b4e3-e6c63bf02b05")]
