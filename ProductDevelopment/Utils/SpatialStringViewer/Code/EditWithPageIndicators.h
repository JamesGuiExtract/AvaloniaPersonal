#pragma once

// This class and the class CPageRulerStatic are based on the classes in 
// http://www.codeproject.com/Articles/6385/Controls-in-controls-A-line-numbering-edit-box


// CEditWithPageIndicators

class CEditWithPageIndicators : public CEdit
{
	DECLARE_DYNAMIC(CEditWithPageIndicators)

public:
	CEditWithPageIndicators();
	virtual ~CEditWithPageIndicators();

	ISpatialStringPtr m_ipSpatialString;

	int GetLastVisibleLine();

protected:
	virtual afx_msg void OnChange();
	virtual afx_msg void OnVScroll(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar);
	virtual afx_msg void OnVscroll();
	virtual afx_msg void OnSize(UINT nType, int cx, int cy);
	virtual afx_msg LRESULT OnSetText(WPARAM wParam, LPARAM lParam); // Maps to WM_SETTEXT
	virtual afx_msg LRESULT OnLineScroll(WPARAM wParam, LPARAM lParam); // Maps to EM_LINESCROLL
	DECLARE_MESSAGE_MAP()

private:

	CPageRulerStatic m_ruler;

	void UpdatePageRuler();
	void PrepareRuler();
	long getPageAtPos(long position);

};


