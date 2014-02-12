#pragma once

// This class and the class CEditWithPageIndicators are based on the classes in 
// http://www.codeproject.com/Articles/6385/Controls-in-controls-A-line-numbering-edit-box


#include <map>

using namespace std;

typedef map<long,long> PageLocationMap;

// CPageRulerStatic

class CPageRulerStatic : public CStatic
{
	DECLARE_DYNAMIC(CPageRulerStatic)

public:
	CPageRulerStatic();
	virtual ~CPageRulerStatic();

	// Method sets the map of locations for page boundries and sets the top displayed pages
	void SetLinePageLocations(const PageLocationMap mapOfPageLocations, int nTop);

protected:
	afx_msg BOOL OnEraseBkgnd(CDC* pDC);
	virtual afx_msg void OnPaint();	
	
	DECLARE_MESSAGE_MAP()

private:
	PageLocationMap m_mapOfPageLocations;

	// Contains the top displayed pages
	int m_nTopPage;
};


