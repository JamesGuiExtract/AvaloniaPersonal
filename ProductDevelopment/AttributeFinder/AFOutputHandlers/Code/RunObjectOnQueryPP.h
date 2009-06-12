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
	ATLControls::CComboBox m_cmbObjectType;
	ATLControls::CComboBox m_cmbObject;
	ATLControls::CButton m_btnConfigure;

	// Used to get the object map
	ICategoryManagerPtr m_ipCategoryMgr;

	// Stores association between registered Object names and ProgIDs
	UCLID_COMUTILSLib::IStrToStrMapPtr	m_ipObjectMap;

	// This is the currently selected componenet
	ICategorizedComponentPtr m_ipObject;

	///////////
	// Methods
	///////////

	// Check licensing
	void validateLicense();
	IID getObjectType(int nIndex);

	// Fills the object combo box with objects based on the currently selected object type
	void populateObjectCombo();

	// sets m_ipObject to a new instance of whatever type of object is selected in the object combo
	void createSelectedObject();

	std::string getCategoryName(IID& riid);

	// This will create a categorized component based on the description string
	// str name
	ICategorizedComponentPtr createObjectFromName(std::string strName);

	// This will show or hide the "Object Must be configured" message based on
	// the current object
	void showReminder();

	// Shows/Hides the configure button
	void updateConfigureButton();

	void selectType(IID& riid);

};
