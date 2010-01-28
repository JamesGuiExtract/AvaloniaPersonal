// CharacterConfidenceCondition.h : Declaration of the CCharacterConfidenceCondition

#pragma once
#include "resource.h"       // main symbols
#include "AFConditions.h"

#include <AFCategories.h>


#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif


// CCharacterConfidenceCondition
class ATL_NO_VTABLE CCharacterConfidenceCondition :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CCharacterConfidenceCondition, &CLSID_CharacterConfidenceCondition>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDispatchImpl<ICharacterConfidenceCondition, &IID_ICharacterConfidenceCondition, &LIBID_UCLID_AFCONDITIONSLib, /*wMajor =*/ 1, /*wMinor =*/ 0>,
	public IDispatchImpl<IAFCondition, &IID_IAFCondition, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<CCharacterConfidenceCondition>
{
public:
	CCharacterConfidenceCondition();

	DECLARE_REGISTRY_RESOURCEID(IDR_CHARACTERCONFIDENCECONDITION)

	BEGIN_COM_MAP(CCharacterConfidenceCondition)
		COM_INTERFACE_ENTRY(ICharacterConfidenceCondition)
		COM_INTERFACE_ENTRY2(IDispatch, ICharacterConfidenceCondition)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(IPersistStream)
		COM_INTERFACE_ENTRY(IAFCondition)
		COM_INTERFACE_ENTRY(ILicensedComponent)
		COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
		COM_INTERFACE_ENTRY(ICategorizedComponent)
		COM_INTERFACE_ENTRY(ICopyableObject)
		COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
	END_COM_MAP()

	BEGIN_PROP_MAP(CCharacterConfidenceCondition)
		PROP_PAGE(CLSID_CharacterConfidenceConditionPP)
	END_PROP_MAP()
	
	BEGIN_CATEGORY_MAP(CCharacterConfidenceCondition)
		IMPLEMENTED_CATEGORY(CATID_AFAPI_CONDITIONS)
	END_CATEGORY_MAP()

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();

	void FinalRelease();

	// IAFCondition
	STDMETHOD(raw_ProcessCondition)(IAFDocument *pAFDoc, VARIANT_BOOL *pbRetVal);

	// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR *pbstrComponentDescription);

	// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown **pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown *pObject);

	// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL *pbValue);

	// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStream);
	STDMETHOD(Save)(IStream *pStream, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

	// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL *pbValue);

	// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

	// ICharacterConfidenceCondition
	STDMETHOD(get_AggregateFunction)(EAggregateFunctions *pVal);
	STDMETHOD(put_AggregateFunction)(EAggregateFunctions newVal);
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
	STDMETHOD(get_IsMet)(VARIANT_BOOL* pVal);
	STDMETHOD(put_IsMet)(VARIANT_BOOL newVal);
public:
	///////////////
	// Variables
	///////////////

	bool m_bDirty;
	
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
	
	// Indicates the aggregate function to use for the character confidence
	EAggregateFunctions m_eAggregateFunction;

	// Flag to indicate if the condition should be met or not met
	bool m_bIsMet;

	///////////////
	// Methods
	///////////////

	// Method returns the result of the comparison lCalculatedConfidence eOp lConditionConfidence
	bool evaluateCondition(const EConditionalOp eOp, const long lCalculatedConfidence, 
		const long lConditionConfidence);

	// Check licensing
	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(CharacterConfidenceCondition), CCharacterConfidenceCondition)
