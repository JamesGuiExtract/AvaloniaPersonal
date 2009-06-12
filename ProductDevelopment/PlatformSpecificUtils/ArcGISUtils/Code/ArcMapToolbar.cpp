// ArcMapToolbar.cpp : Implementation of CArcMapToolbar
#include "stdafx.h"
#include "ArcGISUtils.h"
#include "ArcMapToolbar.h"

#include "UCLIDArcMapToolbarCATID.h"

#include <COMUtils.h>
#include <UCLIDException.h>

using namespace std;
/////////////////////////////////////////////////////////////////////////////
// CArcMapToolbar
CArcMapToolbar::CArcMapToolbar()
{
	try
	{
		m_vecComponentProgIDs = getComponentProgIDsInCategory(UCLID_ARCMAP_TOOLBAR);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04104")
}
CArcMapToolbar::~CArcMapToolbar()
{
}

/////////////////////////////////////////////////////////////////////////////
// ISupportsErrorInfo
STDMETHODIMP CArcMapToolbar::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IToolBarDef
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

/////////////////////////////////////////////////////////////////////////////
// IToolBarDef
STDMETHODIMP CArcMapToolbar::get_ItemCount(long *numItems)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	if (! numItems) return E_POINTER;

	*numItems = m_vecComponentProgIDs.size();

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArcMapToolbar::raw_GetItemInfo(long pos, IItemDef *itemDef)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (pos < long (m_vecComponentProgIDs.size()) )
	{
		itemDef->ID = _bstr_t(m_vecComponentProgIDs.at(pos).c_str());
	}

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArcMapToolbar::get_Name(BSTR *Name)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (! Name) return E_POINTER;

	*Name = _bstr_t("Extract Systems Toolbar").copy();

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CArcMapToolbar::get_Caption(BSTR *Name)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (! Name) return E_POINTER;

	*Name = _bstr_t("Extract Systems Tools").copy();

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
