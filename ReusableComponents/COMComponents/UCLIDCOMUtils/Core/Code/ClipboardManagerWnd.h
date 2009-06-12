
#pragma once

class ClipboardManagerWnd : public CWnd
{
public:
	//---------------------------------------------------------------------------------------------
	// standard ctor / dtor
	ClipboardManagerWnd();
	~ClipboardManagerWnd();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To clear the clipboard and free any contained objects
	void clear();

	//---------------------------------------------------------------------------------------------
	// REQUIRE: The object ipObj must implement ICopyableObject
	// PROMISE:	To update m_ipObj with a copy of ipObj.
	void copyObjectToClipboard(IUnknownPtr ipObj);
	
	//---------------------------------------------------------------------------------------------
	// PROMISE: To return m_ipObj
	IUnknownPtr getObjectFromClipboard();
	
	//---------------------------------------------------------------------------------------------
	// PROMISE: To return true only if the m_ipObj object is of type 
	//			IStrToObjectMap, and each of the objects in the map
	//			is of type IAttributeFindInfo
	bool objectIsAttribute();
	
	//---------------------------------------------------------------------------------------------
	// TODO: for multi-select copy/paste use
	bool objectIsIUnknownVectorOfType(REFIID riid);
	
	//---------------------------------------------------------------------------------------------
	// PROMISE: To return true only if the current item in the clipboard is an
	//			IUnknown vector of IObjectsWithDescription objects whose objects
	//			implement the riid interface
	bool vectorIsOWDOfType(REFIID riid);
	//---------------------------------------------------------------------------------------------
	// PROMISE: To return true only if the current object in the clipboard
	//			implements the riid interface
	bool objectIsOfType(REFIID riid);
	
	//---------------------------------------------------------------------------------------------
	// PROMISE: To return true only if the m_ipObj object is of type 
	//			IObjectWithDescription, and the embedded object 
	//			implements the riid interface
	bool objectIsTypeWithDescription(REFIID riid);
	//---------------------------------------------------------------------------------------------
	// Generated message map functions
	//{{AFX_MSG(RecognizeTextInPolygonDragOperation)
//	afx_msg void OnClipChange();  //clipboard change notification
//	afx_msg void OnChangeCbChain(HWND hWndRemove, HWND hWndAfter);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	IUnknownPtr m_ipObj;
//	HWND m_hNextClipboardViewer;
};
