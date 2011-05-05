#include "TextFunctionExpander.h"
#include "ComUtils.h"
#include "QuickMenuChooser.h"
#include "VectorOperations.h"
#include "UCLIDException.h"

#include <string>
#include <vector>

using namespace std;

// Displays a doc tag menu and applies any selected value to the specified edit box.
// The selection will replace the currently selected text. If the selection is a function,
// the previous selection will be added inside of the parens. If there was no previous selection
// the cursor will be placed between the parens.
// ipTagUtility- The utility used to obtain the list of file tags.
// btnTagButton- The button that was clicked to display the menu (indicates menu position).
// reditTarget- The ATL or MFC edit box the any selection should be applied to.
// bIncludeSourceDocName- true to include <SourceDocName> as an option.
// Returns- true if the user made a selection from the menu. false if the menu was cancelled without
//     a selection having been made.
template <class T, class U, class V>
static const bool ChooseDocTagForEditBox(const T &ipTagUtility, const U &btnTagButton,
	V &reditTarget, bool bIncludeSourceDocName=true)
{
	try
	{
		int nSelStart, nSelEnd;
		reditTarget.GetSel(nSelStart, nSelEnd);
		int nSelCount = nSelEnd - nSelStart;

		CString zControlText;
		reditTarget.GetWindowText(zControlText);

		CString zReplacementText = ChooseDocTag(ipTagUtility, btnTagButton, zControlText, nSelStart,
			nSelCount, bIncludeSourceDocName);

		if (asCppBool(!zReplacementText.IsEmpty()))
		{
			reditTarget.ReplaceSel(zReplacementText);
			reditTarget.SetFocus();
			reditTarget.SetSel(nSelStart, nSelStart + nSelCount);

			return true;
		}

		return false;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32542");
}

// Displays a doc tag menu and applies any selected value to the specified combo box.
// The selection will replace the currently selected text. If the selection is a function,
// the previous selection will be added inside of the parens. If there was no previous selection
// the cursor will be placed between the parens.
// ipTagUtility- The utility used to obtain the list of file tags.
// btnTagButton- The button that was clicked to display the menu (indicates menu position).
// rcomboTarget- The ATL or MFC combo box the any selection should be applied to.
// rdwSelection- The selection in the combo box edit box prior to the combo box losing focus.
//	   This will be updated to indicate the selection that has been applied after selection.
// bIncludeSourceDocName- true to include <SourceDocName> as an option.
// Returns- true if the user made a selection from the menu. false if the menu was cancelled without
//     a selection having been made.
template <class T, class U, class V>
static const bool ChooseDocTagForComboBox(const T &ipTagUtility, const U &btnTagButton,
	V &rcomboTarget, DWORD &rdwSelection, bool bIncludeSourceDocName=true)
{
	try
	{
		int nSelStart = LOWORD(rdwSelection);
		int nSelCount = HIWORD(rdwSelection) - nSelStart;
		int nNewSelStart = nSelStart;
		int nNewSelCount = nSelCount;

		CString zControlText;
		rcomboTarget.GetWindowText(zControlText);

		CString zReplacementText = ChooseDocTag(ipTagUtility, btnTagButton, zControlText,
			nNewSelStart, nNewSelCount, bIncludeSourceDocName);

		if (asCppBool(!zReplacementText.IsEmpty()))
		{
			zControlText.Delete(nSelStart, nSelCount);
			zControlText.Insert(nSelStart, zReplacementText);
			rcomboTarget.SetWindowText(zControlText);
			rcomboTarget.SetFocus();
			rcomboTarget.SetEditSel(nNewSelStart, nNewSelStart + nNewSelCount);

			rdwSelection = MAKELONG(nNewSelStart, nNewSelStart + nNewSelCount);

			return true;
		}

		return false;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32543");
}

// A helper function for ChooseDocTagForEditBox and ChooseDocTagForComboBox.
// Displays a doc tag menu and returns the value that should replace the target control's current
// selection.
template <class T, class U>
static const CString ChooseDocTag(const T &ipTagUtility, const U &btnTagButton, const CString &zControlText,
	int &rnSelStart, int &rnSelCount, bool bIncludeSourceDocName)
{
	try
	{
		ASSERT_ARGUMENT("ELI26320", ipTagUtility != __nullptr);

		CString zReplacmentValue;

		vector<string> vecChoices;

		// Add the built in tags
		IVariantVectorPtr ipVecBuiltInTags = ipTagUtility->GetBuiltInTags();
		long lBuiltInSize = ipVecBuiltInTags->Size;
		for (long i = 0; i < lBuiltInSize; i++)
		{
			_variant_t var = ipVecBuiltInTags->Item[i];
			string str = asString(var.bstrVal);
			if (bIncludeSourceDocName || str != "<SourceDocName>")
			{
				vecChoices.push_back(str);
			}
		}

		// Add a separator if there is at least one built in tag
		if (lBuiltInSize > 0)
		{
			vecChoices.push_back(""); // Separator
		}

		// Add tags in specified ini file
		IVariantVectorPtr ipVecIniTags = ipTagUtility->GetINIFileTags();
		long lIniSize = ipVecIniTags->Size;
		for (long i = 0; i < lIniSize; i++)
		{
			_variant_t var = ipVecIniTags->Item[i];
			string str = asString(var.bstrVal);
			vecChoices.push_back(str);
		}

		// Add a separator if there is at least one tas from INI file
		if (lIniSize > 0)
		{
			vecChoices.push_back(""); // Separator
		}

		int nFirstFunctionIndex = vecChoices.size();

		// Add utility functions
		TextFunctionExpander tfe;
		vector<string> vecFunctions = tfe.getAvailableFunctions();
		vector<string> vecFormattedFunctions = vecFunctions;
		tfe.formatFunctions(vecFormattedFunctions);
		addVectors(vecChoices, vecFormattedFunctions);

		// Create the menu
		QuickMenuChooser qmc;
		qmc.setChoices(vecChoices);

		RECT rect;
		btnTagButton.GetWindowRect(&rect);

		// Display the menu.
		int nSelection = qmc.getChoice(
			CWnd::FromHandle(GetParent(btnTagButton.m_hWnd)), rect.right, rect.top);
		if (nSelection >= 0)
		{
			// If the function index is >= 0, a function was selected.
			int nFunctionIndex = nSelection - nFirstFunctionIndex;

			if (nFunctionIndex >= 0)
			{
				// Add the previously selected text within the parens of the function.
				zReplacmentValue = "$" + CString(vecFunctions[nFunctionIndex].c_str()) + "(";
				CString zSelectedText = zControlText.Mid(rnSelStart, rnSelCount);
				rnSelStart += zReplacmentValue.GetLength();
				zReplacmentValue += zSelectedText + ")";
			}
			else
			{
				// Replace the previous selection with the selected option.
				zReplacmentValue = vecChoices[nSelection].c_str();
				rnSelStart += zReplacmentValue.GetLength();
				rnSelCount = 0;
			}
		}

		return zReplacmentValue;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32544");
}