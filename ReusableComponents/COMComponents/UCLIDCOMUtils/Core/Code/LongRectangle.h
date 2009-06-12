// LongRectangle.h : Declaration of the CLongRectangle

#pragma once

#include "resource.h"       // main symbols

#include <comdef.h>

/////////////////////////////////////////////////////////////////////////////
// CLongRectangle
class ATL_NO_VTABLE CLongRectangle : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CLongRectangle, &CLSID_LongRectangle>,
	public ISupportErrorInfo,
	public IDispatchImpl<ILongRectangle, &IID_ILongRectangle, &LIBID_UCLID_COMUTILSLib>
{
public:
	CLongRectangle();

DECLARE_REGISTRY_RESOURCEID(IDR_LONGRECTANGLE)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CLongRectangle)
	COM_INTERFACE_ENTRY(ILongRectangle)
	COM_INTERFACE_ENTRY2(IDispatch, ILongRectangle)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ICopyableObject)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ICopyableObject
	STDMETHOD(Clone)(/*[out, retval]*/ IUnknown* *pObject);
	STDMETHOD(CopyFrom)(/*[in]*/ IUnknown *pObject);

// ILongRectangle
	STDMETHOD(get_Bottom)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_Bottom)(/*[in]*/ long newVal);
	STDMETHOD(get_Right)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_Right)(/*[in]*/ long newVal);
	STDMETHOD(get_Top)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_Top)(/*[in]*/ long newVal);
	STDMETHOD(get_Left)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_Left)(/*[in]*/ long newVal);
	STDMETHOD(SetBounds)(/*[in]*/ long nLeft, /*[in]*/ long nTop, /*[in]*/ long nRight, /*[in]*/ long nBottom);
	STDMETHOD(Offset)(/*[in]*/ long nX, /*[in]*/ long nY);
	STDMETHOD(Expand)(/*[in]*/ long nX, /*[in]*/ long nY);
	STDMETHOD(Clip)(/*[in]*/ long nLeft, /*[in]*/ long nTop, /*[in]*/ long nRight, /*[in]*/ long nBottom);
	STDMETHOD(Rotate)(/*[in]*/ long nXLimit, /*[in]*/ long nYLimit, /*[in]*/ long nAngleInDegrees);
	STDMETHOD(GetBounds)(long* plLeft, long* plTop, long* plRight, long* plBottom);

private:
	long m_nLeft, m_nRight, m_nTop, m_nBottom;
};
