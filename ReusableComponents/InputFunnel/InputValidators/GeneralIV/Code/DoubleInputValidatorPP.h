// DoubleInputValidatorPP.h : Declaration of the CDoubleInputValidatorPP

#pragma once

#include "resource.h"       // main symbols

EXTERN_C const CLSID CLSID_DoubleInputValidatorPP;

/////////////////////////////////////////////////////////////////////////////
// CDoubleInputValidatorPP
class ATL_NO_VTABLE CDoubleInputValidatorPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CDoubleInputValidatorPP, &CLSID_DoubleInputValidatorPP>,
	public IPropertyPageImpl<CDoubleInputValidatorPP>,
	public CDialogImpl<CDoubleInputValidatorPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CDoubleInputValidatorPP();

	enum {IDD = IDD_DOUBLEINPUTVALIDATORPP};

DECLARE_REGISTRY_RESOURCEID(IDR_DOUBLEINPUTVALIDATORPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CDoubleInputValidatorPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CDoubleInputValidatorPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CDoubleInputValidatorPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_CHECK_HAS_MIN, BN_CLICKED, OnClickedCheckHasMinimum)
	COMMAND_HANDLER(IDC_CHECK_HAS_MAX, BN_CLICKED, OnClickedCheckHasMaximum)
	COMMAND_HANDLER(IDC_CHECK_INCLUDE_MIN, BN_CLICKED, OnClickedCheckIncludeMinimum)
	COMMAND_HANDLER(IDC_CHECK_INCLUDE_MAX, BN_CLICKED, OnClickedCheckIncludeMaximum)
	COMMAND_HANDLER(IDC_CHECK_INCLUDE_ZERO, BN_CLICKED, OnClickedCheckIncludeZero)
	COMMAND_HANDLER(IDC_CHECK_INCLUDE_NEGATIVE, BN_CLICKED, OnClickedCheckIncludeNegative)
END_MSG_MAP()
// Handler prototypes:
//  LRESULT MessageHandler(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
//  LRESULT CommandHandler(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
//  LRESULT NotifyHandler(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);

// IPropertyPage
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedCheckHasMinimum(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedCheckHasMaximum(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedCheckIncludeMinimum(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedCheckIncludeMaximum(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedCheckIncludeZero(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedCheckIncludeNegative(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	////////////
	// Variables
	////////////
	// Is a minimum or maximum limit defined?
	bool	m_bHasMinimum;
	bool	m_bHasMaximum;
	
	// Are the limiting values themselves valid?
	bool	m_bIncludeMinimum;
	bool	m_bIncludeMaximum;

	// Is zero valid?
	bool	m_bIncludeZero;

	// Are negative numbers valid?
	bool	m_bIncludeNegative;

	// Specified limits
	double	m_dMinimum;
	double	m_dMaximum;

	//////////
	// Methods
	//////////
	// Retrieves Maximum and performs basic validation
	bool	isMaximumValid();

	// Retrieves Minimum and performs basic validation
	bool	isMinimumValid();

	// Enable or disable appropriate controls based on current settings
	void	setControlStates();

	// Ensure that this component is licensed
	void	validateLicense();
};
