// NumberInputValidator.h : Declaration of the CNumberInputValidator

#ifndef __NUMBERINPUTVALIDATOR_H_
#define __NUMBERINPUTVALIDATOR_H_

#include "resource.h"       // main symbols
#import "..\..\..\..\..\..\ReusableComponents\COMComponents\UCLIDComponentCategoryMgmt\Code\UCLIDComponentCategoryMgmt.tlb" raw_interfaces_only, raw_native_types, no_namespace, named_guids 

/////////////////////////////////////////////////////////////////////////////
// CNumberInputValidator
class ATL_NO_VTABLE CNumberInputValidator : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CNumberInputValidator, &CLSID_NumberInputValidator>,
	public IDispatchImpl<IInputValidator, &IID_IInputValidator, &LIBID_UCLID_INPUTFUNNELLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLIDCOMPONENTCATEGORYMGMTLib>
{
public:
	CNumberInputValidator();
	~CNumberInputValidator();

DECLARE_REGISTRY_RESOURCEID(IDR_NUMBERINPUTVALIDATOR)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CNumberInputValidator)
	COM_INTERFACE_ENTRY(IInputValidator)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
END_COM_MAP()

public:
// IInputValidator
	STDMETHOD(ValidateInput)(ITextInput * pTextInput, VARIANT_BOOL * pbSuccessful);
	STDMETHOD(GetInputType)(BSTR * pstrInputType);
	
// ICategorizedComponent
	STDMETHOD(getComponentDescription)(BSTR * pbstrComponentDescription)
	{
		*pbstrComponentDescription = CComBSTR("Number Input Validator");	
		return S_OK;
	}
};

#endif //__NUMBERINPUTVALIDATOR_H_
