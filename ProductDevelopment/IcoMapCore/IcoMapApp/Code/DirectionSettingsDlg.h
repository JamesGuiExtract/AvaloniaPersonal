#pragma once
// DirectionSettingsDlg.h : header file
//

#include "resource.h"

#include <DirectionHelper.h>

/////////////////////////////////////////////////////////////////////////////
// DirectionSettingsDlg dialog

class DirectionSettingsDlg : public CPropertyPage
{
	DECLARE_DYNCREATE(DirectionSettingsDlg)

// Construction
public:
	DirectionSettingsDlg();
	~DirectionSettingsDlg();
	void createToolTips();

	int m_iDirectionSettingsPageIndex;

// Dialog Data
	//{{AFX_DATA(DirectionSettingsDlg)
	enum { IDD = IDD_DLG_DIRECTION_SETTINGS };
	int		m_AngleType;
	int		m_DirectionType;
	int		m_nLineAngleDef;
	//}}AFX_DATA


// Overrides
	// ClassWizard generate virtual function overrides
	//{{AFX_VIRTUAL(DirectionSettingsDlg)
	public:
	virtual BOOL OnApply();
	virtual BOOL OnSetActive();
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	CToolTipCtrl *m_pToolTips;
	// Generated message map functions
	//{{AFX_MSG(DirectionSettingsDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnRADIOAngle();
	afx_msg void OnRADIOAzimuth();
	afx_msg void OnRADIOBearing();
	afx_msg void OnRADIOPolarAngle();
	afx_msg void OnRADIODeflection();
	afx_msg void OnRADIOInternal();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	// whether the settings are applied or not
	bool m_bApplied;

	// whether or not to enable the PolarAngle and Azimuth radio buttons
	void enableAngleDirections(bool bEnable = true);

	// Have controls been created yet
	bool m_bInitialized;

	EDirection m_eDirection;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
