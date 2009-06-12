//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	cntritem.h
//
// PURPOSE:	This is an header file for CGenericDisplayCntrItem 
//			where this class has been derived from the COleClientItem()
//			class.  The code written in this file makes it possible for
//			initialization of controls and creating view and document objects.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
// cntritem.h : interface of the CGenericDisplayCntrItem class
//

class CGenericDisplayDocument;
class CGenericDisplayView;
//==================================================================================================
//
// CLASS:	CGenericDisplayCntrItem
//
// PURPOSE:	This class is used to derive OleClientItem from MFC class COleClientItem.
//			This derived control is attached to Frame.  Object of this class is 
//			created in GenericDisplay.cpp.  It has been created objects for CGenericDisplayDocument
//			and CGenericDisplayView classes.
// REQUIRE:	Nothing
// 
// INVARIANTS:
//			
// EXTENSIONS:	Nothing
//			
//
// NOTES:	
//
//==================================================================================================

class CGenericDisplayCntrItem : public COleClientItem
{
	DECLARE_SERIAL(CGenericDisplayCntrItem)

// Constructors
public:
	CGenericDisplayCntrItem(REOBJECT* preo = NULL, CGenericDisplayDocument* pContainer = NULL);
		// Note: pContainer is allowed to be NULL to enable IMPLEMENT_SERIALIZE.
		//  IMPLEMENT_SERIALIZE requires the class have a constructor with
		//  zero arguments.  Normally, OLE items are constructed with a
		//  non-NULL document pointer.

// Attributes
public:
	CGenericDisplayDocument* GetDocument()
		{ return (CGenericDisplayDocument*)COleClientItem::GetDocument(); }

	CGenericDisplayView* GetActiveView()
	{ return (CGenericDisplayView*)COleClientItem::GetActiveView(); }

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CGenericDisplayCntrItem)
	public:
	protected:
	//}}AFX_VIRTUAL

// Implementation
public:
#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif
};

/////////////////////////////////////////////////////////////////////////////
