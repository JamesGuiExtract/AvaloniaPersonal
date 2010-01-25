// CharacterConfidenceDS.h : Declaration of the CCharacterConfidenceDS

#pragma once
#include "resource.h"       // main symbols
#include "AFDataScorers.h"

#include <AFCategories.h>

#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

// CCharacterConfidenceDS

class ATL_NO_VTABLE CCharacterConfidenceDS :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CCharacterConfidenceDS, &CLSID_CharacterConfidenceDS>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<ICharacterConfidenceDS, &IID_ICharacterConfidenceDS, &LIBID_UCLID_AFDATASCORERSLib, /*wMajor =*/ 1, /*wMinor =*/ 0>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IDataScorer, &IID_IDataScorer, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public ISpecifyPropertyPagesImpl<CCharacterConfidenceDS>,
	public IDispatchImpl<IMustBeConfiguredObject, &__uuidof(IMustBeConfiguredObject), &LIBID_UCLID_COMUTILSLib, /* wMajor = */ 1>
{
public:
	CCharacterConfidenceDS();

	DECLARE_REGISTRY_RESOURCEID(IDR_CHARACTERCONFIDENCEDS)

	BEGIN_COM_MAP(CCharacterConfidenceDS)
		COM_INTERFACE_ENTRY(ICharacterConfidenceDS)
		COM_INTERFACE_ENTRY2(IDispatch, IDataScorer)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(IPersistStream)
		COM_INTERFACE_ENTRY(IDataScorer)
		COM_INTERFACE_ENTRY(ICategorizedComponent)
		COM_INTERFACE_ENTRY(ICopyableObject)
		COM_INTERFACE_ENTRY(ILicensedComponent)
		COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
		COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	END_COM_MAP()

	BEGIN_CATEGORY_MAP(CCharacterConfidenceDS)
		IMPLEMENTED_CATEGORY(CATID_AFAPI_DATA_SCORERS)
	END_CATEGORY_MAP()

	BEGIN_PROP_MAP(CCharacterConfidenceDS)
		PROP_PAGE(CLSID_CharacterConfidenceDSPP)
	END_PROP_MAP()

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IDataScorer
	STDMETHOD(raw_GetDataScore1)(IAttribute * pAttribute, LONG * pScore);
	STDMETHOD(raw_GetDataScore2)(IIUnknownVector * pAttributes, LONG * pScore);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown **ppObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IMustBeConfiguredObject Methods
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbConfigured);

// ICharacterConfidenceDS
	STDMETHOD(get_AggregateFunction)(EAggregateFunctions *pVal);
	STDMETHOD(put_AggregateFunction)(EAggregateFunctions newVal);

private:
	//////////
	// Variables
	//////////

	// member variables
	bool m_bDirty; // dirty flag to indicate modified state of this object

	// Indicates the aggregate function to use for the character confidence
	EAggregateFunctions m_eAggregateFunction;
	
	//////////
	// Methods
	//////////

	void validateLicense();

	// Method returns the score for the attribute
	long getAttributeScore(IAttributePtr ipAttr);
};

OBJECT_ENTRY_AUTO(__uuidof(CharacterConfidenceDS), CCharacterConfidenceDS)
