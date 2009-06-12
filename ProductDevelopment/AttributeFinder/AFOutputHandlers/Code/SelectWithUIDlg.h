// SelectWithUIDlg.h : header file
//

#pragma once

#include "resource.h"
#include <CheckGridWnd.h>

#include <vector>

/////////////////////////////////////////////////////////////////////////////
// SelectWithUIDlg dialog

class SelectWithUIDlg : public CDialog
{
// Construction
public:
	SelectWithUIDlg(IIUnknownVector* pAttributes,
					IIUnknownVector* pResultAttributes,
					CWnd* pParent = NULL);   // standard constructor
	~SelectWithUIDlg();

// Dialog Data
	//{{AFX_DATA(SelectWithUIDlg)
	enum { IDD = IDD_DLG_SelectWithUI };
		// NOTE: the ClassWizard will add data members here
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(SelectWithUIDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(SelectWithUIDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnSelectAll();
	afx_msg void OnClearAll();
	afx_msg void OnSelectValid();
	virtual void OnOK();
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnGetMinMaxInfo(MINMAXINFO* pMMI);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:

	///////////////
	// Data Members
	///////////////
	struct AttributeAndNumber
	{
	public:
		AttributeAndNumber()
		: ipAttribute(NULL), nNumOfSameValue(0), bDirty(false)
		{
		}

		IAttributePtr ipAttribute;
		// total number of attributes that have same name/value pair
		long nNumOfSameValue;
		// all purpose flag
		bool bDirty;
	};

	// Grid window
	CCheckGridWnd		m_wndGrid;

	// Original Name/Value pairs
	IIUnknownVectorPtr	m_ipOriginAttributes;

	// Name Value Pairs returned after user modification
	IIUnknownVectorPtr	m_ipReturnAttributes;

	// Vector of Name/Value pairs with distinct counts after sorting
	std::vector<AttributeAndNumber> m_vecAttrAndNum;

	// Have the dialog's controls been instantiated yet - allows for resize
	// and repositioning
	bool				m_bInitialized;

	//////////
	// Methods
	//////////
	// Add processed Name/Value/Count items to grid
	void	displayItems();

	// Process Original vector of Name/Value pairs to sort Names in order 
	// of appearance and compute associated counts
	void	processVector();
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
