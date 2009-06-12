#pragma once

#include "resource.h"
#include "afxwin.h"

#include <string>

using namespace std;

enum EAutoSelectTool
{
	kNoTool = 0,
	kPan = 1,
	kSelectHighlight = 2,
	kNumAutoSelectTools = 3
};

// RedactionVerificationOptionsDlg dialog
class RedactionVerificationOptionsDlg : public CDialog
{
	DECLARE_DYNAMIC(RedactionVerificationOptionsDlg)

public:
	RedactionVerificationOptionsDlg(CWnd* pParent = NULL);   // standard constructor
	virtual ~RedactionVerificationOptionsDlg();

	// Get / set the auto-zoom checkbox
	bool getAutoZoom();
	void setAutoZoom (bool bCheck);

	// Get / set the auto-zoom scale.
	// NOTE: The AutoZoomScale is stored locally as a number between 1-10. This number corresponds
	// to index of giZOOM_LEVEL defined in Settings.h. The return or passed in value will
	// be the values in giZOOM_LEVEL array. If the return or passed in value can not be found
	// in giZOOM_LEVEL, the methods will use the default value, whose index is defined
	// as giDEFAULT_ZOOM_LEVEL_INDEX in Settings.h
	int getAutoZoomScale();
	void setAutoZoomScale(int iScale);

	// Get / set the auto-select tool checkbox
	EAutoSelectTool getAutoSelectTool();
	void setAutoSelectTool(EAutoSelectTool eTool);

// Dialog Data
	enum { IDD = IDD_REDACTIONVERIFICATION_OPTIONS_DLG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();
	virtual void OnOK();
	afx_msg void OnBnClickedCheckAutozoom();
	afx_msg void OnNMCustomdrawZoomSlider(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnBnClickedCheckAutoTool();
	
	DECLARE_MESSAGE_MAP()

private:

	// Controls
	CSliderCtrl m_sliderZoomScale;
	CStatic m_staticZoomScale;
	CButton m_checkAutoTool;
	CComboBox m_comboAutoTool;

	// Variables
	BOOL m_bAutoZoom;
	EAutoSelectTool m_eAutoTool;

	// The scale of the zoom. 
	// NOTE: The dialog accepts and returns values in giZOOM_LEVEL array. Internally the values are
	//			set to the index of giZOOM_LEVEL array. 
	int m_iAutoZoomScale;

	// True if the dialog is initialized, false if it is not. 
	bool m_bInit;

	// Gets the name of the specified auto select tool
	string getAutoSelectToolName(int eTool);

	// Methods
	// Handles the enabling or disabling of the slider control based upon the state
	// of the auto zoom checkbox
	void updateUI();
};
