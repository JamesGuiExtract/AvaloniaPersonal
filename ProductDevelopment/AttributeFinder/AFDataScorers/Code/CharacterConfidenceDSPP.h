// CharacterConfidenceDSPP.h : Declaration of the CCharacterConfidenceDSPP


#pragma once
#include "resource.h"       // main symbols
#include "AFDataScorers.h"

#include <AFCategories.h>

// CCharacterConfidenceDSPP

class ATL_NO_VTABLE CCharacterConfidenceDSPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CCharacterConfidenceDSPP, &CLSID_CharacterConfidenceDSPP>,
	public IPropertyPageImpl<CCharacterConfidenceDSPP>,
	public CDialogImpl<CCharacterConfidenceDSPP>,
	public IDispatchImpl<ILicensedComponent, &__uuidof(ILicensedComponent), &LIBID_UCLID_COMLMLib, /* wMajor = */ 1>
{
public:
	CCharacterConfidenceDSPP();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

	enum {IDD = IDD_CHARACTERCONFIDENCEDSPP};

	DECLARE_REGISTRY_RESOURCEID(IDR_CHARACTERCONFIDENCEDSPP)


	BEGIN_COM_MAP(CCharacterConfidenceDSPP)
		COM_INTERFACE_ENTRY(IPropertyPage)
		COM_INTERFACE_ENTRY(ILicensedComponent)
	END_COM_MAP()

	BEGIN_MSG_MAP(CCharacterConfidenceDSPP)
		CHAIN_MSG_MAP(IPropertyPageImpl<CCharacterConfidenceDSPP>)
		MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	END_MSG_MAP()

	// Handler prototypes:
	//  LRESULT MessageHandler(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	//  LRESULT CommandHandler(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	//  LRESULT NotifyHandler(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);

	// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);

	// IPropertyPage
	STDMETHOD(Apply)(void);

	// ILicensedComponent Methods
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	// Control variables
	CComboBox m_comboAggregateFunction;

	// Methods
	void validateLicense();
};


OBJECT_ENTRY_AUTO(__uuidof(CharacterConfidenceDSPP), CCharacterConfidenceDSPP)
