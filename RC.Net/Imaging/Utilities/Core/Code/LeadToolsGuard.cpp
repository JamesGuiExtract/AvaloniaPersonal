#include "StdAfx.h"
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
	catch (Exception ^e)
	{
		if (m_pRestrict != nullptr)
		{
			delete m_pRestrict;
			m_pRestrict = nullptr;
		}
		throw e;
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
