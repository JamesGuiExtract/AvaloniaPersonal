// MathematicalConditionPP.h : Declaration of the CMathematicalConditionPP

#pragma once

#include "resource.h"       // main symbols

#include <string>

EXTERN_C const CLSID CLSID_MathematicalConditionPP;

/////////////////////////////////////////////////////////////////////////////
// CMathematicalConditionPP
class ATL_NO_VTABLE CMathematicalConditionPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CMathematicalConditionPP, &CLSID_MathematicalConditionPP>,
	public IPropertyPageImpl<CMathematicalConditionPP>,
	public CDialogImpl<CMathematicalConditionPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CMathematicalConditionPP();

	enum {IDD = IDD_MATHCONDITIONPP};

DECLARE_REGISTRY_RESOURCEID(IDR_MATHCONDITIONPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CMathematicalConditionPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CMathematicalConditionPP)
	COMMAND_HANDLER(IDC_MATH_RADIO_RANDOM, BN_CLICKED, OnBnClickedRadio)
	COMMAND_HANDLER(IDC_MATH_RADIO_ONCE_EVERY, BN_CLICKED, OnBnClickedRadio)
	COMMAND_HANDLER(IDC_MATH_RADIO_MODULUS, BN_CLICKED, OnBnClickedRadio)
	CHAIN_MSG_MAP(IPropertyPageImpl<CMathematicalConditionPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
END_MSG_MAP()

// IProperty Page
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnBnClickedRadio(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	///////////
	// Variables
	////////////
	ATLControls::CComboBox m_cmbConsiderCondition;
	ATLControls::CButton m_radioRandom;
	ATLControls::CButton m_radioOnceEvery;
	ATLControls::CButton m_radioModulus;
	ATLControls::CEdit m_editRandomPercent;
	ATLControls::CEdit m_editOnceEvery;
	ATLControls::CEdit m_editModulus;
	ATLControls::CEdit m_editModEquals;

	///////////
	// Methods
	// Update the UI Controls
	void updateControls();

	// ensure that this component is licensed
	void validateLicense();
};