// AFEngineFileProcessorPP.h : Declaration of the CAFEngineFileProcessorPP

#pragma once

#include "resource.h"       // main symbols

#include <ImageButtonWithStyle.h>

EXTERN_C const CLSID CLSID_AFEngineFileProcessorPP;

/////////////////////////////////////////////////////////////////////////////
// CAFEngineFileProcessorPP
class ATL_NO_VTABLE CAFEngineFileProcessorPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CAFEngineFileProcessorPP, &CLSID_AFEngineFileProcessorPP>,
	public IPropertyPageImpl<CAFEngineFileProcessorPP>,
	public CDialogImpl<CAFEngineFileProcessorPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CAFEngineFileProcessorPP();

	enum {IDD = IDD_AFENGINEFILEPROCESSORPP};

	DECLARE_REGISTRY_RESOURCEID(IDR_AFENGINEFILEPROCESSORPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CAFEngineFileProcessorPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CAFEngineFileProcessorPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CAFEngineFileProcessorPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_BTN_BROWSE_AFE, BN_CLICKED, OnClickedBtnBrowse)
	COMMAND_HANDLER(IDC_RADIO_OCR_ALL, BN_CLICKED, OnClickedRadioAllPages)
	COMMAND_HANDLER(IDC_RADIO_OCR_SPECIFIED, BN_CLICKED, OnClickedRadioSpecificPages)
	COMMAND_HANDLER(IDC_RADIO_OCR_NONE, BN_CLICKED, OnClickedRadioOcrNone)
	COMMAND_HANDLER(IDC_BTN_DOCTAGS_AFE, BN_CLICKED, OnClickedBtnRulesFileDocTags)
	COMMAND_HANDLER(IDC_BTN_DOCTAGS_DATA_INPUT, BN_CLICKED, OnClickedBtnDataInputDocTags)
	COMMAND_HANDLER(IDC_BTN_BROWSE_DATA_INPUT, BN_CLICKED, OnClickedBtnBrowseDataInput)
	COMMAND_HANDLER(IDC_CHK_PROVIDE_INPUT, BN_CLICKED, OnClickedCheckUseDataInput)
	// REFLECT_NOTIFICATIONS needed by ImageButtonWithSytle
	REFLECT_NOTIFICATIONS()
END_MSG_MAP()

// IPropertyPage
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedBtnBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnRulesFileDocTags(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioAllPages(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioSpecificPages(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioOcrNone(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnDataInputDocTags(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnBrowseDataInput(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedCheckUseDataInput(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:

	/////////////
	// Variables
	/////////////
	ATLControls::CEdit m_editRuleFileName;
	CImageButtonWithStyle m_btnRuleFileSelectTag;
	ATLControls::CButton m_chkReadUSS;
	ATLControls::CButton m_chkSaveOcrResults;
	ATLControls::CButton m_chkUseCleanedImage;
	ATLControls::CButton m_radioAllPages;
	ATLControls::CButton m_radioSpecificPages;
	ATLControls::CButton m_radioOcrNone;
	ATLControls::CEdit m_editSpecificPages;
	ATLControls::CEdit m_editDataInputFileName;
	ATLControls::CButton m_chkUseDataInputFile;
	CImageButtonWithStyle m_btnDataInputSelectTag;
	ATLControls::CButton m_radioNoParallel;
	ATLControls::CButton m_radioPoliteParallel;
	ATLControls::CButton m_radioGreedyParallel;

	///////////
	// Methods
	///////////
	// ensure that this component is licensed
	void validateLicense();
};
