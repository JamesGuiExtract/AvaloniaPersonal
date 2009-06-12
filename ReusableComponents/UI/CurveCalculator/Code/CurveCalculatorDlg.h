//=============================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	CurveCalculatorDlg.h
//
// PURPOSE:	Header file for main dialog class in Curve Calculator application.
//
// NOTES:	None
//
// AUTHOR:	Wayne Lenius
//
//=============================================================================
#pragma once

#include "resource.h"
#include "CurveCalculator.h"

#include <ECurveParameter.h>
#include <vector>

// Forward declarations
class ICurveCalculationEngine;
class CurveDjinni;

//=============================================================================
//
// CLASS:	CCurveCalculatorDlg
//
// PURPOSE:	Provides the main UI for the CurveCalculator application.
//
// REQUIRE:	MFC.
// 
// INVARIANTS:
//			None.
//
// EXTENSIONS:
//			None.
//
// NOTES:	None.
//
//=============================================================================
class EXPORT_CurveCalculator CCurveCalculatorDlg : public CDialog
{
// Construction
public:
	//=============================================================================
	// PURPOSE: Constructs a dialog object without selecting any curve parameters
	//				or values.
	// REQUIRE: None.
	// PROMISE: None.
	// ARGS:	
	// 			bHideUnits: group box and radio buttons for units are hidden
	// 			bHideOK: OK button is hidden, Cancel button says Close
	CCurveCalculatorDlg(bool bHideUnits = false, 
		bool bHideOK = false, CWnd* pParent = NULL);	// standard constructor

	//=============================================================================
	// PURPOSE: Constructs a dialog object using the preselected curve parameters
	//				and values.  Angle concavity and size are also specified.
	//				Units may or may not be specified
	// REQUIRE: None.
	// PROMISE: None.
	// ARGS:	eSelect1: enumeration of the first curve parameter
	//				pszString1: first curve parameter's value
	//				eSelect2: enumeration of the second curve parameter
	//				pszString2: second curve parameter's value
	//				eSelect3: enumeration of the third curve parameter
	//				pszString3: third curve parameter's value
	//				iConcavity: defines whether curve opens left or right
	//					-1 = not required
	//					0  = concave right
	//					1  = concave left
	//				iAngle: defines whether delta angle > or < PI
	//					-1 = not required
	//					0  = > PI
	//					1  = < PI
	//				bHideUnits: group box and radio buttons for units are hidden
	// 				bHideOK: OK button is hidden, Cancel button says Close
	CCurveCalculatorDlg(ECurveParameterType eSelect1, LPCTSTR pszString1, 
		ECurveParameterType eSelect2, LPCTSTR pszString2, ECurveParameterType eSelect3, 
		LPCTSTR pszString3, int iConcavity, int iAngle,
		bool bHideUnits = false, bool bHideOK = false, CWnd* pParent = NULL);

	//=============================================================================
	// PURPOSE: Provides curve parameter selected in combo box #1, #2, or #3.
	// REQUIRE: None.
	// PROMISE: None.
	// ARGS:	iItem: which combo box.  Valid inputs = {1, 2, 3}
	ECurveParameterType	getParameter(int iItem);

	//=============================================================================
	// PURPOSE: Provides curve parameter value provided in edit box #1, #2, or #3.
	// REQUIRE: None.
	// PROMISE: None.
	// ARGS:	iItem: which edit box.  Valid inputs = {1, 2, 3}
	std::string		getParameterValue(int iItem);

	//=============================================================================
	// PURPOSE: Provides last input value of angle concavity
	// REQUIRE: None.
	// PROMISE: Return value will be:
	//				-1 : Concavity selection not required
	//					0  = concave right
	//					1  = concave left
	// ARGS:	None.
	int				getConcavity();

	//=============================================================================
	// PURPOSE: Provides last input value of angle size
	// REQUIRE: None.
	// PROMISE: Return value will be:
	//				-1 : Angle size selection not required
	//				0  : Angle is less than PI
	//				1  : Angle is greater than PI
	// ARGS:	None.
	int				getAngleSize();

	//=============================================================================
	// PURPOSE: Provides last input value of distance units
	// REQUIRE: None.
	// PROMISE: Return value will be:
	//				true  : Units are feet
	//				false : Units are meters
	// ARGS:	None.
	bool			isUnitsFeet();

// Dialog Data
	//{{AFX_DATA(CCurveCalculatorDlg)
	enum { IDD = IDD_CURVECALCULATOR_DIALOG };
	CEdit	m_edit3;
	CEdit	m_edit2;
	CEdit	m_edit1;
	CComboBox	m_cbnCombo3;
	CComboBox	m_cbnCombo2;
	CComboBox	m_cbnCombo1;
	int		m_nDefaultUnits;
	int		m_nConcavity;
	int		m_nDeltaAngle;
	CString	m_cstrOutput;
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CCurveCalculatorDlg)
	public:
	virtual BOOL DestroyWindow();
	virtual int DoModal();
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	HICON m_hIcon;

	enum EUsedParameterID
	{
		kUnused = 0,
		kParameter1,
		kParameter2,
		kParameter3
	};

	typedef struct 
	{
		ECurveParameterType	eCurveParameterID;
		std::string			strParameterDescription;
		EUsedParameterID	eUsed;
	} ParameterControlBlock;

	typedef std::vector<ParameterControlBlock> PCBCollection;

	// collection of curve parameters that can be displayed in the combo boxes
	PCBCollection m_vecPCB;

	// the curve calculation engine object that computes other curve parameters 
	// from those given by the user
	ICurveCalculationEngine*	m_pEngine;		

	// the CurveDjinni object that determines which parameters are required to 
	// completely specify a curve
	CurveDjinni*				m_pDjinni;		
	
	// Parameter selected from first combo box 
	// that limits available parameters in the second combo box
	ECurveParameterType			m_eCombo1Selection;
	
	// Parameter selected from second combo box
	// that further limits available parameters in the third combo box
	ECurveParameterType			m_eCombo2Selection;
	
	// Parameter selected from third combo box
	ECurveParameterType			m_eCombo3Selection;

	// Curve orientation must be selected in order to completely specify a curve
	// Default is false
	bool	m_bCurveEnable;		

	// Angle size must be selected in order to completely specify a curve
	// Default is false
	bool	m_bAngleEnable;		
	
	// Selection and string parameters should be automatically cleared during 
	// dialog initialization
	// Default is true
	bool	m_bReset;

	// Hide group box and radio buttons that allow the user to specify units
	// Default is false
	bool	m_bHideUnits;

	// Hide OK button and change text of Cancel button to Close
	// Default is false
	bool	m_bHideOK;

	// Storage for strings passed in to constructor before OnInitDialog()
	// Also used for final strings after exit
	std::string	m_strEdit1;
	std::string	m_strEdit2;
	std::string	m_strEdit3;

	// Generated message map functions
	//{{AFX_MSG(CCurveCalculatorDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnSelchangeCombo1();
	afx_msg void OnSelchangeCombo2();
	afx_msg void OnSelchangeCombo3();
	afx_msg void OnCalculate();
	afx_msg void OnChangeEdit1();
	afx_msg void OnChangeEdit2();
	afx_msg void OnChangeEdit3();
	afx_msg void OnRadioLeft();
	afx_msg void OnRadioRight();
	afx_msg void OnRadioLesser();
	afx_msg void OnRadioGreater();
	afx_msg void OnRadioFeet();
	afx_msg void OnRadioMeters();
	afx_msg void OnOK();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
	
	////////////////////////
	// General setup methods
	////////////////////////
private:	
	//=============================================================================
	// PURPOSE: To initialize the first combo box and clear and disable the other 
	//				two.  The associated edit boxes are also cleared.  The second
	//				and third edit boxes are disabled.
	// REQUIRE: Called by OnInitDialog()
	// PROMISE: Will populate first combo box with curve parameters of type: angle, 
	//				bearing, and distance.  Note that location and boolean 
	//				parameters will not be added.
	// ARGS:	None.
	void	resetCombos(void);
	
	//=============================================================================
	// PURPOSE: To enable or disable all radio buttons as appropriate.  Also 
	//				enables or disables the associated group box objects.
	// REQUIRE: Called by OnInitDialog() and when any combo box selection changes.
	//				The associated enable/disable flags must already be set.
	//				m_bCurveEnable
	//				m_bAngleEnable
	// PROMISE: Will enable or disable radio buttons and group boxes for curve 
	//				orientation and angle size.  Will examine units selection 
	//				radio buttons and set units flags appropriately.
	//				m_bMeters
	// ARGS:	None.
	void	enableRadios(void);
	
	/////////////////
	// Helper methods
	/////////////////
	//=============================================================================
	// PURPOSE: Determines whether or not the Calculate button can be enabled.  It
	//				will be enabled if and only if all the required curve 
	//				parameters have been defined according to the CurveDjinni 
	//				object.
	// REQUIRE: Called when the third combo box has changed selection, when any of 
	//				the three edit boxes have been changed, when a curve
	//				orientation has been selected, or when an angle size has been
	//				selected.
	// PROMISE: Enables or disables the Calculate button.
	// ARGS:	None.
	void	checkParametersDefined(void);

	//=============================================================================
	// PURPOSE: Provides the specified curve parameter value to the curve
	//				calculation engine.
	// REQUIRE: Engine must be valid.  Location parameters are not supported.
	// PROMISE: None.
	// ARGS:	eParam: enumeration of the particular curve parameter
	//				dValue: the value
	void	setCurveEngineParameter(ECurveParameterType eParam, double dValue);

	//=============================================================================
	// PURPOSE: Parses the specified curve parameter value with the Filters DLL.
	// REQUIRE: Location parameters are not supported.
	// PROMISE: Returns false with -1.0 stored in pdValue if object is invalid 
	//				after parsing, otherwise true.
	// ARGS:	eParam: enumeration of the particular curve parameter
	//				zStr: string to be parsed by the Filters DLL (in degrees)
	//				pdValue: pointer to double that will hold the value in radians
	// NOTES:	Code modified 10/12/01 to return Value in radians per the 
	//				associated update to the Curve Calculation Engine
	bool	getParsedParameterValue(ECurveParameterType eParam, CString zStr, 
		double* pdValue);

	//=============================================================================
	// PURPOSE: Sends the string associated with a selected curve parameter to
	//				the curve calculation engine.
	// REQUIRE: Engine must be initialized.
	// PROMISE: Returns false and sets focus to string if parsing failed, 
	//				otherwise true.
	// ARGS:	eParam: enumeration of the particular curve parameter
	//				iEditControlID: ID of edit control containing string to send
	bool	sendParameterValue(ECurveParameterType eParam, int iEditControlID);

	//=============================================================================
	// PURPOSE: Provides a string description for the specified curve parameter.
	// REQUIRE: None.
	// PROMISE: None.
	// ARGS:	eParam: enumeration of the particular curve parameter
	std::string	describeCurveParameter(ECurveParameterType eParam);

	//=============================================================================
	// PURPOSE: Initializes the PCBCollection data member.
	// REQUIRE: None.
	// PROMISE: Each item in the vector will: 
	//				- contain a curve parameter ID
	//				- contain a description string
	//				- be set to Unused.
	// ARGS:	None.
	void	createParameterControlBlock(void);

	//=============================================================================
	// PURPOSE: Populates a specific combo box with appropriate curve parameter
	//				strings.  Uses the Curve Djinni matrix to determine presence 
	//				or absence of each parameter.  The matrix will be filtered if 
	//				either 1 or 2 parameters have already been chosen.
	// REQUIRE: None.
	// PROMISE: None.
	// ARGS:	rComboBox: combo box to be populated
	//				iNumSelectionsMade: number of curve parameters already chosen
	//				{0, 1, 2}
	void resetDropDownList(CComboBox& rComboBox, int iNumSelectionsMade);

	//=============================================================================
	// PURPOSE: Sets the eUsed field to kUnused for whichever curve parameter
	//				had been used by the specified ID.
	// REQUIRE: None.
	// PROMISE: None.
	// ARGS:	eID: which combo selection is being cleared
	void	clearUsedParameter(EUsedParameterID eID);

	//=============================================================================
	// PURPOSE: Sets the eUsed field to the specified parameter ID for the given
	//				curve parameter.
	// REQUIRE: None.
	// PROMISE: None.
	// ARGS:	eParam: which curve parameter
	//				eID: which combo selection is being set
	void	setUsedParameter(ECurveParameterType eParam, EUsedParameterID eID);

	//=============================================================================
	// PURPOSE: Provides an output string description for the specified curve 
	//				parameter.  Format is: "Label: Value<CR>". Value we be 
	//				converted to DMS for angle parameters or a bearing for bearing 
	//				parameters.
	// REQUIRE: canCalculateAllParameters() in the Curve Calculation Engine must 
	//				have already been called.
	// PROMISE: None.
	// ARGS:	eParam: enumeration of the particular curve parameter
	std::string	getOutputString(ECurveParameterType eParam);

	//=============================================================================
	// PURPOSE: Selects the specified curve parameter within the specified combo
	//				box.
	// REQUIRE: The specified combo box must have already been populated using 
	//				resetDropDownList().
	// PROMISE: None.
	// ARGS:	rComboBox: combo box to be populated
	//				eParam: enumeration of the particular curve parameter
	void	selectComboItem(CComboBox& rComboBox, ECurveParameterType eParam);

	//=============================================================================
	// PURPOSE: Validates the collected input parameters using the Curve 
	//				Calculation Engine data member.
	// REQUIRE: The Curve Calculation Engine must have already been initialized.
	// PROMISE: None.
	// ARGS:	eSelect1: enumeration of the first curve parameter
	//				pszString1: first curve parameter's value
	//				eSelect2: enumeration of the second curve parameter
	//				pszString2: second curve parameter's value
	//				eSelect3: enumeration of the third curve parameter
	//				pszString3: third curve parameter's value
	//				iConcavity: defines whether curve opens left or right
	//					-1 = not required
	//					0  = concave right
	//					1  = concave left
	//				iAngle: defines whether delta angle > or < PI
	//					-1 = not required
	//					0  = > PI
	//					1  = < PI
	//				bUnitsFeet: units are feet or meters
	bool	isInputValid(ECurveParameterType eSelect1, LPCTSTR pszString1, 
		ECurveParameterType eSelect2, LPCTSTR pszString2, ECurveParameterType eSelect3, 
		LPCTSTR pszString3, int iConcavity, int iAngle);

	//=============================================================================
	// PURPOSE: Checks if parameters needed for output display can be calculated 
	//				by the CCE.
	// REQUIRE: Engine must be initialized.
	// PROMISE: Returns false if any displayed parameter cannot be calculated, 
	//				otherwise true.
	// ARGS:	None.
	bool	canCalculateNeededParameters();
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

