#pragma once
#include "afxext.h"

#include <string>
using namespace std;

enum EUSSViewerToolBarButtonCtrl
{
	kBtnFirstPage,
	kBtnPreviousPage,
	kEditGoToPage,
	kBtnNextPage,
	kBtnLastPage
};

//-------------------------------------------------------------------------------------------------
// USSViewerToolBar class
// Description: Class used to manage the toolbar for the USS File viewer
//-------------------------------------------------------------------------------------------------
class USSViewerToolBar : public CToolBar
{
public:
	USSViewerToolBar(void);
	virtual ~USSViewerToolBar(void);

	// Method to create the goto edit box which replaces the Goto button place holder on the
	// toolbar
	void createGoToPageEditBox();

	// Method to enable or disable the goto edit box
	void enableGoToEditBox(bool bEnable);

	// Method returns the text in the goto edit box
	string getCurrentGoToPageText();

	// Method to set the text in the goto edit box
	void setCurrentGoToPageText(const string& strText);

	// Method clears the goto edit box text
	void clearGoToPageText();

	// Mehtod returns true if the goto edit box has focus false otherwise
	bool gotoPageHasFocus();

private:

	// Pointer to the Goto edit box
	CEdit *m_editGoto;
};

