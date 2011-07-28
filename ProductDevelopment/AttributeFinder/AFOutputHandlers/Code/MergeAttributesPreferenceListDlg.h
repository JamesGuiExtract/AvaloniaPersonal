// MergeAttributesPreferenceListDlg.h : Declaration of the CMergeAttributesPreferenceListDlg

#pragma once
#include "resource.h"       // main symbols
#include "AFOutputHandlers.h"

#include <XInfoTip.h>
#include <ImageButtonWithStyle.h>

#include <string>
#include <vector>
using namespace std;

//--------------------------------------------------------------------------------------------------
// CMergeAttributesPreferenceListDlg
//--------------------------------------------------------------------------------------------------
class CMergeAttributesPreferenceListDlg : 
	public CAxDialogImpl<CMergeAttributesPreferenceListDlg>
{
public:
	CMergeAttributesPreferenceListDlg(const string& strDialogTitle, vector<string> &vecListValues,
		bool bValidateAsIdentifier, bool &rbTreatAsRegex);
	~CMergeAttributesPreferenceListDlg();

	// Displays a dialog box that allows the values of vecListValues to be edited and the
	// value of rbTreatAsRegex to be modified. If rbTreatAsRegex is false upon closing, all list
	// values will tested that the are valid identifiers.
	// Returns true if the user pressed OK and vecListValues and rbTreatAsRegex have been updated.
	static bool EditList(const string& strDialogTitle, vector<string> &vecListValues,
		bool bValidateAsIdentifier, bool &rbTreatAsRegex);

	enum { IDD = IDD_MERGEATTRIBUTESPREFERENCELISTDLG };

	BEGIN_MSG_MAP(CMergeAttributesPreferenceListDlg)
		MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
		COMMAND_HANDLER(IDOK, BN_CLICKED, OnClickedOK)
		COMMAND_HANDLER(IDCANCEL, BN_CLICKED, OnClickedCancel)
		COMMAND_HANDLER(IDC_BTN_ADD_NAME, BN_CLICKED, OnBnClickedBtnAdd)
		COMMAND_HANDLER(IDC_BTN_REMOVE_NAME, BN_CLICKED, OnBnClickedBtnRemove)
		COMMAND_HANDLER(IDC_BTN_MODIFY_NAME, BN_CLICKED, OnBnClickedBtnModify)
		COMMAND_HANDLER(IDC_BTN_NAME_UP, BN_CLICKED, OnBnClickedBtnUp)
		COMMAND_HANDLER(IDC_BTN_NAME_DOWN, BN_CLICKED, OnBnClickedBtnDown)
		NOTIFY_HANDLER(IDC_LIST_NAMES, LVN_ITEMCHANGED, OnItemChangedList)
		NOTIFY_HANDLER(IDC_LIST_NAMES, NM_DBLCLK, OnDblclkList)
		CHAIN_MSG_MAP(CAxDialogImpl<CMergeAttributesPreferenceListDlg>)
		// REFLECT_NOTIFICATIONS needed by ImageButtonWithSytle
		REFLECT_NOTIFICATIONS()
	END_MSG_MAP()

	// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedOK(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedCancel(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedBtnAdd(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedBtnRemove(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedBtnModify(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedBtnUp(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedBtnDown(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnItemChangedList(int idCtrl, LPNMHDR pNMHDR, BOOL& bHandled);
	LRESULT OnDblclkList(int idCtrl, LPNMHDR pNMHDR, BOOL& bHandled);

private:

	// Controls
	ATLControls::CListViewCtrl m_listMergePriority;
	ATLControls::CButton m_btnAdd;
	ATLControls::CButton m_btnModify;
	ATLControls::CButton m_btnRemove;
	CImageButtonWithStyle m_btnUp;
	CImageButtonWithStyle m_btnDown;
	ATLControls::CButton m_btnTreatAsRegEx;

	// The text to appear in the dialog's title bar.
	const string& m_strDialogTitle;

	// The list's values.
	vector<string>& m_vecListValues;

	// Whether the list values should be validated as identifiers if not regex's
	bool m_bValidateAsIdentifier;

	// Whether the list items should be treaed as regexs.
	bool& m_bTreatAsRegEx;

	// Populates the list with the specified vector of values
	void initializeList(const vector<string> &vecEntries);

	// Returns a vector of entries in the list
	vector<string> retrieveListValues();

	// Update the state of the buttons based on the items in the list
	void updateButtons();
};


