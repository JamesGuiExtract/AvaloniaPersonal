// CharacterConfidenceConditionPP.h : Declaration of the CCharacterConfidenceConditionPP


#pragma once
#include "resource.h"       // main symbols
#include "AFConditions.h"

// CCharacterConfidenceConditionPP

class ATL_NO_VTABLE CCharacterConfidenceConditionPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CCharacterConfidenceConditionPP, &CLSID_CharacterConfidenceConditionPP>,
	public IPropertyPageImpl<CCharacterConfidenceConditionPP>,
	public CDialogImpl<CCharacterConfidenceConditionPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CCharacterConfidenceConditionPP();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();

	void FinalRelease();

	enum {IDD = IDD_CHARACTERCONFIDENCECONDITIONPP};

	DECLARE_REGISTRY_RESOURCEID(IDR_CHARACTERCONFIDENCECONDITIONPP)

	BEGIN_COM_MAP(CCharacterConfidenceConditionPP)
		COM_INTERFACE_ENTRY(IPropertyPage)
		COM_INTERFACE_ENTRY(IDispatch)
		COM_INTERFACE_ENTRY(ILicensedComponent)
	END_COM_MAP()

	BEGIN_MSG_MAP(CCharacterConfidenceConditionPP)
		COMMAND_HANDLER(IDC_CHECK_SECOND_CONDITION, BN_CLICKED, OnBnClickedCheckSecondCondition)
		MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
		CHAIN_MSG_MAP(IPropertyPageImpl<CCharacterConfidenceConditionPP>)
	END_MSG_MAP()

	// Handler prototypes:
	//  LRESULT MessageHandler(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	//  LRESULT CommandHandler(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	//  LRESULT NotifyHandler(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);

	// Windows message handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnBnClickedCheckSecondCondition(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);

	// IPropertyPage
	STDMETHOD(Apply)(void);

	// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);
private:
	// Control variables
	CComboBox m_comboMet;
	CComboBox m_comboAggregateFunction;
	CComboBox m_comboFirstCondition;
	CEdit m_editFirstScore;
	CButton m_checkSecondCondition;
	CComboBox m_comboAndOr;
	CComboBox m_comboSecondCondition;
	CEdit m_editSecondScore;
	CStatic m_staticIs;

	///////////
	// Methods
	///////////

	// Enables or disables controls based on the state of the m_checkSecondCondition
	void updateControls();

	void validateLicense();
};


OBJECT_ENTRY_AUTO(__uuidof(CharacterConfidenceConditionPP), CCharacterConfidenceConditionPP)
