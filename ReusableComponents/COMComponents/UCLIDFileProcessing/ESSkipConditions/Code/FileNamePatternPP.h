// FileNamePatternPP.h : Declaration of the CFileNamePatternPP

#pragma once

#include "resource.h"       // main symbols

#include <ImageButtonWithStyle.h>
#include <string>

EXTERN_C const CLSID CLSID_FileNamePatternPP;

/////////////////////////////////////////////////////////////////////////////
// CFileNamePatternPP
class ATL_NO_VTABLE CFileNamePatternPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CFileNamePatternPP, &CLSID_FileNamePatternPP>,
	public IPropertyPageImpl<CFileNamePatternPP>,
	public CDialogImpl<CFileNamePatternPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CFileNamePatternPP();

	enum {IDD = IDD_FILENAMEPATTERNPP};

DECLARE_REGISTRY_RESOURCEID(IDR_FILENAMEPATTERNPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CFileNamePatternPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CFileNamePatternPP)
	COMMAND_HANDLER(IDC_BTN_SELECT_DOC_TAG2, BN_CLICKED, OnBnClickedBtnSelectDocTag2)
	COMMAND_HANDLER(IDC_CHK_REG_EXP_CASE, BN_CLICKED, OnBnClickedChkRegExpCase)
	COMMAND_HANDLER(IDC_RADIO_FILE, BN_CLICKED, OnBnClickedRadioFile)
	COMMAND_HANDLER(IDC_RADIO_TEXT, BN_CLICKED, OnBnClickedRadioText)
	COMMAND_HANDLER(IDC_BTN_SELECT_DOC_TAG, BN_CLICKED, OnBnClickedBtnSelectDocTag)
	COMMAND_HANDLER(IDC_BTN_BROWSE_FILE, BN_CLICKED, OnBnClickedBtnBrowseFile)
	COMMAND_HANDLER(IDC_EDIT_PATTERN, EN_CHANGE, OnEnChangeEditPattern)
	CHAIN_MSG_MAP(IPropertyPageImpl<CFileNamePatternPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	// REFLECT_NOTIFICATIONS needed by ImageButtonWithSytle
	REFLECT_NOTIFICATIONS()
END_MSG_MAP()

// IProperty Page
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnBnClickedBtnSelectDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedBtnBrowseFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedBtnSelectDocTag2(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedChkRegExpCase(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedRadioFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedRadioText(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnEnChangeEditPattern(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	///////////
	// Variables
	////////////
	ATLControls::CEdit m_FileName;
	CImageButtonWithStyle m_btnSelectDocTag;
	ATLControls::CComboBox m_cmbDoes;
	ATLControls::CComboBox m_cmbContain;
	ATLControls::CEdit m_RegFileName;
	CImageButtonWithStyle m_btnSelectDocTag2;
	ATLControls::CButton m_btnBrowse;
	ATLControls::CEdit m_RegPattern;
	ATLControls::CButton m_radioPatternText;
	ATLControls::CButton m_btnCaseSensitive;

	bool m_bIsRegExpFromFile;

	///////////
	// Methods
	// Save the file FAM condition
	bool saveFileFAMCondition(EXTRACT_FAMCONDITIONSLib::IFileNamePatternFAMConditionPtr ipFileNamePattern);
	void updateControls();

	// ensure that this component is licensed
	void validateLicense();
};
