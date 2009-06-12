// OutputToVOA.h : Declaration of the COutputToVOA

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"
#include <string>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// COutputToVOA
class ATL_NO_VTABLE COutputToVOA : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<COutputToVOA, &CLSID_OutputToVOA>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDispatchImpl<IOutputToVOA, &IID_IOutputToVOA, &LIBID_UCLID_AFOUTPUTHANDLERSLib>,
	public IDispatchImpl<IOutputHandler, &IID_IOutputHandler, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<COutputToVOA>
{
public:
	COutputToVOA();

DECLARE_REGISTRY_RESOURCEID(IDR_OUTPUTTOVOA)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(COutputToVOA)
	COM_INTERFACE_ENTRY(IOutputToVOA)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IOutputToVOA)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(IOutputHandler)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(COutputToVOA)
	PROP_PAGE(CLSID_OutputToVOAPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(COutputToVOA)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_OUTPUT_HANDLERS)
END_CATEGORY_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IOutputToVOA
	STDMETHOD(get_FileName)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_FileName)(/*[in]*/ BSTR newVal);

// IOutputHandler
	STDMETHOD(raw_ProcessOutput)(IIUnknownVector *pAttributes, IAFDocument *pAFDoc,
		IProgressStatus *pProgressStatus);

	// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbValue);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

private:
	// flag to keep track of whether object is dirty
	bool m_bDirty;
	IAFUtilityPtr m_ipAFUtils;

	// File Name to save the results to
	string m_strFileName;

	// helper functions
	void validateLicense();

};

