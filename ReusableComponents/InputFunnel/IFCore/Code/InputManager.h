// InputManager.h : Declaration of the CInputManager

#pragma once

#include "resource.h"       // main symbols
#include "ConfigMgrIF.h"
#include <atlctl.h>
#include "IFCoreCP.h"
#include "WindowIRManager.h"

#include <map>
#include <string>
#include <vector>

#include <memory>

class InputRequestInfo
{
public:
	InputRequestInfo();
	~InputRequestInfo();
	void clear();
	void set(UCLID_INPUTFUNNELLib::IInputValidatorPtr ipInputValidator, const std::string& strPrompt);

	UCLID_INPUTFUNNELLib::IInputValidatorPtr m_ipInputValidator;
	std::string m_strPrompt;
};

/////////////////////////////////////////////////////////////////////////////
// CInputManager
class ATL_NO_VTABLE CInputManager : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public IDispatchImpl<IInputManager, &IID_IInputManager, &LIBID_UCLID_INPUTFUNNELLib>,
	public CComControl<CInputManager>,
	public IPersistStreamInitImpl<CInputManager>,
	public IOleControlImpl<CInputManager>,
	public IOleObjectImpl<CInputManager>,
	public IOleInPlaceActiveObjectImpl<CInputManager>,
	public IViewObjectExImpl<CInputManager>,
	public IOleInPlaceObjectWindowlessImpl<CInputManager>,
	public ISupportErrorInfo,
	public IConnectionPointContainerImpl<CInputManager>,
	public IPersistStorageImpl<CInputManager>,
	public ISpecifyPropertyPagesImpl<CInputManager>,
	public IQuickActivateImpl<CInputManager>,
	public IDataObjectImpl<CInputManager>,
	public IProvideClassInfo2Impl<&CLSID_InputManager, &DIID__IInputManagerEvents, &LIBID_UCLID_INPUTFUNNELLib>,
	public IPropertyNotifySinkCP<CInputManager>,
	public CComCoClass<CInputManager, &CLSID_InputManager>,
	public IDispatchImpl<IIREventHandler, &IID_IIREventHandler, &LIBID_UCLID_INPUTFUNNELLib>,
	public CProxy_IInputManagerEvents< CInputManager >,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	STDMETHOD(GetHWND)(/*[out, retval]*/ long *phWnd);
	CInputManager();
	~CInputManager();

DECLARE_REGISTRY_RESOURCEID(IDR_INPUTMANAGER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CInputManager)
	COM_INTERFACE_ENTRY(IInputManager)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(IViewObjectEx)
	COM_INTERFACE_ENTRY(IViewObject2)
	COM_INTERFACE_ENTRY(IViewObject)
	COM_INTERFACE_ENTRY(IOleInPlaceObjectWindowless)
	COM_INTERFACE_ENTRY(IOleInPlaceObject)
	COM_INTERFACE_ENTRY2(IOleWindow, IOleInPlaceObjectWindowless)
	COM_INTERFACE_ENTRY(IOleInPlaceActiveObject)
	COM_INTERFACE_ENTRY(IOleControl)
	COM_INTERFACE_ENTRY(IOleObject)
	COM_INTERFACE_ENTRY(IPersistStreamInit)
	COM_INTERFACE_ENTRY2(IPersist, IPersistStreamInit)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IConnectionPointContainer)
	COM_INTERFACE_ENTRY(ISpecifyPropertyPages)
	COM_INTERFACE_ENTRY(IQuickActivate)
	COM_INTERFACE_ENTRY(IPersistStorage)
	COM_INTERFACE_ENTRY(IDataObject)
	COM_INTERFACE_ENTRY(IProvideClassInfo)
	COM_INTERFACE_ENTRY(IProvideClassInfo2)
	COM_INTERFACE_ENTRY2(IDispatch, IInputManager)
	COM_INTERFACE_ENTRY(IIREventHandler)
	COM_INTERFACE_ENTRY_IMPL(IConnectionPointContainer)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_PROP_MAP(CInputManager)
	PROP_DATA_ENTRY("_cx", m_sizeExtent.cx, VT_UI4)
	PROP_DATA_ENTRY("_cy", m_sizeExtent.cy, VT_UI4)
	// Example entries
	// PROP_ENTRY("Property Description", dispid, clsid)
	// PROP_PAGE(CLSID_StockColorPage)
END_PROP_MAP()

BEGIN_CONNECTION_POINT_MAP(CInputManager)
	CONNECTION_POINT_ENTRY(IID_IPropertyNotifySink)
	CONNECTION_POINT_ENTRY(DIID__IInputManagerEvents)
END_CONNECTION_POINT_MAP()

BEGIN_MSG_MAP(CInputManager)
	CHAIN_MSG_MAP(CComControl<CInputManager>)
	DEFAULT_REFLECTION_HANDLER()
END_MSG_MAP()
// Handler prototypes:
//  LRESULT MessageHandler(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
//  LRESULT CommandHandler(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
//  LRESULT NotifyHandler(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);



// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid)
	{
		static const IID* arr[] = 
		{
			&IID_IInputManager,
			&IID_IIREventHandler,
			&IID_ILicensedComponent
		};
		for (int i=0; i<sizeof(arr)/sizeof(arr[0]); i++)
		{
			if (InlineIsEqualGUID(*arr[i], riid))
				return S_OK;
		}
		return S_FALSE;
	}

// IViewObjectEx
	DECLARE_VIEW_STATUS(0)

// IInputManager
	STDMETHOD(Destroy)();
	STDMETHOD(ShowWindows)(/*[in]*/ VARIANT_BOOL bShow);
	STDMETHOD(get_ParentWndHandle)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_ParentWndHandle)(/*[in]*/ long newVal);
	STDMETHOD(GetInputReceiver)(/*[in]*/ long nIRHandle, /*[out, retval]*/ IInputReceiver **pReceiver);
	STDMETHOD(DisconnectInputReceiver)(/*[in]*/ long nIRHandle);
	STDMETHOD(ConnectInputReceiver)(/*[in]*/ IInputReceiver* pInputReceiver, /*[out, retval]*/ long *pnIRHandle);
	STDMETHOD(CreateNewInputReceiver)(/*[in]*/ BSTR strInputReceiverName, /*[out, retval]*/ long *pnIRHandle);
	STDMETHOD(DisableInput)();
	STDMETHOD(EnableInput2)(/*[in]*/ IInputValidator* pInputValidator, /*[in]*/ BSTR strPrompt, IInputContext* pInputContext);
	STDMETHOD(EnableInput1)(/*[in]*/ BSTR strInputValidatorName, /*[in]*/ BSTR strPrompt, IInputContext* pInputContext);
	STDMETHOD(get_InputIsEnabled)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(get_WindowsShown)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(ProcessTextInput)(/*[in]*/ BSTR strInput);
	STDMETHOD(GetOCRFilterMgr)(/*[out, retval]*/ IOCRFilterMgr **pOCRFilterMgr);
	STDMETHOD(SetOCREngine)(/*[in]*/ IOCREngine *pEngine);
	STDMETHOD(SetInputContext)(/*[in]*/ IInputContext* pInputContext);
	STDMETHOD(GetInputReceivers)(/*[out, retval]*/ IIUnknownVector** pInputReceivers);

	HRESULT OnDrawAdvanced(ATL_DRAWINFO& di)
	{
		RECT& rc = *(RECT*)di.prcBounds;
		Rectangle(di.hdcDraw, rc.left, rc.top, rc.right, rc.bottom);

		SetTextAlign(di.hdcDraw, TA_CENTER|TA_BASELINE);
		LPCTSTR pszText = _T("ATL 3.0 : InputManager");
		TextOut(di.hdcDraw, 
			(rc.left + rc.right) / 2, 
			(rc.top + rc.bottom) / 2, 
			pszText, 
			lstrlen(pszText));

		return S_OK;
	}

// IIREventHandler
	STDMETHOD(NotifyInputReceived)(ITextInput * pTextInput);
	STDMETHOD(NotifyAboutToDestroy)(IInputReceiver * pInputReceiver);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:

	/////////////////////////////
	//	Member variables
	/////////////////////////////
	// the map to store Input Receivers' IDs to Input Receivers' interface pointer
	std::map<unsigned long, UCLID_INPUTFUNNELLib::IInputReceiverPtr > m_mapIDToInputReceiver;
	
	// All detected input receivers under InputReceiver category in Registry
	IStrToStrMapPtr m_ipIRDescriptionToProgIDMap;

	// All detected input validator under InputValidator category in Registry
	IStrToStrMapPtr m_ipIVDescriptionToProgIDMap;

	UCLID_INPUTFUNNELLib::IOCRFilterMgrPtr m_ipOCRFilterMgr;

	// member variable that points to the OCR engine associated with the InputFunnel system
	IOCREnginePtr m_ipOCREngine;

	UCLID_INPUTFUNNELLib::IInputContextPtr m_ipCurrentInputContext;

	// Internally stored input receiver id count
	// It starts with 1;
	unsigned long m_nCurrentIVID;
	// Internally store the state of enabling/disabling input
	bool m_bInputEnabled;
	// Whether windows are shown
	bool m_bWindowShown;

	// current input & prompt
	InputRequestInfo m_currentInputRequestInfo;

	// parent window handle
	long m_lParentWndHandle;

	ConfigMgrIF m_ConfigMgr;
	std::string m_strDefaultOCREngineProgID;

	// WindowIRManager object below is used to keep track of
	// InputReceivers from external applications, where each
	// InputReceiver is a window represented by an HWND
	std::unique_ptr<WindowIRManager> m_apWindowIRManager;

	// module handles after loading the libraries
	HMODULE m_hModuleSSOCR;
	HMODULE m_hModuleCOMUtils;

	/////////////////////////////
	//	Helper functions
	/////////////////////////////
	//----------------------------------------------------------------------------------------------
	UCLID_INPUTFUNNELLib::IInputManagerPtr getThisAsCOMPtr();
	//----------------------------------------------------------------------------------------------
	// Based on the input receiver's id to find the interface pointer to the input receiver
	bool findInputReceiver(unsigned long nID, UCLID_INPUTFUNNELLib::IInputReceiverPtr &ipInputReceiver);
	// Based on the interface pointer to the input receiver to find the input receiver's id
	bool findInputReceiverID(unsigned long &nID, const UCLID_INPUTFUNNELLib::IInputReceiverPtr &ipInputReceiver);
	// a method to lazy-initialize the WindowIRManager pointer
	WindowIRManager* CInputManager::getWindowIRManager();

	// load library of UCLIDCONUtils.dll once
	void loadCOMUtilsDll();

	// check license
	void validateLicense();
};

