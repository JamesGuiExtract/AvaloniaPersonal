// ObjB.cpp : Implementation of CObjB
#include "stdafx.h"
#include "TestObjsWithPropPages.h"
#include "ObjB.h"

/////////////////////////////////////////////////////////////////////////////
// CObjB

STDMETHODIMP CObjB::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IObjB
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

STDMETHODIMP CObjB::get_StartPos(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pVal = m_nStartPos;

	return S_OK;
}

STDMETHODIMP CObjB::put_StartPos(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	m_nStartPos = newVal;

	return S_OK;
}

STDMETHODIMP CObjB::get_EndPos(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pVal = m_nEndPos;

	return S_OK;
}

STDMETHODIMP CObjB::put_EndPos(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	m_nEndPos = newVal;

	return S_OK;
}
