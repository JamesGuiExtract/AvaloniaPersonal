// DataScorerBasedAS.h : Declaration of the CDataScorerBasedAS

#pragma once
#include "resource.h"       // main symbols
#include "AFSelectors.h"

#include <AFCategories.h>


#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

// CDataScorerBasedAS

class ATL_NO_VTABLE CDataScorerBasedAS :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CDataScorerBasedAS, &CLSID_DataScorerBasedAS>,
	public ISupportErrorInfo,
	public IDispatchImpl<IDataScorerBasedAS, &IID_IDataScorerBasedAS, &LIBID_UCLID_AFSELECTORSLib, /*wMajor =*/ 1, /*wMinor =*/ 0>,
	public IPersistStream,
	public IDispatchImpl<IAttributeSelector, &__uuidof(IAttributeSelector), &LIBID_UCLID_AFCORELib, /* wMajor = */ 1>,
	public IDispatchImpl<ICategorizedComponent, &__uuidof(ICategorizedComponent), &LIBID_UCLID_COMUTILSLib, /* wMajor = */ 1>,
	public IDispatchImpl<ICopyableObject, &__uuidof(ICopyableObject), &LIBID_UCLID_COMUTILSLib, /* wMajor = */ 1>,
	public IDispatchImpl<ILicensedComponent, &__uuidof(ILicensedComponent), &LIBID_UCLID_COMLMLib, /* wMajor = */ 1>,
	public ISpecifyPropertyPagesImpl<CDataScorerBasedAS>,
	public IDispatchImpl<IMustBeConfiguredObject, &__uuidof(IMustBeConfiguredObject), &LIBID_UCLID_COMUTILSLib, /* wMajor = */ 1>
{
public:
	CDataScorerBasedAS();
	~CDataScorerBasedAS();

	DECLARE_REGISTRY_RESOURCEID(IDR_DATASCORERBASEDAS)

	BEGIN_COM_MAP(CDataScorerBasedAS)
		COM_INTERFACE_ENTRY(IDataScorerBasedAS)
		COM_INTERFACE_ENTRY2(IDispatch, IAttributeSelector)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(IPersistStream)
		COM_INTERFACE_ENTRY(IAttributeSelector)
		COM_INTERFACE_ENTRY(ICategorizedComponent)
		COM_INTERFACE_ENTRY(ICopyableObject)
		COM_INTERFACE_ENTRY(ILicensedComponent)
		COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
		COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	END_COM_MAP()

	BEGIN_CATEGORY_MAP(CDataScorerBasedAS)
		IMPLEMENTED_CATEGORY(CATID_AFAPI_ATTRIBUTE_SELECTORS)
	END_CATEGORY_MAP()

	BEGIN_PROP_MAP(CDataScorerBasedAS)
		PROP_PAGE(CLSID_DataScorerBasedASPP)
	END_PROP_MAP()

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
		m_ipDataScorer = NULL;
	}

public:
	// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

	// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStream);
	STDMETHOD(Save)(IStream *pStream, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

	// IAttributeSelector Methods
	STDMETHOD(raw_SelectAttributes)(IIUnknownVector * pAttrIn, IAFDocument * pAFDoc, IIUnknownVector * * pAttrOut);

	// ICategorizedComponent Methods
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

	// ICopyableObject Methods
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

	// ILicensedComponent Methods
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

	// IMustBeConfiguredObject Methods
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbConfigured);

	// IDataScorerBasedAS Methods
	STDMETHOD(get_DataScorer)(IObjectWithDescription** ppVal);
	STDMETHOD(put_DataScorer)(IObjectWithDescription* newVal);
	STDMETHOD(get_FirstScoreCondition)(EConditionalOp* pVal);
	STDMETHOD(put_FirstScoreCondition)(EConditionalOp newVal);
	STDMETHOD(get_FirstScoreToCompare)(long* pVal);
	STDMETHOD(put_FirstScoreToCompare)(long newVal);
	STDMETHOD(get_IsSecondCondition)(VARIANT_BOOL* pVal);
	STDMETHOD(put_IsSecondCondition)( VARIANT_BOOL newVal);
	STDMETHOD(get_SecondScoreCondition)(EConditionalOp* pVal);
	STDMETHOD(put_SecondScoreCondition)(EConditionalOp newVal);
	STDMETHOD(get_SecondScoreToCompare)(long* pVal);
	STDMETHOD(put_SecondScoreToCompare)(long newVal);
	STDMETHOD(get_AndSecondCondition)(VARIANT_BOOL* pVal);
	STDMETHOD(put_AndSecondCondition)(VARIANT_BOOL newVal);

private:
	//////////
	// Variables
	//////////

	// member variables
	bool m_bDirty; // dirty flag to indicate modified state of this object

	// Data scorer
	IObjectWithDescriptionPtr m_ipDataScorer;

	// Condition for the first score comparison
	EConditionalOp m_eFirstScoreCondition;

	// Score to compare against for the first condition
	long m_lFirstScoreToCompare;

	// Flag to indicate if there is a second condition
	bool m_bIsSecondCondition;

	// Condition for the second comparison
	EConditionalOp m_eSecondScoreCondition;

	// Score to compare against for the second condition
	long m_lSecondScoreToCompare;

	// Flag to indicate if the 2 conditions should AND'd together
	bool m_bAndConditions;

	//////////
	// Methods
	//////////

	void validateLicense();

	// Method returns the result of the comparison lAttributeScore eOp lConditionScore
	bool evaluateCondition(const EConditionalOp eOp, const long lAttributeScore, 
		const long lConditionScore);
};

OBJECT_ENTRY_AUTO(__uuidof(DataScorerBasedAS), CDataScorerBasedAS)
