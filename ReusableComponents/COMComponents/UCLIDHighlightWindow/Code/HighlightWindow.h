
#pragma once

#include <memory>
class MFCHighlightWindow;

/////////////////////////////////////////////////////////////////////////////
// CHighlightWindow
class ATL_NO_VTABLE CHighlightWindow : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CHighlightWindow, &CLSID_HighlightWindow>,
	public ISupportErrorInfo,
	public IDispatchImpl<IHighlightWindow, &IID_IHighlightWindow, &LIBID_UCLID_HIGHLIGHTWINDOWLib>
{
public:
	CHighlightWindow();
	~CHighlightWindow();

DECLARE_CLASSFACTORY_SINGLETON(CHighlightWindow)

DECLARE_REGISTRY_RESOURCEID(IDR_HIGHLIGHTWINDOW)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CHighlightWindow)
	COM_INTERFACE_ENTRY(IHighlightWindow)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IHighlightWindow
	STDMETHOD(Show)(long hWndParent, long hWndChild);
	STDMETHOD(HideAndForget)();
	STDMETHOD(HideAndRemember)();
	STDMETHOD(Refresh)();
	STDMETHOD(SetDefaultColor)(/*[in]*/ OLE_COLOR color);
	STDMETHOD(SetAlternateColor1)(/*[in]*/ OLE_COLOR color);
	STDMETHOD(UseDefaultColor)();
	STDMETHOD(UseAlternateColor1)();
	STDMETHOD(put_ParentWndHandle)(/*[in]*/ long newVal);

private:
	///////////////////
	// Member variables
	///////////////////
	std::auto_ptr<MFCHighlightWindow> m_apDlg;
	COLORREF m_DefaultColor;
	COLORREF m_AlternateColor1;
	long m_lParentWndHandle;

	//////////////////
	// Helper functions
	//////////////////
	MFCHighlightWindow* getHighlightWindow();
};
