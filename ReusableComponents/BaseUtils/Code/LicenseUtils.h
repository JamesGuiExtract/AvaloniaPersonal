//============================================================================
//
// COPYRIGHT (c) 2007 - 2008 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	LicenseUtils.h
//
// PURPOSE:	Definition of Licensing utility functions
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//============================================================================

#pragma once

#include "BaseUtils.h"
#include "ByteStream.h"

using namespace std;

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
// String constant indicating that Debug Info is encrypted
const std::string	gstrENCRYPTED_PREFIX = "Extract_Encrypted: ";

//--------------------------------------------------------------------------------------------------
// Exported Functions
//--------------------------------------------------------------------------------------------------
// PURPOSE: This function checks licensing of Internal Tools 
//			functionality.  If the computer has a drive mapping to 
//			\\fnp\internal and the drive serial number is correct, 
//			Internal Tools is presumed licensed.
EXPORT_BaseUtils bool isInternalToolsLicensed();

//--------------------------------------------------------------------------------------------------
// Local Functions - only for use within BaseUtils and NOT to be exported
//--------------------------------------------------------------------------------------------------
void	getUEPassword(ByteStream& rPasswordBytes);
