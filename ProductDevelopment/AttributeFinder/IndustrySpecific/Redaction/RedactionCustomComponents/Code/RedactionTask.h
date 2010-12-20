// RedactionTask.h : Declaration of the CRedactionTask

#pragma once

#include "resource.h"
#include "RedactionAppearanceDlg.h"

#include <FPCategories.h>

#include <set>
#include <string>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CRedactionTask
class ATL_NO_VTABLE CRedactionTask : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CRedactionTask, &CLSID_RedactionTask>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDispatchImpl<IRedactionTask, &IID_IRedactionTask, &LIBID_UCLID_REDACTIONCUSTOMCOMPONENTSLib>,
	public IDispatchImpl<IFileProcessingTask, &IID_IFileProcessingTask, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<CRedactionTask>
{
public:
	CRedactionTask();
	virtual ~CRedactionTask();

	HRESULT FinalConstruct();
	void FinalRelease();

DECLARE_REGISTRY_RESOURCEID(IDR_REDACTIONTASK)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CRedactionTask)
	COM_INTERFACE_ENTRY(IRedactionTask)
	COM_INTERFACE_ENTRY(IFileProcessingTask)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY2(IDispatch, IRedactionTask)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CRedactionTask)
	PROP_PAGE(CLSID_RedactionTaskPP)
END_PROP_MAP()


BEGIN_CATEGORY_MAP(CRedactionTask)
	IMPLEMENTED_CATEGORY(CATID_FP_FILE_PROCESSORS)
END_CATEGORY_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IRedactionTask
public:
	STDMETHOD(get_VOAFileName)(BSTR* pVal);
	STDMETHOD(put_VOAFileName)(BSTR newVal);
	STDMETHOD(get_AttributeNames)(IVariantVector** ppVal);
	STDMETHOD(put_AttributeNames)(IVariantVector* pVal);
	STDMETHOD(get_OutputFileName)(BSTR* pVal);
	STDMETHOD(put_OutputFileName)(BSTR newVal);
	STDMETHOD(get_CarryForwardAnnotations)(VARIANT_BOOL* pVal);
	STDMETHOD(put_CarryForwardAnnotations)(VARIANT_BOOL newVal);
	STDMETHOD(get_ApplyRedactionsAsAnnotations)(VARIANT_BOOL* pVal);
	STDMETHOD(put_ApplyRedactionsAsAnnotations)(VARIANT_BOOL newVal);
	STDMETHOD(get_UseRedactedImage)(VARIANT_BOOL* pvbUseRedactedImage);
	STDMETHOD(put_UseRedactedImage)(VARIANT_BOOL vbUseRedactedImage);
	STDMETHOD(get_RedactionText)(BSTR* pbstrRedactionText);
	STDMETHOD(put_RedactionText)(BSTR bstrRedactionText);
	STDMETHOD(get_BorderColor)(long* plBorderColor);
	STDMETHOD(put_BorderColor)(long lBorderColor);
	STDMETHOD(get_FillColor)(long* plFillColor);
	STDMETHOD(put_FillColor)(long lFillColor);
	STDMETHOD(get_FontName)(BSTR* pbstrFontName);
	STDMETHOD(put_FontName)(BSTR bstrFontName);
	STDMETHOD(get_IsBold)(VARIANT_BOOL* pvbBold);
	STDMETHOD(put_IsBold)(VARIANT_BOOL vbBold);
	STDMETHOD(get_IsItalic)(VARIANT_BOOL* pvbItalic);
	STDMETHOD(put_IsItalic)(VARIANT_BOOL vbItalic);
	STDMETHOD(get_FontSize)(long* plFontSize);
	STDMETHOD(put_FontSize)(long lFontSize);
	STDMETHOD(GetFontData)(BSTR* pbstrFontName, VARIANT_BOOL* pvbIsBold,
		VARIANT_BOOL* pvbIsItalic, long* plFontSize);
	STDMETHOD(get_PdfPasswordSettings)(IPdfPasswordSettings** ppPdfSettings);
	STDMETHOD(put_PdfPasswordSettings)(IPdfPasswordSettings* pPdfSettings);

// IFileProcessingTask
	STDMETHOD(raw_Init)(long nActionID, IFAMTagManager* pFAMTM, IFileProcessingDB *pDB);
	STDMETHOD(raw_ProcessFile)(BSTR bstrFileFullName, long nFileID, long nActionID,
		IFAMTagManager* pTagManager, IFileProcessingDB* pDB, IProgressStatus* pProgressStatus,
		VARIANT_BOOL bCancelRequested, EFileProcessingResult* pResult);
	STDMETHOD(raw_Cancel)();
	STDMETHOD(raw_Close)();
	STDMETHOD(raw_RequiresAdminAccess)(VARIANT_BOOL* pbResult);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL* pbValue);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR* pstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown** pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown* pObject);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL* pbValue);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID* pClassID);
	STDMETHOD(IsDirty)();
	STDMETHOD(Load)(IStream* pStm);
	STDMETHOD(Save)(IStream* pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER* pcbSize);

private:
	//////////////
	// Variables
	//////////////

	// Output File Name: can contain Tags
	string m_strOutputFileName;

	// Pointer to AFUtility
	IAFUtilityPtr m_ipAFUtility;

	// Contains the names of the attributes to find
	IVariantVectorPtr m_ipAttributeNames;

	// Set contains the same names as m_ipAttributeNames
	set<string> m_setAttributeNames;

	bool m_bDirty;

	string m_strVOAFileName;

	// Flags to indicate use of annotations
	bool m_bCarryForwardAnnotations;
	bool m_bApplyRedactionsAsAnnotations;

	// Pointer to the IDShield database manager
	UCLID_REDACTIONCUSTOMCOMPONENTSLib::IIDShieldProductDBMgrPtr m_ipIDShieldDB;

	// Whether to use the previously redacted image (true) or the original image (false)
	bool m_bUseRedactedImage;

	// Redaction text and color settings
	RedactionAppearanceOptions m_redactionAppearance;

	// The password settings used when the output format is PDF
	IPdfPasswordSettingsPtr m_ipPdfSettings;

	//////////////
	// Methods
	//////////////

	// Sets properties to their default state
	void clear();

	// Returns m_ipAFUtility, after initializing it if necessary
	IAFUtilityPtr getAFUtility();

	// Puts the attribute names in the set
	void fillAttributeSet(IVariantVectorPtr ipAttributeNames, set<string>& rsetAttributeNames);

	// Allocates m_ipIDShieldDB pointer if it does not exist and returns m_ipIDShieldDB
	UCLID_REDACTIONCUSTOMCOMPONENTSLib::IIDShieldProductDBMgrPtr getIDShieldDBPtr();

	// Gets the exemption codes associated with the specified attribute
	string getExemptionCodes(IAttributePtr ipAttribute);

	// Adds a metadata attribute to the specified voa file using the specified information
	void storeMetaData(const string& strVoaFile, IIUnknownVectorPtr ipAttributes, 
		IIUnknownVectorPtr ipRedactedAttributes, CTime tStartTime, double dSeconds, 
		const string& strSourceDocument, const string& strRedactedImage, bool bOverwroteOutput);

	// Gets next attribute id from the specified attributes
	long getNextId(IIUnknownVectorPtr ipAttributes);

	// Gets the unique id from the specified attribute; or -1 if it doesn't have an id
	long getAttributeId(IAttributePtr ipAttribute);

	// Gets the id attribute from the specified attribute
	IAttributePtr getIdAttribute(IAttributePtr ipAttribute);

	// Assigns unique ids to any attributes that don't have them
	void assignIds(IIUnknownVectorPtr ipAttributes, long llNextId, const string& strSourceDocument);

	// Create an id attribute with the specified value
	IAttributePtr createIdAttribute(const string& strSourceDocument, long lId);

	// Gets the next automated redaction session id from the specified attributes
	long getNextSessionId(IIUnknownVectorPtr ipAttributes);

	// Creates a metadata attribute using the specified information
	IAttributePtr createMetaDataAttribute(long lSession, const string& strVoaFile, 
		IIUnknownVectorPtr ipRedactedAttributes, CTime tStartTime, double dElapsedSeconds, 
		const string& strSourceDocument, const string& strRedactedImage, bool bOverwroteOutput);

	// Creates the user info attribute
	IAttributePtr createUserInfoAttribute(const string& strSourceDocument);

	// Creates the time info attribute
	IAttributePtr createTimeInfoAttribute(const string& strSourceDocument, CTime tStartTime, 
		double dElapsedSeconds);

	// Creates the redacted categories attribute
	IAttributePtr createRedactedCategoriesAttribute(const string& strSourceDocument);

	// Creates the output options attribute
	IAttributePtr createOptionsAttribute(const string& strSourceDocument, bool bOverwroteOutput);

	// Creates the redaction text and color settings attribute
	IAttributePtr createRedactionAppearanceAttribute(const string& strSourceDocument);

	// Creates the redacted entries attribute
	IAttributePtr createRedactedEntriesAttribute(const string& strSourceDocument, 
		IIUnknownVectorPtr ipRedactedAttributes);

	// Creates an attribute with the specified name, non-spatial value, and type
	IAttributePtr createAttribute(const string& strSourceDocument, const string& strName, 
		const string& strValue = "");
	IAttributePtr createAttribute(const string& strSourceDocument, const string& strName, 
		const string& strValue, const string& strType);

	// Returns either "Black" or "White"
	string getColorAsString(COLORREF crColor);

	void validateLicense();
};
