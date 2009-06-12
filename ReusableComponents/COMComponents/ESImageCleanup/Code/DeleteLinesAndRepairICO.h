// DeleteLinesAndRepairICO.h : Declaration of the CDeleteLinesAndRepairICO

#pragma once
#include "resource.h"       // main symbols
#include "ICCategories.h"
#include "stdafx.h"
#include "ESImageCleanup.h"

#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

// CDeleteLinesAndRepairICO

class ATL_NO_VTABLE CDeleteLinesAndRepairICO :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CDeleteLinesAndRepairICO, &CLSID_DeleteLinesAndRepairICO>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDeleteLinesAndRepairICO,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IImageCleanupOperation, &IID_IImageCleanupOperation, &LIBID_ESImageCleanupLib>,
	public ISpecifyPropertyPagesImpl<CDeleteLinesAndRepairICO>
{
public:
	CDeleteLinesAndRepairICO();
	~CDeleteLinesAndRepairICO();

	DECLARE_REGISTRY_RESOURCEID(IDR_DELETELINESANDREPAIRICO)

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	BEGIN_COM_MAP(CDeleteLinesAndRepairICO)
		COM_INTERFACE_ENTRY(IDeleteLinesAndRepairICO)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(IPersistStream)
		COM_INTERFACE_ENTRY(ICategorizedComponent)
		COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
		COM_INTERFACE_ENTRY(ICopyableObject)
		COM_INTERFACE_ENTRY(ILicensedComponent)
		COM_INTERFACE_ENTRY(IImageCleanupOperation)
		COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
	END_COM_MAP()

	BEGIN_PROP_MAP(CDeleteLinesAndRepairICO)
		PROP_PAGE(CLSID_DeleteLinesAndRepairICOPP)
	END_PROP_MAP()

	BEGIN_CATEGORY_MAP(CDeleteLinesAndRepairICO)
		IMPLEMENTED_CATEGORY(CATID_ICO_CLEANING_OPERATIONS)
	END_CATEGORY_MAP()

public:
	// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

	// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

	// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown* *pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown *pObject);

	// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

	// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

	// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbValue);

	// IImageCleanupOperation Methods
	STDMETHOD(Perform)(void* pciRepair);

	// IDeleteLinesAndRepairICO Methods
	STDMETHOD(get_LineLength)(long* plLineLength);
	STDMETHOD(put_LineLength)(long lLineLength);
	STDMETHOD(get_LineGap)(long* plLineGap);
	STDMETHOD(put_LineGap)(long lLineGap);
	STDMETHOD(get_LineDirection)(unsigned long* pulLineDirection);
	STDMETHOD(put_LineDirection)(unsigned long ulLineDirection);

private:
	//----------------------------------------------------------------------------------------------
	// Variables
	//----------------------------------------------------------------------------------------------
	bool m_bDirty;

	// minimum line length in pixels
	long m_lLineLength;

	// maximum gap in line in pixels
	long m_lLineGap;

	// direction of lines to be removed
	unsigned long m_ulLineDirection;

	//----------------------------------------------------------------------------------------------
	// Methods
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To validate the image cleanup license
	void validateLicense();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To check if the line direction has been configured
	bool isDirectionConfigured();
	//----------------------------------------------------------------------------------------------
};
OBJECT_ENTRY_AUTO(__uuidof(DeleteLinesAndRepairICO), CDeleteLinesAndRepairICO)
