// SmoothCharactersICOPP.h : Declaration of the CSmoothCharactersICOPP

#pragma once

#include "resource.h"
#include "stdafx.h"
#include "ESImageCleanup.h"

// CSmoothCharactersICOPP

class ATL_NO_VTABLE CSmoothCharactersICOPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSmoothCharactersICOPP, &CLSID_SmoothCharactersICOPP>,
	public IPropertyPageImpl<CSmoothCharactersICOPP>,
	public CDialogImpl<CSmoothCharactersICOPP>
{
public:
	CSmoothCharactersICOPP();
	~CSmoothCharactersICOPP();

	enum {IDD = IDD_SMOOTHCHARACTERSICOPP};

DECLARE_REGISTRY_RESOURCEID(IDR_SMOOTHCHARACTERSICOPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CSmoothCharactersICOPP)
	COM_INTERFACE_ENTRY(IPropertyPage)
END_COM_MAP()

BEGIN_MSG_MAP(CSmoothCharactersICOPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CSmoothCharactersICOPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
END_MSG_MAP()

// IPropertyPage
	STDMETHOD(Apply)(void);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);

private:
	// Variables
	ATLControls::CButton m_radioLighten;
	ATLControls::CButton m_radioDarken;
};
OBJECT_ENTRY_AUTO(__uuidof(SmoothCharactersICOPP), CSmoothCharactersICOPP)