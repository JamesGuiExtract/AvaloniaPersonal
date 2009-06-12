//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	GenericDisplayPpg.h
//
// PURPOSE:	This is an header file for CGenericDisplayPropPage class
//			where this has been derived from the COlePropertyPage()
//			class.  The code written in this file makes it possible for
//			initialize to set the control.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
#if !defined(AFX_GENERICDISPLAYPPG_H__14981586_9117_11D4_9725_008048FBC96E__INCLUDED_)
#define AFX_GENERICDISPLAYPPG_H__14981586_9117_11D4_9725_008048FBC96E__INCLUDED_


// GenericDisplayPpg.h : Declaration of the CGenericDisplayPropPage property page class.

////////////////////////////////////////////////////////////////////////////
// CGenericDisplayPropPage : See GenericDisplayPpg.cpp.cpp for implementation.
//==================================================================================================
//
// CLASS:	CGenericDisplayPropPage
//
// PURPOSE:	This class is used to derive GenericDisplayPropPage from 
//			MFC class GenericDisplayPropPage. Object of this class is 
//			created in GenericDisplayFrame.cpp
// REQUIRE:	Nothing
// 
// INVARIANTS:
//			
// EXTENSIONS:	Nothing
//
// NOTES:	
//
//==================================================================================================
class CGenericDisplayPropPage : public COlePropertyPage
{
	DECLARE_DYNCREATE(CGenericDisplayPropPage)
	DECLARE_OLECREATE_EX(CGenericDisplayPropPage)

// Constructor
public:
	CGenericDisplayPropPage();

// Dialog Data
	//{{AFX_DATA(CGenericDisplayPropPage)
	enum { IDD = IDD_PROPPAGE_GENERICDISPLAY };
		// NOTE - ClassWizard will add data members here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_DATA

// Implementation
protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

// Message maps
protected:
	//{{AFX_MSG(CGenericDisplayPropPage)
		// NOTE - ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_GENERICDISPLAYPPG_H__14981586_9117_11D4_9725_008048FBC96E__INCLUDED)
