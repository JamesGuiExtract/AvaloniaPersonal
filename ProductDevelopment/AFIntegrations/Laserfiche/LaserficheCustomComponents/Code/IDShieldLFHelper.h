#pragma once

#include "stdafx.h"
#include "resource.h"

class CIDShieldLF;

//--------------------------------------------------------------------------------------------------
// CIDShieldLFHelper
//--------------------------------------------------------------------------------------------------
// A base class for any CIDShieldLF helper class to ensure the CIDShieldLF instance being used is
// not destroyed before the helper that is counting on it is.
class CIDShieldLFHelper
{
public:
	CIDShieldLFHelper(CIDShieldLF *pIDShieldLF);
	virtual ~CIDShieldLFHelper(void);

protected:

	////////////////
	// Variables
	////////////////

	CIDShieldLF *m_pIDShieldLF;
};