// ExtractLinePP.h : Declaration of the CExtractLinePP

#pragma once

#include "resource.h"       // main symbols

#include <string>

#include <XInfoTip.h>

EXTERN_C const CLSID CLSID_ExtractLinePP;

/////////////////////////////////////////////////////////////////////////////
// CExtractLinePP
class ATL_NO_VTABLE CExtractLinePP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CExtractLinePP, &CLSID_ExtractLinePP>,
	public IPropertyPageImpl<CExtractLinePP>,
	public CDialogImpl<CExtractLinePP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CExtractLinePP();

	enum {IDD = IDD_EXTRACTLINEPP};

DECLARE_REGISTRY_RESOURCEID(IDR_EXTRACTLINEPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CExtractLinePP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CExtractLinePP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CExtractLinePP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_RADIO_EXTRACT_LINES, BN_CLICKED, OnClickedRadioExtractLineWithBreaks)
	COMMAND_HANDLER(IDC_RADIO_EXTRACT_LINES2, BN_CLICKED, OnClickedRadioExtractLineWithoutBreaks)
	COMMAND_HANDLER(IDC_RADIO_UNIQUE, BN_CLICKED, OnClickedRadioUnique)
	COMMAND_HANDLER(IDC_LINE_INFO, BN_CLICKED, OnClickedLineInfo)
END_MSG_MAP()

// IPropertyPage
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedRadioExtractLineWithBreaks(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioExtractLineWithoutBreaks(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioUnique(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedLineInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	// this one includes line breaks
	ATLControls::CEdit m_editLineNumbers1;
	// this one does not include line breaks
	ATLControls::CEdit m_editLineNumbers2;

	ATLControls::CStatic m_picLineInfo;

	CXInfoTip m_infoTip;

	//////////
	// Methods
	//////////
	bool storeLineNumbers1(UCLID_AFVALUEFINDERSLib::IExtractLinePtr ipExtractLine);
	bool storeLineNumbers2(UCLID_AFVALUEFINDERSLib::IExtractLinePtr ipExtractLine);

	// ensure that this component is licensed
	void validateLicense();
};
