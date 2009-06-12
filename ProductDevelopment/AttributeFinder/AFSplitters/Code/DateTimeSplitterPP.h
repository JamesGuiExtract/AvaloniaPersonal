// DateTimeSplitterPP.h : Declaration of the CDateTimeSplitterPP

#pragma once

#include "resource.h"       // main symbols

#include <XInfoTip.h>

#include <string>

using namespace std;

EXTERN_C const CLSID CLSID_DateTimeSplitterPP;

/////////////////////////////////////////////////////////////////////////////
// CDateTimeSplitterPP
class ATL_NO_VTABLE CDateTimeSplitterPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CDateTimeSplitterPP, &CLSID_DateTimeSplitterPP>,
	public IPropertyPageImpl<CDateTimeSplitterPP>,
	public CDialogImpl<CDateTimeSplitterPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CDateTimeSplitterPP();

	enum {IDD = IDD_DATETIMESPLITTERPP};

DECLARE_REGISTRY_RESOURCEID(IDR_DATETIMESPLITTERPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CDateTimeSplitterPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CDateTimeSplitterPP)
	COMMAND_HANDLER(IDC_RADIO_TWO_DIGIT_YEAR_SPECIFIED, BN_CLICKED, OnBnClickedRadioTwoDigitYearSpecified)
	COMMAND_HANDLER(IDC_RADIO_TWO_DIGIT_YEAR_CURRENT, BN_CLICKED, OnBnClickedRadioTwoDigitYearCurrent)
	CHAIN_MSG_MAP(IPropertyPageImpl<CDateTimeSplitterPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_CHECK_SHOWFORMATTED, BN_CLICKED, OnClickedCheckShowFormatted)
	COMMAND_HANDLER(IDC_BTN_TESTFORMAT, BN_CLICKED, OnClickedButtonTest)
	COMMAND_HANDLER(IDC_FORMAT_INFO, BN_CLICKED, OnClickedFormatInfo)
END_MSG_MAP()

// IPropertyPage
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedCheckShowFormatted(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedButtonTest(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedFormatInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedRadioTwoDigitYearSpecified(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedRadioTwoDigitYearCurrent(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	// Controls in the property page
	ATLControls::CButton m_btnMonthNumber;
	ATLControls::CButton m_btnMonthName;
	ATLControls::CButton m_btnYearFour;
	ATLControls::CButton m_btnYearTwo;
	ATLControls::CButton m_btnTimeNormal;
	ATLControls::CButton m_btnTimeMilitary;
	ATLControls::CButton m_btnDayOfWeek;
	ATLControls::CButton m_btnShowFormatted;
	ATLControls::CButton m_btnSplitDefaults;
	ATLControls::CEdit m_editFormat;
	ATLControls::CEdit m_editOutput;
	ATLControls::CButton m_btnTwoDigitYearSpecified;
	ATLControls::CButton m_btnTwoDigitYearCurrent;
	ATLControls::CEdit m_editMinimumTwoDigitYear;

	// Tooltip
	CXInfoTip m_infoTip;

	// Exercises Format string with current system time
	// Returns formatted string or empty string
	string	formatCurrentTime();

	// Returns the minimum two digit year from the associated edit control; otherwise returns -1.
	long getMinimumTwoDigitYear();

	// Updates the enabled state of the two digit year edit box
	void updateTwoDigitYearEditBox();

	// Ensure that this component is licensed
	void	validateLicense();
};
