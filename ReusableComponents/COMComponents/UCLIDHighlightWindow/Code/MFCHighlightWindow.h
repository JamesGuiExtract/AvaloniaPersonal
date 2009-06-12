
#pragma once

class MFCHighlightWindow : public CDialog
{
public:

	//----------------------------------------------------------------------------------------------
	MFCHighlightWindow(CWnd* pParent = NULL);   // standard constructor	
	// This HighlightWindow supports highlighting in two mechanisms
	// kHighlightUsingBorder - can be used on all Win32 platforms
	// kHighlightUsingTransparency - can be used only on Win2K and higher
	// On Win2K and later operating systems, kHighlightUsingTransparency
	// is used as the default and both highlight types are supported.
	// On operating systems prior to Win2K, kHighlightUsingBorder is used 
	// as the default and the kHighlightUsingTransparency highlight type is 
	// not supported.
	enum EHighlightType
	{
		kHighlightUsingBorder,
		kHighlightUsingTransparency
	};

	//----------------------------------------------------------------------------------------------
	// REQUIRE: hParentWnd != NULL
	// PROMISE: To show the highlight window exactly on top of hParentWnd.
	//			if hChildWnd != NULL and hChildWnd is entirely within hParentWnd,
	//			then the highlight window will have a "hole" in it where hChildWnd
	//			is.  Subsequent calls to isVisible() will return true.  The highlight
	//			window will be transparent as well as on top of other windows.  On
	//			particular operating systems where transparency is not supported,
	//			a border highlight will be displayed around the window(s) of interest.
	void show(HWND hParentWnd, HWND hChildWnd);
	//----------------------------------------------------------------------------------------------
	// REQUIRE: see documentation for show(HWND, HWND)
	// PROMISE: see documentation for show(HWND, HWND)
	void show(CWnd *pParentWnd, CWnd *pChildWnd);
	//----------------------------------------------------------------------------------------------
	// PROMISE: To hide the highlight window.  Subsequent calls to isVisible()
	//			will return false.
	//			If bRememberWindows == true, then a subsequent call to refresh will
	//			re-show the highlight window on top of the last parent.
	//			If bRememberWindows == false, then the last parent/child windows
	//			are forgotten, and subsquent calls to refresh() don't do anything until
	//			a call to show() is made.
	void hide(bool bRememberWindows = false);
	//----------------------------------------------------------------------------------------------
	// PROMISE: To return true if this HighlightWindow is capable of being
	//			displayed as a transparent window in the current operating system.
	//			False will be returned on operating systems where window transparency
	//		.	features are not supported.
	bool windowTransparencyIsSupported() const;
	//----------------------------------------------------------------------------------------------
	// PROMISE: To return true if this window is currently visible.
	bool isVisible() const;
	//----------------------------------------------------------------------------------------------
	// PROMISE: To modify the current color of the window.  Making a call to this method
	//			does not change the visible-state of the window.  If the window is currently
	//			visible, the color change will take immediate effect.
	void setColor(COLORREF color);
	//----------------------------------------------------------------------------------------------
	// PROMISE: If isVisible()==true, the highlight window will re-position itself
	//			on top of the current coordinates of the last used hParent and child windows
	//			as appropriate.
	void refresh();
	//----------------------------------------------------------------------------------------------
	// REQUIRE: eHighlightType can be set to kHighlightUsingTransparency only if
	//			windowTransparencyIsSupported() == true.
	void setHighlightType(EHighlightType eHighlightType);
	//----------------------------------------------------------------------------------------------

// Dialog Data
	//{{AFX_DATA(HighlightWindow)
	enum { IDD = IDD_HIGHLIGHT_WINDOW_DLG };
		// NOTE: the ClassWizard will add data members here
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(HighlightWindow)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	//----------------------------------------------------------------------------------------------
	void showAsTransparentHighlight(HWND hParentWnd, HWND hChildWnd);
	//----------------------------------------------------------------------------------------------
	void showAsWindowBorder(HWND hParentWnd, HWND hChildWnd);
	//----------------------------------------------------------------------------------------------
	// calculate regions around child window & parent window when showing
	// the window highlight as a border
	void addChildWindowBorderToRegion(CRgn& rRegion, HWND hParentWnd, HWND hWnd);
	void addParentWindowBorderToRegion(CRgn& rRegion, HWND hWnd);

	// internal variable to store the visible state of the window
	bool m_bVisible;

	// current highlight type
	EHighlightType m_eHighlightType;

	// internal variable to store the background color of the window
	// and the color of the brush
	CBrush m_pBrush;
	COLORREF m_BrushColor;

	// internal variables to store the last used parent/child winows
	HWND m_hWndLastParent, m_hWndLastChild;

	// Generated message map functions
	//{{AFX_MSG(HighlightWindow)
	virtual BOOL OnInitDialog();
	afx_msg HBRUSH OnCtlColor(CDC* pDC, CWnd* pWnd, UINT nCtlColor);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
