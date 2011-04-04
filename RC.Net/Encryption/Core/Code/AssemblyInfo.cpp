#include "stdafx.h"

using namespace System;
using namespace System::Reflection;
using namespace System::Runtime::CompilerServices;
using namespace System::Runtime::InteropServices;
using namespace System::Security::Permissions;

//
// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyTitleAttribute("ExtractEncryption")];
[assembly: AssemblyDescriptionAttribute("")];
[assembly: AssemblyConfigurationAttribute("")];
[assembly: AssemblyCompanyAttribute("Extract Systems")];
[assembly: AssemblyProductAttribute("ExtractEncryption")];
[assembly: AssemblyCopyrightAttribute("Copyright (c) Extract Systems 2010")];
[assembly: AssemblyTrademarkAttribute("")];
[assembly: AssemblyCultureAttribute("")];

//
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the value or you can default the Revision and Build Numbers
// by using the '*' as shown below:
[assembly: AssemblyVersion("1.0.0.0")];
[assembly: AssemblyFileVersion("1.0.0.0")];

[assembly: ComVisible(false)];

[assembly: CLSCompliantAttribute(true)];

[assembly: SecurityPermission(SecurityAction::RequestMinimum, UnmanagedCode = true)];

// Wraps C++ exceptions as .NET
[assembly: RuntimeCompatibility(WrapNonExceptionThrows=true)]; 
