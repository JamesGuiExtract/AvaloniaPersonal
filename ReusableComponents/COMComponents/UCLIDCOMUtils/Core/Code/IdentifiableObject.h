//=================================================================================================
//
// COPYRIGHT (c) 2012 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	IdentifiableObject.h
//
// PURPOSE:	Provides implmentation for IdentifiableObject that can be used by objects.
//
// NOTES:	
//
// AUTHORS:	Steve Kurth
//
//=================================================================================================

#pragma once

#include "ComUtilsExport.h"

#include <string>
#include <memory>

using namespace std;

//--------------------------------------------------------------------------------------------------
// CIdentifiableObject
//--------------------------------------------------------------------------------------------------
class EXPORT_UCLIDCOMUtils CIdentifiableObject
{
public:
	CIdentifiableObject(void);
	~CIdentifiableObject(void);

protected:

	GUID getGUID(bool bRegenerate = false);
	void loadGUID(IStream *pStream);
	void saveGUID(IStream *pStream);

private: 

	////////////
	// Variables
	////////////

	unique_ptr<GUID> m_upGUID;

	////////////
	// Methods
	////////////

	void validateLicense();
};

