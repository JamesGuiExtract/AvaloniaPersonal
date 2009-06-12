// SelectPageRegionPP.h : Declaration of the CSelectPageRegionPP

#pragma once

#include "resource.h"       // main symbols

#include <XInfoTip.h>

EXTERN_C const CLSID CLSID_SelectPageRegionPP;

/////////////////////////////////////////////////////////////////////////////
// CSelectPageRegionPP
class ATL_NO_VTABLE CSelectPageRegionPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSelectPageRegionPP, &CLSID_SelectPageRegionPP>,
	public IPropertyPageImpl<CSelectPageRegionPP>,
	public CDialogImpl<CSelectPageRegionPP>
{
public:
	CSelectPageRegionPP();

	enum {IDD = IDD_SELECTPAGEREGIONPP};

DECLARE_REGISTRY_RESOURCEID(IDR_SELECTPAGEREGIONPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CSelectPageRegionPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
END_COM_MAP()

BEGIN_MSG_MAP(CSelectPageRegionPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CSelectPageRegionPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_RADIO_ALL_PAGES, BN_CLICKED, OnClickedRadioAllPages)
	COMMAND_HANDLER(IDC_RADIO_SPECIFIC_PAGE, BN_CLICKED, OnClickedRadioSpecificPages)
	COMMAND_HANDLER(IDC_CHECK_RESTRICT_HORIZON, BN_CLICKED, OnClickedChkRestrictHorizon)
	COMMAND_HANDLER(IDC_CHECK_RESTRICT_VERTICAL, BN_CLICKED, OnClickedChkRestrictVertical)
	COMMAND_HANDLER(IDC_HELP_SPECIFIC_PAGE, BN_CLICKED, OnClickedSpecificPageInfo)
	COMMAND_HANDLER(IDC_CHECK_REGEXP, BN_CLICKED, OnClickedChkRegExp)
	COMMAND_HANDLER(IDC_CHECK_CASE_SENSITIVE, BN_CLICKED, OnClickedChkCaseSensitive)
	COMMAND_HANDLER(IDC_RADIO_REGEXP_PAGE, BN_CLICKED, OnClickedRadioRegExpPages)
	COMMAND_HANDLER(IDC_CHECK_OCR, BN_CLICKED, OnClickedChkOCRRegion)
	
END_MSG_MAP()
// Handler prototypes:
//  LRESULT MessageHandler(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
//  LRESULT CommandHandler(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
//  LRESULT NotifyHandler(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);

	STDMETHOD(Apply)(void);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedRadioAllPages(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioSpecificPages(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedChkRestrictHorizon(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedChkRestrictVertical(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedSpecificPageInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedChkRegExp(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedChkCaseSensitive(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioRegExpPages(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedChkOCRRegion(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	///////////
	// Variables
	////////////

	ATLControls::CComboBox m_cmbIncludeExclude;

	ATLControls::CButton m_radioAllPages;
	ATLControls::CButton m_radioSpecificPages;
	ATLControls::CEdit m_editSpecificPages;

	ATLControls::CButton m_chkHorizontalRestriction;
	ATLControls::CEdit m_editHorizontalStart;
	ATLControls::CEdit m_editHorizontalEnd;
	ATLControls::CButton m_chkVerticalRestriction;
	ATLControls::CEdit m_editVerticalStart;
	ATLControls::CEdit m_editVerticalEnd;

	ATLControls::CComboBox m_cmbRegExpPages;
	ATLControls::CButton m_chkRegExp;
	ATLControls::CButton m_chkCaseSensitive;
	ATLControls::CButton m_radioRegExpPages;
	ATLControls::CEdit m_editRegExp;

	ATLControls::CButton m_chkOCRRegion;
	ATLControls::CEdit m_editRegionRotation;

	CXInfoTip m_infoTip;

	///////////
	// Methods
	///////////
	bool savePageSelections(UCLID_AFPREPROCESSORSLib::ISelectPageRegionPtr ipSelectPageRegion);

	bool saveRestrictions(UCLID_AFPREPROCESSORSLib::ISelectPageRegionPtr ipSelectPageRegion);

	bool saveOCRItems(UCLID_AFPREPROCESSORSLib::ISelectPageRegionPtr ipSelectPageRegion);
};
