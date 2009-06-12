//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	cntritem.h
//
// PURPOSE:	This is an header file for CMCRCntrItem 
//			where this class has been derived from the CRichEditCntrItem()
//			class.  The code written in this file makes it possible for
//			initialization of controls and creating view and document objects.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
// cntritem.h : interface of the CMCRCntrItem class
//

class CMCRDocument;
class CMCRView;

//==================================================================================================
//
// CLASS:	CMCRCntrItem
//
// PURPOSE:	This class is used to derive RichEditCntrItem from MFC class CRichEditCntrItem.
//			This derived control is attached to Frame.  Object of this class is 
//			created in MCRTextViewer.cpp.  It has been created objects for CMCRDocument
//			and CMCRView classes.
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
class CMCRCntrItem : public CRichEditCntrItem
{
	DECLARE_SERIAL(CMCRCntrItem)

// Constructors
public:
	CMCRCntrItem(REOBJECT* preo = NULL, CMCRDocument* pContainer = NULL);
		// Note: pContainer is allowed to be NULL to enable IMPLEMENT_SERIALIZE.
		//  IMPLEMENT_SERIALIZE requires the class have a constructor with
		//  zero arguments.  Normally, OLE items are constructed with a
		//  non-NULL document pointer.

// Attributes
public:
	CMCRDocument* GetDocument()
		{ return (CMCRDocument*)COleClientItem::GetDocument(); }
	CMCRView* GetActiveView()
		{ return (CMCRView*)COleClientItem::GetActiveView(); }

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CMCRCntrItem)
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
