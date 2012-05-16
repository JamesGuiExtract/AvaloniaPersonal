//===========================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	AttributeViewDlg.h
//
// PURPOSE:	To provide edit and view capabilities to collected Feature 
//				attributes.
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//===========================================================================

#pragma once

#include "AttributeViewerDLL.h"
#include "resource.h"

#include <DistanceCore.h>
#include <DirectionHelper.h>

#include <map>
#include <string>
#include <fstream>

// Forward declaration
class CurveCalculationEngineImpl;
class CfgAttributeViewer;

// Data for single item in List as ParameterTypeToValueMap
// used in tagITEMINFO structure
typedef std::map<ECurveParameterType, std::string> PTTVM;

/////////////////////////////////////////////////////////////////////////////
// CAttributeViewDlg dialog
/////////////////////////////////////////////////////////////////////////////
class CLASS_DECL_AttributeViewerDLL CAttributeViewDlg : public CDialog
{
// Construction
public:
	//=============================================================================
	// PURPOSE: Creates a modal Attribute Viewer dialog
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	pCfg - Pointer to configuration object
	//				ptrCurrentFeature - Current attributes
	//				ptrOriginalFeature - Original attributes, or NULL
	//				bMakeCurrReadOnly - Current attributes cannot be modified
	//				bMakeOrigReadOnly - Original attributes cannot be modified
	//			bCanStoreOriginalAttributes - if this is true, then the feature
	//			being edited/viewed has the ability to store original attributes.
	//			If this is false, the feature being edited/viewed does not have
	//			the ability to store original attributes, and consequently, the
	//			bMakeOrigReadOnly flag is ignored.
	CAttributeViewDlg(CfgAttributeViewer* pCfg, IUCLDFeaturePtr ptrCurrentFeature, 
		IUCLDFeaturePtr ptrOriginalFeature, 
		bool bMakeCurrReadOnly, bool bMakeOrigReadOnly, 
		bool bCanStoreOriginalAttributes,
		CWnd* pParent = NULL);

	//=============================================================================
	// PURPOSE: Destroys the Attribute Viewer dialog and cleans up internal data
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	~CAttributeViewDlg();

	//=============================================================================
	// PURPOSE: Check for validity of Current Attributes
	// REQUIRE: Nothing
	// PROMISE: Returns true if collection of attributes is valid, false otherwise.
	// ARGS:	None.
	bool	isCurrentFeatureValid();

	//=============================================================================
	// PURPOSE: Check for validity of Original Attributes
	// REQUIRE: Nothing
	// PROMISE: Returns true if collection of attributes is valid, false otherwise.
	// ARGS:	None.
	bool	isOriginalFeatureValid();

	//=============================================================================
	// PURPOSE: Returns Current attributes, as modified
	// REQUIRE: Nothing
	// PROMISE: Returns NULL if Feature is not valid
	// ARGS:	None.
	IUCLDFeaturePtr	getCurrentFeature();

	//=============================================================================
	// PURPOSE: Returns Original attributes, as modified
	// REQUIRE: Nothing
	// PROMISE: Returns NULL if Feature is not valid
	// ARGS:	None.
	IUCLDFeaturePtr	getOriginalFeature();

	//=============================================================================
	// PURPOSE: Check for Current Attributes empty
	// REQUIRE: Successful completion of OnOK().  If user cancelled from dialog,
	//				the Feature will be reported as empty.
	// PROMISE: Returns true if collection of attributes is empty, false otherwise.
	// ARGS:	None.
	bool	isCurrentFeatureEmpty();

	//=============================================================================
	// PURPOSE: Check for Original Attributes empty
	// REQUIRE: Successful completion of OnOK().  If user cancelled from dialog,
	//				the Feature will be reported as empty.
	// PROMISE: Returns true if collection of attributes is empty, false otherwise.
	// ARGS:	None.
	bool	isOriginalFeatureEmpty();

// Dialog Data
	//{{AFX_DATA(CAttributeViewDlg)
	enum { IDD = IDD_ATTRIBUTEVIEW_DLG };
	CStatic	m_staticCurr;
	CButton	m_ok;
	CButton	m_cancel;
	CStatic	m_staticOrig;
	CStatic	m_staticDisplay;
	CListCtrl	m_listOriginal;
	CListCtrl	m_listCurrent;
	CButton	m_showHide;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CAttributeViewDlg)
	public:
	virtual void OnFinalRelease();
	virtual BOOL DestroyWindow();
	virtual int DoModal();
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	//=======================================================================
	// PURPOSE: Move buttons up or down and show/hide Original attributes
	//				while resizing the dialog as appropriate.
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	bFirst - No resize of dialog when called inside OnInit()
	void	shiftView(bool bFirst=false);

	//=======================================================================
	// PURPOSE: Define column labels and sizes for attribute grids.
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	void	prepareHeaders();

	//=======================================================================
	// PURPOSE: Parse information from provided Feature into specified List.
	// REQUIRE: Nothing
	// PROMISE: Returns false if Feature is NULL or has no Parts, otherwise
	//				true.
	// ARGS:	ptrFeature - Feature data input to List
	//				pList - Pointer to specified List control.
	bool	parseFeature(IUCLDFeaturePtr ptrFeature, CListCtrl* pList);

	//=======================================================================
	// PURPOSE: Parse information from specified Part into specified List.
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	ptrPart - Part with data elements to be stored.
	//				pList - Pointer to specified List control.
	void	parsePart(IPartPtr ptrPart, CListCtrl* pList);

	//=======================================================================
	// PURPOSE: Parse information from specified Segment into specified List.
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	ptrSegment - Line or Arc with data elements to be stored.
	//				pList - Pointer to specified List control.
	void	parseSegment(IESSegmentPtr ptrSegment, CListCtrl* pList);

	//=======================================================================
	// PURPOSE: Provide starting point data to the specified list item.
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	iIndex - Item number in specified list.
	//				ptrPoint - Start point with data elements to be stored.
	//				pList - Pointer to specified List control.
	void	setPoint(int iIndex, ICartographicPointPtr ptrPoint, 
		CListCtrl* pList);

	//=======================================================================
	// PURPOSE: Compute adjusted sizes for small and large versions of the 
	//				dialog.  The small size is used when showing just the 
	//				Current attributes.  The large size is used when showing 
	//				both Current and Original attributes.
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	bSmallFromLarge - If true, present dialog is large and method 
	//				will compute the smaller dialog size.  If false, present 
	//				dialog is small and method will compute the larger dialog 
	//				size.
	void	getDialogSizes(bool bSmallFromLarge);

	//=======================================================================
	// PURPOSE: To create the toolbar
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	void	createToolBar();

	//=======================================================================
	// PURPOSE: To update enable/disable status of toolbar buttons
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	void	doToolBarUpdates();

	//=======================================================================
	// PURPOSE: To update item and Part IDs throughout the list.  This method 
	//				should be called after each addition or deletion 
	//				operation.
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	pList - List control to be updated.
	void	updateView(CListCtrl* pList);

	//=======================================================================
	// PURPOSE: Determine if Edit is allowed for the objects selected in 
	//				the specified list.
	// REQUIRE: Nothing
	// PROMISE: Returns true if Edit is allowed, otherwise false.
	// ARGS:	pList - Pointer to specified List control.
	bool	canEditSelection(CListCtrl* pList);

	//=======================================================================
	// PURPOSE: Determine if Delete is allowed for the objects selected in 
	//				the specified list.
	// REQUIRE: Nothing
	// PROMISE: Returns true if Delete is allowed, otherwise false.
	// ARGS:	pList - Pointer to specified List control.
	bool	canDeleteSelection(CListCtrl* pList);

	//=======================================================================
	// PURPOSE: Determine if Append Part is allowed for the specified list.
	// REQUIRE: Nothing
	// PROMISE: Returns true if Append Part is allowed, otherwise false.
	// ARGS:	pList - Pointer to specified List control.
	//				bMenu - true if permission is being determined for 
	//				context menu, false if permission for toolbar
	bool	canAppendPart(CListCtrl* pList, bool bContextMenu = false);

	//=======================================================================
	// PURPOSE: Determine if Insert Line is allowed for the specified list.
	// REQUIRE: Nothing
	// PROMISE: Returns true if Insert Line is allowed, otherwise false.
	// ARGS:	pList - Pointer to specified List control.
	bool	canInsertLine(CListCtrl* pList);

	//=======================================================================
	// PURPOSE: Determine if Insert Curve is allowed for the specified list.
	// REQUIRE: Nothing
	// PROMISE: Returns true if Insert Curve is allowed, otherwise false.
	// ARGS:	pList - Pointer to specified List control.
	bool	canInsertCurve(CListCtrl* pList);

	//=======================================================================
	// PURPOSE: Determine number of Parts defined in the specified list.
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	pList - List control with Parts to be counted.
	int		getNumParts(CListCtrl* pList);

	//=======================================================================
	// PURPOSE: Determine number of Segments defined in the specified Part 
	//				and list.
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	pList - List control with Parts to be counted.
	//				iPart - Which Part within the List
	int		getNumSegments(CListCtrl* pList, int iPart);

	//=======================================================================
	// PURPOSE: Determine if the specified Part and list have more than one 
	//				Bearing among associated LineBearings and ChordBearings.
	// REQUIRE: Nothing
	// PROMISE: Returns true if two ro more distinct bearings were found, 
	//				otherwise false.
	// ARGS:	pList - List control with Parts to be evaluated.
	//				iPart - Which Part within the List
	bool	hasDistinctBearings(CListCtrl* pList, int iPart);

	//=======================================================================
	// PURPOSE: Return a Feature built from Parts defined in the specified 
	//				list.
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	iList - List control with items to be rolled into a Feature.
	//				1 = Current Attributes
	//				2 = Original Attributes
	IUCLDFeaturePtr	getFeature(int iList);

	//=======================================================================
	// PURPOSE: Return a Feature built from Parts defined in the specified 
	//				list.
	// REQUIRE: Nothing
	// PROMISE: Returns false with a non-zero error ID if the specified 
	//				Feature data is invalid, otherwise true.
	// ARGS:	pList - List control with items to be rolled into a Feature.
	//				piErrorStringID - ID of error's string table entry
	bool	validateFeature(CListCtrl* pList, int* piErrorStringID);

	//=======================================================================
	// PURPOSE: Adds an Arc segment to the specified Part
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	mapData - Collection of parameters from existing curve
	//				ptrPart - Part to receive the curve
	void	addCurveToPart(PTTVM mapData, IPartPtr ptrPart);

	//=======================================================================
	// PURPOSE: Retrieves a string from ItemData representing the specified 
	//				data.
	// REQUIRE: Nothing
	// PROMISE: true if data type was found in the map, otherwise false.
	// ARGS:	mapData - Collection of Type/Value pairs for the item
	//				eType - Desired data item
	//				rstrText - Reference to string to contain data value
	bool	getItemDataString(PTTVM mapData, ECurveParameterType eType, 
		std::string& rstrText);

	//=======================================================================
	// PURPOSE: Determines three original curve parameters, suitable for 
	//				Curve Calculator dialog.
	// REQUIRE: Nothing
	// PROMISE: Returns true if three suitable data types were found in the 
	//				map, otherwise false.
	// ARGS:	mapData - Reference to collection of Type/Value pairs for 
	//				the item
	//				peP1 - First parameter
	//				peP2 - Second parameter
	//				peP3 - Third parameter
	bool	getCurveParameters(PTTVM &mapData, ECurveParameterType* peP1, 
		ECurveParameterType* peP2, ECurveParameterType* peP3);

	//=======================================================================
	// PURPOSE: Deletes all items in the specified list that have been
	//				marked for deletion.
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	pList - List control with items to be rolled into a Feature.
	void	deleteMarked(CListCtrl* pList);


	// Generated message map functions
	//{{AFX_MSG(CAttributeViewDlg)
	afx_msg void OnShowhide();
	virtual void OnOK();
	virtual BOOL OnInitDialog();
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnInsertCurve();
	afx_msg void OnInsertLine();
	afx_msg void OnInsertPart();
	afx_msg void OnItemEdit();
	afx_msg void OnItemTransfer();
	afx_msg void OnItemView();
	afx_msg void OnItemDelete();
	afx_msg void OnItemClosure();
	afx_msg void OnSetfocusList1(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnSetfocusList2(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnClickList1(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnClickList2(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnKillfocusList1(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnKillfocusList2(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnKeydownList1(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnKeydownList2(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnContextMenu(CWnd* pWnd, CPoint point);
	afx_msg void OnDblclkList1(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnDblclkList2(NMHDR* pNMHDR, LRESULT* pResult);
	virtual void OnCancel();
	afx_msg void OnGetMinMaxInfo(MINMAXINFO* pMMI);
	//}}AFX_MSG
	afx_msg BOOL OnToolTipText(UINT nID, NMHDR* pNMHDR, LRESULT* pResult);
	DECLARE_MESSAGE_MAP()
	// Generated OLE dispatch map functions
	//{{AFX_DISPATCH(CAttributeViewDlg)
		// NOTE - the ClassWizard will add and remove member functions here.
	//}}AFX_DISPATCH
	DECLARE_DISPATCH_MAP()
	DECLARE_INTERFACE_MAP()

private:

	//////////
	// Methods
	//////////

	// convert distance string to have the current unit, or convert direction
	// to have the current direction format
	std::string convertDistanceOrDirection(ECurveParameterType eCurveType, const std::string& strInValue);

	// convert the input distance string into a distance string, which distance 
	// unit is the current distance unit
	std::string convertInCurrentDistanceUnit(const std::string& strDistance);

	// convert distance value in double to string with unit
	std::string distanceValueToString(double dDistanceInCurrentUnit);

	// get the closure report in a string
	void getClosureReport(std::ofstream& ofs);

	// based on the curve parameter type, give the full name
	std::string getCurveTypeName(ECurveParameterType eCurveParamType);

	// based on user specified number of decimal places, return the 
	// format string for the distance
	CString getDistanceFormatString();

	// get the error segment report
	// Return: true - there's a error segment, 
	//		   false -  the part is perfectly closed, there's no error segment
	bool getErrorSegmentReport(IPartPtr ipPart, std::string& strTotalError, std::ofstream& ofs);

	// get the segment info in a string. Content will be something like the segment number,
	// the segment type, the direction, length, etc.
	// nSegmentNumber - 1-based number
	void getSegmentReport(int nSegmentNumber, IESSegmentPtr ipSegment, std::ofstream& ofs);

	// given curve parameter type, is it a direction or a distance?
	bool isDirection(ECurveParameterType eType);
	bool isDistance(ECurveParameterType eType);


	///////////
	// Variables
	///////////
	// Toolbar object - providing View/Edit/Insert/Transfer functionality
	CToolBar		m_toolBar;

	// Determines whether the selected item should only be Viewed and 
	// not Edited
	bool			m_bOnlyViewItem;

	// Stores current visibility state of Original Attributes
	bool			m_bShowOriginalList;

	// Size computed as appropriate for dialog displaying only Current 
	// Attributes
	CSize			m_sizeSmall;

	// Size computed as appropriate for dialog displaying both Current and 
	// Original Attributes
	CSize			m_sizeLarge;

	// Have the dialog's controls been instantiated yet - allows for resize
	// and repositioning
	bool			m_bInitialized;

	// Number of pixels allowed for spacing between controls
	int				m_iControlSpace;

	// Used to determine enable and disable state for toolbar buttons
	// 0 - Neither list control has focus
	// 1 - Current Attributes list has focus
	// 2 - Original Attributes list has focus
	int				m_iListWithFocus;

	// Can Current Attributes be changed
	bool			m_bCurrentIsReadOnly;

	// Can Original Attributes be changed
	bool			m_bOriginalIsReadOnly;

	// Can Original Attributes be stored
	// False will result in a warning being displayed in the list
	bool			m_bCanStoreOriginalAttributes;

	// Was an Original Feature provided
	bool			m_bOriginalDefined;

	// Has Output Current Feature been calculated (and is valid)
	bool			m_bCurrentFeatureValid;

	// Has Output Original Feature been calculated (and is valid)
	bool			m_bOriginalFeatureValid;

	// Output Current Feature has been calculated and is empty
	bool			m_bCurrentFeatureEmpty;

	// Output Original Feature has been calculated and is empty
	bool			m_bOriginalFeatureEmpty;

	// Icon to be displayed on dialog's title bar
	HICON			m_hIcon;

	// Curve calculation engine object that computes other curve parameters 
	// from those given by the user
	CurveCalculationEngineImpl*	m_pEngine;		

	// Holds input data for Current Attributes
	IUCLDFeaturePtr		m_ptrCurrFeature;

	// Holds input data for Original Attributes
	IUCLDFeaturePtr		m_ptrOrigFeature;

	// Holds final data for Current Attributes
	IUCLDFeaturePtr		m_ptrFinalCurrFeature;

	// Holds final data for Original Attributes
	IUCLDFeaturePtr		m_ptrFinalOrigFeature;

	// Last item selected in current list
	int				m_iCurrentLastSelected;

	// Last item selected in original list
	int				m_iOriginalLastSelected;

	// Handles configuration persistence
	CfgAttributeViewer*	m_pCfg;			

	// Distance object for unit conversions
	DistanceCore	m_distance;

	DirectionHelper m_directionHelper;

	Bearing m_bearing;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
