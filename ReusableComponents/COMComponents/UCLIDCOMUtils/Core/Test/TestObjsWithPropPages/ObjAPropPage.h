// ObjAPropPage.h : Declaration of the CObjAPropPage

#ifndef __OBJAPROPPAGE_H_
#define __OBJAPROPPAGE_H_

#include "resource.h"       // main symbols

EXTERN_C const CLSID CLSID_ObjAPropPage;

/////////////////////////////////////////////////////////////////////////////
// CObjAPropPage
class ATL_NO_VTABLE CObjAPropPage :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CObjAPropPage, &CLSID_ObjAPropPage>,
	public IPropertyPageImpl<CObjAPropPage>,
	public CDialogImpl<CObjAPropPage>
{
public:
	CObjAPropPage() 
	{
		m_dwTitleID = IDS_TITLEObjAPropPage;
		m_dwHelpFileID = IDS_HELPFILEObjAPropPage;
		m_dwDocStringID = IDS_DOCSTRINGObjAPropPage;
	}

	enum {IDD = IDD_OBJAPROPPAGE};

DECLARE_REGISTRY_RESOURCEID(IDR_OBJAPROPPAGE)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CObjAPropPage) 
	COM_INTERFACE_ENTRY(IPropertyPage)
END_COM_MAP()

BEGIN_MSG_MAP(CObjAPropPage)
	CHAIN_MSG_MAP(IPropertyPageImpl<CObjAPropPage>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_EDIT_REGEXPR, EN_CHANGE, OnChangeEdit_regexpr)
END_MSG_MAP()
// Handler prototypes:
//  LRESULT MessageHandler(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
//  LRESULT CommandHandler(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
//  LRESULT NotifyHandler(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);

	STDMETHOD(Apply)(void)
	{
		ATLTRACE(_T("CObjAPropPage::Apply\n"));
		for (UINT i = 0; i < m_nObjects; i++)
		{
			CComQIPtr<IObjA> ipObjA = m_ppUnk[i];

			char pszTemp[128];
			GetDlgItemText(IDC_EDIT_REGEXPR, pszTemp, 128);
			ipObjA->put_RegExpr(_bstr_t(pszTemp));
		}
		SetDirty(FALSE);
		return S_OK;
	}
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
	{
		if (m_nObjects > 0)
		{
			CComQIPtr<IObjA> ipObjA = m_ppUnk[0];

			CComBSTR bstrRegExpr;
			ipObjA->get_RegExpr(&bstrRegExpr);
			SetDlgItemText(IDC_EDIT_REGEXPR, CString(bstrRegExpr));
		}
		SetDirty(FALSE);
		return 0;
	}
	LRESULT OnChangeEdit_regexpr(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
	{
		SetDirty(TRUE);
		return 0;
	}
};

#endif //__OBJAPROPPAGE_H_
