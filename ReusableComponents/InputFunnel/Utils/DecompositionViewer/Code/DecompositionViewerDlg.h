// DecompositionViewerDlg.h : header file
//

#if !defined(AFX_DECOMPOSITIONVIEWERDLG_H__5E712B1A_D360_4419_A0B2_CF51BA9D093B__INCLUDED_)
#define AFX_DECOMPOSITIONVIEWERDLG_H__5E712B1A_D360_4419_A0B2_CF51BA9D093B__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000


#include <list>
using std::list;
#include <string>
using std::string;
/////////////////////////////////////////////////////////////////////////////
// CDecompositionViewerDlg dialog

class CDecompositionViewerDlg : public CDialog
{
// Construction
public:
	CDecompositionViewerDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	//{{AFX_DATA(CDecompositionViewerDlg)
	enum { IDD = IDD_DECOMPOSITIONVIEWER_DIALOG };
	CListBox	m_SpatialStringListBox2;
	CListBox	m_SpatialStringListBox;
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CDecompositionViewerDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	//{{AFX_MSG(CDecompositionViewerDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnLoadUssFileClicked();
	afx_msg void OnShowWordsClicked();
	afx_msg void OnShowZonesClicked();
	afx_msg void OnShowLinesClicked();
	afx_msg void OnShowParagraphsClicked();
	afx_msg void OnTextSelectSelectionChange();
	afx_msg void OnLoadUssFile2Clicked();
	afx_msg void OnTextSelect2SelectionChange();
	afx_msg void OnShowLettersClicked();
	afx_msg void OnCalclineheight();
	afx_msg void OnSelectall();
	afx_msg void OnUnselectall();
	afx_msg void OnCalclineheight2();
	afx_msg void OnViewlineheights();
	afx_msg void OnDblclkTextSelect();
	afx_msg void OnSelectall2();
	afx_msg void OnUnselectall2();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

		void OnCancel();
private:
	////////////
	// Enumerations
	////////////
	enum EListRegionType
	{
		kNone,
		kZones,
		kWords,
		kLines,
		kParagraphs,
		kLetters,
	};
	
	////////////
	// Classes
	////////////
	class ListItemInfo
	{
	public:
		long m_id;
		bool m_bWasSelected;
		long m_page;
		ISpatialStringPtr m_ipSpatialString;
	//	long m_LineHeight;
	};

	////////////
	// Methods
	////////////
	void clear(const ISpotRecognitionWindowPtr ipSRIR, CListBox* pListBox);
	void clearHighlights(const ISpotRecognitionWindowPtr ipSRIR, CListBox* pListBox);;
	long createZoneEntity(const ISpotRecognitionWindowPtr ipSRIR, const ISpatialStringPtr ipSpatialString, long page, long color = 0x00ffff);
	void deleteZoneEntity(const ISpotRecognitionWindowPtr ipSRIR,long id);
	void populateList(EListRegionType type, const ISpotRecognitionWindowPtr ipSRIR, ISpatialStringPtr ipSpatialString, CListBox* pListBox);

	////////////
	// Variables
	////////////
	EListRegionType m_eCurrentRegionType;
	
	IInputManagerPtr m_ipInputManager;
	IInputReceiverPtr m_ipInputReceiver1;
	IInputReceiverPtr m_ipInputReceiver2;

	ISpotRecognitionWindowPtr m_ipSRIR1;
	ISpotRecognitionWindowPtr m_ipSRIR2;

	ISpatialStringPtr m_ipSpatialString1;
	ISpatialStringPtr m_ipSpatialString2;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_DECOMPOSITIONVIEWERDLG_H__5E712B1A_D360_4419_A0B2_CF51BA9D093B__INCLUDED_)
