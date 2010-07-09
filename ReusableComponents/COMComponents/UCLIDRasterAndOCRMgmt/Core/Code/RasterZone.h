// RasterZone.h : Declaration of the CRasterZone

#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CRasterZone
class ATL_NO_VTABLE CRasterZone : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CRasterZone, &CLSID_RasterZone>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IRasterZone, &IID_IRasterZone, &LIBID_UCLID_RASTERANDOCRMGMTLib>,
	public IDispatchImpl<IComparableObject, &IID_IComparableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CRasterZone();

DECLARE_REGISTRY_RESOURCEID(IDR_RASTERZONE)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CRasterZone)
	COM_INTERFACE_ENTRY(IRasterZone)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IRasterZone)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IComparableObject)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IRasterZone
	STDMETHOD(get_StartX)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_StartX)(/*[in]*/ long newVal);
	STDMETHOD(get_StartY)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_StartY)(/*[in]*/ long newVal);
	STDMETHOD(get_EndX)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_EndX)(/*[in]*/ long newVal);
	STDMETHOD(get_EndY)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_EndY)(/*[in]*/ long newVal);
	STDMETHOD(get_Height)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_Height)(/*[in]*/ long newVal);
	STDMETHOD(get_PageNumber)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_PageNumber)(/*[in]*/ long newVal);
	STDMETHOD(Equals)(/*[in]*/ IRasterZone *pRasterZone, /*[out, retval]*/ VARIANT_BOOL *pbValue);
	STDMETHOD(Clear)();
	STDMETHOD(CopyDataTo)(/*[in]*/ IRasterZone *pRasterZone);
	STDMETHOD(RotateBy)(/*[in]*/ double dAngleInDegrees);
	STDMETHOD(GetData)(/*[in]*/ long *pStartX, /*[in]*/ long *pStartY, /*[in]*/ long *pEndX, /*[in]*/ long *pEndY, /*[in]*/ long *pHeight, /*[in]*/ long *pPageNum);
	STDMETHOD(CreateFromLongRectangle)(/*[in]*/ ILongRectangle *pRectangle, /*[in]*/ long nPageNum);
	STDMETHOD(GetRectangularBounds)(/*[in]*/ ILongRectangle *pPageBounds, 
		/*[out, retval]*/ ILongRectangle* *pRectangle);
	STDMETHOD(GetBoundaryPoints)(/*[out, retval]*/ IIUnknownVector** pRetVal);
	STDMETHOD(get_Area)(/*[out, retval]*/ long *pVal);
	STDMETHOD(GetAreaOverlappingWith)(/*[in]*/ IRasterZone *pRasterZone, /*[out, retval]*/ double *pVal);
	STDMETHOD(GetBoundsFromMultipleRasterZones)(IIUnknownVector* pZones,
		ISpatialPageInfo* pPageInfo, ILongRectangle** ppRectangle);
	STDMETHOD(CreateFromData)(long lStartX, long lStartY, long lEndX, long lEndY,
		long lHeight, long lPageNum);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(/*[out, retval]*/ VARIANT_BOOL * pbValue);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown **pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown *pObject);

// IComparableObject
	STDMETHOD(raw_IsEqualTo)(IUnknown * pObj, VARIANT_BOOL * pbValue);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

private:
	long m_nStartX, m_nStartY, m_nEndX, m_nEndY, m_nHeight, m_nPage;

	bool m_bDirty;

	// PURPOSE: To validate proper licensing of this component
	// PROMISE: To throw an exception if this component is not licensed to run
	void validateLicense();

	// PURPOSE: Clear all the member variables, do not set the dirty flag
	void clear();

	// PURPOSE: To determine if the specified raster zone is equal to this raster zone
	bool equals(UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone);
};
