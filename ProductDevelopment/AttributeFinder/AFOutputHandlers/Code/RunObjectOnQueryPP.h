// RunObjectOnQueryPP.h : Declaration of the CRunObjectOnQueryPP

#pragma once

#include "resource.h"       // main symbols
#include <string>


EXTERN_C const CLSID CLSID_RunObjectOnQueryPP;

/////////////////////////////////////////////////////////////////////////////
// CRunObjectOnQueryPP
class ATL_NO_VTABLE CRunObjectOnQueryPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CRunObjectOnQueryPP, &CLSID_RunObjectOnQueryPP>,
	public IPropertyPageImpl<CRunObjectOnQueryPP>,
	public CDialogImpl<CRunObjectOnQueryPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CRunObjectOnQueryPP();

	enum {IDD = IDD_RUNOBJECTONQUERYPP};

DECLARE_REGISTRY_RESOURCEID(IDR_RUNOBJECTONQUERYPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CRunObjectOnQueryPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CRunObjectOnQueryPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CRunObjectOnQueryPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_BTN_CONFIGURE, BN_CLICKED, OnClickedBtnConfigure)
	COMMAND_HANDLER(IDC_CMB_OBJECT_TYPE, CBN_SELCHANGE, OnSelChangeCmbObjectType)
	COMMAND_HANDLER(IDC_CMB_OBJECT, CBN_SELCHANGE, OnSelChangeCmbObject)
	COMMAND_HANDLER(IDC_CHK_USE_SELECTOR, BN_CLICKED, OnBnClickedCheckUseSelector)
	COMMAND_HANDLER(IDC_BUTTON_CONFIGURE_SELECTOR, BN_CLICKED, OnBnClickedButtonConfigureSelector)
	COMMAND_HANDLER(IDC_COMBO_ATTRIBUTE_SELECTOR, CBN_SELCHANGE, OnCbnSelchangeComboAttributeSelector)
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
	LRESULT OnClickedBtnConfigure(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnSelChangeCmbObjectType(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnSelChangeCmbObject(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedCheckUseSelector(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
	LRESULT OnBnClickedButtonConfigureSelector(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
	LRESULT OnCbnSelchangeComboAttributeSelector(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);

private:

	//////////////
	// Enums
	//////////////
	enum ObjectType
	{
		kNone,
		kAttributeModifier,
		kAttributeSplitter,
		kOutputHandler
	};

	//////////////
	// Variables
	//////////////
	ATLControls::CEdit m_editAttributeQuery;
	ATLControls::CButton m_btnUseAttributeSelector;
	ATLControls::CComboBox m_cmbAttributeSelectors;
	ATLControls::CButton m_btnConfigureSelector;
	ATLControls::CComboBox m_cmbObjectType;
	ATLControls::CComboBox m_cmbObject;
	ATLControls::CButton m_btnConfigure;

	// Used to get the object map
	ICategoryManagerPtr m_ipCategoryMgr;

	// Stores association between registered Object names and ProgIDs
	UCLID_COMUTILSLib::IStrToStrMapPtr	m_ipObjectMap;

	// This is the currently selected component
	ICategorizedComponentPtr m_ipObject;

	// Maps attribute selector names to ProgIDs
	UCLID_COMUTILSLib::IStrToStrMapPtr m_ipSelectorMap;

	// This is the currently selected selector
	ICategorizedComponentPtr m_ipSelector;

	///////////
	// Methods
	///////////

	// Check licensing
	void validateLicense();
	IID getObjectType(int nIndex);

	// Fills the object combo box with objects based on the currently selected object type
	void populateObjectCombo();

	// Fills the selector combo box with the available attribute selectors
	void populateSelectorCombo();

	// Sets m_ipObject to a new instance of whatever type of object is selected in the object combo
	void createSelectedObject();

	// Sets m_ipSelector to a new instance of the selector type in the selector combo
	void createSelectedSelector();

	std::string getCategoryName(IID& riid);

	// This will create a categorized component based on the description string
	// str name
	ICategorizedComponentPtr createObjectFromName(std::string strName);

	// This will create a categorized component for the selector type indicated by strName
	ICategorizedComponentPtr createSelectorFromName(std::string strName);

	// This will show or hide the "Object Must be configured" message indicated by nLabelID based
	// on the specified ipObject.
	void showReminder(ICategorizedComponentPtr ipObject, int nLabelID);

	// Shows/Hides the configure button indicated by nButtonID bast on the configuration state of
	// ipObject
	void updateConfigureButton(ICategorizedComponentPtr ipObject, int nButtonID);
	
	// Enables/disables and hides/shows attribute selector controls based on the current settings.
	void updateSelectorControls();

	void selectType(IID& riid);

};
