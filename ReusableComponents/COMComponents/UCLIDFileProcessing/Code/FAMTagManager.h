// FAMTagManager.h : Declaration of the CFAMTagManager

#pragma once

#include "resource.h"       // main symbols
#include "UCLIDFileProcessing.h"
#include <string>
#include <vector>
#include <map>
#include <memory>
#include <StringCSIS.h>

class IConfigurationSettingsPersistenceMgr;
class MRUList;

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CFAMTagManager
class ATL_NO_VTABLE CFAMTagManager : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CFAMTagManager, &CLSID_FAMTagManager>,
	public ISupportErrorInfo,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ITagUtility, &IID_ITagUtility, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,	
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
	COM_INTERFACE_ENTRY(ICopyableObject)
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
	STDMETHOD(raw_ExpandTags)(BSTR bstrInput, BSTR bstrSourceDocName, IUnknown *pData, VARIANT_BOOL vbStopEarly, BSTR *pbstrOutput);
	STDMETHOD(raw_ExpandTagsAndFunctions)(BSTR bstrInput, BSTR bstrSourceDocName, IUnknown *pData, BSTR *pbstrOutput);
	STDMETHOD(raw_GetBuiltInTags)(IVariantVector* *ppTags);
	STDMETHOD(raw_GetCustomFileTags)(IVariantVector* *ppTags);
	STDMETHOD(raw_GetAllTags)(IVariantVector* *ppTags);
	STDMETHOD(raw_GetFunctionNames)(IVariantVector** ppFunctionNames);
	STDMETHOD(raw_GetFormattedFunctionNames)(IVariantVector** ppFunctionNames);
	STDMETHOD(raw_EditCustomTags)(long hParentWindow);
	STDMETHOD(raw_AddTag)(BSTR bstrTagName, BSTR bstrTagValue);
	STDMETHOD(raw_GetAddedTags)(IIUnknownVector **ppStringPairTags);
	STDMETHOD(raw_ExpandFunction)(BSTR bstrFunctionName, IVariantVector *pArgs,
		BSTR bstrSourceDocName, IUnknown *pData, BSTR *pbstrOutput);

// IFAMTagManager
	STDMETHOD(get_FPSFileDir)(BSTR *strFPSDir);
	STDMETHOD(put_FPSFileDir)(BSTR strFPSDir);
	STDMETHOD(get_FPSFileName)(BSTR *strFPSFileName);
	STDMETHOD(put_FPSFileName)(BSTR strFPSFileName);
	STDMETHOD(ExpandTags)(BSTR bstrInput, BSTR bstrSourceName, BSTR *pbstrOutput);
	STDMETHOD(ExpandTagsAndFunctions)(BSTR bstrInput, BSTR bstrSourceName, BSTR *pbstrOutput);
	STDMETHOD(StringContainsInvalidTags)(BSTR strInput, VARIANT_BOOL *pbValue);
	STDMETHOD(StringContainsTags)(BSTR strInput, VARIANT_BOOL *pbValue);
	STDMETHOD(get_AlwaysShowDatabaseTags)(VARIANT_BOOL *pbValue);
	STDMETHOD(put_AlwaysShowDatabaseTags)(VARIANT_BOOL bValue);
	STDMETHOD(ValidateConfiguration)(BSTR bstrDatabaseServer, BSTR bstrDatabaseName,
		BSTR* pbstrWarning);
	STDMETHOD(get_ActiveContext)(BSTR *strActiveContext);
	STDMETHOD(get_DatabaseServer)(BSTR *strDatabaseServer);
	STDMETHOD(put_DatabaseServer)(BSTR strDatabaseServer);
	STDMETHOD(get_DatabaseName)(BSTR *strDatabaseName);
	STDMETHOD(put_DatabaseName)(BSTR strDatabaseName);
	STDMETHOD(get_ActionName)(BSTR *strActionName);
	STDMETHOD(put_ActionName)(BSTR strActionName);
	STDMETHOD(RefreshContextTags)();
	STDMETHOD(get_Workflow)(BSTR *strWorkflow);
	STDMETHOD(put_Workflow)(BSTR strWorkflow);
	STDMETHOD(GetFAMTagManagerWithWorkflow)(BSTR bstrWorkflow, IFAMTagManager** ppFAMTagManager);
	STDMETHOD(get_FAMDB)(IFileProcessingDB** ppFAMDB);
	STDMETHOD(put_FAMDB)(IFileProcessingDB* pFAMDB);
	STDMETHOD(SetContextTagProvider)(IContextTagProvider* pContextTagProvider);	

	// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

private:

	///////////
	//Variables
	//////////

	// There should never be more than one FPSDir or FPSFileName per-process. By making these
	// static, values set in one instance can be shared with a later instance that otherwise would
	// not have access to the currently loaded FPS file.
	static string ms_strFPSDir;
	static string ms_strFPSFileName;

	// Evaluates environment-specific tags. Also static since the environment tags will always be
	// the same for a given context path.
	static IContextTagProviderPtr ms_ipContextTagProvider;

	// A cache of the tag values from ms_ipContextTagProvider.
	static map<stringCSIS, map<stringCSIS, stringCSIS>> ms_mapWorkflowContextTags;

	// Controls access to the above static variables.
	static CCriticalSection ms_criticalsection;

	// Mutex for accessing the MRU context list in the registry
	CMutex m_mutexMRU;

	// Indicates whether database path tags should be used to 
	bool m_bAlwaysShowDatabaseTags;

	// The current values to use for the database tags.
	string m_strDatabaseServer;
	string m_strDatabaseName;
	string m_strDatabaseAction;
	string m_strWorkflow;

	// Programmatically added path tags.
	map<string, string> m_mapAddedTags;
	
	// Critical section for accessing m_mapAddedTags
	CCriticalSection m_criticalSectionAddedTags;

	// pointer to the utility object to use for path function expansion.
	IMiscUtilsPtr m_ipMiscUtils;

	// IFileProcessingDB instance used to evaluate the $Metadata function.
	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr m_ipFAMDB;

	// Used to maintain MRU list for recent contexts
	unique_ptr<IConfigurationSettingsPersistenceMgr> m_upUserCfgMgr;
	unique_ptr<MRUList> m_upContextMRUList;

	//////////
	//Methods
	/////////

	//---------------------------------------------------------------------------------------------
	// PROMISE: To return all tag names in strInput.
	//			The returned strings will include the < and > chars
	void getTagNames(const std::string& strInput, 
		std::vector<std::string>& rvecTagNames) const;

	void expandTags(std::string &strInput, const std::string &strSourceDocName, bool stopEarly);

	//string expandAttributeFunction(const string& strFunction, const string& strSourceDocName,
	//	const string& strAttributeSetName, const string& strMetadataFieldName);
	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr getFAMDB();

	// Refreshes context tag info based on current FPSFileDir from ContextTags.sdf.
	void refreshContextTags();

	void validateLicense();
};
