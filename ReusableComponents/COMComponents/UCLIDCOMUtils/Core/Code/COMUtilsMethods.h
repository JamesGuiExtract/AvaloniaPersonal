//=================================================================================================
//
// COPYRIGHT (c) 2015 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	COMUtilsMehtods.h
//
// PURPOSE:	Provides generic methods that use UCLIDComUtils objects
//
// NOTES:	
//
// AUTHORS:	William Parr
//
//=================================================================================================

#pragma once

#include "ComUtilsExport.h"

#include <comdef.h>
#include <string>

using namespace std;

// PROMISE: Clone an object depending on the parameter bWithCloneIdentifiable and whether ipObject
//			implements the ICloneIdentifiableObject interface
// Args:	strELI - ELI code of exception thrown if ipObject doesn't implement ICopyableObject
//			If ipObject is null returns null
//			If bWithCloneIdentifiableObject is true and ipObject defines the interface ICloneIdentifiableObject
//			then object returned is cloned using ICloneIdentifiableObject interface
//			Otherwise object is cloned using the ICopyableObject interface
EXPORT_UCLIDCOMUtils IUnknownPtr cloneObject(string strELI, IUnknownPtr ipObject, bool bWithCloneIdentifiableObject);

