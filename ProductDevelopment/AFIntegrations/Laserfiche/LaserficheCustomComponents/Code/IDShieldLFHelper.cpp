// IDShieldLFHelper.cpp : Implmentation for CIDShieldLFHelper
#include "StdAfx.h"
#include "IDShieldLFHelper.h"
#include "IDShieldLF.h"

#include <Win32Event.h>
#include <UCLIDException.h>

//--------------------------------------------------------------------------------------------------
// CIDShieldLFHelper
//--------------------------------------------------------------------------------------------------
CIDShieldLFHelper::CIDShieldLFHelper(CIDShieldLF *pIDShieldLF)
	: m_pIDShieldLF(pIDShieldLF)
{
	try
	{
		ASSERT_RESOURCE_ALLOCATION("ELI21016", m_pIDShieldLF != NULL);

		m_pIDShieldLF->m_eventHelpersDone.reset();
		InterlockedIncrement(&m_pIDShieldLF->m_nHelperReferenceCount);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI21015");
}
//--------------------------------------------------------------------------------------------------
CIDShieldLFHelper::~CIDShieldLFHelper(void)
{
	try
	{
		InterlockedDecrement(&m_pIDShieldLF->m_nHelperReferenceCount);
		if (m_pIDShieldLF->m_nHelperReferenceCount == 0)
		{
			m_pIDShieldLF->m_eventHelpersDone.signal();
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI21014");
}
//--------------------------------------------------------------------------------------------------