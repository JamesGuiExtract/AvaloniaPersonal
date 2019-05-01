#include "StdAfx.h"

#define pointer_safety Pointer_safety
#define errc Errc
#define io_errc IO_errc

#include "LeadToolsGuard.h"

#include <LeadToolsLicenseRestrictor.h>

using namespace System;
using namespace Extract::Imaging::Utilities;

LeadtoolsGuard::LeadtoolsGuard()
	: m_pRestrict(nullptr)
{
	try
	{
		m_pRestrict = new LeadToolsLicenseRestrictor();
	}
	catch (Exception ^)
	{
		if (m_pRestrict != nullptr)
		{
			delete m_pRestrict;
			m_pRestrict = nullptr;
		}
		throw;
	}
}
//----------------------------------------------------------------------------------------------------------------------
LeadtoolsGuard::~LeadtoolsGuard()
{
	if (m_pRestrict != nullptr)
	{
		delete m_pRestrict;
		m_pRestrict = nullptr;
	}
}
