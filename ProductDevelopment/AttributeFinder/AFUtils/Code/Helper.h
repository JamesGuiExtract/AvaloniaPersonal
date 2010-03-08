#pragma once

#include <string>
using namespace std;

// for the given entry zEntry, check to see if the same value
// already exists in the list control
bool findDuplicateEntry(const CString& zEntry, ATLControls::CListViewCtrl lst);

// Gives a prompt dialog for entering entry value into the given
// list view control
// Return true if the user enters a valid value.
// Return false if the user dismisses the prompt for value dialog
// The third param is the header of the file name, used to judge if the string is a file name
// The fourth param is the current selected item's index in the list box, -1 means adding a new item
bool promptForValue(CString& zEntry, ATLControls::CListViewCtrl lst, const CString zHeader = "",
					int nItemIndex = 0, bool bValidateIdentifier = false);

// Return true if the first entry in the list box is a file name with proper header
bool isFirstEntryDynamicFile(ATLControls::CListViewCtrl lst, const CString zHeader);

// Extract a long value from the specified edit box. If strErrorIfBlank is specified 
// and the edit box is blank, it will fail validation and strErrorIfBlank will be assigned 
// as the text of the resulting UCLIDException
long verifyControlValueAsLong(ATLControls::CEdit &rctrl, int nDefaultValue = 0,
							  const string &strErrorIfBlank = "");

// Extract a long value from the specified edit box and ensure it falls within the range specificied
// by nMinimum and nMaximum. If the value does not fall within the specified range, strErrorIfOutsideRange
// will be assigned as the text of the resulting UCLIDException.  If strErrorIfBlank is specified and the 
// edit box is blank, it will fail validation and strErrorIfBlank will be assigned as the text of the 
// resulting UCLIDException.
long verifyControlValueAsLong(ATLControls::CEdit &rctrl, int nMinimum, int nMaximum, 
							  const string &strErrorIfOutsideRange, int nDefaultValue = 0,
							  const string &strErrorIfBlank = "");

// Extract a double value from the specified edit box. If strErrorIfBlank is specified 
// and the edit box is blank, it will fail validation and strErrorIfBlank will be assigned 
// as the text of the resulting UCLIDException
double verifyControlValueAsDouble(ATLControls::CEdit &rctrl, double dDefaultValue = 0,
								  const string &strErrorIfBlank = "");

// Extract a double value from the specified edit box and ensure it falls within the range specificied
// by nMinimum and nMaximum. If the value does not fall within the specified range, strErrorIfOutsideRange
// will be assigned as the text of the resulting UCLIDException.  If strErrorIfBlank is specified and the 
// edit box is blank, it will fail validation and strErrorIfBlank will be assigned as the text of the 
// resulting UCLIDException.
double verifyControlValueAsDouble(ATLControls::CEdit &rctrl, double dMinimum, double dMaximum, 
								  const string &strErrorIfOutsideRange, double dDefaultValue = 0,
								  const string &strErrorIfBlank = "");

// Extract a BSTR value from the specified edit box. If strErrorIfBlank is specified 
// and the edit box is blank, it will fail validation and strErrorIfBlank will be assigned 
// as the text of the resulting UCLIDException.
BSTR verifyControlValueAsBSTR(ATLControls::CEdit &rctrl, const string &strErrorIfBlank = "");
