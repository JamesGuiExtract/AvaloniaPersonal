using NUnit.Framework;
using System.Runtime.InteropServices;

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("209dd91d-62c2-40f9-9bc1-74e78346cc65")]

// Fixes problem with the FAMProcessingSession test class (c++/cli assembly Extract.Licensing.dll couldn't load Extract.dll otherwise)
[assembly: TestAssemblyDirectoryResolve]
