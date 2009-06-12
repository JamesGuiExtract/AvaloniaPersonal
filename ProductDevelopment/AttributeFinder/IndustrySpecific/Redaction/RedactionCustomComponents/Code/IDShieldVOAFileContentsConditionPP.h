// IDShieldVOAFileContentsConditionPP.h : Declaration of the CIDShieldVOAFileContentsConditionPP

#pragma once
#include "resource.h"       // main symbols
#include "RedactionCustomComponents.h"
#include <string>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CIDShieldVOAFileContentsConditionPP
/////////////////////////////////////////////////////////////////////////////
class ATL_NO_VTABLE CIDShieldVOAFileContentsConditionPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CIDShieldVOAFileContentsConditionPP, &CLSID_IDShieldVOAFileContentsConditionPP>,
	public IPropertyPageImpl<CIDShieldVOAFileContentsConditionPP>,
	public CDialogImpl<CIDShieldVOAFileContentsConditionPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CIDShieldVOAFileContentsConditionPP();

	enum {IDD = IDD_IDSHIELDVOAFILECONTENTSCONDITIONPP};

DECLARE_REGISTRY_RESOURCEID(IDR_IDSHIELDVOAFILECONTENTSCONDITIONPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

BEGIN_COM_MAP(CIDShieldVOAFileContentsConditionPP)
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CIDShieldVOAFileContentsConditionPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CIDShieldVOAFileContentsConditionPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_CHECK_CONTAINS_DATA, BN_CLICKED, OnClickedContainsData)
	COMMAND_HANDLER(IDC_CHECK_DOCTYPE, BN_CLICKED, OnClickedIndicatesDocType)
	COMMAND_HANDLER(IDC_BTN_SELECT_DOCTYPE, BN_CLICKED, OnClickedDocType)
	COMMAND_HANDLER(IDC_BTN_CONFIG_DATAFILE, BN_CLICKED, OnClickedConfigDataFile)
END_MSG_MAP()

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedContainsData(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedIndicatesDocType(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedDocType(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedConfigDataFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

// IProperty Page
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	///////////
	// Variables
	////////////
	ATLControls::CButton m_checkContainsData;
	ATLControls::CComboBox m_comboAttributeQuantifier;
	ATLControls::CButton m_checkContainsHCData;
	ATLControls::CButton m_checkContainsMCData;
	ATLControls::CButton m_checkContainsLCData;
	ATLControls::CButton m_checkContainsManualData;
	ATLControls::CButton m_checkContainsClues;
	ATLControls::CButton m_checkIndicatesDocType;
	ATLControls::CListBox m_listDocTypes;
	ATLControls::CButton m_btnSelectDocType;
	ATLControls::CStatic m_txtDataFileStatus;
	ATLControls::CButton m_btnConfigDataFile;
	ATLControls::CButton m_btnErrorOnMissingData;
	ATLControls::CButton m_btnSatisfiedOnMissingData;
	ATLControls::CButton m_btnUnsatisfiedOnMissingData;

	string m_strDocCategory;
	IVariantVectorPtr m_ipDocTypes;
	string m_csTargetDataFile;

	///////////
	// Methods
	///////////

	// Gets a string describing the specified attribute quantifier
	static string getAttributeQuantifierName(long eQuantifier);

	// Populate list box with data from m_ipDocTypes
	void loadDocTypesList();
	// ensure that this component is licensed
	void validateLicense();
};

