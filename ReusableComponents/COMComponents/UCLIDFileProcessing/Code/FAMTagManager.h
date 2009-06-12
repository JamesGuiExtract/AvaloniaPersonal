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

// IFAMTagManager
	STDMETHOD(get_FPSFileDir)(BSTR *strFPSDir);
	STDMETHOD(put_FPSFileDir)(BSTR strFPSDir);
	STDMETHOD(ExpandTags)(BSTR bstrInput, BSTR bstrSourceName, BSTR *pbstrOutput);
	STDMETHOD(GetBuiltInTags)(IVariantVector* *ppTags);
	STDMETHOD(GetINIFileTags)(IVariantVector* *ppTags);
	STDMETHOD(GetAllTags)(IVariantVector* *ppTags);
	STDMETHOD(StringContainsInvalidTags)(BSTR strInput, VARIANT_BOOL *pbValue);
	STDMETHOD(StringContainsTags)(BSTR strInput, VARIANT_BOOL *pbValue);

private:

	///////////
	//Variables
	//////////

	std::string m_strFPSDir;

	//////////
	//Methods
	/////////

	//---------------------------------------------------------------------------------------------
	// PROMISE: To return all tag names in strInput.
	//			The returned strings will include the < and > chars
	void getTagNames(const std::string& strInput, 
		std::vector<std::string>& rvecTagNames) const;

	void validateLicense();
};
