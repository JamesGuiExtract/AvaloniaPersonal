#if !defined(AFX_UCLIDMCRTEXTVIEWER_H__3258A795_6C3C_4FB8_A34F_07754172BF08__INCLUDED_)
#define AFX_UCLIDMCRTEXTVIEWER_H__3258A795_6C3C_4FB8_A34F_07754172BF08__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// Machine generated IDispatch wrapper class(es) created by Microsoft Visual C++

// NOTE: Do not modify the contents of this file.  If this class is regenerated by
//  Microsoft Visual C++, your modifications will be overwritten.

/////////////////////////////////////////////////////////////////////////////
// CUCLIDMCRTextViewer wrapper class

class CUCLIDMCRTextViewer : public CWnd
{
protected:
	DECLARE_DYNCREATE(CUCLIDMCRTextViewer)
public:
	CLSID const& GetClsid()
	{
		static CLSID const clsid
			= { 0x7758f110, 0xd3, 0x4e95, { 0x81, 0xce, 0x86, 0xc8, 0xf4, 0x83, 0xe3, 0xb3 } };
		return clsid;
	}
	virtual BOOL Create(LPCTSTR lpszClassName,
		LPCTSTR lpszWindowName, DWORD dwStyle,
		const RECT& rect,
		CWnd* pParentWnd, UINT nID,
		CCreateContext* pContext = NULL)
	{ return CreateControl(GetClsid(), lpszWindowName, dwStyle, rect, pParentWnd, nID); }

    BOOL Create(LPCTSTR lpszWindowName, DWORD dwStyle,
		const RECT& rect, CWnd* pParentWnd, UINT nID,
		CFile* pPersist = NULL, BOOL bStorage = FALSE,
		BSTR bstrLicKey = NULL)
	{ return CreateControl(GetClsid(), lpszWindowName, dwStyle, rect, pParentWnd, nID,
		pPersist, bStorage, bstrLicKey); }

// Attributes
public:

// Operations
public:
	void open(LPCTSTR strFileName);
	void save();
	void saveAs(LPCTSTR strFileName);
	void clear();
	void parseText();
	void enableTextSelection(long ulValue);
	CString getEntityText(long ulTextEntityID);
	void pasteTextFromClipboard();
	void copyTextToClipboard();
	void setTextFontName(LPCTSTR strFontName);
	CString getTextFontName();
	void setTextSize(long ulTextSize);
	long getTextSize();
	void increaseTextSize();
	void decreaseTextSize();
	void appendTextFromClipboard();
	void setText(LPCTSTR strNewText);
	void appendText(LPCTSTR strNewText);
	long isModified();
	void print();
	CString getFileName();
	void setEntityText(long ulTextEntityID, LPCTSTR strNewText);
	void setTextHighlightColor(long ulTextEntityID, unsigned long newTextHighlightColor);
	long isEntityIDValid(long lEntityID);
	void setInputFinder(LPUNKNOWN pInputFinder);
	unsigned long getEntityColor(long ulTextEntityID);
	void setEntityColor(long ulTextEntityID, unsigned long newTextColor);
	CString getText();
	CString getSelectedText();
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_UCLIDMCRTEXTVIEWER_H__3258A795_6C3C_4FB8_A34F_07754172BF08__INCLUDED_)
