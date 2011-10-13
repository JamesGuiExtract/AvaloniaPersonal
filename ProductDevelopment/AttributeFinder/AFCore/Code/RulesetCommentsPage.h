#pragma once

#include "resource.h"

#include <string>
using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CRuleSetCommentsPage dialog
/////////////////////////////////////////////////////////////////////////////
class CRuleSetCommentsPage : public CPropertyPage
{
	DECLARE_DYNAMIC(CRuleSetCommentsPage)

public:
	CRuleSetCommentsPage(UCLID_AFCORELib::IRuleSetPtr ipRuleSet);
	virtual ~CRuleSetCommentsPage();

	// Applies the properties in the property page to the ruleset.
	void Apply();

// Dialog Data
	enum { IDD = IDD_RULESET_COMMENTS_PAGE };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();

	DECLARE_MESSAGE_MAP()

private:

	//////////////
	// Variables
	//////////////

	UCLID_AFCORELib::IRuleSetPtr m_ipRuleSet;

	CEdit m_editRulesetComments;
};
