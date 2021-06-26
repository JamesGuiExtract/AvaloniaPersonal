// CategoryManager.h : Declaration of the CCategoryManager

#pragma once

#include "resource.h"       // main symbols

#include <string>

/////////////////////////////////////////////////////////////////////////////
// CCategoryManager
class ATL_NO_VTABLE CCategoryManager : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CCategoryManager, &CLSID_CategoryManager>,
	public ISupportErrorInfo,
	public IDispatchImpl<ICategoryManager, &IID_ICategoryManager, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CCategoryManager();

DECLARE_REGISTRY_RESOURCEID(IDR_CATEGORYMANAGER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CCategoryManager)
	COM_INTERFACE_ENTRY(ICategoryManager)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, ICategoryManager)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ICategoryManager
	STDMETHOD(GetCategoryNames)(/*[in]*/ BSTR strPrefix, /*[out, retval]*/ IVariantVector **pCategoryNames);
	STDMETHOD(GetDescriptionToProgIDMap1)(/*[in]*/ BSTR strCategoryName, 
		/*[out, retval]*/ IStrToStrMap **pMap);
	STDMETHOD(GetDescriptionToProgIDMap2)(/*[in]*/ BSTR strCategoryName, 
		/*[in]*/ long nNumIIDs, /*[in, size_is(nNumIIDs)]*/ IID pIIDs[],
		/*[out, retval]*/ IStrToStrMap **pMap);
	STDMETHOD(DeleteCache)(/*[in]*/ BSTR strCategoryName);
	STDMETHOD(CheckForNewComponents)(IVariantVector *pCategoryNames);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	std::string m_strCacheFileRoot;
	CMutex m_mutex;
	void verifyCategoryName(const std::string& strCategoryName);
	void verifyComponentDescription(const std::string& strDescription);

	void validateLicense();
	std::string getCacheFileName(const std::string& strCategoryName);
	std::string getCacheFileRoot();
	void createCacheFile(BSTR bstrCategoryName,
		const std::string& strFileName);

	bool componentImplementsRequiredInteraces(const std::string& strProgID,
		long nNumIIDs, IID pIIDs[]);

	UCLID_COMUTILSLib::ICategoryManagerPtr getThisAsCOMPtr();
};
