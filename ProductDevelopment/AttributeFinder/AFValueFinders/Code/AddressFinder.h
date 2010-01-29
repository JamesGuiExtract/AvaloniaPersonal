#pragma once


#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"
#include <string>

/////////////////////////////////////////////////////////////////////////////
// CAddressFinder
class ATL_NO_VTABLE CAddressFinder : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CAddressFinder, &CLSID_AddressFinder>,
	public ISupportErrorInfo,
	public IDispatchImpl<IAddressFinder, &IID_IAddressFinder, &LIBID_UCLID_AFVALUEFINDERSLib>,
	public IDispatchImpl<IAttributeFindingRule, &IID_IAttributeFindingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CAddressFinder();
	~CAddressFinder();

DECLARE_REGISTRY_RESOURCEID(IDR_ADDRESSFINDER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CAddressFinder)
	COM_INTERFACE_ENTRY(IAddressFinder)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IAddressFinder)
	COM_INTERFACE_ENTRY(IAttributeFindingRule)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_CATEGORY_MAP(CAddressFinder)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_FINDERS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IAddressFinder

// IAttributeFindingRule
	STDMETHOD(raw_ParseText)(IAFDocument * pAFDoc, IProgressStatus *pProgressStatus,
		IIUnknownVector **pAttributes);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStream);
	STDMETHOD(Save)(IStream *pStream, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	bool m_bDirty;

	void validateLicense();
	std::string loadRegExp(std::string strFileName, IAFDocumentPtr ipAFDoc);
	IIUnknownVectorPtr chooseAddressBlocks(IRegularExprParserPtr ipSuffixParser, IIUnknownVectorPtr ipBlocks);

	IAFUtilityPtr	m_ipAFUtility;
	IMiscUtilsPtr	m_ipMiscUtils;
	IRegularExprParserPtr m_ipRegExpParser;
};

