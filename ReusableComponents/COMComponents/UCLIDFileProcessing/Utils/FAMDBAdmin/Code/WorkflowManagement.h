#pragma once

#include "ListCtrlHelper.h"
#include <UCLIDException.h>

namespace Extract {
	namespace FAMDBAdmin {

		using namespace System;
		using namespace System::ComponentModel;
		using namespace System::Collections;
		using namespace System::Collections::Generic;
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

			// Handle the case that either the included or main sequence check boxes has been clicked.
			void HandleCellContentClick(System::Object ^sender, System::Windows::Forms::DataGridViewCellEventArgs ^e);

			// Handle case that the checkbox to toggle the enabled status of load balancing is changed.
			void HandleLoadBalanceCheckBox_CheckedChanged(System::Object^  sender, System::EventArgs^  e);

#pragma endregion

#pragma region Helper methods

			// Loads all of the actions from the FAM database and checks the actions associated with the current workflow
			Void loadActionsList();

			// Checks the boxes in the actions list that are associated with the current workflow
			Void setActionChecksForCurrentWorkflow();

			// Gets the ID for the currently selected workflow.
			int getSelectedWorkflowId();

			// Toggle the state of the action grid checkbox indicating whether an action is included
			// in the workflow
			Void toggleActionIncluded(DataGridViewRow ^row);

			// Toggle the state of the action grid checkbox indicating whether an included action is
			// part of the main sequence for the workflow.
			Void toggleMainSequenceAction(DataGridViewRow ^row);

			// Updates the status of the buttons (enables/disables)
			Void updateButtons();

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

			// The set of action names that to this point have been at some point included in the
			// current workflow. This is tracked in order to know when to default the main sequence
			// checkbox to checked (vs keeping it the same value it had been previously).
			HashSet<String ^> _initializedActions;

			private: System::Windows::Forms::Button^  addWorkflowButton;
			private: System::Windows::Forms::GroupBox^  workflowGroupBox;
			private: System::Windows::Forms::GroupBox^  actionsGroupBox;
			private: System::Windows::Forms::DataGridView^  actionsGridView; 

			private: System::Windows::Forms::CheckBox^  _loadBalanceCheckBox;

			static int ActionIDColumnIndex = 0;
			static int ActionIncludedColumnIndex = 1;
			static int ActionNameColumnIndex = 2;
			static int ActionMainSequenceColumnIndex = 3;
private: System::Windows::Forms::DataGridViewTextBoxColumn^ ActionIDColumn;
private: System::Windows::Forms::DataGridViewCheckBoxColumn^ ActionIncludedColumn;
private: System::Windows::Forms::DataGridViewTextBoxColumn^ ActionNameColumn;
private: System::Windows::Forms::DataGridViewCheckBoxColumn^ ActionMainSequenceColumn;

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
					*_pipfamDatabase = NULL;
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




			System::Windows::Forms::Button^  addActionButton;
			System::Windows::Forms::Button^  deleteActionButton;
			System::Windows::Forms::Button^  renameActionButton;
			System::Windows::Forms::Button^  closeButton;
			System::Windows::Forms::Button^  modifyWorkflowButton;

			System::Windows::Forms::Button^  saveChangesButton;

			System::ComponentModel::Container ^components;


			/// <summary>
			/// Required method for Designer support - do not modify
			/// the contents of this method with the code editor.
			/// </summary>
			void InitializeComponent(void)
			{
				System::Windows::Forms::DataGridViewCellStyle^ dataGridViewCellStyle1 = (gcnew System::Windows::Forms::DataGridViewCellStyle());
				System::Windows::Forms::DataGridViewCellStyle^ dataGridViewCellStyle3 = (gcnew System::Windows::Forms::DataGridViewCellStyle());
				System::Windows::Forms::DataGridViewCellStyle^ dataGridViewCellStyle4 = (gcnew System::Windows::Forms::DataGridViewCellStyle());
				System::Windows::Forms::DataGridViewCellStyle^ dataGridViewCellStyle2 = (gcnew System::Windows::Forms::DataGridViewCellStyle());
				this->workflowComboBox = (gcnew System::Windows::Forms::ComboBox());
				this->deleteWorkflowButton = (gcnew System::Windows::Forms::Button());
				this->addActionButton = (gcnew System::Windows::Forms::Button());
				this->deleteActionButton = (gcnew System::Windows::Forms::Button());
				this->renameActionButton = (gcnew System::Windows::Forms::Button());
				this->closeButton = (gcnew System::Windows::Forms::Button());
				this->modifyWorkflowButton = (gcnew System::Windows::Forms::Button());
				this->saveChangesButton = (gcnew System::Windows::Forms::Button());
				this->addWorkflowButton = (gcnew System::Windows::Forms::Button());
				this->workflowGroupBox = (gcnew System::Windows::Forms::GroupBox());
				this->_loadBalanceCheckBox = (gcnew System::Windows::Forms::CheckBox());
				this->actionsGroupBox = (gcnew System::Windows::Forms::GroupBox());
				this->actionsGridView = (gcnew System::Windows::Forms::DataGridView());
				this->ActionIDColumn = (gcnew System::Windows::Forms::DataGridViewTextBoxColumn());
				this->ActionIncludedColumn = (gcnew System::Windows::Forms::DataGridViewCheckBoxColumn());
				this->ActionNameColumn = (gcnew System::Windows::Forms::DataGridViewTextBoxColumn());
				this->ActionMainSequenceColumn = (gcnew System::Windows::Forms::DataGridViewCheckBoxColumn());
				this->workflowGroupBox->SuspendLayout();
				this->actionsGroupBox->SuspendLayout();
				(cli::safe_cast<System::ComponentModel::ISupportInitialize^>(this->actionsGridView))->BeginInit();
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
				this->workflowComboBox->Size = System::Drawing::Size(366, 21);
				this->workflowComboBox->TabIndex = 0;
				this->workflowComboBox->SelectionChangeCommitted += gcnew System::EventHandler(this, &WorkflowManagement::HandleWorkflowSelectionChangeCommitted);
				// 
				// deleteWorkflowButton
				// 
				this->deleteWorkflowButton->Anchor = static_cast<System::Windows::Forms::AnchorStyles>((System::Windows::Forms::AnchorStyles::Top | System::Windows::Forms::AnchorStyles::Right));
				this->deleteWorkflowButton->Location = System::Drawing::Point(255, 47);
				this->deleteWorkflowButton->Name = L"deleteWorkflowButton";
				this->deleteWorkflowButton->Size = System::Drawing::Size(118, 23);
				this->deleteWorkflowButton->TabIndex = 2;
				this->deleteWorkflowButton->Text = L"Delete";
				this->deleteWorkflowButton->UseVisualStyleBackColor = true;
				this->deleteWorkflowButton->Click += gcnew System::EventHandler(this, &WorkflowManagement::HandleDeleteWorkflowButton_Click);
				// 
				// addActionButton
				// 
				this->addActionButton->Anchor = static_cast<System::Windows::Forms::AnchorStyles>((System::Windows::Forms::AnchorStyles::Bottom | System::Windows::Forms::AnchorStyles::Left));
				this->addActionButton->Location = System::Drawing::Point(6, 274);
				this->addActionButton->Name = L"addActionButton";
				this->addActionButton->Size = System::Drawing::Size(118, 23);
				this->addActionButton->TabIndex = 5;
				this->addActionButton->Text = L"Add";
				this->addActionButton->UseVisualStyleBackColor = true;
				this->addActionButton->Click += gcnew System::EventHandler(this, &WorkflowManagement::HandleAddActionButton_Click);
				// 
				// deleteActionButton
				// 
				this->deleteActionButton->Anchor = System::Windows::Forms::AnchorStyles::Bottom;
				this->deleteActionButton->Location = System::Drawing::Point(131, 274);
				this->deleteActionButton->Name = L"deleteActionButton";
				this->deleteActionButton->Size = System::Drawing::Size(118, 23);
				this->deleteActionButton->TabIndex = 6;
				this->deleteActionButton->Text = L"Delete";
				this->deleteActionButton->UseVisualStyleBackColor = true;
				this->deleteActionButton->Click += gcnew System::EventHandler(this, &WorkflowManagement::HandleDeleteActionButton_Click);
				// 
				// renameActionButton
				// 
				this->renameActionButton->Anchor = static_cast<System::Windows::Forms::AnchorStyles>((System::Windows::Forms::AnchorStyles::Bottom | System::Windows::Forms::AnchorStyles::Right));
				this->renameActionButton->Location = System::Drawing::Point(255, 274);
				this->renameActionButton->Name = L"renameActionButton";
				this->renameActionButton->Size = System::Drawing::Size(118, 23);
				this->renameActionButton->TabIndex = 7;
				this->renameActionButton->Text = L"Rename";
				this->renameActionButton->UseVisualStyleBackColor = true;
				this->renameActionButton->Click += gcnew System::EventHandler(this, &WorkflowManagement::HandleRenameActionButton_Click);
				// 
				// closeButton
				// 
				this->closeButton->Anchor = static_cast<System::Windows::Forms::AnchorStyles>((System::Windows::Forms::AnchorStyles::Bottom | System::Windows::Forms::AnchorStyles::Right));
				this->closeButton->DialogResult = System::Windows::Forms::DialogResult::OK;
				this->closeButton->Location = System::Drawing::Point(281, 427);
				this->closeButton->Name = L"closeButton";
				this->closeButton->Size = System::Drawing::Size(118, 23);
				this->closeButton->TabIndex = 9;
				this->closeButton->Text = L"Close";
				this->closeButton->UseVisualStyleBackColor = true;
				// 
				// modifyWorkflowButton
				// 
				this->modifyWorkflowButton->Anchor = System::Windows::Forms::AnchorStyles::Top;
				this->modifyWorkflowButton->Location = System::Drawing::Point(131, 47);
				this->modifyWorkflowButton->Name = L"modifyWorkflowButton";
				this->modifyWorkflowButton->Size = System::Drawing::Size(118, 23);
				this->modifyWorkflowButton->TabIndex = 1;
				this->modifyWorkflowButton->Text = L"Modify";
				this->modifyWorkflowButton->UseVisualStyleBackColor = true;
				this->modifyWorkflowButton->Click += gcnew System::EventHandler(this, &WorkflowManagement::HandleModifyWorkflowButton_Click);
				// 
				// saveChangesButton
				// 
				this->saveChangesButton->Anchor = System::Windows::Forms::AnchorStyles::Bottom;
				this->saveChangesButton->Enabled = false;
				this->saveChangesButton->Location = System::Drawing::Point(157, 427);
				this->saveChangesButton->Name = L"saveChangesButton";
				this->saveChangesButton->Size = System::Drawing::Size(118, 23);
				this->saveChangesButton->TabIndex = 8;
				this->saveChangesButton->Text = L"Save";
				this->saveChangesButton->UseVisualStyleBackColor = true;
				this->saveChangesButton->Click += gcnew System::EventHandler(this, &WorkflowManagement::HandleSaveChangesButton_Click);
				// 
				// addWorkflowButton
				// 
				this->addWorkflowButton->Location = System::Drawing::Point(6, 47);
				this->addWorkflowButton->Name = L"addWorkflowButton";
				this->addWorkflowButton->Size = System::Drawing::Size(118, 23);
				this->addWorkflowButton->TabIndex = 1;
				this->addWorkflowButton->Text = L"Add";
				this->addWorkflowButton->UseVisualStyleBackColor = true;
				this->addWorkflowButton->Click += gcnew System::EventHandler(this, &WorkflowManagement::HandleAddWorkflowButton_Click);
				// 
				// workflowGroupBox
				// 
				this->workflowGroupBox->Anchor = static_cast<System::Windows::Forms::AnchorStyles>(((System::Windows::Forms::AnchorStyles::Top | System::Windows::Forms::AnchorStyles::Left)
					| System::Windows::Forms::AnchorStyles::Right));
				this->workflowGroupBox->Controls->Add(this->_loadBalanceCheckBox);
				this->workflowGroupBox->Controls->Add(this->addWorkflowButton);
				this->workflowGroupBox->Controls->Add(this->workflowComboBox);
				this->workflowGroupBox->Controls->Add(this->deleteWorkflowButton);
				this->workflowGroupBox->Controls->Add(this->modifyWorkflowButton);
				this->workflowGroupBox->Location = System::Drawing::Point(14, 12);
				this->workflowGroupBox->Name = L"workflowGroupBox";
				this->workflowGroupBox->Size = System::Drawing::Size(381, 100);
				this->workflowGroupBox->TabIndex = 10;
				this->workflowGroupBox->TabStop = false;
				this->workflowGroupBox->Text = L"Workflow";
				// 
				// _loadBalanceCheckBox
				// 
				this->_loadBalanceCheckBox->AutoSize = true;
				this->_loadBalanceCheckBox->Location = System::Drawing::Point(7, 76);
				this->_loadBalanceCheckBox->Name = L"_loadBalanceCheckBox";
				this->_loadBalanceCheckBox->Size = System::Drawing::Size(293, 17);
				this->_loadBalanceCheckBox->TabIndex = 3;
				this->_loadBalanceCheckBox->Text = L"Enable load balancing when processing <All workflows> ";
				this->_loadBalanceCheckBox->UseVisualStyleBackColor = true;
				this->_loadBalanceCheckBox->CheckedChanged += gcnew System::EventHandler(this, &WorkflowManagement::HandleLoadBalanceCheckBox_CheckedChanged);
				// 
				// actionsGroupBox
				// 
				this->actionsGroupBox->Anchor = static_cast<System::Windows::Forms::AnchorStyles>((((System::Windows::Forms::AnchorStyles::Top | System::Windows::Forms::AnchorStyles::Bottom)
					| System::Windows::Forms::AnchorStyles::Left)
					| System::Windows::Forms::AnchorStyles::Right));
				this->actionsGroupBox->Controls->Add(this->actionsGridView);
				this->actionsGroupBox->Controls->Add(this->renameActionButton);
				this->actionsGroupBox->Controls->Add(this->addActionButton);
				this->actionsGroupBox->Controls->Add(this->deleteActionButton);
				this->actionsGroupBox->Location = System::Drawing::Point(14, 118);
				this->actionsGroupBox->Name = L"actionsGroupBox";
				this->actionsGroupBox->Size = System::Drawing::Size(381, 306);
				this->actionsGroupBox->TabIndex = 11;
				this->actionsGroupBox->TabStop = false;
				this->actionsGroupBox->Text = L"Actions";
				// 
				// actionsGridView
				// 
				this->actionsGridView->AllowUserToAddRows = false;
				this->actionsGridView->AllowUserToDeleteRows = false;
				this->actionsGridView->AllowUserToResizeColumns = false;
				this->actionsGridView->AllowUserToResizeRows = false;
				this->actionsGridView->Anchor = static_cast<System::Windows::Forms::AnchorStyles>((((System::Windows::Forms::AnchorStyles::Top | System::Windows::Forms::AnchorStyles::Bottom)
					| System::Windows::Forms::AnchorStyles::Left)
					| System::Windows::Forms::AnchorStyles::Right));
				this->actionsGridView->BackgroundColor = System::Drawing::Color::White;
				this->actionsGridView->CellBorderStyle = System::Windows::Forms::DataGridViewCellBorderStyle::None;
				this->actionsGridView->ClipboardCopyMode = System::Windows::Forms::DataGridViewClipboardCopyMode::EnableWithoutHeaderText;
				this->actionsGridView->ColumnHeadersBorderStyle = System::Windows::Forms::DataGridViewHeaderBorderStyle::None;
				dataGridViewCellStyle1->Alignment = System::Windows::Forms::DataGridViewContentAlignment::MiddleLeft;
				dataGridViewCellStyle1->BackColor = System::Drawing::Color::FromArgb(static_cast<System::Int32>(static_cast<System::Byte>(196)),
					static_cast<System::Int32>(static_cast<System::Byte>(216)), static_cast<System::Int32>(static_cast<System::Byte>(242)));
				dataGridViewCellStyle1->Font = (gcnew System::Drawing::Font(L"Microsoft Sans Serif", 8.25F, System::Drawing::FontStyle::Regular,
					System::Drawing::GraphicsUnit::Point, static_cast<System::Byte>(0)));
				dataGridViewCellStyle1->ForeColor = System::Drawing::SystemColors::WindowText;
				dataGridViewCellStyle1->SelectionBackColor = System::Drawing::SystemColors::Highlight;
				dataGridViewCellStyle1->SelectionForeColor = System::Drawing::SystemColors::HighlightText;
				dataGridViewCellStyle1->WrapMode = System::Windows::Forms::DataGridViewTriState::False;
				this->actionsGridView->ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
				this->actionsGridView->ColumnHeadersHeightSizeMode = System::Windows::Forms::DataGridViewColumnHeadersHeightSizeMode::AutoSize;
				this->actionsGridView->Columns->AddRange(gcnew cli::array< System::Windows::Forms::DataGridViewColumn^  >(4) {
					this->ActionIDColumn,
						this->ActionIncludedColumn, this->ActionNameColumn, this->ActionMainSequenceColumn
				});
				dataGridViewCellStyle3->Alignment = System::Windows::Forms::DataGridViewContentAlignment::MiddleLeft;
				dataGridViewCellStyle3->BackColor = System::Drawing::SystemColors::Window;
				dataGridViewCellStyle3->Font = (gcnew System::Drawing::Font(L"Microsoft Sans Serif", 8.25F, System::Drawing::FontStyle::Regular,
					System::Drawing::GraphicsUnit::Point, static_cast<System::Byte>(0)));
				dataGridViewCellStyle3->ForeColor = System::Drawing::SystemColors::ControlText;
				dataGridViewCellStyle3->SelectionBackColor = System::Drawing::SystemColors::Highlight;
				dataGridViewCellStyle3->SelectionForeColor = System::Drawing::SystemColors::HighlightText;
				dataGridViewCellStyle3->WrapMode = System::Windows::Forms::DataGridViewTriState::False;
				this->actionsGridView->DefaultCellStyle = dataGridViewCellStyle3;
				this->actionsGridView->EditMode = System::Windows::Forms::DataGridViewEditMode::EditProgrammatically;
				this->actionsGridView->EnableHeadersVisualStyles = false;
				this->actionsGridView->GridColor = System::Drawing::Color::White;
				this->actionsGridView->Location = System::Drawing::Point(7, 19);
				this->actionsGridView->MultiSelect = false;
				this->actionsGridView->Name = L"actionsGridView";
				dataGridViewCellStyle4->Alignment = System::Windows::Forms::DataGridViewContentAlignment::MiddleLeft;
				dataGridViewCellStyle4->BackColor = System::Drawing::SystemColors::Control;
				dataGridViewCellStyle4->Font = (gcnew System::Drawing::Font(L"Microsoft Sans Serif", 8.25F, System::Drawing::FontStyle::Regular,
					System::Drawing::GraphicsUnit::Point, static_cast<System::Byte>(0)));
				dataGridViewCellStyle4->ForeColor = System::Drawing::SystemColors::WindowText;
				dataGridViewCellStyle4->SelectionBackColor = System::Drawing::SystemColors::Highlight;
				dataGridViewCellStyle4->SelectionForeColor = System::Drawing::SystemColors::HighlightText;
				dataGridViewCellStyle4->WrapMode = System::Windows::Forms::DataGridViewTriState::True;
				this->actionsGridView->RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
				this->actionsGridView->RowHeadersVisible = false;
				this->actionsGridView->RowTemplate->Height = 18;
				this->actionsGridView->SelectionMode = System::Windows::Forms::DataGridViewSelectionMode::FullRowSelect;
				this->actionsGridView->ShowCellErrors = false;
				this->actionsGridView->ShowCellToolTips = false;
				this->actionsGridView->ShowEditingIcon = false;
				this->actionsGridView->ShowRowErrors = false;
				this->actionsGridView->Size = System::Drawing::Size(365, 249);
				this->actionsGridView->TabIndex = 4;
				this->actionsGridView->CellContentClick += gcnew System::Windows::Forms::DataGridViewCellEventHandler(this, &WorkflowManagement::HandleCellContentClick);
				this->actionsGridView->AutoSizeRowsMode = DataGridViewAutoSizeRowsMode::AllCells;
				this->actionsGridView->AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode::AllCells;
				// 
				// ActionIDColumn
				// 
				this->ActionIDColumn->HeaderText = L"";
				this->ActionIDColumn->Name = L"ActionIDColumn";
				this->ActionIDColumn->Visible = false;
				// 
				// ActionIncludedColumn
				// 
				dataGridViewCellStyle2->Alignment = System::Windows::Forms::DataGridViewContentAlignment::MiddleLeft;
				dataGridViewCellStyle2->NullValue = false;
				dataGridViewCellStyle2->WrapMode = System::Windows::Forms::DataGridViewTriState::False;
				this->ActionIncludedColumn->DefaultCellStyle = dataGridViewCellStyle2;
				this->ActionIncludedColumn->HeaderText = L"";
				this->ActionIncludedColumn->MinimumWidth = 20;
				this->ActionIncludedColumn->Name = L"ActionIncludedColumn";
				this->ActionIncludedColumn->Width = 20;
				// 
				// ActionNameColumn
				// 
				this->ActionNameColumn->AutoSizeMode = System::Windows::Forms::DataGridViewAutoSizeColumnMode::Fill;
				this->ActionNameColumn->HeaderText = L"Name";
				this->ActionNameColumn->Name = L"ActionNameColumn";
				// 
				// ActionMainSequenceColumn
				// 
				this->ActionMainSequenceColumn->HeaderText = L"Main sequence";
				this->ActionMainSequenceColumn->MinimumWidth = 90;
				this->ActionMainSequenceColumn->Name = L"ActionMainSequenceColumn";
				this->ActionMainSequenceColumn->Width = 90;
				// 
				// WorkflowManagement
				// 
				this->AutoScaleDimensions = System::Drawing::SizeF(6, 13);
				this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
				this->CancelButton = this->closeButton;
				this->ClientSize = System::Drawing::Size(405, 459);
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
				this->ShowInTaskbar = false;
				this->StartPosition = System::Windows::Forms::FormStartPosition::CenterParent;
				this->Text = L"Workflow Management";
				this->FormClosing += gcnew System::Windows::Forms::FormClosingEventHandler(this, &WorkflowManagement::HandleWorkflowManagement_FormClosing);
				this->Load += gcnew System::EventHandler(this, &WorkflowManagement::HandleWorkflowManagement_Load);
				this->workflowGroupBox->ResumeLayout(false);
				this->workflowGroupBox->PerformLayout();
				this->actionsGroupBox->ResumeLayout(false);
				(cli::safe_cast<System::ComponentModel::ISupportInitialize^>(this->actionsGridView))->EndInit();
				this->ResumeLayout(false);

			}
#pragma endregion

};
	};
}

