// ObjA.cpp : Implementation of CObjA
#include "stdafx.h"
#include "TestObjsWithPropPages.h"
#include "ObjA.h"

/////////////////////////////////////////////////////////////////////////////
// CObjA

STDMETHODIMP CObjA::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IObjA
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

STDMETHODIMP CObjA::get_RegExpr(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pVal = m_bstrRegExpr.copy();

	return S_OK;
}

STDMETHODIMP CObjA::put_RegExpr(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	m_bstrRegExpr = newVal;

	return S_OK;
}
