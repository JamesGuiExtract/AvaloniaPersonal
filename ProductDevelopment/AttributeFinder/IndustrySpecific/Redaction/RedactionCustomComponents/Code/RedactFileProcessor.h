// RedactFileProcessor.h : Declaration of the CRedactFileProcessor

#pragma once

#include "resource.h"       // main symbols
#include "RedactionAppearanceDlg.h"

#include <FPCategories.h>
#include <CachedObjectFromFile.h>
#include "..\..\..\..\AFCore\Code\RuleSetLoader.h"

#include <set>

/////////////////////////////////////////////////////////////////////////////
// CRedactFileProcessor
class ATL_NO_VTABLE CRedactFileProcessor : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CRedactFileProcessor, &CLSID_RedactFileProcessor>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDispatchImpl<IRedactFileProcessor, &IID_IRedactFileProcessor, &LIBID_UCLID_AFUTILSLib>,
	public IDispatchImpl<IFileProcessingTask, &IID_IFileProcessingTask, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<CRedactFileProcessor>
{
public:
	CRedactFileProcessor();
	~CRedactFileProcessor();

DECLARE_REGISTRY_RESOURCEID(IDR_REDACTFILEPROCESSOR)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CRedactFileProcessor)
	COM_INTERFACE_ENTRY(IRedactFileProcessor)
	COM_INTERFACE_ENTRY(IFileProcessingTask)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY2(IDispatch, IRedactFileProcessor)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CRedactFileProcessor)
	PROP_PAGE(CLSID_RedactFileProcessorPP)
END_PROP_MAP()


BEGIN_CATEGORY_MAP(CRedactFileProcessor)
	IMPLEMENTED_CATEGORY(CATID_FP_FILE_PROCESSORS)
END_CATEGORY_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IRedactFileProcessor
public:
	STDMETHOD(get_VOAFileName)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_VOAFileName)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_UseVOA)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_UseVOA)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_AttributeNames)(/*[out, retval]*/ IVariantVector * *pVal);
	STDMETHOD(put_AttributeNames)(/*[in]*/ IVariantVector * newVal);
	STDMETHOD(get_ReadFromUSS)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_ReadFromUSS)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_OutputFileName)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_OutputFileName)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_RuleFileName)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_RuleFileName)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_CreateOutputFile)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_CreateOutputFile)(/*[in]*/ long newVal);
	STDMETHOD(get_CarryForwardAnnotations)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_CarryForwardAnnotations)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_ApplyRedactionsAsAnnotations)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_ApplyRedactionsAsAnnotations)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_UseRedactedImage)(VARIANT_BOOL* pvbUseRedactedImage);
	STDMETHOD(put_UseRedactedImage)(VARIANT_BOOL vbUseRedactedImage);
	STDMETHOD(get_RedactionText)(BSTR *pbstrRedactionText);
	STDMETHOD(put_RedactionText)(BSTR bstrRedactionText);
	STDMETHOD(get_BorderColor)(long *plBorderColor);
	STDMETHOD(put_BorderColor)(long lBorderColor);
	STDMETHOD(get_FillColor)(long *plFillColor);
	STDMETHOD(put_FillColor)(long lFillColor);
	STDMETHOD(get_FontName)(BSTR *pbstrFontName);
	STDMETHOD(put_FontName)(BSTR bstrFontName);
	STDMETHOD(get_IsBold)(VARIANT_BOOL *pvbBold);
	STDMETHOD(put_IsBold)(VARIANT_BOOL vbBold);
	STDMETHOD(get_IsItalic)(VARIANT_BOOL *pvbItalic);
	STDMETHOD(put_IsItalic)(VARIANT_BOOL vbItalic);
	STDMETHOD(get_FontSize)(long *plFontSize);
	STDMETHOD(put_FontSize)(long lFontSize);

// IFileProcessingTask
	STDMETHOD(raw_Init)();
	STDMETHOD(raw_ProcessFile)(BSTR bstrFileFullName, IFAMTagManager *pTagManager, 
		IFileProcessingDB *pDB, IProgressStatus *pProgressStatus, VARIANT_BOOL bCancelRequested,
		VARIANT_BOOL *pbSuccessfulCompletion);
	STDMETHOD(raw_Cancel)();
	STDMETHOD(raw_Close)();

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbValue);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

private:
	//////////////
	// Variables
	//////////////

	// Rules File Name
	std::string m_strRuleFileName;
	// Output File Name: can contain Tags
	std::string m_strOutputFileName;
	// Flag to indicate if a USS file should be used instead of and image file if the USS file exists
	bool m_bReadFromUSS;
	// Flag to indicate if output file should be created
	//   0 : Always create output file
	//   1 : Create output file only if redactable data was found
	long m_lCreateIfRedact;

	// Pointer to AFUtility
	UCLID_AFUTILSLib::IAFUtilityPtr	m_ipAFUtility;

	// Contains the currently loaded Rules file
	CachedObjectFromFile<IRuleSetPtr, RuleSetLoader> m_ipRuleSet;

	// Contains the names of the attributes to find
	IVariantVectorPtr m_ipAttributeNames;

	// Set contains the same names as m_ipAttributeNames
	std::set<std::string> m_setAttributeNames;

	bool m_bDirty;

	// Flag to indicate use of voa file instead of rule results
	bool m_bUseVOA;

	std::string m_strVOAFileName;

	// Flags to indicate use of annotations
	bool m_bCarryForwardAnnotations;
	bool m_bApplyRedactionsAsAnnotations;

	// Pointer to the IDShield database manager
	UCLID_REDACTIONCUSTOMCOMPONENTSLib::IIDShieldProductDBMgrPtr m_ipIDShieldDB;

	// Whether to use the previously redacted image (true) or the original image (false)
	bool m_bUseRedactedImage;

	// Redaction text and color settings
	RedactionAppearanceOptions m_redactionAppearance;

	//////////////
	// Methods
	//////////////

	// Sets properties to their default state
	void clear();

	// Returns m_ipAFUtility, after initializing it if necessary
	UCLID_AFUTILSLib::IAFUtilityPtr getAFUtility();

	// Returns m_ipRuleSet, after initializing it if necessary
	// The ruleset will also be loaded using the m_strRuleFileName if it hasn't been already
	IRuleSetPtr getRuleSet(IFAMTagManagerPtr ipFAMTagManager, const string& strInput);

	// Puts the attribute names in the set
	void fillAttributeSet(IVariantVectorPtr ipAttributeNames, set<string>& rsetAttributeNames);

	// Allocates m_ipIDShieldDB pointer if it does not exist and returns m_ipIDShieldDB
	UCLID_REDACTIONCUSTOMCOMPONENTSLib::IIDShieldProductDBMgrPtr getIDShieldDBPtr();

	void validateLicense();
};
