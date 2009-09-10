// TagConditionPP.h : Declaration of the CTagConditionPP

#pragma once
#include "resource.h"       // main symbols
#include "ESSkipConditions.h"

////////////////////////////////////////////////////////////////////////////////////////////////////
// CTagConditionPP
////////////////////////////////////////////////////////////////////////////////////////////////////
class ATL_NO_VTABLE CTagConditionPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CTagConditionPP, &CLSID_TagConditionPP>,
	public IPropertyPageImpl<CTagConditionPP>,
	public CDialogImpl<CTagConditionPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CTagConditionPP();
	~CTagConditionPP();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

	enum {IDD = IDD_TAGCONDITIONPP};

	DECLARE_REGISTRY_RESOURCEID(IDR_TAGCONDITIONPP)

	BEGIN_COM_MAP(CTagConditionPP)
		COM_INTERFACE_ENTRY(IPropertyPage)
		COM_INTERFACE_ENTRY(IDispatch)
		COM_INTERFACE_ENTRY(ILicensedComponent)
	END_COM_MAP()

	BEGIN_MSG_MAP(CTagConditionPP)
		CHAIN_MSG_MAP(IPropertyPageImpl<CTagConditionPP>)
		MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	END_MSG_MAP()

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);

// IPropertyPage
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	////////////
	// Variables
	////////////

	// Controls
	ATLControls::CComboBox m_cmbConsiderMet;
	ATLControls::CComboBox m_cmbAnyAll;
	ATLControls::CListViewCtrl m_listTags;

	///////////
	// Methods
	///////////

	void validateLicense();

	// Set up the list control
	void setUpListContrl(const IFileProcessingDBPtr& ipDB);

	// Upate the UI from the condition object
	void updateUIFromTagCondition(const EXTRACT_FAMCONDITIONSLib::ITagConditionPtr& ipTagCondition);
};

OBJECT_ENTRY_AUTO(__uuidof(TagConditionPP), CTagConditionPP)