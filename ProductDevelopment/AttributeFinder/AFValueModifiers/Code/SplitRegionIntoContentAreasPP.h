// SplitRegionIntoContentAreasPP.h : Declaration of the CSplitRegionIntoContentAreasPP

#pragma once
#include "resource.h"       // main symbols
#include "AFValueModifiers.h"

//--------------------------------------------------------------------------------------------------
// CSplitRegionIntoContentAreasPP
//--------------------------------------------------------------------------------------------------
class ATL_NO_VTABLE CSplitRegionIntoContentAreasPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSplitRegionIntoContentAreasPP, &CLSID_SplitRegionIntoContentAreasPP>,
	public IPropertyPageImpl<CSplitRegionIntoContentAreasPP>,
	public CDialogImpl<CSplitRegionIntoContentAreasPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CSplitRegionIntoContentAreasPP();
	~CSplitRegionIntoContentAreasPP();
	
	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

	enum {IDD = IDD_SPLITREGIONINTOCONTENTAREASPP};

	DECLARE_REGISTRY_RESOURCEID(IDR_SPLITREGIONINTOCONTENTAREASPP)

	BEGIN_COM_MAP(CSplitRegionIntoContentAreasPP)
		COM_INTERFACE_ENTRY(IPropertyPage)
		COM_INTERFACE_ENTRY(IDispatch)
		COM_INTERFACE_ENTRY(ILicensedComponent)
	END_COM_MAP()

	BEGIN_MSG_MAP(CSplitRegionIntoContentAreasPP)
		COMMAND_HANDLER(IDC_CHECK_INCLUDE_GOOD_OCR, BN_CLICKED, OnBnClickedCheckIncludeGoodOcr)
		COMMAND_HANDLER(IDC_CHECK_INCLUDE_POOR_OCR, BN_CLICKED, OnBnClickedCheckIncludePoorOcr)
		CHAIN_MSG_MAP(IPropertyPageImpl<CSplitRegionIntoContentAreasPP>)
		MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	END_MSG_MAP()

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnBnClickedCheckIncludeGoodOcr(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedCheckIncludePoorOcr(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

// IPropertyPage
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:

	/////////////
	// Variables
	/////////////

	ATLControls::CEdit		m_editMinimumWidth;
	ATLControls::CEdit		m_editMinimumHeight;
	ATLControls::CButton	m_chkIncludeGoodOCR;
	ATLControls::CButton	m_chkIncludePoorOCR;
	ATLControls::CEdit		m_editGoodOCRType;
	ATLControls::CEdit		m_editPoorOCRType;
	ATLControls::CEdit		m_editOCRThreshold;
	ATLControls::CButton	m_chkUseLines;
	ATLControls::CButton	m_chkReOCRWithHandwriting;
	ATLControls::CButton	m_chkIncludeOCRAsTrueSpatialString;
	ATLControls::CEdit		m_editAttributeName;
	ATLControls::CEdit		m_editDefaultAttributeText;

	/////////////
	// Methods
	/////////////

	void updateUI();

	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(SplitRegionIntoContentAreasPP), CSplitRegionIntoContentAreasPP)
