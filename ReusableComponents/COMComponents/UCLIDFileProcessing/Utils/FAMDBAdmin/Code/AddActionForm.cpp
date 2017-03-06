#include "StdAfx.h"
#include "AddActionForm.h"

using namespace Extract::Utilities;
using namespace System::Windows::Forms;

namespace Extract
{
	namespace FAMDBAdmin
	{
#pragma region Event handlers

		Void AddActionForm::HandleAddActionFormActionName_Validating(System::Object ^ sender, System::ComponentModel::CancelEventArgs ^ e)
		{
			if (!UtilityMethods::IsValidIdentifier(actionNameTextBox->Text))
			{
				MessageBox::Show(this, "Action name is not valid.", "Error", MessageBoxButtons::OK, MessageBoxIcon::Error);
				e->Cancel = true;
			}
		}

#pragma endregion

	}
}