#include "StdAfx.h"
#include "WorkflowVerifySettingsForm.h"
#include "ListCtrlHelper.h"

#include <UCLIDException.h>

using namespace msclr::interop;
using namespace AttributeDbMgrComponentsLib;

namespace Extract
{
	namespace FAMDBAdmin
	{

#pragma region Constructors

		WorkflowVerifySettingsForm::WorkflowVerifySettingsForm(IFileProcessingDBPtr famDatabase, Int32 workflowID, RedactionVerificationSettings ^settings)
		{
			try
			{
				InitializeComponent();

				_pipfamDatabase = new IFileProcessingDBPtr();
				*_pipfamDatabase = famDatabase;

				_workflowID = workflowID;

				// If an existing settings instance was not provided, create one here.
				_settings = (settings == __nullptr)
					? gcnew RedactionVerificationSettings()
					: settings;

				// Create the pointer to the smart pointer for the WorkflowDefinition
				_pipWorkflowDefinition = new IWorkflowDefinitionPtr;
				if (_workflowID > 0)
				{
					ipWorkflowDefinition = _ipfamDatabase->GetWorkflowDefinition(_workflowID);
				}
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45075");
		}

#pragma endregion

#pragma region Event handlers

		Void WorkflowVerifySettingsForm::HandleWorkflowVerifySettingsForm_Load(System::Object ^ sender, System::EventArgs ^ e)
		{
			try
			{
				loadRedactionTypeGrid();

				// Now that settings have been loaded into the form, do not expose settings to
				// caller until the user has OK'd configured settings.
				_settings = __nullptr;
			}
			CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI45076");
		}

		Void WorkflowVerifySettingsForm::HandleOkButton_Click(System::Object ^ sender, System::EventArgs ^ e)
		{
			try
			{
				// Set the result so that if an exception is thrown the dialog will stay open
				DialogResult = System::Windows::Forms::DialogResult::None;

				auto settings = gcnew RedactionVerificationSettings();

				settings->RedactionTypes = getConfiguredRedactionTypes();
				if (_autoCloseSessionCheckBox->Checked)
				{
					settings->InactivityTimeout = Decimal::ToInt32(_inactivityTimeoutMinutesUpDown->Value);
				}
				else
				{
					settings->InactivityTimeout = 0;
				}

				_settings = settings;

				// Successfully added/updated the workflow definition so set result to ok
				DialogResult = System::Windows::Forms::DialogResult::OK;
			}
			CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI45077");			
		}

		Void WorkflowVerifySettingsForm::HandleAutoCloseSessionCheckBox_CheckedChanged(System::Object^  sender, System::EventArgs^ e)
		{
			try
			{
				_inactivityTimeoutMinutesUpDown->Enabled = _autoCloseSessionCheckBox->Checked;
				
			}
			CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI46740");
		}

#pragma endregion

#pragma region Helper methods

#pragma region Helper methods

		Void WorkflowVerifySettingsForm::loadRedactionTypeGrid()
		{
			_redactionTypesDataGridView->Rows->Clear();

			if (_settings != __nullptr)
			{
				// If this is a new workflow load all the actions
				// if this is an existing workflow just load the actions associated with the workflow
				for each (String^ redactionType in _settings->RedactionTypes)
				{
					_redactionTypesDataGridView->Rows->Add(gcnew cli::array<String^> { redactionType });
				}

				if (_settings->InactivityTimeout > 0)
				{
					_autoCloseSessionCheckBox->Checked = true;
					_inactivityTimeoutMinutesUpDown->Value = _settings->InactivityTimeout;
				}
				else
				{
					RedactionVerificationSettings defaults;
					_autoCloseSessionCheckBox->Checked = false;
					_inactivityTimeoutMinutesUpDown->Value = defaults.InactivityTimeout;
					_inactivityTimeoutMinutesUpDown->Enabled = false;
				}
			}
		}

		System::Collections::Generic::IEnumerable<String ^>^ WorkflowVerifySettingsForm::getConfiguredRedactionTypes()
		{
			List<String ^> ^redactionTypes = gcnew List<String ^>();

			for (int i = 0; i < _redactionTypesDataGridView->RowCount; i++)
			{
				if (i != _redactionTypesDataGridView->NewRowIndex)
				{
					redactionTypes->Add((String^)_redactionTypesDataGridView->Rows[i]->Cells[0]->Value);
				}
			}

			return redactionTypes;
		}

#pragma endregion

	}
}