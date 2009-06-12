//===========================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	MCRTextViewerPpg.h
//
// PURPOSE:	This is an header file for CMCRTextViewerPropPage class
//			where this has been derived from the COlePropertyPage()
//			class.  The code written in this file makes it possible for
//			initialize to set the control.
// NOTES:	
//
// AUTHORS:	
//
//===========================================================================
#if !defined(AFX_UCLIDMCRTEXTVIEWERPPG_H__A17F7A0B_625C_4B89_9506_09B3C41DC525__INCLUDED_)
#define AFX_UCLIDMCRTEXTVIEWERPPG_H__A17F7A0B_625C_4B89_9506_09B3C41DC525__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

// MCRTextViewerPpg.h : Declaration of the CMCRTextViewerPropPage property page class.

////////////////////////////////////////////////////////////////////////////
// CMCRTextViewerPropPage : See MCRTextViewerPpg.cpp.cpp for implementation.

class CMCRTextViewerPropPage : public COlePropertyPage
{
	DECLARE_DYNCREATE(CMCRTextViewerPropPage)
	DECLARE_OLECREATE_EX(CMCRTextViewerPropPage)

// Constructor
public:
	CMCRTextViewerPropPage();

// Dialog Data
	//{{AFX_DATA(CMCRTextViewerPropPage)
	enum { IDD = IDD_PROPPAGE_UCLIDMCRTEXTVIEWER };
		// NOTE - ClassWizard will add data members here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_DATA

// Implementation
protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

// Message maps
protected:
	//{{AFX_MSG(CMCRTextViewerPropPage)
		// NOTE - ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_UCLIDMCRTEXTVIEWERPPG_H__A17F7A0B_625C_4B89_9506_09B3C41DC525__INCLUDED)
