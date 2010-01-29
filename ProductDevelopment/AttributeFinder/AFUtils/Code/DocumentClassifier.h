// DocumentClassifier.h : Declaration of the CDocumentClassifier

#pragma once

#include "resource.h"       // main symbols

#include "..\..\AFCore\Code\AFCategories.h"
#include "DocTypeInterpreter.h"
#include "DocPageCache.h"
#include <string>
#include <vector>
#include <map>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CDocumentClassifier
class ATL_NO_VTABLE CDocumentClassifier : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CDocumentClassifier, &CLSID_DocumentClassifier>,
	public ISupportErrorInfo,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IDocumentClassifier, &IID_IDocumentClassifier, &LIBID_UCLID_AFUTILSLib>,
	public IDispatchImpl<IDocumentPreprocessor, &IID_IDocumentPreprocessor, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IDocumentClassificationUtils, &IID_IDocumentClassificationUtils, &LIBID_UCLID_AFUTILSLib>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CDocumentClassifier>
{
public:
	CDocumentClassifier();
	~CDocumentClassifier();

DECLARE_REGISTRY_RESOURCEID(IDR_DOCUMENTCLASSIFIER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

BEGIN_COM_MAP(CDocumentClassifier)
	COM_INTERFACE_ENTRY(IDocumentClassifier)
	COM_INTERFACE_ENTRY2(IDispatch,IDocumentClassifier)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IDocumentPreprocessor)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(IDocumentClassificationUtils)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CDocumentClassifier)
	PROP_PAGE(CLSID_DocumentClassifierPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CCountyDocumentClassifier)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_DOCUMENT_PREPROCESSORS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IDocumentClassifier
	STDMETHOD(get_IndustryCategoryName)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_IndustryCategoryName)(/*[in]*/ BSTR newVal);

// IDocumentPreprocessor
	STDMETHOD(raw_Process)(/*[in]*/ IAFDocument* pDocument,/*[in]*/ IProgressStatus *pProgressStatus);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbValue);

// IDocumentClassificationUtils
	STDMETHOD(GetDocumentIndustries)(
		/*[out, retval]*/ IVariantVector** ppIndustries);
	STDMETHOD(GetSpecialDocTypeTags)(
		/*[out, retval]*/ IVariantVector** ppTags);
	STDMETHOD(GetDocumentTypes)(
		/*[in]*/ BSTR strIndustry, 
		/*[out, retval]*/ IVariantVector** ppTypes);
	STDMETHOD(GetDocTypeSelection)(
		/*[in, out]*/ BSTR* pbstrIndustry, 
		/*[in]*/ VARIANT_BOOL bAllowIndustryModification,
		/*[in]*/ VARIANT_BOOL bAllowMultipleSelection,
		/*[in]*/ VARIANT_BOOL bAllowSpecialTags,
		/*[out, retval]*/ IVariantVector** ppTypes);

private:

	/////////////
	// Variables
	/////////////
	bool m_bDirty;

	string m_strIndustryCategoryName;

	UCLID_AFUTILSLib::IAFUtilityPtr m_ipAFUtility;

	// Each industry name associated with a vector of DocTypeInterpreters
	// Each interpreter loads individual doc type file, interpret 
	// the contents and is able to tell a confidence level
	// given the input text (document)
	map<string, vector<DocTypeInterpreter> > m_mapNameToVecInterpreters;

	// Each industry name associated with a vector of DocType names
	map<string, vector<string> > m_mapNameToVecDocTypes;

	//////////
	// Methods
	//////////
	// Adds special document type tags to rvecTags
	void	appendSpecialTags(vector<string>& rvecTags);

	// create document probability and confidence level in pAFDoc
	void createDocTags(IAFDocumentPtr ipAFDoc, const string& strSpecificIndustryName);

	// load DocTypes.idx file, which contains all available document
	// types for specified industry category. Based on these type names,
	// find doc type file with exactly same name and load the file into each
	// DocTypeInterpreter from m_vecDocTypeInterpreters.
	// strSpecificIndustryName - ex, County Documents, etc.
	void loadDocTypeFiles(const string& strSpecificIndustryName);

	void validateLicense();
};

