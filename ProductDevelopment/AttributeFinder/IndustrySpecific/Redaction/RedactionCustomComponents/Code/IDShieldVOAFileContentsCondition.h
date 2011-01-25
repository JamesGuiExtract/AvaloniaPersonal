// IDShieldVOAFileContentsCondition.h : Declaration of the CIDShieldVOAFileContentsCondition

#pragma once
#include "resource.h"       // main symbols
#include "RedactionCustomComponents.h"
#include "RedactionCCConstants.h"
#include "..\..\..\..\..\..\ReusableComponents\COMComponents\UCLIDFileProcessing\Code\FPCategories.h"

#include <string>
#include <set>
#include <memory>

using namespace std;

class AttributeTester;

/////////////////////////////////////////////////////////////////////////////
// CIDShieldVOAFileContentsCondition
/////////////////////////////////////////////////////////////////////////////
class ATL_NO_VTABLE CIDShieldVOAFileContentsCondition :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CIDShieldVOAFileContentsCondition, &CLSID_IDShieldVOAFileContentsCondition>,
	public ISupportErrorInfo,
	public IDispatchImpl<IIDShieldVOAFileContentsCondition, &IID_IIDShieldVOAFileContentsCondition, &LIBID_UCLID_REDACTIONCUSTOMCOMPONENTSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IFAMCondition, &IID_IFAMCondition, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CIDShieldVOAFileContentsCondition>
{

public:
	CIDShieldVOAFileContentsCondition();
	~CIDShieldVOAFileContentsCondition();

DECLARE_REGISTRY_RESOURCEID(IDR_IDSHIELDVOAFILECONTENTSCONDITION)

BEGIN_COM_MAP(CIDShieldVOAFileContentsCondition)
	COM_INTERFACE_ENTRY(IIDShieldVOAFileContentsCondition)
	COM_INTERFACE_ENTRY2(IDispatch, IIDShieldVOAFileContentsCondition)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(IFAMCondition)
	COM_INTERFACE_ENTRY(IAccessRequired)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CIDShieldVOAFileContentsCondition)
	PROP_PAGE(CLSID_IDShieldVOAFileContentsConditionPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CIDShieldVOAFileContentsCondition)
	IMPLEMENTED_CATEGORY(CATID_FP_FAM_CONDITIONS)
END_CATEGORY_MAP()

DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

// IIDShieldVOAFileContentsCondition
	STDMETHOD(get_CheckDataContents)(VARIANT_BOOL* pVal);
	STDMETHOD(put_CheckDataContents)(VARIANT_BOOL newVal);
	STDMETHOD(get_LookForHCData)(VARIANT_BOOL* pVal);
	STDMETHOD(put_LookForHCData)(VARIANT_BOOL newVal);
	STDMETHOD(get_LookForMCData)(VARIANT_BOOL* pVal);
	STDMETHOD(put_LookForMCData)(VARIANT_BOOL newVal);
	STDMETHOD(get_LookForLCData)(VARIANT_BOOL* pVal);
	STDMETHOD(put_LookForLCData)(VARIANT_BOOL newVal);
	STDMETHOD(get_LookForManualData)(VARIANT_BOOL* pVal);
	STDMETHOD(put_LookForManualData)(VARIANT_BOOL newVal);
	STDMETHOD(get_LookForClues)(VARIANT_BOOL* pVal);
	STDMETHOD(put_LookForClues)(VARIANT_BOOL newVal);
	STDMETHOD(get_CheckDocType)(VARIANT_BOOL* pVal);
	STDMETHOD(put_CheckDocType)(VARIANT_BOOL newVal);
	STDMETHOD(get_DocCategory)(BSTR* pVal);
	STDMETHOD(put_DocCategory)(BSTR newVal);
	STDMETHOD(get_DocTypes)(IVariantVector** pVal);
	STDMETHOD(put_DocTypes)(IVariantVector* newVal);
	STDMETHOD(get_TargetFileName)(BSTR* pVal);
	STDMETHOD(put_TargetFileName)(BSTR newVal);
	STDMETHOD(get_MissingFileBehavior)(EMissingVOAFileBehavior* pVal);
	STDMETHOD(put_MissingFileBehavior)(EMissingVOAFileBehavior newVal);
	STDMETHOD(get_AttributeQuantifier)(EAttributeQuantifier* pVal);
	STDMETHOD(put_AttributeQuantifier)(EAttributeQuantifier newVal);
	STDMETHOD(get_ConfigureConditionsOnly)(VARIANT_BOOL* pVal);
	STDMETHOD(put_ConfigureConditionsOnly)(VARIANT_BOOL newVal);

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL* pbValue);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR* pstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown** pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown* pObject);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL* pbValue);

// IFAMCondition
	STDMETHOD(raw_FileMatchesFAMCondition)(IFileRecord* pFileRecord, IFileProcessingDB* pFPDB, 
		long lActionID, IFAMTagManager* pFAMTM, VARIANT_BOOL* pRetVal);

// IAccessRequired
	STDMETHOD(raw_RequiresAdminAccess)(VARIANT_BOOL* pbResult);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID* pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream* pStm);
	STDMETHOD(Save)(IStream* pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER* pcbSize);

private:
	//////////////
	// Variables
	//////////////
	bool m_bDirty;
	bool m_bCheckDataContents;
	bool m_bLookForHCData;
	bool m_bLookForMCData;
	bool m_bLookForLCData;
	bool m_bLookForManualData;
	bool m_bLookForClues;
	bool m_bCheckDocType;
	string m_strDocCategory;
	set<string> m_setDocTypes;
	string m_strTargetFileName;
	UCLID_REDACTIONCUSTOMCOMPONENTSLib::EMissingVOAFileBehavior m_eMissingFileBehavior;
	UCLID_REDACTIONCUSTOMCOMPONENTSLib::EAttributeQuantifier m_eAttributeQuantifier;
	bool m_bConfigureConditionsOnly;

	// Tests if the attributes match the specified condition
	auto_ptr<AttributeTester> m_apTester;

	/////////////
	// Methods
	/////////////

	// get COM ptr to self
	UCLID_REDACTIONCUSTOMCOMPONENTSLib::IIDShieldVOAFileContentsConditionPtr getThisAsCOMPtr();

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Sets the dirty flag and resets the attribute tester as necessary.
	void setDirty(bool bIsDirty);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Initializes the attribute tester if it is not already initialized.
	void updateAttributeTester();
	
	void validateLicense();	
};