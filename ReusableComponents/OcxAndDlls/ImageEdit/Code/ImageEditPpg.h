//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ImageEditPpg.h
//
// PURPOSE:	This is an header file for CImageEditPropPage class
//			where this has been derived from the COlePropertyPage()
//			class.  The code written in this file makes it possible for
//			initialize to set the control.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
#if !defined(AFX_IMAGEEDITPPG_H__9C7C6FEF_C8F9_4AD6_B993_21E575E2C940__INCLUDED_)
#define AFX_IMAGEEDITPPG_H__9C7C6FEF_C8F9_4AD6_B993_21E575E2C940__INCLUDED_

// ImageEditPpg.h : Declaration of the CImageEditPropPage property page class.

////////////////////////////////////////////////////////////////////////////
// CImageEditPropPage : See ImageEditPpg.cpp.cpp for implementation.
//==================================================================================================
//
// CLASS:	CImageEditPropPage
//
// PURPOSE:	This class is used to derive CImageEditPropPage from 
//			MFC class ImageEditPropPage.
// REQUIRE:	Nothing
// 
// INVARIANTS:
//			
// EXTENSIONS:	Nothing
//
// NOTES:	
//
class CImageEditPropPage : public COlePropertyPage
{
	DECLARE_DYNCREATE(CImageEditPropPage)
	DECLARE_OLECREATE_EX(CImageEditPropPage)

// Constructor
public:
	CImageEditPropPage();

// Dialog Data
	//{{AFX_DATA(CImageEditPropPage)
	enum { IDD = IDD_PROPPAGE_IMAGEEDIT };
		// NOTE - ClassWizard will add data members here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_DATA

// Implementation
protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

// Message maps
protected:
	//{{AFX_MSG(CImageEditPropPage)
		// NOTE - ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

};
//==================================================================================================

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_IMAGEEDITPPG_H__9C7C6FEF_C8F9_4AD6_B993_21E575E2C940__INCLUDED)
