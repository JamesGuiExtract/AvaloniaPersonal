// RegExprInputValidator.h : Declaration of the CRegExprInputValidator

#pragma once

#include "resource.h"       // main symbols
#include "..\..\..\IFCore\Code\IFCategories.h"

#include <string>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CRegExprInputValidator
class ATL_NO_VTABLE CRegExprInputValidator : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CRegExprInputValidator, &CLSID_RegExprInputValidator>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<IRegExprInputValidator, &IID_IRegExprInputValidator, &LIBID_UCLID_REGEXPRIVLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IInputValidator, &IID_IInputValidator, &LIBID_UCLID_INPUTFUNNELLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<CRegExprInputValidator>,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>
{
public:
	CRegExprInputValidator();
	~CRegExprInputValidator();

DECLARE_REGISTRY_RESOURCEID(IDR_REGEXPRINPUTVALIDATOR)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CRegExprInputValidator)
	COM_INTERFACE_ENTRY(IRegExprInputValidator)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IRegExprInputValidator)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(IInputValidator)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
	COM_INTERFACE_ENTRY(ITestableComponent)
END_COM_MAP()

BEGIN_CATEGORY_MAP(CLimitAsMidPart)
	IMPLEMENTED_CATEGORY(CATID_INPUTFUNNEL_INPUT_VALIDATORS)
END_CATEGORY_MAP()

BEGIN_PROP_MAP(CRegExprInputValidator)
	PROP_PAGE(CLSID_RegExprIVPP)
END_PROP_MAP()

public:
// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pbstrComponentDescription);

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IRegExprInputValidator
	STDMETHOD(SetInputType)(/*[in]*/ BSTR strInputTypeName);
	STDMETHOD(GetInputType)(/*[out, retval]*/ BSTR* strInputTypeName);
	STDMETHOD(get_IgnoreCase)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IgnoreCase)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_Pattern)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_Pattern)(/*[in]*/ BSTR newVal);

// IInputValidator
	STDMETHOD(raw_ValidateInput)(ITextInput * pTextInput, VARIANT_BOOL * pbSuccessful);
	STDMETHOD(raw_GetInputType)(BSTR * pstrInputType);

// ICopyableObject
	STDMETHOD(raw_Clone)(/*[out, retval]*/ IUnknown* *pObject);
	STDMETHOD(raw_CopyFrom)(/*[in]*/ IUnknown *pObject);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbValue);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ITestableComponent
	STDMETHOD(raw_RunAutomatedTests)(IVariantVector * pParams, BSTR strTCLFile);
	STDMETHOD(raw_RunInteractiveTests)();
	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(raw_SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);

protected:

	//=======================================================================
	// PURPOSE: Runs automated tests of pattern and case sensitivity.
	// REQUIRE: None.
	// PROMISE: None
	// ARGS:	None
	void	doTest1();

private:
	// Functions
	//----------------------------------------------------------------------------------------------
	UCLID_REGEXPRIVLib::IRegExprInputValidatorPtr getThisAsCOMPtr();
	//----------------------------------------------------------------------------------------------
	// Gets a regular expression parser with the specified pattern and case sensitivity settings
	IRegularExprParserPtr getParser();
	//----------------------------------------------------------------------------------------------

	void validateLicense();

	string m_strInputTypeName;

	string m_strPattern;

	bool m_bIgnoreCase;

	// Pointer to test result logger that stores and displays test results
	ITestResultLoggerPtr		m_ipLogger;

	IMiscUtilsPtr m_ipMiscUtils;

	// flag to keep track of whether this object has changed
	// since the last save-to-stream operation
	bool m_bDirty;
};
