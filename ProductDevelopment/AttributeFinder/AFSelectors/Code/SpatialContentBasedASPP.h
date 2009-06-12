// SpatialContentBasedASPP.h : Declaration of the CSpatialContentBasedASPP


#pragma once
#include "resource.h"       // main symbols
#include "AFSelectors.h"

// CSpatialContentBasedASPP

class ATL_NO_VTABLE CSpatialContentBasedASPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSpatialContentBasedASPP, &CLSID_SpatialContentBasedASPP>,
	public IPropertyPageImpl<CSpatialContentBasedASPP>,
	public CDialogImpl<CSpatialContentBasedASPP>,
	public IDispatchImpl<ILicensedComponent, &__uuidof(ILicensedComponent), &LIBID_UCLID_COMLMLib, /* wMajor = */ 1>
{
public:
	CSpatialContentBasedASPP();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

	enum {IDD = IDD_SPATIALCONTENTBASEDASPP};

	DECLARE_REGISTRY_RESOURCEID(IDR_SPATIALCONTENTBASEDASPP)


	BEGIN_COM_MAP(CSpatialContentBasedASPP)
		COM_INTERFACE_ENTRY(IPropertyPage)
		COM_INTERFACE_ENTRY(ILicensedComponent)
	END_COM_MAP()

	BEGIN_MSG_MAP(CSpatialContentBasedASPP)
		CHAIN_MSG_MAP(IPropertyPageImpl<CSpatialContentBasedASPP>)
		MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	END_MSG_MAP()

	// Handler prototypes:
	//  LRESULT MessageHandler(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	//  LRESULT CommandHandler(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	//  LRESULT NotifyHandler(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);

	// IPropertyPage
	STDMETHOD(Apply)(void);

	// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	
	// ILicensedComponent Methods
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	CComboBox m_cmbContains;
	CEdit m_editConsecutiveRows;
	CEdit m_editMinPercent;
	CEdit m_editMaxPercent;
	CButton m_checkIncludeNonSpatial;

	// Methods
	void validateLicense();

};


OBJECT_ENTRY_AUTO(__uuidof(SpatialContentBasedASPP), CSpatialContentBasedASPP)
