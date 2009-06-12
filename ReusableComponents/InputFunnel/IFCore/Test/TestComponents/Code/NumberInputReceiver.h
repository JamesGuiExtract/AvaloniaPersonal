// NumberInputReceiver.h : Declaration of the CNumberInputReceiver

#pragma once

#include "resource.h"       // main symbols

#include <map>
#import "..\..\..\..\..\..\ReusableComponents\COMComponents\UCLIDComponentCategoryMgmt\Code\UCLIDComponentCategoryMgmt.tlb" raw_interfaces_only, raw_native_types, no_namespace, named_guids 


class NumberInputDlg;
/////////////////////////////////////////////////////////////////////////////
// CNumberInputReceiver
class ATL_NO_VTABLE CNumberInputReceiver : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CNumberInputReceiver, &CLSID_NumberInputReceiver>,
	public IDispatchImpl<INumberInputReceiver, &IID_INumberInputReceiver, &LIBID_TESTCOMPONENTSLib>,
	public IDispatchImpl<IInputEntityManager, &IID_IInputEntityManager, &LIBID_UCLID_INPUTFUNNELLib>,
	public IDispatchImpl<IInputReceiver, &IID_IInputReceiver, &LIBID_UCLID_INPUTFUNNELLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLIDCOMPONENTCATEGORYMGMTLib>
{
public:
	CNumberInputReceiver();
	~CNumberInputReceiver();

DECLARE_REGISTRY_RESOURCEID(IDR_NUMBERINPUTRECEIVER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CNumberInputReceiver)
	COM_INTERFACE_ENTRY(INumberInputReceiver)
	COM_INTERFACE_ENTRY(IInputEntityManager)
	COM_INTERFACE_ENTRY2(IDispatch, IInputEntityManager)
	COM_INTERFACE_ENTRY(IInputReceiver)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
END_COM_MAP()

public:
// IInputEntityManager
	STDMETHOD(Delete)(BSTR strID);
	STDMETHOD(SetText)(BSTR strID, BSTR strText);
	STDMETHOD(GetText)(BSTR strID, BSTR * pstrText);
	STDMETHOD(CanBeMarkedAsUsed)(BSTR strID, VARIANT_BOOL * pbCanBeMarkedAsUsed);
	STDMETHOD(MarkAsUsed)(BSTR strID, VARIANT_BOOL bValue);
	STDMETHOD(IsMarkedAsUsed)(BSTR strID, VARIANT_BOOL * pbIsMarkedAsUsed);
	STDMETHOD(IsFromPersistentSource)(BSTR strID, VARIANT_BOOL * pbIsFromPersistentSource);
	STDMETHOD(GetPersistentSourceName)(BSTR strID, BSTR * pstrSourceName);
	STDMETHOD(HasBeenOCRed)(BSTR strID, VARIANT_BOOL * pbHasBeenOCRed);

// IInputReceiver
	STDMETHOD(get_WindowShown)(VARIANT_BOOL * pVal);
	STDMETHOD(get_InputIsEnabled)(VARIANT_BOOL * pVal);
	STDMETHOD(get_HasWindow)(VARIANT_BOOL * pVal);
	STDMETHOD(get_WindowHandle)(LONG * pVal);
	STDMETHOD(EnableInput)(BSTR strInputType, BSTR strPrompt);
	STDMETHOD(DisableInput)();
	STDMETHOD(SetEventHandler)(IIREventHandler * pEventHandler);
	STDMETHOD(ShowWindow)(VARIANT_BOOL bShow);

// INumberInputReceiver
	STDMETHOD(OnAboutToDestroy)();
	STDMETHOD(OnInputReceived)(/*[in]*/ BSTR bstrTextInput);
	STDMETHOD(CreateNewInputReceiver)(/*[in]*/ IInputReceiver *ipInputReceiver);

// ICategorizedComponent
	STDMETHOD(getComponentDescription)(BSTR * pbstrComponentDescription)
	{
		if (pbstrComponentDescription == NULL)
			return E_POINTER;
			
		*pbstrComponentDescription = CComBSTR("Number Input Receiver");

		return S_OK;
	}
private:
	NumberInputDlg* m_pNumberInputDlg;

	CComPtr<IIREventHandler> m_ipEventHandler;
	bool m_bWindowShown;
	bool m_bInputEnabled;

	NumberInputDlg* getNumberInputDlg();
};

