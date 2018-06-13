#include "stdafx.h"

using namespace System;
using namespace System::Reflection;
using namespace System::Runtime::CompilerServices;
using namespace System::Runtime::InteropServices;

//
// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: CLSCompliant(true)];
[assembly: AssemblyVersion("1.0.0.0")];
[assembly: ComVisible(false)];
[assembly: RuntimeCompatibility(WrapNonExceptionThrows=true)]; // Wraps C++ exceptions as .NET

// This namespace only contains three types: MapLabel, LicenseUtilities, and LicenseStateCache.
// As a general rule you should not create a new namespace for just a couple
// of types, but this namespace has been created specifically for licensing
// and to obscure the licensing functions via unmanaged code so it is safe
// to ignore this warning.
[module: System::Diagnostics::CodeAnalysis::SuppressMessage("Microsoft.Design",
	"CA1020:AvoidNamespacesWithFewTypes", Scope="namespace", Target="Extract.Licensing")];

// These suppress messages are added due to the need to suppress errors coming from
// the CLR/std::cpp library interop marshaling code in the StringHelperFinctions.h file
[module: System::Diagnostics::CodeAnalysis::SuppressMessage("Microsoft.Performance",
	"CA1823:AvoidUnusedPrivateFields", Scope="member",
	Target="msclr.interop.context_node_base.#_Needs_Context")];
[module: System::Diagnostics::CodeAnalysis::SuppressMessage("Microsoft.Performance",
	"CA1812:AvoidUninstantiatedInternalClasses", Scope="type",
	Target="msclr.interop.marshal_context")];
[module: System::Diagnostics::CodeAnalysis::SuppressMessage("Microsoft.Performance",
	"CA1812:AvoidUninstantiatedInternalClasses", Scope="type",
	Target="msclr.interop.context_node<char const *,System::String ^>")];
[module: System::Diagnostics::CodeAnalysis::SuppressMessage("Microsoft.Performance",
	"CA1812:AvoidUninstantiatedInternalClasses", Scope="type",
	Target="msclr.interop.context_node<wchar_t const *,System::String ^>")];