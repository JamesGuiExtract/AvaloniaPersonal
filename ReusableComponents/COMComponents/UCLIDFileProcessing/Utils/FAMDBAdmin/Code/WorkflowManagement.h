#pragma once

#include "ListCtrlHelper.h"
#include <UCLIDException.h>

namespace Extract {
	namespace FAMDBAdmin {

		using namespace System;
		using namespace System::ComponentModel;
		using namespace System::Collections;
		using namespace System::Windows::Forms;
		using namespace System::Data;
		using namespace System::Drawing;
		using namespace System::Collections::Specialized;
		using namespace UCLID_FILEPROCESSINGLib;

		/// <summary>
		/// Summary for WorkflowManagement
		/// </summary>
		public ref class WorkflowManagement : public System::Windows::Forms::Form
		{
		public:

#pragma region  Constructors

			// Constructor
			//		famDatabase : FAM database to use to manage workflows and actions
			//		currentWorkflow : The name of the current workflow to initially select
			WorkflowManagement(IFileProcessingDBPtr famDatabase, String ^ currentWorkflow)
			{
				InitializeComponent();
				_currentWorkflow = currentWorkflow;

				// Create the pointer to the smart pointer for the database
				_pipfamDatabase = new IFileProcessingDBPtr();
				ASSERT_RESOURCE_ALLOCATION("ELI42058", _pipfamDatabase != __nullptr);

				// Set the fam database to the smart pointer
				*_pipfamDatabase = famDatabase;
			}

#pragma endregion

		private:

#pragma region Event Handlers

			// Action buttons handlers
			Void HandleAddActionButton_Click(System::Object^  sender, System::EventArgs^  e);
			Void HandleDeleteActionButton_Click(System::Object^  sender, System::EventArgs^  e);
			Void HandleRenameActionButton_Click(System::Object^  sender, System::EventArgs^  e);

			// Workflow form handlers
			Void HandleWorkflowManagement_Load(System::Object^  sender, System::EventArgs^  e);

			// Workflow buttons handlers
			Void HandleModifyWorkflowButton_Click(System::Object^  sender, System::EventArgs^  e);
			Void HandleAddWorkflowButton_Click(System::Object^  sender, System::EventArgs^  e);
			Void HandleDeleteWorkflowButton_Click(System::Object^  sender, System::EventArgs^  e);

			// Workflow combo handlers
			Void HandleWorkflowSelectionChangeCommitted(System::Object^ sender, System::EventArgs^ e);

			// Action list box handlers
			Void HandleActionsCheckedListBox_ItemCheck(System::Object^ sender, ItemCheckEventArgs^ e);

			// Save button handler
			Void HandleSaveChangesButton_Click(System::Object^  sender, System::EventArgs^  e);
			
			// Form closing event handler
			Void HandleWorkflowManagement_FormClosing(System::Object^  sender, System::Windows::Forms::FormClosingEventArgs^  e);

			// Handle the CheckedListBox mouse click so that box is only checked when it is clicked on
			Void HandleActionsCheckedListBox_MouseClick(System::Object^  sender, MouseEventArgs^ e);

#pragma endregion

#pragma region Helper methods

			// Loads all of the actions from the FAM database and checks the actions associated with the current workflow
			Void loadActionsList();

			// Checks the boxes in the actions list that are associated with the current workflow
			Void setActionChecksForCurrentWorkflow();

			// Updates the status of the buttons (enables/disables)
			Void updateButtons();

			// Checks the box for the given action
			Void checkAction(System::String ^ action);

			// Loads the workflow combo which also checks the actions for the workflow
			Void loadWorkflowCombo();

			// Prompts user to save the changes to the Actions associated with the workflow 
			// if changes have been made 
			Void promptAndSaveActionsForWorkFlow(ListItemPair ^workFlowToUpdate);

			// Saves the actions for the workflow
			Void saveActionsForWorkflow(ListItemPair ^workFlowToUpdate);

#pragma endregion

#pragma region  Private variables

			// Flag to indicate that there have been changes to the selected actions for the current workflow
			bool workflowActionsDirty;

			// The name of the current workflow
			String^ _currentWorkflow;
		private: System::Windows::Forms::Button^  addWorkflowButton;

		private: System::Windows::Forms::GroupBox^  workflowGroupBox;
		private: System::Windows::Forms::GroupBox^  actionsGroupBox;



			// used for internal access to fam database
			property IFileProcessingDBPtr _ipfamDatabase
			{
				IFileProcessingDBPtr get()
				{
					return *_pipfamDatabase;
				}
			}

			// The FAM database that is being managed
			IFileProcessingDBPtr *_pipfamDatabase;

#pragma endregion

#pragma region Destructors

		protected:
			/// <summary>
			/// Clean up any resources being used.
			/// </summary>
			~WorkflowManagement()
			{
				if (components)
				{
					delete components;
				}
				
				if (_pipfamDatabase)
				{
					*_pipfamDatabase = __nullptr;
					delete _pipfamDatabase;
					_pipfamDatabase = __nullptr;
				}
			}

#pragma endregion

#pragma region Windows Form Designer generated code

		private:

			/// <summary>
			/// Required designer variable.
			/// </summary>	

			System::Windows::Forms::ComboBox^  workflowComboBox;

			System::Windows::Forms::Button^  deleteWorkflowButton;

			System::Windows::Forms::CheckedListBox^  actionsCheckedListBox;


			System::Windows::Forms::Button^  addActionButton;
			System::Windows::Forms::Button^  deleteActionButton;
			System::Windows::Forms::Button^  renameActionButton;
			System::Windows::Forms::Button^  closeButton;
private: System::Windows::Forms::Button^  modifyWorkflowButton;

			System::Windows::Forms::Button^  saveChangesButton;

			System::ComponentModel::Container ^components;


			/// <summary>
			/// Required method for Designer support - do not modify
			/// the contents of this method with the code editor.
			/// </summary>
			void InitializeComponent(void)
			{
				this->workflowComboBox = (gcnew System::Windows::Forms::ComboBox());
				this->deleteWorkflowButton = (gcnew System::Windows::Forms::Button());
				this->actionsCheckedListBox = (gcnew System::Windows::Forms::CheckedListBox());
				this->addActionButton = (gcnew System::Windows::Forms::Button());
				this->deleteActionButton = (gcnew System::Windows::Forms::Button());
				this->renameActionButton = (gcnew System::Windows::Forms::Button());
				this->closeButton = (gcnew System::Windows::Forms::Button());
				this->modifyWorkflowButton = (gcnew System::Windows::Forms::Button());
				this->saveChangesButton = (gcnew System::Windows::Forms::Button());
				this->addWorkflowButton = (gcnew System::Windows::Forms::Button());
				this->workflowGroupBox = (gcnew System::Windows::Forms::GroupBox());
				this->actionsGroupBox = (gcnew System::Windows::Forms::GroupBox());
				this->workflowGroupBox->SuspendLayout();
				this->actionsGroupBox->SuspendLayout();
				this->SuspendLayout();
				// 
				// workflowComboBox
				// 
				this->workflowComboBox->Anchor = static_cast<System::Windows::Forms::AnchorStyles>(((System::Windows::Forms::AnchorStyles::Top | System::Windows::Forms::AnchorStyles::Left)
					| System::Windows::Forms::AnchorStyles::Right));
				this->workflowComboBox->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
				this->workflowComboBox->FormattingEnabled = true;
				this->workflowComboBox->Location = System::Drawing::Point(7, 20);
				this->workflowComboBox->Name = L"workflowComboBox";
				this->workflowComboBox->Size = System::Drawing::Size(323, 21);
				this->workflowComboBox->TabIndex = 0;
				this->workflowComboBox->SelectionChangeCommitted += gcnew System::EventHandler(this, &WorkflowManagement::HandleWorkflowSelectionChangeCommitted);
				// 
				// deleteWorkflowButton
				// 
				this->deleteWorkflowButton->Anchor = static_cast<System::Windows::Forms::AnchorStyles>((System::Windows::Forms::AnchorStyles::Top | System::Windows::Forms::AnchorStyles::Right));
				this->deleteWorkflowButton->Location = System::Drawing::Point(225, 47);
				this->deleteWorkflowButton->Name = L"deleteWorkflowButton";
				this->deleteWorkflowButton->Size = System::Drawing::Size(105, 23);
				this->deleteWorkflowButton->TabIndex = 2;
				this->deleteWorkflowButton->Text = L"Delete";
				this->deleteWorkflowButton->UseVisualStyleBackColor = true;
				this->deleteWorkflowButton->Click += gcnew System::EventHandler(this, &WorkflowManagement::HandleDeleteWorkflowButton_Click);
				// 
				// actionsCheckedListBox
				// 
				this->actionsCheckedListBox->Anchor = static_cast<System::Windows::Forms::AnchorStyles>(((System::Windows::Forms::AnchorStyles::Top | System::Windows::Forms::AnchorStyles::Left)
					| System::Windows::Forms::AnchorStyles::Right));
				this->actionsCheckedListBox->CheckOnClick = true;
				this->actionsCheckedListBox->FormattingEnabled = true;
				this->actionsCheckedListBox->Location = System::Drawing::Point(7, 19);
				this->actionsCheckedListBox->Name = L"actionsCheckedListBox";
				this->actionsCheckedListBox->Size = System::Drawing::Size(323, 244);
				this->actionsCheckedListBox->TabIndex = 4;
				this->actionsCheckedListBox->ItemCheck += gcnew System::Windows::Forms::ItemCheckEventHandler(this, &WorkflowManagement::HandleActionsCheckedListBox_ItemCheck);
				this->actionsCheckedListBox->MouseClick += gcnew System::Windows::Forms::MouseEventHandler(this, &WorkflowManagement::HandleActionsCheckedListBox_MouseClick);
				// 
				// addActionButton
				// 
				this->addActionButton->Anchor = static_cast<System::Windows::Forms::AnchorStyles>((System::Windows::Forms::AnchorStyles::Bottom | System::Windows::Forms::AnchorStyles::Left));
				this->addActionButton->Location = System::Drawing::Point(6, 268);
				this->addActionButton->Name = L"addActionButton";
				this->addActionButton->Size = System::Drawing::Size(105, 23);
				this->addActionButton->TabIndex = 5;
				this->addActionButton->Text = L"Add";
				this->addActionButton->UseVisualStyleBackColor = true;
				this->addActionButton->Click += gcnew System::EventHandler(this, &WorkflowManagement::HandleAddActionButton_Click);
				// 
				// deleteActionButton
				// 
				this->deleteActionButton->Anchor = System::Windows::Forms::AnchorStyles::Bottom;
				this->deleteActionButton->Location = System::Drawing::Point(115, 268);
				this->deleteActionButton->Name = L"deleteActionButton";
				this->deleteActionButton->Size = System::Drawing::Size(105, 23);
				this->deleteActionButton->TabIndex = 6;
				this->deleteActionButton->Text = L"Delete";
				this->deleteActionButton->UseVisualStyleBackColor = true;
				this->deleteActionButton->Click += gcnew System::EventHandler(this, &WorkflowManagement::HandleDeleteActionButton_Click);
				// 
				// renameActionButton
				// 
				this->renameActionButton->Anchor = static_cast<System::Windows::Forms::AnchorStyles>((System::Windows::Forms::AnchorStyles::Bottom | System::Windows::Forms::AnchorStyles::Right));
				this->renameActionButton->Location = System::Drawing::Point(224, 268);
				this->renameActionButton->Name = L"renameActionButton";
				this->renameActionButton->Size = System::Drawing::Size(105, 23);
				this->renameActionButton->TabIndex = 7;
				this->renameActionButton->Text = L"Rename";
				this->renameActionButton->UseVisualStyleBackColor = true;
				this->renameActionButton->Click += gcnew System::EventHandler(this, &WorkflowManagement::HandleRenameActionButton_Click);
				// 
				// closeButton
				// 
				this->closeButton->Anchor = static_cast<System::Windows::Forms::AnchorStyles>((System::Windows::Forms::AnchorStyles::Bottom | System::Windows::Forms::AnchorStyles::Right));
				this->closeButton->DialogResult = System::Windows::Forms::DialogResult::OK;
				this->closeButton->Location = System::Drawing::Point(238, 399);
				this->closeButton->Name = L"closeButton";
				this->closeButton->Size = System::Drawing::Size(105, 23);
				this->closeButton->TabIndex = 9;
				this->closeButton->Text = L"Close";
				this->closeButton->UseVisualStyleBackColor = true;
				// 
				// modifyWorkflowButton
				// 
				this->modifyWorkflowButton->Anchor = System::Windows::Forms::AnchorStyles::Top;
				this->modifyWorkflowButton->Location = System::Drawing::Point(116, 47);
				this->modifyWorkflowButton->Name = L"modifyWorkflowButton";
				this->modifyWorkflowButton->Size = System::Drawing::Size(105, 23);
				this->modifyWorkflowButton->TabIndex = 1;
				this->modifyWorkflowButton->Text = L"Modify";
				this->modifyWorkflowButton->UseVisualStyleBackColor = true;
				this->modifyWorkflowButton->Click += gcnew System::EventHandler(this, &WorkflowManagement::HandleModifyWorkflowButton_Click);
				// 
				// saveChangesButton
				// 
				this->saveChangesButton->Anchor = System::Windows::Forms::AnchorStyles::Bottom;
				this->saveChangesButton->Enabled = false;
				this->saveChangesButton->Location = System::Drawing::Point(129, 399);
				this->saveChangesButton->Name = L"saveChangesButton";
				this->saveChangesButton->Size = System::Drawing::Size(105, 23);
				this->saveChangesButton->TabIndex = 8;
				this->saveChangesButton->Text = L"Save";
				this->saveChangesButton->UseVisualStyleBackColor = true;
				this->saveChangesButton->Click += gcnew System::EventHandler(this, &WorkflowManagement::HandleSaveChangesButton_Click);
				// 
				// addWorkflowButton
				// 
				this->addWorkflowButton->Location = System::Drawing::Point(7, 47);
				this->addWorkflowButton->Name = L"addWorkflowButton";
				this->addWorkflowButton->Size = System::Drawing::Size(105, 23);
				this->addWorkflowButton->TabIndex = 1;
				this->addWorkflowButton->Text = L"Add";
				this->addWorkflowButton->UseVisualStyleBackColor = true;
				this->addWorkflowButton->Click += gcnew System::EventHandler(this, &WorkflowManagement::HandleAddWorkflowButton_Click);
				// 
				// workflowGroupBox
				// 
				this->workflowGroupBox->Anchor = static_cast<System::Windows::Forms::AnchorStyles>(((System::Windows::Forms::AnchorStyles::Top | System::Windows::Forms::AnchorStyles::Bottom)
					| System::Windows::Forms::AnchorStyles::Left));
				this->workflowGroupBox->Controls->Add(this->addWorkflowButton);
				this->workflowGroupBox->Controls->Add(this->workflowComboBox);
				this->workflowGroupBox->Controls->Add(this->deleteWorkflowButton);
				this->workflowGroupBox->Controls->Add(this->modifyWorkflowButton);
				this->workflowGroupBox->Location = System::Drawing::Point(14, 12);
				this->workflowGroupBox->Name = L"workflowGroupBox";
				this->workflowGroupBox->Size = System::Drawing::Size(338, 80);
				this->workflowGroupBox->TabIndex = 10;
				this->workflowGroupBox->TabStop = false;
				this->workflowGroupBox->Text = L"Workflow";
				// 
				// actionsGroupBox
				// 
				this->actionsGroupBox->Controls->Add(this->actionsCheckedListBox);
				this->actionsGroupBox->Controls->Add(this->renameActionButton);
				this->actionsGroupBox->Controls->Add(this->addActionButton);
				this->actionsGroupBox->Controls->Add(this->deleteActionButton);
				this->actionsGroupBox->Location = System::Drawing::Point(14, 96);
				this->actionsGroupBox->Name = L"actionsGroupBox";
				this->actionsGroupBox->Size = System::Drawing::Size(338, 300);
				this->actionsGroupBox->TabIndex = 11;
				this->actionsGroupBox->TabStop = false;
				this->actionsGroupBox->Text = L"Actions";
				// 
				// WorkflowManagement
				// 
				this->AutoScaleDimensions = System::Drawing::SizeF(6, 13);
				this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
				this->CancelButton = this->closeButton;
				this->ClientSize = System::Drawing::Size(362, 431);
				this->Controls->Add(this->saveChangesButton);
				this->Controls->Add(this->closeButton);
				this->Controls->Add(this->workflowGroupBox);
				this->Controls->Add(this->actionsGroupBox);
				this->FormBorderStyle = System::Windows::Forms::FormBorderStyle::FixedSingle;
				this->MaximizeBox = false;
				this->MinimizeBox = false;
				this->MinimumSize = System::Drawing::Size(375, 456);
				this->Name = L"WorkflowManagement";
				this->ShowIcon = false;
				this->StartPosition = System::Windows::Forms::FormStartPosition::CenterParent;
				this->Text = L"Workflow Management";
				this->FormClosing += gcnew System::Windows::Forms::FormClosingEventHandler(this, &WorkflowManagement::HandleWorkflowManagement_FormClosing);
				this->Load += gcnew System::EventHandler(this, &WorkflowManagement::HandleWorkflowManagement_Load);
				this->workflowGroupBox->ResumeLayout(false);
				this->actionsGroupBox->ResumeLayout(false);
				this->ResumeLayout(false);

			}
#pragma endregion


};
	};
}