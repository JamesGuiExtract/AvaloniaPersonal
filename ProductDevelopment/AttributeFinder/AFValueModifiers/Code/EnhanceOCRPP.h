// EnhanceOCRPP.h : Declaration of the CEnhanceOCRPP

#pragma once
#include "resource.h"       // main symbols
#include "AFValueModifiers.h"

#include <XInfoTip.h>

//--------------------------------------------------------------------------------------------------
// CEnhanceOCRPP
//--------------------------------------------------------------------------------------------------
class ATL_NO_VTABLE CEnhanceOCRPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CEnhanceOCRPP, &CLSID_EnhanceOCRPP>,
	public IPropertyPageImpl<CEnhanceOCRPP>,
	public CDialogImpl<CEnhanceOCRPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CEnhanceOCRPP();
	~CEnhanceOCRPP();
	
	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

	enum {IDD = IDD_ENHANCEOCRPP};

	DECLARE_REGISTRY_RESOURCEID(IDR_ENHANCEOCRPP)

	BEGIN_COM_MAP(CEnhanceOCRPP)
		COM_INTERFACE_ENTRY(IPropertyPage)
		COM_INTERFACE_ENTRY(IDispatch)
		COM_INTERFACE_ENTRY(ILicensedComponent)
	END_COM_MAP()

	BEGIN_MSG_MAP(CEnhanceOCRPP)
		CHAIN_MSG_MAP(IPropertyPageImpl<CEnhanceOCRPP>)
		MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
		COMMAND_HANDLER(IDC_RADIO_GENERAL_FILTERS, BN_CLICKED, OnFilterRadioButton)
		COMMAND_HANDLER(IDC_RADIO_HALFTONE_FILTERS, BN_CLICKED, OnFilterRadioButton)
		COMMAND_HANDLER(IDC_RADIO_ALIASED_FILTERS, BN_CLICKED, OnFilterRadioButton)
		COMMAND_HANDLER(IDC_RADIO_SMUDGED_FILTERS, BN_CLICKED, OnFilterRadioButton)
		COMMAND_HANDLER(IDC_RADIO_CUSTOM_FILTERS, BN_CLICKED, OnFilterRadioButton)
		COMMAND_HANDLER(IDC_BTN_CUSTOM_FILTERS_DOC_TAG, BN_CLICKED, OnClickedCustomFiltersDocTag)
		COMMAND_HANDLER(IDC_BTN_CUSTOM_FILTERS_BROWSE, BN_CLICKED, OnClickedCustomFiltersBrowse)
		COMMAND_HANDLER(IDC_BTN_PREFERRED_FORMAT_DOC_TAG, BN_CLICKED, OnClickedPreferredFormatDocTag)
		COMMAND_HANDLER(IDC_BTN_PREFERRED_FORMAT_BROWSE, BN_CLICKED, OnClickedPreferredFormatBrowse)

	END_MSG_MAP()

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnFilterRadioButton(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedCustomFiltersDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedCustomFiltersBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedPreferredFormatDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedPreferredFormatBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

// IPropertyPage
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:

	/////////////
	// Variables
	/////////////

	ATLControls::CEdit m_editConfidenceCriteria;
	ATLControls::CButton m_radioGeneralFilters;
	ATLControls::CButton m_radioHalftoneSpeckledFilters;
	ATLControls::CButton m_radioAliasedDiffuseFilters;
	ATLControls::CButton m_radioLinesSmudgedFilters;
	ATLControls::CButton m_radioCustomFilters;
	ATLControls::CTrackBarCtrl m_sliderFilterLevel;
	ATLControls::CStatic m_labelFilterLevel1;
	ATLControls::CStatic m_labelFilterLevel2;
	ATLControls::CStatic m_labelFilterLevel3;
	ATLControls::CEdit m_editCustomerFilters;
	ATLControls::CButton m_btnCustomFiltersDocTag;
	ATLControls::CButton m_btnCustomFiltersBrowse;
	ATLControls::CEdit m_editPreferredFormatRegexFile;
	ATLControls::CButton m_btnPreferredFormatDocTag;
	ATLControls::CButton m_btnPreferredFormatBrowse;
	ATLControls::CEdit m_editCharsToIgnore;
	ATLControls::CButton m_chkOutputFilteredImages;

	CXInfoTip m_infoTip;
	IMiscUtilsPtr m_ipMiscUtils;
	IAFUtilityPtr m_ipAFUtility;

	/////////////
	// Methods
	/////////////

	EFilterPackage getSelectedFilterPackage();

	void updateUI();

	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(EnhanceOCRPP), CEnhanceOCRPP)