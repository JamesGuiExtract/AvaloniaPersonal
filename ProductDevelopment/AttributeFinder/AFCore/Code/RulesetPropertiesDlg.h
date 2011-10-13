#pragma once

#include "resource.h"
#include "RuleSetPropertiesPage.h"
#include "RuleSetCommentsPage.h"

#include <ResizablePropertySheet.h>

#include <string>
using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CRuleSetPropertiesDlg dialog
/////////////////////////////////////////////////////////////////////////////
class CRuleSetPropertiesDlg : public CDialog
{
	DECLARE_DYNAMIC(CRuleSetPropertiesDlg)

public:
	CRuleSetPropertiesDlg(UCLID_AFCORELib::IRuleSetPtr ipRuleSet, bool bReadOnly,
		CWnd* pParent = NULL); 
	virtual ~CRuleSetPropertiesDlg();

// Dialog Data
	enum { IDD = IDD_RULESET_PROPERTIES_DLG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();
	virtual void OnOK();

	DECLARE_MESSAGE_MAP()

private:

	//////////////
	// Variables
	//////////////
	ResizablePropertySheet m_propSheet;
	CRuleSetPropertiesPage m_ruleSetPropertiesPage;
	CRuleSetCommentsPage m_ruleSetCommentsPage;
	bool m_bReadOnly;
};
