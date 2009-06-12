// ArcMapToolbar.h : Declaration of the CArcMapToolbar

#pragma once

#include "resource.h"       // main symbols

#include "..\..\..\..\ReusableComponents\APIs\ArcGIS\Inc\ArcCATIDs.h"

#include <string>
#include <vector>

/////////////////////////////////////////////////////////////////////////////
// CArcMapToolbar
class ATL_NO_VTABLE CArcMapToolbar : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CArcMapToolbar, &CLSID_ArcMapToolbar>,
	public ISupportErrorInfo,
	public IToolBarDef
{
public:
	CArcMapToolbar();
	~CArcMapToolbar();

DECLARE_REGISTRY_RESOURCEID(IDR_ARCMAPTOOLBAR)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CArcMapToolbar)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IToolBarDef)
END_COM_MAP()

BEGIN_CATEGORY_MAP(CUCLIDToolbar)
	IMPLEMENTED_CATEGORY(__uuidof(CATID_MxCommandBars))
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IToolBarDef
	STDMETHOD(get_ItemCount)(LONG * numItems);
	STDMETHOD(raw_GetItemInfo)(LONG pos, IItemDef * itemDef);
	STDMETHOD(get_Name)(BSTR * Name);
	STDMETHOD(get_Caption)(BSTR * Name);

private:
	// all components registered under uclid toolbar
	std::vector<std::string> m_vecComponentProgIDs;
};
