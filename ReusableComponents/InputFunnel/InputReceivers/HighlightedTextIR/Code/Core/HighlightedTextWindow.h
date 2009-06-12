//============================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	HighlightedTextWindow.h
//
// PURPOSE:	Declaration of HighlightedTextWindow class
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//============================================================================

#pragma once

#include "resource.h"       // main symbols

#include "..\..\..\..\IFCore\Code\IFCategories.h"

#include <string>
#include <memory>

// forward declarations
class HighlightedTextDlg;

/////////////////////////////////////////////////////////////////////////////
// CHighlightedTextWindow
class ATL_NO_VTABLE CHighlightedTextWindow : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CHighlightedTextWindow, &CLSID_HighlightedTextWindow>,
	public ISupportErrorInfo,
	public IDispatchImpl<IHighlightedTextWindow, &IID_IHighlightedTextWindow, &LIBID_UCLID_HIGHLIGHTEDTEXTIRLib>,
	public IDispatchImpl<IInputEntityManager, &IID_IInputEntityManager, &LIBID_UCLID_INPUTFUNNELLib>,
	public IDispatchImpl<IInputReceiver, &IID_IInputReceiver, &LIBID_UCLID_INPUTFUNNELLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CHighlightedTextWindow();
	~CHighlightedTextWindow();

DECLARE_REGISTRY_RESOURCEID(IDR_HIGHLIGHTEDTEXTWINDOW)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CHighlightedTextWindow)
	COM_INTERFACE_ENTRY(IHighlightedTextWindow)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IHighlightedTextWindow)
	COM_INTERFACE_ENTRY(IInputEntityManager)
	COM_INTERFACE_ENTRY(IInputReceiver)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_CATEGORY_MAP(CHighlightedTextWindow)
	IMPLEMENTED_CATEGORY(CATID_INPUTFUNNEL_INPUT_RECEIVERS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IHighlightedTextWindow
	STDMETHOD(SetText)(/*[in]*/ BSTR strText);
	STDMETHOD(Clear)();
	STDMETHOD(Save)();
	STDMETHOD(SaveAs)(/*[in]*/ BSTR strFilename);
	STDMETHOD(Open)(/*[in]*/ BSTR strFilename);
	STDMETHOD(SetInputFinder)(/*[in]*/ BSTR strName);
	STDMETHOD(IsModified)(/*[out, retval]*/ VARIANT_BOOL *pbIsModified);
	STDMETHOD(GetText)(/*[out, retval]*/ BSTR *pstrText);
	STDMETHOD(GetInputFinderName)(/*[out, retval]*/ BSTR *pstrName);
	STDMETHOD(GetFileName)(/*[out, retval]*/ BSTR *pstrFileName);
	STDMETHOD(SetIndirectSource)(BSTR strIndirectSourceName);
	STDMETHOD(ShowOpenDialogBox)();
	STDMETHOD(AddInputFinder)(/*[in]*/ BSTR strInputFinderName, /*[in]*/ IInputFinder* pInputFinder);
	STDMETHOD(ClearTextProcessors)();
	STDMETHOD(SetTextProcessors)(/*[in]*/ IIUnknownVector *pvecTextProcessors);

// IInputEntityManager
	STDMETHOD(raw_CanBeDeleted)(BSTR strID, VARIANT_BOOL * pbCanBeDeleted);
	STDMETHOD(raw_Delete)(BSTR strID);
	STDMETHOD(raw_SetText)(BSTR strID, BSTR strText);
	STDMETHOD(raw_GetText)(BSTR strID, BSTR * pstrText);
	STDMETHOD(raw_CanBeMarkedAsUsed)(BSTR strID, VARIANT_BOOL * pbCanBeMarkedAsUsed);
	STDMETHOD(raw_MarkAsUsed)(BSTR strID, VARIANT_BOOL bValue);
	STDMETHOD(raw_IsMarkedAsUsed)(BSTR strID, VARIANT_BOOL * pbIsMarkedAsUsed);
	STDMETHOD(raw_IsFromPersistentSource)(BSTR strID, VARIANT_BOOL * pbIsFromPersistentSource);
	STDMETHOD(raw_GetPersistentSourceName)(BSTR strID, BSTR * pstrSourceName);
	STDMETHOD(raw_HasBeenOCRed)(BSTR strID, VARIANT_BOOL * pbHasBeenOCRed);
	STDMETHOD(raw_GetOCRImage)(/*[in]*/BSTR strID, /*[out, retval]*/BSTR* pbstrImageFileName);
	STDMETHOD(raw_HasIndirectSource)(BSTR strID, VARIANT_BOOL *pbHasIndirectSource);
	STDMETHOD(raw_GetIndirectSource)(BSTR strID, BSTR *pstrIndirectSourceName);
	STDMETHOD(raw_GetOCRZones)(BSTR strID, IIUnknownVector **pRasterZones);

// IInputReceiver
	STDMETHOD(raw_get_ParentWndHandle)(/*[out, retval]*/ long *pVal);
	STDMETHOD(raw_put_ParentWndHandle)(/*[in]*/ long newVal);
	STDMETHOD(raw_get_WindowShown)(VARIANT_BOOL * pVal);
	STDMETHOD(raw_get_InputIsEnabled)(VARIANT_BOOL * pVal);
	STDMETHOD(raw_get_HasWindow)(VARIANT_BOOL * pVal);
	STDMETHOD(raw_get_WindowHandle)(LONG * pVal);
	STDMETHOD(raw_EnableInput)(BSTR strInputType, BSTR strPrompt);
	STDMETHOD(raw_DisableInput)();
	STDMETHOD(raw_SetEventHandler)(IIREventHandler * pEventHandler);
	STDMETHOD(raw_ShowWindow)(VARIANT_BOOL bShow);
	STDMETHOD(raw_get_UsesOCR)(VARIANT_BOOL *pVal);
	STDMETHOD(raw_SetOCRFilter)(IOCRFilter *pFilter);
	STDMETHOD(raw_SetOCREngine)(IOCREngine *pEngine);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pbstrComponentDescription);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

protected:

	void	validateLicense();

	HighlightedTextDlg* getHighlightedTextDlg();

private:

	std::auto_ptr<HighlightedTextDlg> m_apDlg;

	long m_lParentWndHandle;
};
