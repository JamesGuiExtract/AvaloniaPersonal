// MathematicalCondition.h : Declaration of the CMathematicalCondition

#pragma once
#include "resource.h"       // main symbols

#include "..\..\Code\FPCategories.h"
#include "ESSkipConditions.h"

/////////////////////////////////////////////////////////////////////////////
// CMathematicalCondition
class ATL_NO_VTABLE CMathematicalCondition :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CMathematicalCondition, &CLSID_MathematicalCondition>,
	public ISupportErrorInfo,
	public IDispatchImpl<IMathematicalFAMCondition, &IID_IMathematicalFAMCondition, &LIBID_EXTRACT_FAMCONDITIONSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IClipboardCopyable, &IID_IClipboardCopyable, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IFAMCondition, &IID_IFAMCondition, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CMathematicalCondition>
{
public:
	CMathematicalCondition();
	~CMathematicalCondition();


DECLARE_REGISTRY_RESOURCEID(IDR_MATHCONDITION)


BEGIN_COM_MAP(CMathematicalCondition)
	COM_INTERFACE_ENTRY(IMathematicalFAMCondition)
	COM_INTERFACE_ENTRY2(IDispatch, IMathematicalFAMCondition)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IClipboardCopyable)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(IFAMCondition)
	COM_INTERFACE_ENTRY(IAccessRequired)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CMathematicalCondition)
	PROP_PAGE(CLSID_MathematicalConditionPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CMathematicalCondition)
	IMPLEMENTED_CATEGORY(CATID_FP_FAM_CONDITIONS)
END_CATEGORY_MAP()

DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease();

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IMathematicalConditionFAMCondition
	STDMETHOD(get_ConsiderMet)(VARIANT_BOOL* pVal);
	STDMETHOD(put_ConsiderMet)(VARIANT_BOOL newVal);
	STDMETHOD(get_MathematicalCondition)(IMathConditionChecker** ppCondition);
	STDMETHOD(put_MathematicalCondition)(IMathConditionChecker* pCondition);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL* pbValue);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR* pstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown** pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown* pObject);

// IClipboardCopyable
	STDMETHOD(raw_NotifyCopiedFromClipboard)();

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

	// Specifies whether the condition should be considered met/not met if
	// the MathConditionChecker is satisfied
	bool m_bConsiderConditionMet;

	// Hold the condition checker object to be used to check this condition
	IMathConditionCheckerPtr m_ipConditionChecker;

	/////////////
	// Methods
	/////////////
	void validateLicense();

	// Checks whether this object is configured or not
	VARIANT_BOOL isConfigured();
};
