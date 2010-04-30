//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	RubberBandThrd.h
//
// PURPOSE:	The purpose of this file is to implement the Threading that needs to be supported by
//			the GenericDisplay ActiveX control.  This is an header file for CRubberBandThrd 
//			where this class has been derived from the CWinThread()class.  
//			The code written in this file makes it possible to implement the various methods.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
#pragma once

// RubberBandThrd.h : header file
//

#include <vector>

class CGenericDisplayView;

/////////////////////////////////////////////////////////////////////////////
// CRubberBandThrd thread
//==================================================================================================
//
// CLASS:	CRubberBandThrd
//
// PURPOSE:	This class is used to create a separate thread from MFC class CWinThread.
//			This derived class is attached to GenericDisplayView.  Object of this class is 
//			created in GenericDisplayView.cpp.
//
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
class CGenericDisplayCtrl;

class CRubberBandThrd : public CWinThread
{
	DECLARE_DYNCREATE(CRubberBandThrd)
public:
	CRubberBandThrd();           // protected constructor used by dynamic creation
	virtual ~CRubberBandThrd();

// Attributes
public:
	//	GenericDisplayView
	CGenericDisplayView	*m_pGenView;

	// Operations
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CRubberBandThrd)
	public:
	virtual BOOL InitInstance();
	virtual int ExitInstance();
	//}}AFX_VIRTUAL

// Implementation
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To set a new start point 2 for drawing polygon rubberband
	// REQUIRE: X and Y coordinate values
	// PROMISE: None
	void setStartPoint2(int nX, int nY);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To draw the rubber band line or reactangle
	// REQUIRE: X and Y coordinate values
	// PROMISE: None
	void Draw (int iEX, int iEY);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To set the Threading parameters
	// REQUIRE: None
	// PROMISE: None
	// Param:	eRubberbandType: 1 - Line rubberband that takes a start point
	//							 2 - Rectangular rubberband that takes a start point
	//							 3 - Polygon rubberband that takes a start point 
	void setThreadParameters (long eRubberbandType, double dStartX, double dStartY);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To get the Threading parameters
	// REQUIRE: StartPoint and RubberBanding type and thread must have been enabled
	// PROMISE: None
	void getThreadParameters (long*& peRBType, double*& pdStartX, double*& pdStartY);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To enable Rubber Banding
	// REQUIRE: TRUE or FALSE
	// PROMISE: None
	void enableRubberBand (BOOL bValue);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To update the Rubber Banding
	// REQUIRE: None
	// PROMISE: None
	void updateRubberBand ();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To initialize the Thread Parameters
	// REQUIRE: None
	// PROMISE: None
	void initThreadParameters();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: to remove the earlier rubber band lines. When once rubberBandingParameters 
	//          are set and enabled and if rubberBanding is again set without disabling, this
	//          method is called to remove the earlier lines. This method is also called when
	//          rubberBanding is set and enabled and control's painting method is called. Paint
	//          method is called generally called on resize, on focus change etc
	// REQUIRE: Rubberbanding parameters are set and enabled
	// PROMISE: removes earlier lines of rubberBanding
	void removeEarlierLines();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To return TRUE if rubber banding is enabled other wise FALSE
	// REQUIRE: None
	// PROMISE: returns the enable condition of rubber band
	BOOL IsRubberBandingEnabled() {return m_bEnableRubberBand;}
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To return TRUE if rubber banding parameters are set other wise FALSE
	// REQUIRE: None
	// PROMISE: returns whether rubber banding parameters are currenly set
	BOOL IsRubberBandingParametersSet() {return m_bSetParameters;}
	//----------------------------------------------------------------------------------------------
	// PURPOSE: to set current related CGenericDisplayCtrl object. This is required to enable UGD to
	// REQUIRE: create multiple instances and to link view and frame of corresponding ctrl object
	// PROMISE: 
	void setGenericDisplayCtrl(CGenericDisplayCtrl* pGenericDisplayCtrl) {m_pGenericDisplayCtrl = pGenericDisplayCtrl;}
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To draw the rubber banding line or rectangle.
	// REQUIRE: Rubberbanding should be started.
	// PROMISE: 
	void DrawRubberBanding();
	//----------------------------------------------------------------------------------------------

protected:

	// Generated message map functions
	//{{AFX_MSG(CRubberBandThrd)
		// NOTE - the ClassWizard will add and remove member functions here.
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()

private:
	
	// Attributes
	//	Boolean variable for setting rubbe band with line
	long	m_eRubberbandType;

	//	Boolean variable to enable rubber banding
	BOOL	m_bEnableRubberBand;

	//	Boolean variable to start the thread
	BOOL	m_bStartThread;

	//	Boolean variable to set the parameters
	BOOL	m_bSetParameters;

	// whether or not the rubberband has alrealy been drawn
	bool m_bIsRubberbandDrawn;

	// store the real world coordinates to send with getThreadParameters()
	double m_dStartXInRealWorld;
	double m_dStartYInRealWorld;
	//	start point 2 coordinates in world coordinates
	double	m_dStartPoint2X;
	double  m_dStartPoint2Y;
	//	End point coordinates in world coordinates
	double  m_dEndPointX;
	double	m_dEndPointY;

	//	to set the newest end point and start point2 in world coordinates
	double	m_dNewEndX;
	double	m_dNewEndY;
	double	m_dNewStartPoint2X; 
	double	m_dNewStartPoint2Y;

	CPen m_Brush;

	// have a local variable for the control
	CGenericDisplayCtrl *m_pGenericDisplayCtrl;

	// vector of entity ids
	std::vector<long> m_vecEntityIDs;

	void addLineEntity(double dEndX, double dEndY);
	void drawRubberbands(int nStartX, int nStartY, int nEndX, int nEndY, int nStart2X, int nStart2Y);
	void convertToClientCoordinates(double dX, double dY, int& nX, int& nY);
	void removeLineEntities();
};

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
