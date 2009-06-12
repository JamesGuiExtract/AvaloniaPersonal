#pragma once

#include "FiltersDLL.h"

#include <afxmt.h>
#include <vector>
#include <string>

class TPPoint; 

class EXT_FILTERS_DLL AbstractMeasurement
{
public:
	// order is important to CAlternateSelectionsDlg::OnInitDialog()
	enum EType { kUnknown, kBearing, kDistance, kAngle };
	
	virtual ~AbstractMeasurement(void){};
	//----------------------------------------------------------------------------------------------
	// Purpose: To create a new instance of the measurement object.
	//
	// Require: Nothing.
	//
	// Promise: Returns the a newly created instance of the measurement object.  Each derived
	//			class is expected to override this method and return a newly created instance
	//			of itself when this method is called.  The caller is responsible to delete
	//			the newly created object.
	virtual AbstractMeasurement* createNew() = 0;
	//----------------------------------------------------------------------------------------------
	// Purpose: To get the type of the measurement.
	//
	// Require: Nothing.
	//
	// Promise: Returns the type of the measurement.  
	virtual EType getType() const = 0;
	//----------------------------------------------------------------------------------------------
	// Purpose: To get the type of the measurement in stringized form
	//
	// Require: Nothing.
	//
	// Promise: Returns the type of the measurement in stringized form
	virtual const std::string& getTypeAsString() const = 0;
	//----------------------------------------------------------------------------------------------
	// Purpose: To guess the most appropriate type of abstract measurement for strText.
	//
	// Require: Nothing.
	//
	// Promise: Returns the guessed type.  	
	static EType guessType(const std::string &strText);
	//----------------------------------------------------------------------------------------------
	// Purpose: To initialize this instance of the abstract measurement class.
	//
	// Require: Nothing.
	//
	// Promise: If pszText is a valid string, then isValid() shall return true.
	//			If pszAngle is not a valid angle string, then isValid() shall return false.	
	virtual void evaluate(const char *pszText) = 0;
	//----------------------------------------------------------------------------------------------
	// Purpose: To initialize this instance of the abstract measurement class.
	//
	// Require: Depends upon implementation of base class.
	//
	// Promise: if the measurement can be determined from the points p1 and p2, isvalid()
	//			shall return true.  Otherwise, isValid shall return false.
	//			NOTE: the meaning of p1 and p2, and how that is related to the measurement 
	//			depends upon the implementation of the derived class.  For instance, a derived
	//			class called "angle" may evaluate the angle from p1 to p2, while a derived
	//			class called "distance" may evaluate the distance from p1 to p2.
	//			Note: the default implementation does nothing.
	virtual void evaluate(const TPPoint &p1, const TPPoint & p2);
	//----------------------------------------------------------------------------------------------
	// Purpose: To clear the status of the measurement.
	//
	// Require: Nothing
	//
	// Promise: Returns the measurement to exactly the same state as it is when you first create it 
	//			with a default constructor.
	virtual void resetVariables() = 0;	
	//----------------------------------------------------------------------------------------------
	// Purpose: To determine the internal status of this instance of the abstract measurement class.
	//
	// Require: Nothing.
	//
	// Promise: To return true, if a valid string has been passed to the evaluate function and 
	//			if the string has been successfully parsed. Promise to return false otherwise.
	virtual bool isValid(void) = 0;
	//----------------------------------------------------------------------------------------------
	// Purpose: If the string that was evaluated was not valid, then this function should
	//			return a list of possible alternate strings that would make the original
	//			string valid.  These options will be shown to the user in a dialog box, and the
	//			user will be requested to pick from one of these options.
	//
	// Require: Nothing.
	//
	// Promise: If isValid() is true, then the vector must contain one string which has the value
	//			asString().  If isValid() is false, then the vector should contain possible
	//			alternatives that would make the entered string valid (This method shall
	//			therefore take into account all the different spots where a typo could have been 
	//			made or the OCR program could have recognized the measurement wrongly.)	
	virtual std::vector<std::string> getAlternateStrings(void) = 0;
	//----------------------------------------------------------------------------------------------
	// Purpose: To return this instance of the measurement class in a string format.
	//
	// Require: isValid() == true;
	// 
	// Promise: To return this instance of the measurement class in a standard string format if 
	//			isValid() == true;
	//			If isValid() == false, then promise to throw string("ERROR: Invalid angle!");
	virtual std::string asString(void) = 0;
	//----------------------------------------------------------------------------------------------
	// Purpose: To return this instance of the measurement class in a string format which reflects 
	//			the actual value output from the filters.
	//
	// Require: isValid() == true;
	// 
	// Promise: To return this instance of the measurement class in a standard string format if 
	//			isValid() == true;
	//			If isValid() == false, then promise to throw string("ERROR: Invalid angle!");
	virtual std::string interpretedValueAsString(void) = 0;
	//----------------------------------------------------------------------------------------------
	// Purpose: Set which positions in pszText are invalid
    //
    // Require: nothing
    //
    // Promise: vecInvalidPositions will contain all the invalid in pszText
    //          and nothing else. 
	void setInvalidPositions(const char *pszText);
    //----------------------------------------------------------------------------------------------
	// Purpose: To indicate which characters in the original string were invalid or missing.
    //
    // Require: Nothing
    //
    // Promise: If a string has been evaluated, return the indices into the string each indicating
    //          a position in the string that is an invalid character. 
	std::vector<int> getInvalidPositions() const;

	//----------------------------------------------------------------------------------------------
	//Purpose: To set the previous mode
	//
	//Require: Pass boolean variable
	//
	//Promise: To set the m_sbPrevReverseMode
	static void setPrevInReverseMode(bool bMode)
	{
		CSingleLock lg(&m_sReverseModeMutex, TRUE);

		m_sbPrevReverseMode = bMode;
	}

	//----------------------------------------------------------------------------------------------
	//Purpose: To make class work in bMode mode
	//
	//Require: Pass boolean variable
	//
	//Promise: To set the bReverseMode
	static void workInReverseMode(bool bMode)
	{
		CSingleLock lg(&m_sReverseModeMutex, TRUE);

		m_sbReverseMode = bMode;
	}

	//----------------------------------------------------------------------
	//Purpose: To make class work in opposite mode 
	//
	//Require: isValid() == true
	//
	//Promise: To set bReverseMode to opposite
	static void toggleReverseMode()
	{
		CSingleLock lg(&m_sReverseModeMutex, TRUE);

		if(m_sbReverseMode == true)
		{
			m_sbReverseMode = false;
		}
		else
		{
			m_sbReverseMode = true;
		}
	}
	//----------------------------------------------------------------------
	//Purpose: To determine Previous Reverse Mode
	//
	//Require: Nothing
	//
	//Promise: return true if m_sbPrevReverseMode is true
	static bool isPrevInReverseMode()
	{
		CSingleLock lg(&m_sReverseModeMutex, TRUE);

		return m_sbPrevReverseMode;
	}

	//----------------------------------------------------------------------
	//Purpose: To determine ReverseMode
	//
	//Require: Nothing
	//
	//Promise: return true if bReverseMode is true
	static bool isInReverseMode()
	{
		CSingleLock lg(&m_sReverseModeMutex, TRUE);

		return m_sbReverseMode;
	}
	//-----------------------------------------------------------------------

	static std::string checkTilde(std::string str);
	//erase tilde sign from str 

	AbstractMeasurement& operator=(const AbstractMeasurement& valueToAssign);

protected:

	// do generic pre-processing such as removing the degree symbol and replacing
	// it with something else, etc. It's only used for OCR corrections
	virtual void preProcessString(std::string& strStringToProcess);

private:
	// Mutex to protect ReverseMode across classes
	static CMutex m_sReverseModeMutex;
	//something for reverseMode
	static bool m_sbReverseMode;  //static variable
	// static variable to memories the previous mode
	static bool m_sbPrevReverseMode; 

	std::vector<int> vecInvalidPositions; // holds all invalid positions in the string
};

class EXT_FILTERS_DLL ReverseModeValueRestorer
{
public:
	ReverseModeValueRestorer()
	{
		m_bInitialValue = AbstractMeasurement::isInReverseMode();
	}

	~ReverseModeValueRestorer()
	{
		AbstractMeasurement::workInReverseMode(m_bInitialValue);
	}

private:
	bool m_bInitialValue;
};