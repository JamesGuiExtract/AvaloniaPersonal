#pragma once
#include "afxwin.h"
#include "resource.h"       // main symbols
#include <string>

// DisplayQtrsNotDrawnDlg dialog

class DisplayQtrsNotDrawnDlg : public CDialog
{
	DECLARE_DYNAMIC(DisplayQtrsNotDrawnDlg)

public:
	DisplayQtrsNotDrawnDlg(CWnd* pParent = NULL);   // standard constructor
	virtual ~DisplayQtrsNotDrawnDlg();

// Dialog Data
	enum { IDD = IDD_QUARTERS_NOT_DRAWN };

// public Methods
	void setQuartersNotDrawnText( std::string strQuartersNotDrawn );
protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	DECLARE_MESSAGE_MAP()

private:
	CString m_zQtrsNotDrawn;
};
