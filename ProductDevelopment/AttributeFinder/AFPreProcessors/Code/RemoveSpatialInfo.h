// RemoveSpatialInfo.h : Declaration of the CRemoveSpatialInfo

#ifndef __REMOVESPATIALINFO_H_
#define __REMOVESPATIALINFO_H_

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"

#include <string>
#include <vector>

/////////////////////////////////////////////////////////////////////////////
// CRemoveSpatialInfo
class ATL_NO_VTABLE CRemoveSpatialInfo : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CRemoveSpatialInfo, &CLSID_RemoveSpatialInfo>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDispatchImpl<IRemoveSpatialInfo, &IID_IRemoveSpatialInfo, &LIBID_UCLID_AFPREPROCESSORSLib>,
	public IDispatchImpl<IDocumentPreprocessor, &IID_IDocumentPreprocessor, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<IAttributeModifyingRule, &IID_IAttributeModifyingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<IOutputHandler, &IID_IOutputHandler, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CRemoveSpatialInfo();
	~CRemoveSpatialInfo();

DECLARE_REGISTRY_RESOURCEID(IDR_REMOVESPATIALINFO)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CRemoveSpatialInfo)
	COM_INTERFACE_ENTRY(IRemoveSpatialInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IRemoveSpatialInfo)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IDocumentPreprocessor)
	COM_INTERFACE_ENTRY(IAttributeModifyingRule)
	COM_INTERFACE_ENTRY(IOutputHandler)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
END_COM_MAP()

BEGIN_CATEGORY_MAP(CRemoveSpatialInfo)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_DOCUMENT_PREPROCESSORS)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_MODIFIERS)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_OUTPUT_HANDLERS)
END_CATEGORY_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IRemoveSpatialInfo

// IDocumentPreprocessor
	STDMETHOD(raw_Process)(/*[in]*/ IAFDocument* pDocument, IProgressStatus *pProgressStatus);

// IOutputHandler
	STDMETHOD(raw_ProcessOutput)(IIUnknownVector *pAttributes, IAFDocument *pAFDoc,
		IProgressStatus *pProgressStatus);

// IAttributeModifyingRule
	STDMETHOD(raw_ModifyValue)(IAttribute* pAttribute, IAFDocument* pOriginInput, 
		IProgressStatus *pProgressStatus);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICopyableObject
	STDMETHOD(raw_Clone)(/*[out, retval]*/ IUnknown* *pObject);
	STDMETHOD(raw_CopyFrom)(/*[in]*/ IUnknown *pObject);
private:

	bool m_bDirty;

	// remove spatial information from a SpatialString
	void removeSpatialInfo(ISpatialStringPtr ipSS);
	// recursive Function removes all spatial information
	// of all attributes and sub-attributes
	void removeSpatialInfo(IAttributePtr ipAttribute);

	void validateLicense();
};

#endif //__REMOVESPATIALINFO_H_
