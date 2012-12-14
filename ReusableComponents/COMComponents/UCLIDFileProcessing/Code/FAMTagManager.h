// FAMTagManager.h : Declaration of the CFAMTagManager

#pragma once

#include "resource.h"       // main symbols
#include "UCLIDFileProcessing.h"
#include <string>
#include <vector>

/////////////////////////////////////////////////////////////////////////////
// CFAMTagManager
class ATL_NO_VTABLE CFAMTagManager : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CFAMTagManager, &CLSID_FAMTagManager>,
	public ISupportErrorInfo,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ITagUtility, &IID_ITagUtility, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IFAMTagManager, &IID_IFAMTagManager, &LIBID_UCLID_FILEPROCESSINGLib>
{
public:
	CFAMTagManager();
	~CFAMTagManager();

DECLARE_REGISTRY_RESOURCEID(IDR_FAMTagManager)

BEGIN_COM_MAP(CFAMTagManager)
	COM_INTERFACE_ENTRY(IFAMTagManager)
	COM_INTERFACE_ENTRY2(IDispatch,IFAMTagManager)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ITagUtility)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
END_COM_MAP()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ITagUtility
	STDMETHOD(raw_ExpandTags)(BSTR bstrInput, BSTR bstrSourceDocName, IUnknown *pData, BSTR *pbstrOutput);
	STDMETHOD(raw_ExpandTagsAndFunctions)(BSTR bstrInput, BSTR bstrSourceDocName, IUnknown *pData, BSTR *pbstrOutput);
	STDMETHOD(raw_GetBuiltInTags)(IVariantVector* *ppTags);
	STDMETHOD(raw_GetINIFileTags)(IVariantVector* *ppTags);
	STDMETHOD(raw_GetAllTags)(IVariantVector* *ppTags);
	STDMETHOD(raw_GetFunctionNames)(IVariantVector** ppFunctionNames);
	STDMETHOD(raw_GetFormattedFunctionNames)(IVariantVector** ppFunctionNames);

// IFAMTagManager
	STDMETHOD(get_FPSFileDir)(BSTR *strFPSDir);
	STDMETHOD(put_FPSFileDir)(BSTR strFPSDir);
	STDMETHOD(ExpandTags)(BSTR bstrInput, BSTR bstrSourceName, BSTR *pbstrOutput);
	STDMETHOD(ExpandTagsAndFunctions)(BSTR bstrInput, BSTR bstrSourceName, BSTR *pbstrOutput);
	STDMETHOD(StringContainsInvalidTags)(BSTR strInput, VARIANT_BOOL *pbValue);
	STDMETHOD(StringContainsTags)(BSTR strInput, VARIANT_BOOL *pbValue);

private:

	///////////
	//Variables
	//////////

	std::string m_strFPSDir;

	// pointer to the utility object to use for path function expansion.
	IMiscUtilsPtr m_ipMiscUtils;

	//////////
	//Methods
	/////////

	//---------------------------------------------------------------------------------------------
	// PROMISE: To return all tag names in strInput.
	//			The returned strings will include the < and > chars
	void getTagNames(const std::string& strInput, 
		std::vector<std::string>& rvecTagNames) const;

	void expandTags(std::string &strInput, const std::string &strSourceDocName);

	void validateLicense();
};
