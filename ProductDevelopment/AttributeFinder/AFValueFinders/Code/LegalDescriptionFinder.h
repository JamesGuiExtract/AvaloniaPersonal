// LegalDescriptionFinder.h : Declaration of the CLegalDescriptionFinder

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"

#include <string>
#include <vector>

/////////////////////////////////////////////////////////////////////////////
// CLegalDescriptionFinder
class ATL_NO_VTABLE CLegalDescriptionFinder : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CLegalDescriptionFinder, &CLSID_LegalDescriptionFinder>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDispatchImpl<ILegalDescriptionFinder, &IID_ILegalDescriptionFinder, &LIBID_UCLID_AFVALUEFINDERSLib>,
	public IDispatchImpl<IAttributeFindingRule, &IID_IAttributeFindingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>
{
public:
	CLegalDescriptionFinder();
	~CLegalDescriptionFinder();

DECLARE_REGISTRY_RESOURCEID(IDR_LEGALDESCRIPTIONFINDER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CLegalDescriptionFinder)
	COM_INTERFACE_ENTRY(ILegalDescriptionFinder)
	COM_INTERFACE_ENTRY2(IDispatch,ILegalDescriptionFinder)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IAttributeFindingRule)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IPersistStream)
END_COM_MAP()

BEGIN_CATEGORY_MAP(CLegalDescriptionFinder)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_FINDERS)
END_CATEGORY_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IAttributeFindingRule
	STDMETHOD(raw_ParseText)(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus,
		IIUnknownVector **pAttributes);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICopyableObject
	STDMETHOD(raw_Clone)(/*[out, retval]*/ IUnknown* *pObject);
	STDMETHOD(raw_CopyFrom)(/*[in]*/ IUnknown *pObject);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// ILegalDescriptionFinder
public:

private:	
	struct Position
	{
		long m_nStartPos;
		long m_nEndPos;
	};

	////////////
	// Variables
	////////////
	// flag to keep track of whether this object has been modified
	// since the last save-to-stream operation
	bool m_bDirty;
	// use block finder to extract paragraphs
	UCLID_AFVALUEFINDERSLib::IBlockFinderPtr m_ipBlockFinder;


	///////////
	// Methods
	///////////
	// Examine the text in between every two adjacent attributes.
	// If the text only contains white spaces, then combine these two
	// attributes into one, and so on.
	void combineNearbyAttributes(ISpatialStringPtr ipOriginInput, IIUnknownVectorPtr ipAttributes);

	// find the line break string from the input string
	std::string getLineBreakString(const std::string& strInput);

	void validateLicense();
};

