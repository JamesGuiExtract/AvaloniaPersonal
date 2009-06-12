#pragma once

#include "resource.h"
#include "afxwin.h"
#include "afxcmn.h"
#include "MasterExemptionCodeList.h"
#include "ExemptionCodeList.h"

#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// CExemptionCodesDlg dialog
//-------------------------------------------------------------------------------------------------
class CExemptionCodesDlg : public CDialog
{
	DECLARE_DYNAMIC(CExemptionCodesDlg)

public:

	//---------------------------------------------------------------------------------------------
	// Constructor/destructor
	//---------------------------------------------------------------------------------------------

	// Creates a dialog that allows the user to select from the specified exemption codes
	CExemptionCodesDlg(const MasterExemptionCodeList& exemptionCodes, CWnd* pParent = NULL);
	virtual ~CExemptionCodesDlg();

	//---------------------------------------------------------------------------------------------
	// Methods
	//---------------------------------------------------------------------------------------------

	// Gets/sets the selected exemption codes
	ExemptionCodeList getExemptionCodes() const;
	void setExemptionCodes(const ExemptionCodeList& codes);

	// Enables the apply last exemption button
	void enableApplyLastExemption(bool bEnable=true);

	// Sets the last applied exemption codes
	void setLastAppliedExemption(const ExemptionCodeList& codes);

// Dialog Data
	enum { IDD = IDD_DIALOG_EXEMPTION_CODES };

protected:

	//---------------------------------------------------------------------------------------------
	// Message handlers
	//---------------------------------------------------------------------------------------------
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();
	virtual void OnOK();

	afx_msg void OnCbnSelchangeComboExemptionCategory();
	afx_msg void OnLvnItemchangedListExemptionCodes(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnBnClickedCheckExemptionOther();
	afx_msg void OnEnUpdateEditExemptionOther();
	afx_msg void OnBnClickedButtonExemptionClearAll();
	afx_msg void OnBnClickedButtonExemptionApplyLast();

	DECLARE_MESSAGE_MAP()

private:

	//---------------------------------------------------------------------------------------------
	// Methods
	//---------------------------------------------------------------------------------------------

	// Gets the exemption codes that are currently selected on the dialog
	ExemptionCodeList getDisplayedExemptionCodes() const;

	// Gets the newly selected and the currently selected category
	string getSelectedCategory() const;

	// Get the exemption code that corresponds to the ith list view item
	string getItemCode(int i) const;

	// Sets the state of all the controls to match the specified exemption codes
	void selectExemptionCodes(const ExemptionCodeList& codes);

	// Updates the exemption code description edit box
	void updateDescription();
	void updateDescription(const string& strDescription);

	// Updates the exemption code sample list
	void updateSample();

	//---------------------------------------------------------------------------------------------
	// Variables
	//---------------------------------------------------------------------------------------------

	// The master list of all selectable exemption categories and codes
	MasterExemptionCodeList m_allCodes;

	// The list of exemption codes used for the public accessors
	ExemptionCodeList m_selectedCodes;

	// true if the apply last button should be enabled; false if it should be disabled
	bool m_bEnableApplyLast;

	// The list of the exemption codes that were last applied
	ExemptionCodeList m_lastApplied;

	// The last applied exemption category
	string m_strLastExemptionCategory;

	//---------------------------------------------------------------------------------------------
	// Controls
	//---------------------------------------------------------------------------------------------
	CComboBox m_comboCategory;
	CListCtrl m_listCodes;
	CEdit m_editDescription;
	CButton m_checkOtherText;
	CEdit m_editOtherText;
	CEdit m_editSample;
	CButton m_buttonApplyLast;
};
