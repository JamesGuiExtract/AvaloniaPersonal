// RuleSet.h : Declaration of the CRuleSet

#pragma once

#include "resource.h"       // main symbols
#include "RuleSetEditor.h"
#include "SafeNetLicenseMgr.h"
#include "IdentifiableObject.h"

#include <afxmt.h>

#include <memory>
#include <string>
#include <vector>

using namespace std;

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
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IIdentifiableObject, &IID_IIdentifiableObject, &LIBID_UCLID_COMUTILSLib>,
	public CIdentifiableObject
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
	COM_INTERFACE_ENTRY(IIdentifiableObject)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IRuleSetUI
	STDMETHOD(ShowUIForEditing)(BSTR strFileName, BSTR strBinFolder);

// IRuleSet
	STDMETHOD(ExecuteRulesOnText)(IAFDocument* pAFDoc, IVariantVector *pvecAttributeNames,
		BSTR bstrAlternateComponentDataDir, IProgressStatus *pProgressStatus,
		IIUnknownVector** pAttributes);
	STDMETHOD(Cleanup)();	
	STDMETHOD(SaveTo)(BSTR strFullFileName, VARIANT_BOOL bClearDirty, VARIANT_BOOL* pbGUIDsRegenerated);
	STDMETHOD(LoadFrom)(BSTR strFullFileName, VARIANT_BOOL bSetDirtyFlagToTrue);
	STDMETHOD(get_AttributeNameToInfoMap)(IStrToObjectMap * *pVal);
	STDMETHOD(put_AttributeNameToInfoMap)(IStrToObjectMap * newVal);
	STDMETHOD(get_GlobalDocPreprocessor)(IObjectWithDescription **pVal);
	STDMETHOD(put_GlobalDocPreprocessor)(IObjectWithDescription *newVal);
	STDMETHOD(get_FileName)(BSTR *pVal);
	STDMETHOD(put_FileName)(BSTR newVal);
	STDMETHOD(get_IsEncrypted)(VARIANT_BOOL *pVal);
	STDMETHOD(get_GlobalOutputHandler)(IObjectWithDescription **pVal);
	STDMETHOD(put_GlobalOutputHandler)(IObjectWithDescription *newVal);
	STDMETHOD(get_UsePaginationCounter)(VARIANT_BOOL *pVal);
	STDMETHOD(put_UsePaginationCounter)(VARIANT_BOOL newVal);
	STDMETHOD(get_UsePagesRedactionCounter)(VARIANT_BOOL *pVal);
	STDMETHOD(put_UsePagesRedactionCounter)(VARIANT_BOOL newVal);
	STDMETHOD(get_UseDocsRedactionCounter)(VARIANT_BOOL *pVal);
	STDMETHOD(put_UseDocsRedactionCounter)(VARIANT_BOOL newVal);
	STDMETHOD(get_UseIndexingCounter)(VARIANT_BOOL *pVal);
	STDMETHOD(put_UseIndexingCounter)(VARIANT_BOOL newVal);
	STDMETHOD(get_ForInternalUseOnly)(VARIANT_BOOL *pVal);
	STDMETHOD(put_ForInternalUseOnly)(VARIANT_BOOL newVal);
	STDMETHOD(get_KeySerialList)(BSTR  *pVal);
	STDMETHOD(put_KeySerialList)(BSTR newVal);
	STDMETHOD(get_VersionNumber)(long *nVersion);
	STDMETHOD(get_IsSwipingRule)(VARIANT_BOOL *pVal);
	STDMETHOD(put_IsSwipingRule)(VARIANT_BOOL newVal);
	STDMETHOD(get_CanSave)(VARIANT_BOOL *pVal);
	STDMETHOD(get_FKBVersion)(BSTR *pVal);
	STDMETHOD(put_FKBVersion)(BSTR newVal);
	STDMETHOD(get_IgnorePreprocessorErrors)(VARIANT_BOOL *pVal);
	STDMETHOD(put_IgnorePreprocessorErrors)(VARIANT_BOOL newVal);
	STDMETHOD(get_IgnoreOutputHandlerErrors)(VARIANT_BOOL *pVal);
	STDMETHOD(put_IgnoreOutputHandlerErrors)(VARIANT_BOOL newVal);
	STDMETHOD(get_Comments)(BSTR *pVal);
	STDMETHOD(put_Comments)(BSTR newVal);
	STDMETHOD(get_RuleExecutionCounters)(IIUnknownVector **pVal);
	STDMETHOD(put_RuleExecutionCounters)(IIUnknownVector *pNewVal);

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

// IIdentifiableObject
	STDMETHOD(get_InstanceGUID)(GUID *pVal);

private:
	//////////////////////
	// Member variables
	//////////////////////
	IMiscUtilsPtr m_ipMiscUtils;
	
	IStrToObjectMapPtr m_ipAttributeNameToInfoMap;

	unique_ptr<CRuleSetEditor> m_apDlg;

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
	string m_strFileName;
	
	// The filename this ruleset was last saved as. Used to determine if the IdentifiableRuleGUIDs
	// need to be updated when saving.
	string m_strPreviousFileName;

	// this mutex is used in the constructor and destructor to protect the m_apSafeNetMgr member
	static CMutex ms_mutexLM;

	// Used to synchronize construction/destruction of rulesets (for threadsafe checks against
	// ms_referenceCount)
	static CMutex ms_mutexConstruction;

	// The should be only one SafeNetLicenseMgr object
	// protected by ms_mutexLM
	static unique_ptr<SafeNetLicenseMgr> m_apSafeNetMgr;
	
	// This is need to keep track of the number of RuleSet instances that are active so it will
	// be possible to know when the m_apSafeNetMgr object can safely be deleted 
	// protected by ms_mutexLM
	static int m_iSNMRefCount;

	// A vector of RuleExecutionCounters to be decremented when rules are run. If specified, when
	// running a ruleset each counter shall be tried in order until one with the proper counterID
	// is found with enough counts available. If there is no available counter with enough counts
	// (or no RuleExecutionCounters are provided in the first place), a USB key shall be decremented
	// if one is available.
	IIUnknownVectorPtr m_ipRuleExecutionCounters;

	// This mutex is used to protect the same instance between threads
	CMutex m_mutex;

	// Counter Flags
	bool m_bUseIndexingCounter;
	bool m_bUsePaginationCounter;
	bool m_bUsePagesRedactionCounter;
	bool m_bUseDocsRedactionCounter;

	string m_strKeySerialNumbers;
	vector<DWORD> m_vecSerialNumbers;

	// a flag to indicate whether this ruleset should only be used
	// internally (i.e. from with other rule objects that UCLID delivers to customers)
	bool m_bRuleSetOnlyForInternalUse;

	// Indicates whether this rule is a swiping rule
	bool m_bSwipingRule;

	// The FKB version to use for this ruleset.
	string m_strFKBVersion;

	// version number of the ruleset
	long m_nVersionNumber;

	bool m_bIgnorePreprocessorErrors;
	bool m_bIgnoreOutputHandlerErrors;

	// Comments for the ruleset.
	string m_strComments;

	// Used to track data for each counter in order to determine the number of counts that may be
	// accumulated prior to deducting them from the USB key. [LegacyRCAndUtils:6170]
	struct CounterData
	{
		int m_nCountsDecrementedInProcess;
		int m_nLastCountValue;
		int m_nCountDecrementAccumulation;
	};

	static CounterData ms_indexingCounterData;
	static CounterData ms_paginationCounterData;
	static CounterData ms_redactionCounterData;

	// Keep track of the number of Rulesets in existence. Before the last closes, flush any
	// accumulated USB counts.
	static int ms_referenceCount;

	/////////////////
	// Helper functions
	/////////////////
	//----------------------------------------------------------------------------------------------
	UCLID_AFCORELib::IRuleSetPtr getThisAsCOMPtr();
	//----------------------------------------------------------------------------------------------
	//	PURPOSE: to return m_ipDocPreprocessor
	//
	//	PROMISE: to create an instance of DocumentPreprocessor and assign it to m_ipDocPreprocessor 
	//			if m_ipDocPreprocessor is __nullptr
	IObjectWithDescriptionPtr getDocPreprocessor();
	//----------------------------------------------------------------------------------------------
	//	PURPOSE: to return m_ipOutputHandler
	//
	//	PROMISE: to create an instance of OutputHandler and assign it to m_ipOutputHandler 
	//			 if m_ipOutputHandler is __nullptr
	IObjectWithDescriptionPtr getOutputHandler();
	//----------------------------------------------------------------------------------------------
	// Validates Rule Set license
	void validateLicense();

	// Validates Rule Set Editor license
	void validateUILicense();

	// create the MiscUtils object if necessary and return it
	IMiscUtilsPtr getMiscUtils();

	// Method decrements counters if any are set 
	void decrementCounters( ISpatialStringPtr ipText );

	// Decrements the specified DataCell by the specified amount. This method includes code that
	// can allow a small number of counts to accumulate before deducting them from the key to limit
	// the extent to which a USB key can be a bottleneck.
	void decrementCounter(DataCell cell, int nNumToDecrement, CounterData& counterData);

	// Decrements from the USB key any accumulated counts right away.
	void flushCounter(DataCell cell, CounterData& counterData);

	// Attempts to decrement using m_ipRuleExecutionCounters. Note that DataCell is used only to
	// indicate the proper CounterID to decrement while disturbing the existing code as little as
	// possible. In the future, the should be factored out and a counter ID should be used in its
	// place.
	// Returns the number of counts remaining. If there is not a counter available or not enough
	// counts on an available counter, -1 will be returned;
	int decrementRuleExecutionCounter(DataCell cell, int nNumToDecrement);

	// returns a reference of the member variable m_vecSerialNumbers
	// this function separates the serial numbers in the string m_strKeySerialNumbers
	vector<DWORD>& getSerialListAsDWORDS(); 

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

	// true if the Rule Development Toolkit is licensed; otherwise false.
	bool isRdtLicensed();

	// true if any counter is being used, false if all counters are not being used.
	bool isUsingCounter();

	// true if any of the following are true:
	// 1) The Rule Development Toolkit is licensed
	// 2) At least one usb counter is enabled
	// 3) This is an internal ruleset
	bool isLicensedToSave();
};
