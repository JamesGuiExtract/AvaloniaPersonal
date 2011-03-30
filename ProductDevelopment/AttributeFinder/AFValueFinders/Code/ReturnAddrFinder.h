#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"
#include <string>
#include <RegistryPersistenceMgr.h>

/////////////////////////////////////////////////////////////////////////////
// CReturnAddrFinder
class ATL_NO_VTABLE CReturnAddrFinder : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CReturnAddrFinder, &CLSID_ReturnAddrFinder>,
	public ISupportErrorInfo,
	public IDispatchImpl<IReturnAddrFinder, &IID_IReturnAddrFinder, &LIBID_UCLID_AFVALUEFINDERSLib>,
	public IDispatchImpl<IAttributeFindingRule, &IID_IAttributeFindingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CReturnAddrFinder>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CReturnAddrFinder();
	~CReturnAddrFinder();

DECLARE_REGISTRY_RESOURCEID(IDR_RETURNADDRFINDER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CReturnAddrFinder)
	COM_INTERFACE_ENTRY(IReturnAddrFinder)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IReturnAddrFinder)
	COM_INTERFACE_ENTRY(IAttributeFindingRule)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_PROP_MAP(CReturnAddrFinder)
	PROP_PAGE(CLSID_ReturnAddrFinderPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CReturnAddrFinder)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_FINDERS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IReturnAddrFinder
	STDMETHOD(get_FindNonReturnAddresses)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_FindNonReturnAddresses)(/*[in]*/ VARIANT_BOOL newVal);

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
	////////////
	// Variables
	////////////
	// flag to keep track of whether this object has been modified
	// since the last save-to-stream operation
	bool m_bDirty;

	// UCLID_AFCORELib::IAttributeFindInfoPtr m_ipFindInfo;
	UCLID_AFCORELib::IRuleSetPtr m_ipImportedRuleSet;
	
	// Used for AutoEncryption and Expanding Tags
	IAFUtilityPtr	m_ipAFUtility;
	IMiscUtilsPtr	m_ipMiscUtils;

	// when this falg is set, if no return addresses are found the finder will search for 
	// other addresses as a backup 
	bool m_bFindNonReturnAddresses;

	// used for checking registry entries
	std::unique_ptr<IConfigurationSettingsPersistenceMgr> ma_pUserCfgMgr;

	///////////
	// Private Methods
	///////////
	void validateLicense();

	// resets all member variables
	// do a default state
	void reset();

	// This function is used to check the registry backdoors to see which
	// parts of the return address finder should be run
	bool checkRegistryBool(std::string key, bool defaultValue);

	// autoencrypts and loads a regular expression
	std::string getRegExp(const char* strFileName, IAFDocumentPtr ipAFDoc);

	ISpatialStringPtr chooseBlock(IRegularExprParserPtr ipSuffixParser, IIUnknownVectorPtr ipBlocks);

	ISpatialStringPtr getAddressNearPrefix(ISpatialStringSearcherPtr ipSearcher, ISpatialStringPtr ipPrefix,
		IRegularExprParserPtr ipRegExpParser);
};
