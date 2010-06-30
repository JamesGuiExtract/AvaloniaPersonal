#pragma once

#include "StdAfx.h"

#include <COMUtils.h>
#include <cpputil.h>

#include <string>
#include <map>

using namespace std;

// USSProperty dialog
class USSPropertyDlg : public CDialog
{
	DECLARE_DYNAMIC(USSPropertyDlg)

public:
	USSPropertyDlg(const string& strSrc, const string& strOrig, const string& strFile,
		const string& strOCREngineVersion, const ILongToObjectMapPtr& ipISpatialPageInfoCollection,
		CWnd* pParent = NULL);
	virtual ~USSPropertyDlg();

// Dialog Data
	enum { IDD = IDD_DLG_PROPERTY };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	// Generated message map functions
	virtual BOOL OnInitDialog();
	DECLARE_MESSAGE_MAP()

private:
	////////////
	// Variables
	////////////
	// The uss file name
	string m_USSFileName;

	// the source and original information about the file
	string m_strMsgSrc;
	string m_strMsgOrig;

	// The OCR engine version
	string m_strOCREngineVersion;

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
	
	// returns a string rounded to 1 decimal place from the deskew in degrees
	string getDeskew(double dDeskewInDegrees);

	// process the orientation enum and convert it to a string
	string getOrientationString(const EOrientation& reOrientation);
};
//-------------------------------------------------------------------------------------------------