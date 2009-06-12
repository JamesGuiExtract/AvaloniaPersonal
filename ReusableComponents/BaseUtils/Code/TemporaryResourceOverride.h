//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	UCLIDException.h
//
// PURPOSE:	Definition of the UCLIDException class
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#pragma once

#include "BaseUtils.h"

//==================================================================================================
//
// CLASS:	TemporaryResourceOverride
//
// PURPOSE:	TemporaryResourceOverride is responsible for overriding the source of Windows resources.
//			Typically resources are retrieved from the EXE, but TemporaryResourceOverride will
//			instruct Win32 to get resources from wherever you decide, e.g. from a DLL containing its
//			own resources.
//
// REQUIRE:	
// 
// INVARIANTS:
//
// EXTENSIONS:
//
// NOTES:	
//
class EXPORT_BaseUtils TemporaryResourceOverride
{
public:
	//------------------------------------------------------------------------------------------
	// PURPOSE: Specify the default source of resources
	// REQUIRE: 
	// PROMISE: To initialize the contructed object with the provided arguments.
	// ARGS:	hInstNew	- specifies the default module instance from which to get resources
    static void sSetDefaultResource(HINSTANCE hInstNew);

	//------------------------------------------------------------------------------------------
	// PURPOSE: Default constructor uses the default source of resource
	// REQUIRE: 
	// PROMISE: 
	// ARGS:	hInstNew	- specifies the default module instance from which to get resources
    TemporaryResourceOverride(); 

	//------------------------------------------------------------------------------------------
	// PURPOSE: Constructor uses the specified source of resource
	// REQUIRE: 
	// PROMISE: 
	// ARGS:	hInstNew	- specifies the module instance from which to get resources
    TemporaryResourceOverride(HINSTANCE hInstNew);

	//------------------------------------------------------------------------------------------
	// PURPOSE: Destructor
	// REQUIRE: 
	// PROMISE: 
	// ARGS:	
    virtual ~TemporaryResourceOverride();


private:
    static HINSTANCE m_hInstanceDefault;	// default source from which to retrieve resources

	//------------------------------------------------------------------------------------------
	// PURPOSE: Common initialization method for use by constructors.
	// REQUIRE: 
	// PROMISE: Restores the previous source of resources
	// ARGS:	
    void Init(HINSTANCE);

    HINSTANCE m_hInstanceOld;		// handle to previous source of resources
};
