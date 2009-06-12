// ObjA.h : Declaration of the CObjA

#ifndef __OBJA_H_
#define __OBJA_H_

#include "resource.h"       // main symbols

#include <string>

/////////////////////////////////////////////////////////////////////////////
// CObjA
class ATL_NO_VTABLE CObjA : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CObjA, &CLSID_ObjA>,
	public ISupportErrorInfo,
	public ISpecifyPropertyPagesImpl<CObjA>,
	public IDispatchImpl<IObjA, &IID_IObjA, &LIBID_TESTOBJSWITHPROPPAGESLib>
{
public:
	CObjA()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_OBJA)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CObjA)
	COM_INTERFACE_ENTRY(IObjA)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CObjA)
	PROP_PAGE(CLSID_ObjAPropPage)
END_PROP_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IObjA
	STDMETHOD(get_RegExpr)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_RegExpr)(/*[in]*/ BSTR newVal);

private:
	_bstr_t m_bstrRegExpr;
};

#endif //__OBJA_H_
