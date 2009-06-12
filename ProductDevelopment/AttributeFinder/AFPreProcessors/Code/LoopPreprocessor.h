// LoopPreprocessor.h : Declaration of the CLoopPreprocessor

#pragma once
#include "AFPreprocessors.h"
#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"

#include <comsvcs.h>

// CLoopPreprocessor

class ATL_NO_VTABLE CLoopPreprocessor :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CLoopPreprocessor, &CLSID_LoopPreprocessor>,
	public ISupportErrorInfo,
	public IDispatchImpl<ILoopPreprocessor, &IID_ILoopPreprocessor, &LIBID_UCLID_AFPREPROCESSORSLib, /*wMajor =*/ 1, /*wMinor =*/ 0>,
	public IDispatchImpl<IDocumentPreprocessor, &__uuidof(IDocumentPreprocessor), &LIBID_UCLID_AFCORELib, /* wMajor = */ 1>,
	public IDispatchImpl<ICategorizedComponent, &__uuidof(ICategorizedComponent), &LIBID_UCLID_COMUTILSLib, /* wMajor = */ 1>,
	public IDispatchImpl<ICopyableObject, &__uuidof(ICopyableObject), &LIBID_UCLID_COMUTILSLib, /* wMajor = */ 1>,
	public IDispatchImpl<ILicensedComponent, &__uuidof(ILicensedComponent), &LIBID_UCLID_COMLMLib, /* wMajor = */ 1>,
	public IDispatchImpl<IMustBeConfiguredObject, &__uuidof(IMustBeConfiguredObject), &LIBID_UCLID_COMUTILSLib, /* wMajor = */ 1>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CLoopPreprocessor>
{
public:
	CLoopPreprocessor();
	~CLoopPreprocessor();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();

	void FinalRelease();

	DECLARE_REGISTRY_RESOURCEID(IDR_LOOPPREPROCESSOR)

	BEGIN_COM_MAP(CLoopPreprocessor)
		COM_INTERFACE_ENTRY(ILoopPreprocessor)
		COM_INTERFACE_ENTRY2(IDispatch, IDocumentPreprocessor)
		COM_INTERFACE_ENTRY(IDocumentPreprocessor)
		COM_INTERFACE_ENTRY(ICategorizedComponent)
		COM_INTERFACE_ENTRY(ICopyableObject)
		COM_INTERFACE_ENTRY(ILicensedComponent)
		COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
		COM_INTERFACE_ENTRY(IPersistStream)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
	END_COM_MAP()

	BEGIN_PROP_MAP(CLoopPreprocessor)
		PROP_PAGE(CLSID_LoopPreprocessorPP)
	END_PROP_MAP()

	BEGIN_CATEGORY_MAP(CLoopPreprocessor)
		IMPLEMENTED_CATEGORY(CATID_AFAPI_DOCUMENT_PREPROCESSORS)
	END_CATEGORY_MAP()

	// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

	// IDocumentPreprocessor
	STDMETHOD(raw_Process)(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus);

	// ICategorizedComponent Methods
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

	// ICopyableObject Methods
	STDMETHOD(raw_Clone)(LPUNKNOWN * pObject);
	STDMETHOD(raw_CopyFrom)(LPUNKNOWN pObject);

	// ILicensedComponent Methods
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

	// IMustBeConfiguredObject Methods
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbConfigured);

	// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

	// ILoopPreprocessor
	STDMETHOD(get_Preprocessor)(IObjectWithDescription ** pVal);
	STDMETHOD(put_Preprocessor)(IObjectWithDescription * newVal);
	STDMETHOD(get_Condition)(IObjectWithDescription ** pVal);
	STDMETHOD(put_Condition)(IObjectWithDescription * newVal);
	STDMETHOD(get_ConditionValue)( VARIANT_BOOL *pVal);
	STDMETHOD(put_ConditionValue)( VARIANT_BOOL newVal);
	STDMETHOD(get_LogExceptionForMaxIterations)( VARIANT_BOOL *pVal);
	STDMETHOD(put_LogExceptionForMaxIterations)( VARIANT_BOOL newVal);
	STDMETHOD(get_Iterations)( long *pVal);
	STDMETHOD(put_Iterations)( long newVal);
	STDMETHOD(get_LoopType)(ELoopType *pVal);
	STDMETHOD(put_LoopType)(ELoopType newVal);

private:
	// Data Members
	bool m_bDirty;

	// Preprocessor to run after the rule
	IObjectWithDescriptionPtr m_ipPreprocessor;

	// Condition used for do or while loops
	IObjectWithDescriptionPtr m_ipCondition;

	// Number of iterations to run for the for loop and
	// Maximum iterations for the do or while loop
	long m_nIterations;

	// Value condition should be to continue loop
	bool m_bConditionValue;

	// Flag to indicate if exception should be logged if maximum number of iterations exceeded
	bool m_bLogExceptionForMaxIterations;

	// Type of Loop
	ELoopType m_eLoopType;

	// Methods
	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(LoopPreprocessor), CLoopPreprocessor)
