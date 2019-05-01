// RuleSet.h : Declaration of the CRuleSet

#pragma once

#include "resource.h"       // main symbols
#include "RuleSetEditor.h"
#include "IdentifiableObject.h"
#include "CounterInfo.h"

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
	public IDispatchImpl<IRunMode, &IID_IRunMode, &LIBID_UCLID_AFCORELib>,
	public CIdentifiableObject,
	public IDispatchImpl<IHasOCRParameters, &IID_IHasOCRParameters, &LIBID_UCLID_RASTERANDOCRMGMTLib>,
	public IDispatchImpl<ILoadOCRParameters, &IID_ILoadOCRParameters, &LIBID_UCLID_RASTERANDOCRMGMTLib>
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
	COM_INTERFACE_ENTRY(IRunMode)
	COM_INTERFACE_ENTRY(IHasOCRParameters)
	COM_INTERFACE_ENTRY(ILoadOCRParameters)
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
	STDMETHOD(RunAttributeFinder)(IAFDocument* pAFDoc, BSTR bstrAttributeName,
		BSTR bstrAlternateComponentDataDir,
		IIUnknownVector** pAttributes);
	STDMETHOD(Cleanup)();	
	STDMETHOD(SaveTo)(BSTR strFullFileName, VARIANT_BOOL bClearDirty, VARIANT_BOOL* pbGUIDsRegenerated);
	STDMETHOD(LoadFrom)(BSTR strFullFileName, VARIANT_BOOL bSetDirtyFlagToTrue);
	STDMETHOD(get_AttributeNameToInfoMap)(IStrToObjectMap * *pVal);
	STDMETHOD(put_AttributeNameToInfoMap)(IStrToObjectMap * newVal);
	STDMETHOD(get_DefinedAttributeNames)(IVariantVector **pVal);
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
	STDMETHOD(get_UseDocsIndexingCounter)(VARIANT_BOOL *pVal);
	STDMETHOD(put_UseDocsIndexingCounter)(VARIANT_BOOL newVal);
	STDMETHOD(get_ForInternalUseOnly)(VARIANT_BOOL *pVal);
	STDMETHOD(put_ForInternalUseOnly)(VARIANT_BOOL newVal);
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
	STDMETHOD(get_UsePagesIndexingCounter)(VARIANT_BOOL *pVal);
	STDMETHOD(put_UsePagesIndexingCounter)(VARIANT_BOOL newVal);
	STDMETHOD(get_CustomCounters)(IIUnknownVector **pVal);
	STDMETHOD(put_CustomCounters)(IIUnknownVector *pNewVal);
	STDMETHOD(FlushCounters)();

	// IRunMode
	STDMETHOD(get_RunMode)(ERuleSetRunMode *pRunMode);
	STDMETHOD(put_RunMode)(ERuleSetRunMode runMode);
	STDMETHOD(get_InsertAttributesUnderParent)(VARIANT_BOOL *pbInsertAttributesUnderParent);
	STDMETHOD(put_InsertAttributesUnderParent)(VARIANT_BOOL bInsertAttributesUnderParent);
	STDMETHOD(get_InsertParentName)(BSTR* pInsertParentName);
	STDMETHOD(put_InsertParentName)(BSTR InsertParentName);
	STDMETHOD(get_InsertParentValue)(BSTR* pInsertParentValue);
	STDMETHOD(put_InsertParentValue)(BSTR InsertParentValue);
	STDMETHOD(get_DeepCopyInput)(VARIANT_BOOL *pbDeepCopyInput);
	STDMETHOD(put_DeepCopyInput)(VARIANT_BOOL bDeepCopyInput);

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

// IHasOCRParameters
	STDMETHOD(get_OCRParameters)(IOCRParameters** ppOCRParameters);
	STDMETHOD(put_OCRParameters)(IOCRParameters* pOCRParameters);

// ILoadOCRParameters
	STDMETHOD(raw_LoadOCRParameters)(BSTR strRuleSetName);

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

	// Used to protect the data in CounterData instances.
	static CCriticalSection ms_criticalSectionCounterData;

	// Used to synchronize construction/destruction of rulesets (for threadsafe checks against
	// ms_referenceCount)
	static CCriticalSection ms_criticalSectionConstruction;

	// A vector of RuleExecutionCounters to be decremented when rules are run. If specified, when
	// running a ruleset each counter shall be tried in order until one with the proper counterID
	// is found with enough counts available. If there is no available counter with enough counts
	// (or no RuleExecutionCounters are provided in the first place), a USB key shall be decremented
	// if one is available.
	IIUnknownVectorPtr m_ipRuleExecutionCounters;

	// This critical section is used to protect the same instance between threads
	CCriticalSection m_criticalSection;

	// Counter Flags
	bool m_bUseDocsIndexingCounter;
	bool m_bUsePaginationCounter;
	bool m_bUsePagesRedactionCounter;
	bool m_bUseDocsRedactionCounter;
	bool m_bUsePagesIndexingCounter;

	// Keeps track of non-standard counters that have been specified in the counter grid.
	IIUnknownVectorPtr m_ipCustomCounters;

	// A working copy of all counter data (both standard and custom). Populated via the above
	// counter flags in combination with m_ipCustomCounters.
	unique_ptr<map<long, CounterInfo>> m_apmapCounters;

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

	// Keep track of the number of Rulesets in existence. Before the last closes, flush any
	// accumulated counts.
	static int ms_referenceCount;
	
	// RuleSet run mode
	ERuleSetRunMode m_eRuleSetRunMode;

	// Flag to indicate if attributes should be put under a parent node
	bool m_bInsertAttributesUnderParent;
	
	// Name of the parent node to put the attributes under if m_bInsertAttributesUnderParent is true
	string m_strInsertParentName;

	// Value for the parent node if m_bInsertAttributesUnderParent is true
	string m_strInsertParentValue;

	// Flag to indicate that the attributes should be cloned when m_eRuleSetRunMode == kPassInputVOAToOutput
	bool m_bDeepCopyInput;

	UCLID_AFCORELib::IRuleExecutionEnvPtr m_ipRuleExecutionEnv;

	// Parallel ruleset class used to run attribute finders in parallel per page
	UCLID_AFCORELib::IParallelRuleSetPtr m_ipParallelRuleSet;
	
	// Struct to store progress data counts
	struct ProgressDataItems
	{
		long nInitialization;
		long nPreprocessor;
		long nPerAttribute;
		long nTotalAttribute;
		long nOutputHandler;
		long nTotal;
	} m_sProgressCounts;

	IOCRParametersPtr m_ipOCRParameters;

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

	// Gets a map of counter IDs to CounterInfo instances describing the counters. If
	// m_ipRuleExecutionCounters has been specified, these are assigned to the appropriate
	// CounterInfo instances to be accessible for decrementing.
	map<long, CounterInfo>& getCounterInfo();

	// Method decrements counters if any are set 
	void decrementCounters(UCLID_AFCORELib::IAFDocumentPtr ipAFDoc);

	// Decrements the specified counter by the specified amount. This method includes code that
	// can allow a small number of counts to accumulate before deducting them from the key to limit
	// the extent to which counter decrements can be a bottleneck. If bAllowAccumulation = false
	// no such accumulation is allowed (and any counts already accumulated will be decremented).
	void decrementCounter(CounterInfo& counter, int nNumToDecrement, bool bAllowAccumulation,
		UCLID_AFCORELib::IAFDocumentPtr ipAFDoc);

	// Decrements from the specified counter any accumulated counts right away.
	void flushCounters();

	// Returns true if counter decrements should be skipped. This implements machine-level locking
	// and is managed via license file.
	bool countersDisabled();

	// Returns true if a counter is checked OR if internal-use flag is checked OR 
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
	// 2) At least one counter is enabled
	// 3) This is an internal ruleset
	bool isLicensedToSave();

	// Creates an attribute with the given strName, value of ipValue and subattributes ipAttributes
	UCLID_AFCORELib::IAttributePtr createParentAttribute(string strName, ISpatialStringPtr ipValue, 
		IIUnknownVectorPtr ipAttributes);

	// Returns a IIUnknownVector of AFDocuments to process 
	// if the Run mode is kPassInputVOAToOutput returns an empty vector
	IIUnknownVectorPtr setupRunMode(UCLID_AFCORELib::IAFDocumentPtr ipAFDoc, IIUnknownVectorPtr ipPages);

	// if the run mode is kPassInputVOAToOutput will return attributes from AFDoc that
	// is passed in modified based on the run mode flags
	// otherwise will return an empty vector
	IIUnknownVectorPtr passVOAToOutput(UCLID_AFCORELib::IAFDocumentPtr ipAFDoc);
	
	// Create parent value using the attributes on the AFDocument and the settings
	ISpatialStringPtr createParentValueFromAFDocAttributes(UCLID_AFCORELib::IAFDocumentPtr ipAFDoc, string pageString);

	// Recursively adds the ruleset name and attribute name to an attribute and its subattributes
	void addAttributeHistoryInfo(UCLID_AFCORELib::IAttributePtr ipAttribute, string strAttributeName);

	// Checks the rule execution environment to see whether attribute history information should be added to attributes
	bool shouldAddAttributeHistory();

	// Checks the rule execution environment to see whether parallel processing is enabled (registry setting)
	bool isParallelProcessingEnabled();

	// Runs the document preprocessor, if there is one
	void runGlobalDocPreprocessor(UCLID_AFCORELib::IAFDocumentPtr ipAFDoc,
		IProgressStatusPtr ipProgressStatus);

	// Runs a single attribute finder (collection of rules)
	IIUnknownVectorPtr runAttributeFinder(UCLID_AFCORELib::IAFDocumentPtr ipAFDoc, BSTR bstrAttributeName);

	// Runs the named attribute finders
	IIUnknownVectorPtr runAttributeFinders(UCLID_AFCORELib::IAFDocumentPtr ipAFDoc, IVariantVectorPtr ipAttributeName,
		IProgressStatusPtr ipProgressStatus);

	// Runs the output handler, if there is one
	void runOutputHandler(UCLID_AFCORELib::IAFDocumentPtr ipAFDoc, IIUnknownVectorPtr ipAttributes,
		IProgressStatusPtr ipProgressStatus);

	// Initializes m_sProgressCounts. Number of total counts is adjusted for number of attributes, run-mode and number of documents
	// (number of documents can be > 1 if run mode is per-page)
	void calculateProgressItems(long nNumAttributesToRun, long nNumDocs);

	// Checks version and internal-use flags, etc
	void validateRunability();

	IOCRParametersPtr getOCRParameters();
};
