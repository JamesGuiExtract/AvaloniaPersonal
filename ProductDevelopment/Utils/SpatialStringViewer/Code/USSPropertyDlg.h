#pragma once

#include "StdAfx.h"

#include <COMUtils.h>
#include <cpputil.h>

#include <string>
#include <map>

// USSProperty dialog
class USSPropertyDlg : public CDialog
{
	DECLARE_DYNAMIC(USSPropertyDlg)

public:
	USSPropertyDlg(std::string strSrc, std::string strOrig, std::string strFile,
		const ILongToObjectMapPtr& ripISpatialPageInfoCollection, CWnd* pParent = NULL);
	virtual ~USSPropertyDlg();

// Dialog Data
	enum { IDD = IDD_DLG_PROPERTY };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	// Generated message map functions
	//{{AFX_MSG(FindRegExDlg)
	virtual BOOL OnInitDialog();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	////////////
	// Variables
	////////////
	// The uss file name
	std::string m_USSFileName;

	// the source and original information about the file
	std::string m_strMsgSrc;
	std::string m_strMsgOrig;

	// collection of SpatialPageInfo objects
	ILongToObjectMapPtr m_ipSpatialPageInfoCollection;

	CListCtrl m_lstPageInfo;

	////////////
	// Methods
	////////////
	// Update the title of the property dialog
	void updateDlgTitle();
	
	// sets the list controls header, columns, and style
	void setUpListControl();
	
	// puts the data into the list control
	void populateListControl();
	
	// converts the deskew from radians to degrees and returns as a string
	std::string getDeskewInDegrees(const double& rdDeskewInRadians);

	// process the orientation enum and convert it to a string
	std::string getOrientationString(const EOrientation& reOrientation);
};
//-------------------------------------------------------------------------------------------------