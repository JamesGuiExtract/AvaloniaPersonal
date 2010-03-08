#include "stdafx.h"
#include "Helper.h"

#include <PromptDlg.h>
#include <ComUtils.h>
#include <UCLIDException.h>

const int NUM_OF_CHARS = 4096;

//--------------------------------------------------------------------------------------------------
bool findDuplicateEntry(const CString& zEntry, ATLControls::CListViewCtrl lst)
{
	// go through all entries in the list
	int nSize = lst.GetItemCount();
	for (int n=0; n<nSize; n++)
	{
		// get individual list item
		char pszValue[NUM_OF_CHARS];
		lst.GetItemText(n, 0, pszValue, NUM_OF_CHARS);
		
		// do a case sensitive comparison
		if (strcmp(pszValue, zEntry) == 0)
		{
			// we found a duplicate
			return true;
		}
	}

	return false;
}
//--------------------------------------------------------------------------------------------------
bool promptForValue(CString& zEntry, ATLControls::CListViewCtrl lst, const CString zHeader,
					int nItemIndex, bool bValidateIdentifier)
{
	CString	zCopy(zEntry);

	PromptDlg promptDlg("Text", "Specify the text : ", zEntry);

	while (true)
	{
		int nRes = promptDlg.DoModal();
		if (nRes == IDOK)
		{
			zEntry = promptDlg.m_zInput;
			if (zEntry.IsEmpty())
			{
				AfxMessageBox("Please provide non-empty string.");
				continue;
			}

			if (bValidateIdentifier)
			{
				try
				{
					validateIdentifier((LPCTSTR) zEntry);
				}
				catch(...)
				{
					CString zMsg("");
					zMsg.Format("<%s> is not a valid identifier. Please specify another identifier.", zEntry);
					AfxMessageBox(zMsg);
					continue;
				}
			}
			// If header string is specified, we need to check if the string is a file name
			if (zHeader != "")
			{
				// Get the length of the file header
				int iLengthOfHeader = zHeader.GetLength();
				// Get the first iLengthOfHeader characters of the string
				CString zFileHeader = zEntry.Left(iLengthOfHeader);

				// If the string in the first edit box is a valid file name (has a valid header), and
				// this string doesn't contain only the header, it will not be treated as a file name
				if (zFileHeader.CompareNoCase(zHeader) == 0 && zEntry.CompareNoCase(zHeader) != 0)
				{
					// We need to check if we can put a file name into the list box
					// 1. If there is more than one items inside the list box
					// 2. If there is only one item inside the list box but the user want to add a file name to the list
					// we can not add a file name into list box in the above two cases
					if (lst.GetItemCount() > 1 || 
						(lst.GetItemCount() == 1 && nItemIndex == -1))
					{
						// Create prompt string
						CString zPrompt = "If a file name is specified for dynamically loading strings, \nit should be the only entry in the list box.\n\n Do you want to overwrite the existing entries with this file name?";

						int iResult;
						iResult = MessageBox(NULL, (LPCTSTR)zPrompt, "Confirm file selection", MB_YESNO|MB_ICONINFORMATION);
						if (iResult == IDYES)
						{
							// Delete all items in the list box
							lst.DeleteAllItems();
						}
						else
						{
							continue;
						}
					}
				}
			}

			// Check to see if the text changed
			if (zEntry.Compare(zCopy) == 0)
			{
				// User did not change the string, just return false
				return false;
			}

			// check whether or not to entered the zEntry already exists in the lst
			if (findDuplicateEntry(zEntry, lst))
			{
				CString zMsg("");
				zMsg.Format("<%s> already exists in the list. Please specify another clue text", zEntry);
				AfxMessageBox(zMsg);
				continue;
			}
			
			return true;
		}

		break;
	}

	return false;
}
//--------------------------------------------------------------------------------------------------
bool isFirstEntryDynamicFile(ATLControls::CListViewCtrl lst, const CString zHeader)
{
	// Get the string in the first row and first column
	char pszValue[NUM_OF_CHARS];
	lst.GetItemText(0, 0, pszValue, NUM_OF_CHARS);
	CString zFirstRowCol(pszValue);

	// If the string in the first row and first column of the list box is a valid file name
	// and if the string in the first row and first column of the list box doesn't contain only the header
	CString zTemp = zFirstRowCol.Left(zHeader.GetLength());
	if (zTemp.CompareNoCase(zHeader) == 0 && zFirstRowCol.CompareNoCase(zHeader) != 0)
	{
		return true;
	}

	return false;
}
//--------------------------------------------------------------------------------------------------
long verifyControlValueAsLong(ATLControls::CEdit &rctrl, int nMinimum, int nMaximum, 
							  const string &strErrorIfOutsideRange, int nDefaultValue/* = 0*/,
							  const string &strErrorIfBlank/* = ""*/)
{
	// Obtain control value
	CComBSTR bstrValue;
	rctrl.GetWindowText(&bstrValue);

	if (bstrValue.Length() == 0)
	{
		// control value is blank

		if (strErrorIfBlank.empty())
		{
			// blank value is allowed; return default value
			return nDefaultValue;
		}
		else
		{
			// blank value is  not allowed; give focus to offending control and throw exception
			rctrl.SetSel(0, -1);
			rctrl.SetFocus();

			UCLIDException ue("ELI19756", strErrorIfBlank);
			throw ue;
		}
	}
	else
	{
		try
		{
			// Get value a long
			int nValue = asLong(asString(bstrValue));

			// Ensure the value falls in the range nMinimum to nMaximum
			if (nValue < nMinimum || nValue > nMaximum)
			{
				// Value is outside of range; throw exception
				UCLIDException ue("ELI19757", strErrorIfOutsideRange);
				ue.addDebugInfo("Specified value", asString(nValue));
				ue.addDebugInfo("Control minimum", asString(nMinimum));
				ue.addDebugInfo("Control maximum", asString(nMaximum));
				throw ue;
			}

			return nValue;
		}
		catch (...)
		{
			// If the conversion failed, give focus to offending control and throw exception
			rctrl.SetSel(0, -1);
			rctrl.SetFocus();

			throw;
		}
	}
}
//-------------------------------------------------------------------------------------------------
long verifyControlValueAsLong(ATLControls::CEdit &rctrl, int nDefaultValue/* = 0*/,
							  const string &strErrorIfBlank/* = ""*/)
{
	// Obtain control value
	CComBSTR bstrValue;
	rctrl.GetWindowText(&bstrValue);

	if (bstrValue.Length() == 0)
	{	
		// control value is blank

		if (strErrorIfBlank.empty())
		{
			// blank value is allowed; return default value
			return nDefaultValue;
		}
		else
		{
			// blank value is  not allowed; give focus to offending control and throw exception
			rctrl.SetSel(0, -1);
			rctrl.SetFocus();

			UCLIDException ue("ELI18946", strErrorIfBlank);
			throw ue;
		}
	}
	else
	{
		try
		{
			// return value a long
			return asLong(asString(bstrValue));
		}
		catch (...)
		{
			// If the conversion failed, give focus to offending control and throw exception
			rctrl.SetSel(0, -1);
			rctrl.SetFocus();

			throw;
		}
	}
}
//--------------------------------------------------------------------------------------------------
double verifyControlValueAsDouble(ATLControls::CEdit &rctrl, double dMinimum, double dMaximum, 
							      const string &strErrorIfOutsideRange, double dDefaultValue/* = 0*/,
							      const string &strErrorIfBlank/* = ""*/)
{
	// Obtain control value
	CComBSTR bstrValue;
	rctrl.GetWindowText(&bstrValue);

	if (bstrValue.Length() == 0)
	{
		// control value is blank

		if (strErrorIfBlank.empty())
		{
			// blank value is allowed; return default value
			return dDefaultValue;
		}
		else
		{
			// blank value is  not allowed; give focus to offending control and throw exception
			rctrl.SetSel(0, -1);
			rctrl.SetFocus();

			UCLIDException ue("ELI22316", strErrorIfBlank);
			throw ue;
		}
	}
	else
	{
		try
		{
			// Get value a double
			double dValue = asDouble(asString(bstrValue));

			// Ensure the value falls in the range nMinimum to nMaximum
			if (dValue < dMinimum || dValue > dMaximum)
			{
				// Value is outside of range; throw exception
				UCLIDException ue("ELI22317", strErrorIfOutsideRange);
				ue.addDebugInfo("Specified value", asString(dValue));
				ue.addDebugInfo("Control minimum", asString(dMinimum));
				ue.addDebugInfo("Control maximum", asString(dMaximum));
				throw ue;
			}

			return dValue;
		}
		catch (...)
		{
			// If the conversion failed, give focus to offending control and throw exception
			rctrl.SetSel(0, -1);
			rctrl.SetFocus();

			throw;
		}
	}
}
//-------------------------------------------------------------------------------------------------
double verifyControlValueAsDouble(ATLControls::CEdit &rctrl, double dDefaultValue/* = 0*/,
								  const string &strErrorIfBlank/* = ""*/)
{
	// Obtain control value
	CComBSTR bstrValue;
	rctrl.GetWindowText(&bstrValue);

	if (bstrValue.Length() == 0)
	{	
		// control value is blank

		if (strErrorIfBlank.empty())
		{
			// blank value is allowed; return default value
			return dDefaultValue;
		}
		else
		{
			// blank value is  not allowed; give focus to offending control and throw exception
			rctrl.SetSel(0, -1);
			rctrl.SetFocus();

			UCLIDException ue("ELI22318", strErrorIfBlank);
			throw ue;
		}
	}
	else
	{
		try
		{
			// return value a double
			return asDouble(asString(bstrValue));
		}
		catch (...)
		{
			// If the conversion failed, give focus to offending control and throw exception
			rctrl.SetSel(0, -1);
			rctrl.SetFocus();

			throw;
		}
	}
}
//--------------------------------------------------------------------------------------------------
BSTR verifyControlValueAsBSTR(ATLControls::CEdit &rctrl, const string &strErrorIfBlank/* = ""*/)
{
	// Obtain control value
	CComBSTR bstrValue;
	rctrl.GetWindowText(&bstrValue);

	if (bstrValue.Length() == 0)
	{	
		// control value is blank

		if (strErrorIfBlank.empty())
		{
			// blank value is allowed
			return bstrValue;
		}
		else
		{
			// blank value is  not allowed; give focus to offending control and throw exception
			rctrl.SetSel(0, -1);
			rctrl.SetFocus();

			UCLIDException ue("ELI22940", strErrorIfBlank);
			throw ue;
		}
	}
	else
	{
		return bstrValue;
	}
}
//--------------------------------------------------------------------------------------------------