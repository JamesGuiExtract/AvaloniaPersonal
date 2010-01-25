// DataScorerBasedASPP.h : Declaration of the CDataScorerBasedASPP


#pragma once
#include "resource.h"       // main symbols
#include "AFSelectors.h"

// CDataScorerBasedASPP

class ATL_NO_VTABLE CDataScorerBasedASPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CDataScorerBasedASPP, &CLSID_DataScorerBasedASPP>,
	public IPropertyPageImpl<CDataScorerBasedASPP>,
	public CDialogImpl<CDataScorerBasedASPP>,
	public IDispatchImpl<ILicensedComponent, &__uuidof(ILicensedComponent), &LIBID_UCLID_COMLMLib, /* wMajor = */ 1>
{
public:
	CDataScorerBasedASPP();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

	enum {IDD = IDD_DATASCORERBASEDASPP};

	DECLARE_REGISTRY_RESOURCEID(IDR_DATASCORERBASEDASPP)

	BEGIN_COM_MAP(CDataScorerBasedASPP)
		COM_INTERFACE_ENTRY(IPropertyPage)
		COM_INTERFACE_ENTRY(ILicensedComponent)
	END_COM_MAP()

	BEGIN_MSG_MAP(CDataScorerBasedASPP)
		COMMAND_HANDLER(IDC_CHECK_SECOND_CONDITION, BN_CLICKED, OnBnClickedCheckSecondCondition)
		COMMAND_HANDLER(IDC_BUTTON_COMMANDS_DATA_SCORER, BN_CLICKED, OnBnClickedButtonCommandsDataScorer)
		CHAIN_MSG_MAP(IPropertyPageImpl<CDataScorerBasedASPP>)
		MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	END_MSG_MAP()

	// Handler prototypes:
	//  LRESULT MessageHandler(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	//  LRESULT CommandHandler(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	//  LRESULT NotifyHandler(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);

	// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnBnClickedCheckSecondCondition(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
	LRESULT OnBnClickedButtonCommandsDataScorer(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);

	// IPropertyPage
	STDMETHOD(Apply)(void);

	// ILicensedComponent Methods
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);
	
private:
	// Control variables
	CComboBox m_comboFirstCondition;
	CEdit m_editFirstScore;
	CButton m_checkSecondCondition;
	CComboBox m_comboAndOr;
	CComboBox m_comboSecondCondition;
	CEdit m_editSecondScore;
	CStatic m_staticDataScorer;
	CStatic m_staticIs;
	CButton m_buttonCommand;

	// Used to store the Currently selected data scorer 
	IObjectWithDescriptionPtr m_ipSelectedDataScorer;

	// Utility for auto-encrypting feature
	IMiscUtilsPtr m_ipMiscUtils;

	// Methods
	void validateLicense();

	// Enables or disables controls based on the state of the m_checkSecondCondition
	void updateControls();
};

OBJECT_ENTRY_AUTO(__uuidof(DataScorerBasedASPP), CDataScorerBasedASPP)
