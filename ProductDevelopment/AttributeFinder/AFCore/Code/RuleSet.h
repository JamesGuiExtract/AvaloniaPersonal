// RuleSet.h : Declaration of the CRuleSet

#pragma once

#include "resource.h"       // main symbols
#include "RuleSetEditor.h"
#include "SafeNetLicenseMgr.h"
#include <afxmt.h>

#include <vector>
#include <string>
#include <memory>

/////////////////////////////////////////////////////////////////////////////
// CRuleSet
class ATL_NO_VTABLE CRuleSet : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CRuleSet, &CLSID_RuleSet>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDispatchImpl<IRuleSet, &IID_IRuleSet, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IRuleSetUI, &IID_IRuleSetUI, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>
{
public:
	CRuleSet();
	~CRuleSet();

DECLARE_REGISTRY_RESOURCEID(IDR_RULESET)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CRuleSet)
	COM_INTERFACE_ENTRY(IRuleSet)
	COM_INTERFACE_ENTRY2(IDispatch, IRuleSet)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(IRuleSetUI)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IRuleSetUI
	STDMETHOD(ShowUIForEditing)(BSTR strFileName, BSTR strBinFolder);

// IRuleSet
	STDMETHOD(ExecuteRulesOnText)(/*[in]*/ IAFDocument* pAFDoc, 
		/*[in]*/ IVariantVector *pvecAttributeNames,
		/*[in]*/ IProgressStatus *pProgressStatus,
		/*[out, retval]*/ IIUnknownVector** pAttributes);
	STDMETHOD(SaveTo)(/*[in]*/ BSTR strFullFileName, VARIANT_BOOL bClearDirty);
	STDMETHOD(LoadFrom)(/*[in]*/ BSTR strFullFileName, VARIANT_BOOL bSetDirtyFlagToTrue);
	STDMETHOD(get_AttributeNameToInfoMap)(/*[out, retval]*/ IStrToObjectMap * *pVal);
	STDMETHOD(put_AttributeNameToInfoMap)(/*[in]*/ IStrToObjectMap * newVal);
	STDMETHOD(get_GlobalDocPreprocessor)(/*[out, retval]*/ IObjectWithDescription **pVal);
	STDMETHOD(put_GlobalDocPreprocessor)(/*[in]*/ IObjectWithDescription *newVal);
	STDMETHOD(get_FileName)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_FileName)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_IsEncrypted)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(get_GlobalOutputHandler)(/*[out, retval]*/ IObjectWithDescription **pVal);
	STDMETHOD(put_GlobalOutputHandler)(/*[in]*/ IObjectWithDescription *newVal);
	STDMETHOD(get_UsePaginationCounter)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_UsePaginationCounter)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_UsePagesRedactionCounter)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_UsePagesRedactionCounter)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_UseDocsRedactionCounter)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_UseDocsRedactionCounter)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_UseIndexingCounter)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_UseIndexingCounter)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_ForInternalUseOnly)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_ForInternalUseOnly)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_KeySerialList)(/*[out, retval]*/ BSTR  *pVal);
	STDMETHOD(put_KeySerialList)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_VersionNumber)(/*[out, retval]*/ long *nVersion);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

private:
	//////////////////////
	// Member variables
	//////////////////////
	IMiscUtilsPtr m_ipMiscUtils;
	
	IStrToObjectMapPtr m_ipAttributeNameToInfoMap;

	std::auto_ptr<CRuleSetEditor> m_apDlg;

	// Global Document Preprocessor
	IObjectWithDescriptionPtr m_ipDocPreprocessor;

	// Global Output Handler
	IObjectWithDescriptionPtr m_ipOutputHandler;

	// the stream name used to store the ruleset within a storage object
	_bstr_t m_bstrStreamName;

	bool m_bDirty;

	// Indicates if the ruleset was read from an encrypted file
	bool m_bIsEncrypted;

	// the filename associated with this ruleset
	std::string m_strFileName;

	// this mutex is used in the constructor and destructor to protect the m_apSafeNetMgr member
	static CMutex ms_mutexLM;

	// The should be only one SafeNetLicenseMgr object
	// protected by ms_mutexLM
	static std::auto_ptr<SafeNetLicenseMgr> m_apSafeNetMgr;
	
	// This is need to to keep track of the number of RuleSet intances that are active so it will
	// be possible to know when the m_apSafeNetMgr object can safely be deleted 
	// protected by ms_mutexLM
	static int m_iSNMRefCount;

	// This mutex is used to protect the same instance between threads
	CMutex m_mutex;

	// Counter Flags
	bool m_bUseIndexingCounter;
	bool m_bUsePaginationCounter;
	bool m_bUsePagesRedactionCounter;
	bool m_bUseDocsRedactionCounter;

	std::string m_strKeySerialNumbers;
	std::vector<DWORD> m_vecSerialNumbers;

	// a flag to indicate whether this ruleset should only be used
	// internally (i.e. from with other rule objects that UCLID delivers to customers)
	bool m_bRuleSetOnlyForInternalUse;

	// version number of the ruleset
	long m_nVersionNumber;

	/////////////////
	// Helper functions
	/////////////////
	//----------------------------------------------------------------------------------------------
	UCLID_AFCORELib::IRuleSetPtr getThisAsCOMPtr();
	//----------------------------------------------------------------------------------------------
	//	PURPOSE: to return m_ipDocPreprocessor
	//
	//	PROMISE: to create an instance of DocumentPreprocessor and assign it to m_ipDocPreprocessor 
	//			if m_ipDocPreprocessor is NULL
	IObjectWithDescriptionPtr getDocPreprocessor();
	//----------------------------------------------------------------------------------------------
	//	PURPOSE: to return m_ipOutputHandler
	//
	//	PROMISE: to create an instance of OutputHandler and assign it to m_ipOutputHandler 
	//			 if m_ipOutputHandler is NULL
	IObjectWithDescriptionPtr getOutputHandler();
	//----------------------------------------------------------------------------------------------
	// Validates Rule Set license
	void validateLicense();

	// Validates Rule Set Editor license
	void validateUILicense();

	// Validates selection of a USB Counter unless full RDT license
	void validateUSBCounter();

	// create the MiscUtils object if necessary and return it
	IMiscUtilsPtr getMiscUtils();

	// Method decrements counters if any are set 
	void decrementCounters( ISpatialStringPtr ipText );

	// returns a reference of the member variable m_vecSerialNumbers
	// this function separates the serial numbers in the string m_strKeySerialNumbers
	std::vector<DWORD>& getSerialListAsDWORDS(); 

	// Returns true if consideration of USB Key counters is to be ignored.
	// This implements machine-level locking and is managed via license file.
	bool usbCountersDisabled();

	// Returns true if consideration of serial number of USB Key is to be ignored.
	// This is managed via license file.
	bool usbSerialNumbersDisabled();

	// Checks the serial number against the list of serial numbers
	// if no serial numbers in the list assume any will work
	// if the serial number of the key is not in the list then an
	// exception is thrown
	void validateKeySerialNumber();

	// Returns true if a USB counter is checked OR if internal-use flag is checked OR 
	// if a full RDT license is available.  Otherwise returns false.
	bool isRuleExecutionAllowed();

	// This method will return true if and only if a document pre-processor is defined
	// for this RuleSet, and the 'run document pre-processor' checkbox is enabled.
	bool enabledDocumentPreprocessorExists();
	
	// This method will return true if and only if an output handler is defined
	// for this RuleSet, and the 'run output handler' checkbox is enabled.
	bool enabledOutputHandlerExists();
};
