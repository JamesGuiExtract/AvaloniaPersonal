// SpatialPageInfo.h : Declaration of the CSpatialPageInfo

#ifndef __SPATIALPAGEINFO_H_
#define __SPATIALPAGEINFO_H_

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CSpatialPageInfo
class ATL_NO_VTABLE CSpatialPageInfo : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSpatialPageInfo, &CLSID_SpatialPageInfo>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ISpatialPageInfo, &IID_ISpatialPageInfo, &LIBID_UCLID_RASTERANDOCRMGMTLib>
{
public:
	CSpatialPageInfo();
	~CSpatialPageInfo();

DECLARE_REGISTRY_RESOURCEID(IDR_SPATIALPAGEINFO)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CSpatialPageInfo)
	COM_INTERFACE_ENTRY(ISpatialPageInfo)
	COM_INTERFACE_ENTRY2(IDispatch, ISpatialPageInfo)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IPersistStream)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ISpatialPageInfo
public:
	STDMETHOD(get_Deskew)(double *pVal);
	STDMETHOD(put_Deskew)(double newVal);

	STDMETHOD(get_Orientation)(EOrientation *pVal);
	STDMETHOD(put_Orientation)(EOrientation newVal);

	STDMETHOD(get_Width)(long *pVal);
	STDMETHOD(put_Width)(long newVal);

	STDMETHOD(get_Height)(long *pVal);
	STDMETHOD(put_Height)(long newVal);
	STDMETHOD(SetPageInfo)(long lWidth, long lHeight, EOrientation eOrientation, double dDeskew);
	STDMETHOD(GetWidthAndHeight)(long* plWidth, long* plHeight);
	STDMETHOD(GetPageInfo)(long* plWidth, long* plHeight,
		EOrientation* peOrientation, double* pdDeskew);
	STDMETHOD(Equal)(ISpatialPageInfo* pPageInfo, VARIANT_BOOL* pEqual);
	STDMETHOD(GetTheta)(double* pdTheta);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown **pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown *pObject);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

private:
	UCLID_RASTERANDOCRMGMTLib::EOrientation m_eOrientation;
	double m_fDeskew;

	long m_nWidth;
	long m_nHeight;

	bool m_bDirty;
};

#endif //__SPATIALPAGEINFO_H_
