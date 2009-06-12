// DateInputValidator.h : Declaration of the CDateInputValidator

#pragma once

#include "resource.h"       // main symbols

#include <string>

/////////////////////////////////////////////////////////////////////////////
// CDateInputValidator
class ATL_NO_VTABLE CDateInputValidator : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CDateInputValidator, &CLSID_DateInputValidator>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDispatchImpl<IDateInputValidator, &IID_IDateInputValidator, &LIBID_UCLID_GENERALIVLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IInputValidator, &IID_IInputValidator, &LIBID_UCLID_INPUTFUNNELLib>,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>
{
public:
	CDateInputValidator();

DECLARE_REGISTRY_RESOURCEID(IDR_DATEINPUTVALIDATOR)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CDateInputValidator)
	COM_INTERFACE_ENTRY(IDateInputValidator)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IDateInputValidator)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IInputValidator)
	COM_INTERFACE_ENTRY(ITestableComponent)
END_COM_MAP()

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// IInputValidator
	STDMETHOD(raw_ValidateInput)(ITextInput * pTextInput, VARIANT_BOOL * pbSuccessful);
	STDMETHOD(raw_GetInputType)(BSTR * pstrInputType);

// ICopyableObject
	STDMETHOD(raw_Clone)(/*[out, retval]*/ IUnknown* *pObject);
	STDMETHOD(raw_CopyFrom)(/*[in]*/ IUnknown *pObject);

// ITestableComponent
	STDMETHOD(raw_RunAutomatedTests)(IVariantVector * pParams, BSTR strTCLFile);
	STDMETHOD(raw_RunInteractiveTests)();
	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(raw_SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);

protected:

	//=======================================================================
	// PURPOSE: Runs automated tests of MM/DD/YY and MM/DD/YYYY inputs.
	// REQUIRE: None.
	// PROMISE: None
	// ARGS:	None
	void	doTest1();

	//=======================================================================
	// PURPOSE: Runs automated tests of Month DD, YYYY inputs.
	// REQUIRE: None.
	// PROMISE: None
	// ARGS:	None
	void	doTest2();

	//=======================================================================
	// PURPOSE: Runs automated tests of DD Month YYYY inputs.
	// REQUIRE: None.
	// PROMISE: None
	// ARGS:	None
	void	doTest3();

	//=======================================================================
	// PURPOSE: Runs automated tests of leap day inputs.
	// REQUIRE: None.
	// PROMISE: None
	// ARGS:	None
	void	doTest4();

	//=======================================================================
	// PURPOSE: Runs automated tests of MM DD YYYY inputs with various 
	//             delimiters.
	// REQUIRE: None.
	// PROMISE: None
	// ARGS:	None
	void	doTest5();

private:

	//Retrieves an IInputValidatorPtr to this instance.
	IInputValidatorPtr getThisAsInputValidatorPtr();

	// Validate license, throw exception if the component is not licensed
	void validateLicense();

	// Pointer to test result logger that stores and displays test results
	ITestResultLoggerPtr		m_ipLogger;

	// flag to keep track of whether this object has changed
	// since the last save-to-stream operation
	bool m_bDirty;
};
