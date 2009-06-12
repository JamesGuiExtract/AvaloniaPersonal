#include "stdafx.h"
#include "BaseUtils.h"
#include "UCLIDException.h"

#include <new.h>

//-------------------------------------------------------------------------------------------------
// Handles invalid parameter by creating and throwing a UCLIDException
void esInvalidParameterHandler(const wchar_t* expression,
							   const wchar_t* function, 
							   const wchar_t* file, 
							   unsigned int line, 
							   uintptr_t pReserved)
{
	// Convert parameters to CStrings
	CString zExpression( expression );
	CString zFunction( function );
	CString zFile( file );

	// Create and throw a UCLIDException
	UCLIDException ue("ELI15824", "Invalid Parameter Handler.");
	ue.addDebugInfo("Expression", zExpression.operator LPCTSTR() );
	ue.addDebugInfo("Function", zFunction.operator LPCTSTR() );
	ue.addDebugInfo("File", zFile.operator LPCTSTR() );
	ue.addDebugInfo("Line", line );
	throw ue;
}
//-------------------------------------------------------------------------------------------------
// Handles call to pure virtual function by creating and throwing a UCLIDException
void esPurecallHandler(void)
{
	// Create and throw a UCLIDException
	throw UCLIDException("ELI12937", "Invalid Call To Pure Virtual Function.");
}
//-------------------------------------------------------------------------------------------------
// Handles failure in memory allocation by creating and throwing a UCLIDException
int esNoMemoryHandler( size_t n )
{
	// Create and throw a UCLIDException
	UCLIDException ue("ELI12938", "Memory Allocation Failure.");
	ue.addDebugInfo( "Size", n );
	throw ue;
}

//-------------------------------------------------------------------------------------------------
// Class used to set some low-level handlers and replace default behavior
class DefaultHandlers
{
public:
	DefaultHandlers()
	{
		// Handle invalid parameters
		_set_invalid_parameter_handler( esInvalidParameterHandler );

		// Handle calls to pure virtual functions
		_set_purecall_handler( esPurecallHandler );

		// Handle failure in memory allocation
		_set_new_handler( esNoMemoryHandler );

		// Failures in malloc() will now call above handler
		_set_new_mode( 1 );
	};
};

// Global object to ensure that all usage of BaseUtils catches various errors
DefaultHandlers handler;
