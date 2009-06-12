// ImageRegionWithLinesPP.h : Declaration of the CImageRegionWithLinesPP


#pragma once
#include "resource.h"       // main symbols
#include "AFValueFinders.h"

#include <string>
using namespace std;

////////////////////////////////////////
// CImageRegionWithLinesPP
////////////////////////////////////////
class ATL_NO_VTABLE CImageRegionWithLinesPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CImageRegionWithLinesPP, &CLSID_ImageRegionWithLinesPP>,
	public IPropertyPageImpl<CImageRegionWithLinesPP>,
	public CDialogImpl<CImageRegionWithLinesPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CImageRegionWithLinesPP();
	~CImageRegionWithLinesPP();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

	enum {IDD = IDD_IMAGEREGIONWITHLINESPP};

	DECLARE_REGISTRY_RESOURCEID(IDR_IMAGEREGIONWITHLINESPP)

BEGIN_COM_MAP(CImageRegionWithLinesPP)
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CImageRegionWithLinesPP)
	COMMAND_HANDLER(IDC_RADIO_ALL_PAGES, BN_CLICKED, OnBnClickedSelectedPages)
	COMMAND_HANDLER(IDC_RADIO_FIRST_PAGES, BN_CLICKED, OnBnClickedSelectedPages)
	COMMAND_HANDLER(IDC_RADIO_LAST_PAGES, BN_CLICKED, OnBnClickedSelectedPages)
	COMMAND_HANDLER(IDC_RADIO_SPECIFIED_PAGES, BN_CLICKED, OnBnClickedSelectedPages)
	CHAIN_MSG_MAP(IPropertyPageImpl<CImageRegionWithLinesPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
END_MSG_MAP()

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnBnClickedSelectedPages(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

// IPropertyPage
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	////////////
	// Variables
	////////////

	ATLControls::CComboBox m_cmbRowCountMin;
	ATLControls::CComboBox m_cmbRowCountMax;
	ATLControls::CComboBox m_cmbColumnCountMin;
	ATLControls::CComboBox m_cmbColumnCountMax;
	ATLControls::CEdit m_editColumnWidthMin;
	ATLControls::CEdit m_editColumnWidthMax;
	ATLControls::CEdit m_editLineSpacingMin;
	ATLControls::CEdit m_editLineSpacingMax;
	ATLControls::CEdit m_editColumnSpacingMax;
	ATLControls::CButton m_radioAllPages;
	ATLControls::CButton m_radioFirstPages;
	ATLControls::CButton m_radioLastPages;
	ATLControls::CButton m_radioSpecifiedPages;
	ATLControls::CEdit m_editFirstPageNums;
	ATLControls::CEdit m_editLastPageNums;
	ATLControls::CEdit m_editSpecifiedPageNums;

	ATLControls::CEdit m_editAttributeText;

	ATLControls::CButton m_chkIncludeLines;

	///////////
	// Methods
	///////////

	void initializeControls();

	// Enables/checks/unchecks appropriate controls given the selection of the specified
	// pages selection radion button
	void onSelectPages(WORD wRadioId);

	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(ImageRegionWithLinesPP), CImageRegionWithLinesPP)