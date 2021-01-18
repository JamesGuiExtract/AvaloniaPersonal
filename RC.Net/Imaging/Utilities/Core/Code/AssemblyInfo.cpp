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
[assembly:AssemblyTitleAttribute("ExtractImagingUtilities")];
[assembly:AssemblyDescriptionAttribute("")];
[assembly:AssemblyConfigurationAttribute("")];
[assembly:AssemblyCompanyAttribute("Extract Systems")];
[assembly:AssemblyProductAttribute("ExtractImagingUtilities")];
[assembly:AssemblyCopyrightAttribute("Copyright (c) Extract Systems 2021")];
[assembly:AssemblyTrademarkAttribute("")];
[assembly:AssemblyCultureAttribute("")];

[assembly:CLSCompliantAttribute(true)];
[assembly:AssemblyVersionAttribute("1.0.0.0")];
[assembly:ComVisible(false)];
[assembly: RuntimeCompatibility(WrapNonExceptionThrows=true)]; // Wraps C++ exceptions as .NET

// This namespace currently only contains one type.
// As a general rule you should not create a new namespace for just a couple
// of types, but this namespace has been created specifically to provide
// .Net wrappers around C++ imaging functions so that they are visible
// to our .Net classes.
[module: System::Diagnostics::CodeAnalysis::SuppressMessage("Microsoft.Design",
	"CA1020:AvoidNamespacesWithFewTypes", Scope="namespace", Target="Extract.Imaging.Utilities")];
