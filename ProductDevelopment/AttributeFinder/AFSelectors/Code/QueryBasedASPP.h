// QueryBasedASPP.h : Declaration of the CQueryBasedASPP


#pragma once
#include "resource.h"       // main symbols
#include "AFSelectors.h"

// CQueryBasedASPP

class ATL_NO_VTABLE CQueryBasedASPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CQueryBasedASPP, &CLSID_QueryBasedASPP>,
	public IPropertyPageImpl<CQueryBasedASPP>,
	public CDialogImpl<CQueryBasedASPP>,
	public IDispatchImpl<ILicensedComponent, &__uuidof(ILicensedComponent), &LIBID_UCLID_COMLMLib, /* wMajor = */ 1>
{
public:
	CQueryBasedASPP();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

	enum {IDD = IDD_QUERYBASEDASPP};

	DECLARE_REGISTRY_RESOURCEID(IDR_QUERYBASEDASPP)


	BEGIN_COM_MAP(CQueryBasedASPP)
		COM_INTERFACE_ENTRY(IPropertyPage)
		COM_INTERFACE_ENTRY(ILicensedComponent)
	END_COM_MAP()

	BEGIN_MSG_MAP(CQueryBasedASPP)
		CHAIN_MSG_MAP(IPropertyPageImpl<CQueryBasedASPP>)
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
	CEdit m_editQuery;

	// Methods
	
	void validateLicense();
};


OBJECT_ENTRY_AUTO(__uuidof(QueryBasedASPP), CQueryBasedASPP)
