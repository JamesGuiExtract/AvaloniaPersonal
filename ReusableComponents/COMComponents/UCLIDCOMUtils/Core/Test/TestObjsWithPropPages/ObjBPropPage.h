// ObjBPropPage.h : Declaration of the CObjBPropPage

#ifndef __OBJBPROPPAGE_H_
#define __OBJBPROPPAGE_H_

#include "resource.h"       // main symbols

EXTERN_C const CLSID CLSID_ObjBPropPage;

/////////////////////////////////////////////////////////////////////////////
// CObjBPropPage
class ATL_NO_VTABLE CObjBPropPage :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CObjBPropPage, &CLSID_ObjBPropPage>,
	public IPropertyPageImpl<CObjBPropPage>,
	public CDialogImpl<CObjBPropPage>
{
public:
	CObjBPropPage() 
	{
		m_dwTitleID = IDS_TITLEObjBPropPage;
		m_dwHelpFileID = IDS_HELPFILEObjBPropPage;
		m_dwDocStringID = IDS_DOCSTRINGObjBPropPage;
	}

	enum {IDD = IDD_OBJBPROPPAGE};

DECLARE_REGISTRY_RESOURCEID(IDR_OBJBPROPPAGE)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CObjBPropPage) 
	COM_INTERFACE_ENTRY(IPropertyPage)
END_COM_MAP()

BEGIN_MSG_MAP(CObjBPropPage)
	CHAIN_MSG_MAP(IPropertyPageImpl<CObjBPropPage>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_EDIT_ENDPOS, EN_CHANGE, OnChangeEdit_endpos)
	COMMAND_HANDLER(IDC_EDIT_STARTPOS, EN_CHANGE, OnChangeEdit_startpos)
END_MSG_MAP()
// Handler prototypes:
//  LRESULT MessageHandler(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
//  LRESULT CommandHandler(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
//  LRESULT NotifyHandler(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);

	STDMETHOD(Apply)(void)
	{
		ATLTRACE(_T("CObjBPropPage::Apply\n"));
		for (UINT i = 0; i < m_nObjects; i++)
		{
			// validate the input first
			long nStartPos = GetDlgItemInt(IDC_EDIT_STARTPOS);
			long nEndPos = GetDlgItemInt(IDC_EDIT_ENDPOS);
			if (nStartPos > nEndPos)
			{
				AfxMessageBox("ERROR: Start position must be less than or equal to end position!");
				CWindow wnd(GetDlgItem(IDC_EDIT_STARTPOS));

				CEdit *pEdit = (CEdit *) CWnd::FromHandle(GetDlgItem(IDC_EDIT_STARTPOS));
				pEdit->SetSel(0, -1);
				wnd.SetFocus();
				return S_FALSE;
			}

			// input has been validated...now flush the settings to the
			// object
			CComQIPtr<IObjB> ipObjB = m_ppUnk[i];
			ipObjB->put_StartPos(nStartPos);
			ipObjB->put_EndPos(nEndPos);
		}
		SetDirty(FALSE);
		return S_OK;
	}
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
	{
		if (m_nObjects > 0)
		{
			CComQIPtr<IObjB> ipObjB = m_ppUnk[0];
			long nStartPos, nEndPos;
			ipObjB->get_StartPos(&nStartPos);
			ipObjB->get_EndPos(&nEndPos);
			SetDlgItemInt(IDC_EDIT_STARTPOS, nStartPos);
			SetDlgItemInt(IDC_EDIT_ENDPOS, nEndPos);
		}
		SetDirty(FALSE);
		return 0;
	}
	LRESULT OnChangeEdit_endpos(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
	{
		SetDirty(TRUE);
		return 0;
	}
	LRESULT OnChangeEdit_startpos(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
	{
		SetDirty(TRUE);
		return 0;
	}
};

#endif //__OBJBPROPPAGE_H_
