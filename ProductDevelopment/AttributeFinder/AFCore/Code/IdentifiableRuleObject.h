//=================================================================================================
//
// COPYRIGHT (c) 2012 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	IdentifiableRuleObject.h
//
// PURPOSE:	Provides implmentation for IdentifiableRuleObject that can be used by rule objects.
//
// NOTES:	
//
// AUTHORS:	Steve Kurth
//
//=================================================================================================

#pragma once

#include "Export.h"

#include <string>
#include <memory>

using namespace std;

//--------------------------------------------------------------------------------------------------
// CIdentifiableRuleObject
//--------------------------------------------------------------------------------------------------
class EXPORT_AFCore CIdentifiableRuleObject
{
public:
	CIdentifiableRuleObject(void);
	~CIdentifiableRuleObject(void);

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

