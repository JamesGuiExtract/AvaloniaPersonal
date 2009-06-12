// ObjB.h : Declaration of the CObjB

#ifndef __OBJB_H_
#define __OBJB_H_

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CObjB
class ATL_NO_VTABLE CObjB : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CObjB, &CLSID_ObjB>,
	public ISupportErrorInfo,
	public ISpecifyPropertyPagesImpl<CObjB>,
	public IDispatchImpl<IObjB, &IID_IObjB, &LIBID_TESTOBJSWITHPROPPAGESLib>
{
public:
	CObjB()
	{
		m_nStartPos = 0;
		m_nEndPos = 0;
	}

DECLARE_REGISTRY_RESOURCEID(IDR_OBJB)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CObjB)
	COM_INTERFACE_ENTRY(IObjB)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CObjB)
	PROP_PAGE(CLSID_ObjBPropPage)
END_PROP_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IObjB
	STDMETHOD(get_EndPos)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_EndPos)(/*[in]*/ long newVal);
	STDMETHOD(get_StartPos)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_StartPos)(/*[in]*/ long newVal);

private:
	long m_nStartPos, m_nEndPos;
};

#endif //__OBJB_H_
