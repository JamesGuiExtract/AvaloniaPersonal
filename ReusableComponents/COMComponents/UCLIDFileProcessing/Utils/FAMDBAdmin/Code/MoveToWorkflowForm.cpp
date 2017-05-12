#include "StdAfx.h"

#include "ListCtrlHelper.h"
#include "MoveToWorkflowForm.h"

// Disable all the warnings that generated by including the msclr header files
#pragma warning(disable : 4945)

// includes to allow simple syntax for marshaling between c++ types and .net types
#include <msclr\marshal_cppstd.h>
#include <msclr\marshal_windows.h>


namespace Extract {
	namespace FAMDBAdmin {

		Void MoveToWorkflowForm::HandleMoveToWorkflowForm_Load(System::Object^  sender, System::EventArgs^  e)
		{
			try
			{
				loadWorkflowCombos();

				if (_destinationWorkflowComboBox->Items->Count < 1)
				{
					MessageBox::Show("No workflows defined in database.", "No Workflows",
						MessageBoxButtons::OK, MessageBoxIcon::Exclamation);
					Close();
				}

				_selectFilesSummaryTextBox->Text = marshal_as<String^>(_ipFileSelector->GetSummaryString());
			}
			CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI43395");
		}

		Void MoveToWorkflowForm::HandleMoveToWorkflowForm_SelectFilesButton_Click(System::Object ^ sender, System::EventArgs ^ e)
		{
			try
			{
				// Display the select files configuration dialog
				bool bAppliedSettings = asCppBool(_ipFileSelector->Configure(_ipfamDatabase,
					"Select files to change action status for", "SELECT FAMFile.ID FROM FAMFile"));

				// Update the summary text if new settings were applied.
				if (bAppliedSettings)
				{
					String^ strSummaryString = marshal_as<String^>(_ipFileSelector->GetSummaryString());
					_selectFilesSummaryTextBox->Text = strSummaryString;
				}

			}
			CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI43398");
		}

		Void MoveToWorkflowForm::HandleMoveToWorkflowForm_OKButton_Click(System::Object ^ sender, System::EventArgs ^ e)
		{
			try
			{
				applyWorkflowChanges(true);
			}
			CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI43399");
		}

		Void MoveToWorkflowForm::HandleMoveToWorkflowForm_ApplyButton_Click(System::Object ^ sender, System::EventArgs ^ e)
		{
			try
			{
				applyWorkflowChanges(false);
			}
			CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI43400");
		}

		Void MoveToWorkflowForm::loadWorkflowCombos()
		{
			IStrToStrMapPtr workflows = _ipfamDatabase->GetWorkflows();
			ASSERT_RESOURCE_ALLOCATION("ELI41935", workflows != __nullptr);

			_sourceWorkflowComboBox->Items->Clear();

			_sourceWorkflowComboBox->DisplayMember = "Name";
			_sourceWorkflowComboBox->ValueMember = "ID";

			ListCtrlHelper::LoadListCtrl(_sourceWorkflowComboBox, workflows);

			ListItemPair ^tempListItem = gcnew ListItemPair("<All workflows>", 0);
			_sourceWorkflowComboBox->Items->Insert(0, tempListItem);
			tempListItem = gcnew ListItemPair("<No workflow>", -1);
			_sourceWorkflowComboBox->Items->Insert(0, tempListItem);

			_destinationWorkflowComboBox->Items->Clear();

			_destinationWorkflowComboBox->DisplayMember = "Name";
			_destinationWorkflowComboBox->ValueMember = "ID";

			ListCtrlHelper::LoadListCtrl(_destinationWorkflowComboBox, workflows);
		}

		Void MoveToWorkflowForm::applyWorkflowChanges(bool closeDialog)
		{
			if (!areSettingsValid())
			{
				return;
			}

			if (asCppBool(_ipfamDatabase->IsAnyFAMActive()))
			{
				MessageBox::Show(
					"All processing must be stopped before moving files to another workflow.",
					"Processing is active",
					MessageBoxButtons::OK,
					MessageBoxIcon::Error
				);
				return;
			}
			ListItemPair ^selectedDestWorkflow = safe_cast<ListItemPair^ >(_destinationWorkflowComboBox->SelectedItem);
			ListItemPair ^selectedSourceWorkflow = safe_cast<ListItemPair^ >(_sourceWorkflowComboBox->SelectedItem);

			IFileProcessingDBPtr ipTempDB(CLSID_FileProcessingDB);
			ipTempDB->DuplicateConnection(_ipfamDatabase);

			if (selectedSourceWorkflow->ID > 0)
			{
				ipTempDB->ActiveWorkflow = marshal_as<_bstr_t>(selectedSourceWorkflow->Name);
			}
			else
			{
				ipTempDB->ActiveWorkflow = "";
			}

			_bstr_t bstrQuery = _ipFileSelector->BuildQuery(ipTempDB, "FAMFILE.ID", "", true);
			 _ipfamDatabase->MoveFilesToWorkflowFromQuery(bstrQuery, selectedSourceWorkflow->ID, selectedDestWorkflow->ID);

			 MessageBox::Show("The selected files have been moved to " + selectedDestWorkflow->Name, 
				 "Workflow moved", MessageBoxButtons::OK);

			if (closeDialog)
			{
				Close();
			}
		}

		bool MoveToWorkflowForm::areSettingsValid()
		{
			if (_sourceWorkflowComboBox->SelectedIndex < 0)
			{
				MessageBox::Show("You must select a source workflow", "No workflow",
					MessageBoxButtons::OK, MessageBoxIcon::Error);
				_sourceWorkflowComboBox->Focus();
				return false;
			}

			if (_destinationWorkflowComboBox->SelectedIndex < 0)
			{
				MessageBox::Show("You must select a destination workflow", "No workflow", 
					MessageBoxButtons::OK, MessageBoxIcon::Error);
				_destinationWorkflowComboBox->Focus();
				return false;
			}

			ListItemPair ^selectedSource = safe_cast<ListItemPair^ >(_sourceWorkflowComboBox->SelectedItem);
			ListItemPair ^selectedDest = safe_cast<ListItemPair^ >(_destinationWorkflowComboBox->SelectedItem);

			// Check that the source and destination workflows are not the same
			if (selectedSource->Name == selectedDest->Name)
			{
				MessageBox::Show("Source and destination workflows cannot be the same.", "No workflow", 
					MessageBoxButtons::OK, MessageBoxIcon::Error);
				_destinationWorkflowComboBox->Focus();
				return false;				
			}

			return true;
		}
	}
}

