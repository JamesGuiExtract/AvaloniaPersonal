// DoubleInputValidator.h : Declaration of the CDoubleInputValidator

#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CDoubleInputValidator
class ATL_NO_VTABLE CDoubleInputValidator : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CDoubleInputValidator, &CLSID_DoubleInputValidator>,
	public ISupportErrorInfo,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CDoubleInputValidator>,
	public IDispatchImpl<IDoubleInputValidator, &IID_IDoubleInputValidator, &LIBID_UCLID_GENERALIVLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IInputValidator, &IID_IInputValidator, &LIBID_UCLID_INPUTFUNNELLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>
{
public:
	CDoubleInputValidator();
	~CDoubleInputValidator();

DECLARE_REGISTRY_RESOURCEID(IDR_DOUBLEINPUTVALIDATOR)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CDoubleInputValidator)
	COM_INTERFACE_ENTRY(IDoubleInputValidator)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IDoubleInputValidator)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(IInputValidator)
	COM_INTERFACE_ENTRY(ITestableComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CDoubleInputValidator)
	PROP_PAGE(CLSID_DoubleInputValidatorPP)
END_PROP_MAP()

public:
// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// IDoubleInputValidator
	STDMETHOD(get_IncludeMaxInRange)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IncludeMaxInRange)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_IncludeMinInRange)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IncludeMinInRange)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_NegativeAllowed)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_NegativeAllowed)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_ZeroAllowed)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_ZeroAllowed)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_HasMax)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_HasMax)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_HasMin)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_HasMin)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_Max)(/*[out, retval]*/ double *pVal);
	STDMETHOD(put_Max)(/*[in]*/ double newVal);
	STDMETHOD(get_Min)(/*[out, retval]*/ double *pVal);
	STDMETHOD(put_Min)(/*[in]*/ double newVal);

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);
	
// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pbstrComponentDescription);
	
// IInputValidator
	STDMETHOD(raw_ValidateInput)(ITextInput * pTextInput, VARIANT_BOOL * pbSuccessful);
	STDMETHOD(raw_GetInputType)(BSTR * pstrInputType);
	
// ICopyableObject
	STDMETHOD(raw_Clone)(/*[out, retval]*/ IUnknown* *pObject);
	STDMETHOD(raw_CopyFrom)(/*[in]*/ IUnknown *pObject);

// ITestableComponent
	STDMETHOD(raw_RunAutomatedTests)(IVariantVector* pParams, BSTR strTCLFile);
	STDMETHOD(raw_RunInteractiveTests)();
	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(raw_SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);

protected:

	//=======================================================================
	// PURPOSE: Runs automated test checking setting of minimum values.
	// REQUIRE: None.
	// PROMISE: None
	// ARGS:	None
	void	doTest1();

	//=======================================================================
	// PURPOSE: Runs automated test checking setting of maximum values.
	// REQUIRE: None.
	// PROMISE: None
	// ARGS:	None
	void	doTest2();

	//=======================================================================
	// PURPOSE: Runs automated test of allowing zeros.
	// REQUIRE: None.
	// PROMISE: None
	// ARGS:	None
	void	doTest3();

	//=======================================================================
	// PURPOSE: Runs automated test of allowing negative numbers.
	// REQUIRE: None.
	// PROMISE: None
	// ARGS:	None
	void	doTest4();

	//=======================================================================
	// PURPOSE: Runs automated test checking inclusion of minimum values.
	// REQUIRE: None.
	// PROMISE: None
	// ARGS:	None
	void	doTest5();

	//=======================================================================
	// PURPOSE: Runs automated test checking inclusion of maximum values.
	// REQUIRE: None.
	// PROMISE: None
	// ARGS:	None
	void	doTest6();

	//=======================================================================
	// PURPOSE: Runs automated test checking various input string formats.
	// REQUIRE: None.
	// PROMISE: None
	// ARGS:	None
	void	doTest7();

private:
	// Pointer to test result logger that stores and displays test results
	ITestResultLoggerPtr		m_ipLogger;

	// Pointer to executor for interactive tests
	IInteractiveTestExecuterPtr m_ipExecuter;

	// Check appropriate value member against min/max and other limits
	bool	checkLimits();

	// Set member variable defaults
	void	setDefaults();

	// Retrieves an IInputValidatorPtr to this instance.
	IInputValidatorPtr getThisAsInputValidatorPtr();

	// Validate license, throw exception if the component is not licensed
	void	validateLicense();

	// Member variables
	double			m_dMinimum;
	double			m_dMaximum;
	double			m_dValue;

	bool			m_bMinDefined;
	bool			m_bMaxDefined;
	bool			m_bZeroAllowed;
	bool			m_bNegativeAllowed;
	bool			m_bIncludeMinimum;
	bool			m_bIncludeMaximum;

	// flag to keep track of whether this object has changed
	// since the last save-to-stream operation
	bool m_bDirty;
};
