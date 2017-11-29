#pragma once

#include "RedactionVerificationSettings.h"

namespace Extract {
	namespace FAMDBAdmin {

		using namespace System;
		using namespace System::ComponentModel;
		using namespace System::Collections::Generic;
		using namespace System::Data;
		using namespace System::Drawing;
		using namespace System::IO;
		using namespace System::Linq;
		using namespace	System::Runtime::Serialization;
		using namespace	System::Runtime::Serialization::Json;
		using namespace System::Text;
		using namespace System::Windows::Forms;
		
		using namespace UCLID_FILEPROCESSINGLib;

		/// <summary>
		/// Allows configuration of a RedactionVerificationSettings instance.
		/// </summary>
		public ref class WorkflowVerifySettingsForm : public System::Windows::Forms::Form
		{
		public:

#pragma region Constructors
			// Constructor
			//		famDatabase: FAM database to use to manage workflows and actions
			//		workflowID:  The ID of the current workflow modify - if this is -1 
			//					 the form will assume it will be creating a new workflow
			//		settings:	 The RedactionVerificationSettings instance to be configured.
			//					 If null, a new instance should be created.
			WorkflowVerifySettingsForm(IFileProcessingDBPtr famDatabase, Int32 workflowID, RedactionVerificationSettings ^ settings);

#pragma endregion	
			
#pragma region Properties

			// This property is set internally based with the workflow definition associated
			// with the workflowID passed into the constructor
			// This will be populated with the changes made by this form 
			property IWorkflowDefinitionPtr ipWorkflowDefinition {
				IWorkflowDefinitionPtr get()
				{
					return *_pipWorkflowDefinition;
				}
				// This should only be set internally
				private: Void set(IWorkflowDefinitionPtr value)
				{
					*_pipWorkflowDefinition = value;
				}
			}

			// Represents the configured instance of RedactionVerificationSettings if the user OK'd
			// the settings. __nullptr if the user canceled configuration.
			property RedactionVerificationSettings^ Settings {
				RedactionVerificationSettings^ get()
				{
					return _settings;
				}
			}

		private:

			// This is a private property used internally to access the fam
			property IFileProcessingDBPtr _ipfamDatabase
			{
				IFileProcessingDBPtr get()
				{
					return *_pipfamDatabase;
				}
			}
			
#pragma endregion

		protected:

#pragma region Destructors

			/// <summary>
			/// Clean up any resources being used.
			/// </summary>
			~WorkflowVerifySettingsForm()
			{
				if (components)
				{
					delete components;
				}
				// Release the memory for the workflow definition 
				if (_pipWorkflowDefinition)
				{
					*_pipWorkflowDefinition = __nullptr;
					delete _pipWorkflowDefinition;
					_pipWorkflowDefinition = __nullptr;
				}

				if (_pipfamDatabase)
				{
					*_pipfamDatabase = __nullptr;
					delete _pipfamDatabase;
					_pipfamDatabase = __nullptr;
				}
			}

#pragma endregion

		private:

#pragma region Event handlers

			Void HandleWorkflowVerifySettingsForm_Load(System::Object^  sender, System::EventArgs^  e);
			Void HandleOkButton_Click(System::Object^  sender, System::EventArgs^  e);

#pragma endregion

#pragma region Helper methods

			Void loadActionsCombos();
			Void loadRedactionTypeGrid();
			System::Collections::Generic::IEnumerable<String ^>^ getConfiguredRedactionTypes();

#pragma endregion

#pragma region Variables
			
			// Pointer to a smart pointer for the workflow definition used by the 
			// property ipWorkflowDefinition -- if I want to include a COM smart pointer
			// as part of a managed class I have to create a pointer to it
			IWorkflowDefinitionPtr *_pipWorkflowDefinition;

			// The FAM database that is being managed
			IFileProcessingDBPtr *_pipfamDatabase;

			// workflow ID of the workflow being edited
			int _workflowID;
			
			// The RedactionVerificationSettings instance represented by this form.
			RedactionVerificationSettings ^_settings;

#pragma endregion

#pragma region Windows Form Designer generated code	
		






		private: System::Windows::Forms::Label^  label2;
private: System::Windows::Forms::ComboBox^  _postVerifyActionComboBox;














		private: System::Windows::Forms::Button^  okButton;
		private: System::Windows::Forms::Button^  cancelButton;








private: System::Windows::Forms::Label^  label1;
private: System::Windows::Forms::DataGridView^  _redactionTypesDataGridView;


private: System::Windows::Forms::DataGridViewTextBoxColumn^  _docTypeColumn;
private: System::Windows::Forms::ComboBox^  _verifyActionComboBox;

private: System::Windows::Forms::Label^  label3;






		private:
			/// <summary>
			/// Required designer variable.
			/// </summary>
			System::ComponentModel::Container ^components;


			/// <summary>
			/// Required method for Designer support - do not modify
			/// the contents of this method with the code editor.
			/// </summary>
			void InitializeComponent(void)
			{
				this->label2 = (gcnew System::Windows::Forms::Label());
				this->_postVerifyActionComboBox = (gcnew System::Windows::Forms::ComboBox());
				this->okButton = (gcnew System::Windows::Forms::Button());
				this->cancelButton = (gcnew System::Windows::Forms::Button());
				this->label1 = (gcnew System::Windows::Forms::Label());
				this->_redactionTypesDataGridView = (gcnew System::Windows::Forms::DataGridView());
				this->_docTypeColumn = (gcnew System::Windows::Forms::DataGridViewTextBoxColumn());
				this->_verifyActionComboBox = (gcnew System::Windows::Forms::ComboBox());
				this->label3 = (gcnew System::Windows::Forms::Label());
				(cli::safe_cast<System::ComponentModel::ISupportInitialize^>(this->_redactionTypesDataGridView))->BeginInit();
				this->SuspendLayout();
				// 
				// label2
				// 
				this->label2->AutoSize = true;
				this->label2->Location = System::Drawing::Point(13, 43);
				this->label2->Name = L"label2";
				this->label2->Size = System::Drawing::Size(88, 13);
				this->label2->TabIndex = 2;
				this->label2->Text = L"Post-verify action";
				// 
				// _postVerifyActionComboBox
				// 
				this->_postVerifyActionComboBox->Anchor = static_cast<System::Windows::Forms::AnchorStyles>(((System::Windows::Forms::AnchorStyles::Top | System::Windows::Forms::AnchorStyles::Left)
					| System::Windows::Forms::AnchorStyles::Right));
				this->_postVerifyActionComboBox->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
				this->_postVerifyActionComboBox->FormattingEnabled = true;
				this->_postVerifyActionComboBox->Location = System::Drawing::Point(125, 39);
				this->_postVerifyActionComboBox->Name = L"_postVerifyActionComboBox";
				this->_postVerifyActionComboBox->Size = System::Drawing::Size(259, 21);
				this->_postVerifyActionComboBox->Sorted = true;
				this->_postVerifyActionComboBox->TabIndex = 3;
				// 
				// okButton
				// 
				this->okButton->Anchor = static_cast<System::Windows::Forms::AnchorStyles>((System::Windows::Forms::AnchorStyles::Bottom | System::Windows::Forms::AnchorStyles::Right));
				this->okButton->Location = System::Drawing::Point(228, 292);
				this->okButton->Name = L"okButton";
				this->okButton->Size = System::Drawing::Size(75, 23);
				this->okButton->TabIndex = 6;
				this->okButton->Text = L"O&K";
				this->okButton->UseVisualStyleBackColor = true;
				this->okButton->Click += gcnew System::EventHandler(this, &WorkflowVerifySettingsForm::HandleOkButton_Click);
				// 
				// cancelButton
				// 
				this->cancelButton->Anchor = static_cast<System::Windows::Forms::AnchorStyles>((System::Windows::Forms::AnchorStyles::Bottom | System::Windows::Forms::AnchorStyles::Right));
				this->cancelButton->CausesValidation = false;
				this->cancelButton->DialogResult = System::Windows::Forms::DialogResult::Cancel;
				this->cancelButton->Location = System::Drawing::Point(309, 292);
				this->cancelButton->Name = L"cancelButton";
				this->cancelButton->Size = System::Drawing::Size(75, 23);
				this->cancelButton->TabIndex = 7;
				this->cancelButton->Text = L"&Cancel";
				this->cancelButton->UseVisualStyleBackColor = true;
				// 
				// label1
				// 
				this->label1->AutoSize = true;
				this->label1->Location = System::Drawing::Point(13, 71);
				this->label1->Name = L"label1";
				this->label1->Size = System::Drawing::Size(84, 13);
				this->label1->TabIndex = 4;
				this->label1->Text = L"Redaction types";
				// 
				// _redactionTypesDataGridView
				// 
				this->_redactionTypesDataGridView->Anchor = static_cast<System::Windows::Forms::AnchorStyles>((((System::Windows::Forms::AnchorStyles::Top | System::Windows::Forms::AnchorStyles::Bottom)
					| System::Windows::Forms::AnchorStyles::Left)
					| System::Windows::Forms::AnchorStyles::Right));
				this->_redactionTypesDataGridView->ColumnHeadersHeightSizeMode = System::Windows::Forms::DataGridViewColumnHeadersHeightSizeMode::AutoSize;
				this->_redactionTypesDataGridView->ColumnHeadersVisible = false;
				this->_redactionTypesDataGridView->Columns->AddRange(gcnew cli::array< System::Windows::Forms::DataGridViewColumn^  >(1) { this->_docTypeColumn });
				this->_redactionTypesDataGridView->Location = System::Drawing::Point(125, 66);
				this->_redactionTypesDataGridView->Name = L"_redactionTypesDataGridView";
				this->_redactionTypesDataGridView->Size = System::Drawing::Size(258, 220);
				this->_redactionTypesDataGridView->TabIndex = 5;
				// 
				// _docTypeColumn
				// 
				this->_docTypeColumn->AutoSizeMode = System::Windows::Forms::DataGridViewAutoSizeColumnMode::Fill;
				this->_docTypeColumn->HeaderText = L"Doc Type";
				this->_docTypeColumn->Name = L"_docTypeColumn";
				// 
				// _verifyActionComboBox
				// 
				this->_verifyActionComboBox->Anchor = static_cast<System::Windows::Forms::AnchorStyles>(((System::Windows::Forms::AnchorStyles::Top | System::Windows::Forms::AnchorStyles::Left)
					| System::Windows::Forms::AnchorStyles::Right));
				this->_verifyActionComboBox->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
				this->_verifyActionComboBox->FormattingEnabled = true;
				this->_verifyActionComboBox->Location = System::Drawing::Point(125, 12);
				this->_verifyActionComboBox->Name = L"_verifyActionComboBox";
				this->_verifyActionComboBox->Size = System::Drawing::Size(259, 21);
				this->_verifyActionComboBox->Sorted = true;
				this->_verifyActionComboBox->TabIndex = 1;
				// 
				// label3
				// 
				this->label3->AutoSize = true;
				this->label3->Location = System::Drawing::Point(13, 16);
				this->label3->Name = L"label3";
				this->label3->Size = System::Drawing::Size(65, 13);
				this->label3->TabIndex = 0;
				this->label3->Text = L"Verify action";
				// 
				// WorkflowVerifySettingsForm
				// 
				this->AcceptButton = this->okButton;
				this->AutoScaleDimensions = System::Drawing::SizeF(6, 13);
				this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
				this->CancelButton = this->cancelButton;
				this->ClientSize = System::Drawing::Size(395, 325);
				this->Controls->Add(this->_verifyActionComboBox);
				this->Controls->Add(this->label3);
				this->Controls->Add(this->_redactionTypesDataGridView);
				this->Controls->Add(this->label1);
				this->Controls->Add(this->cancelButton);
				this->Controls->Add(this->okButton);
				this->Controls->Add(this->_postVerifyActionComboBox);
				this->Controls->Add(this->label2);
				this->MaximizeBox = false;
				this->MinimizeBox = false;
				this->MinimumSize = System::Drawing::Size(411, 316);
				this->Name = L"WorkflowVerifySettingsForm";
				this->ShowIcon = false;
				this->ShowInTaskbar = false;
				this->StartPosition = System::Windows::Forms::FormStartPosition::CenterParent;
				this->Text = L"Redaction Verification Settings";
				this->Load += gcnew System::EventHandler(this, &WorkflowVerifySettingsForm::HandleWorkflowVerifySettingsForm_Load);
				(cli::safe_cast<System::ComponentModel::ISupportInitialize^>(this->_redactionTypesDataGridView))->EndInit();
				this->ResumeLayout(false);
				this->PerformLayout();

			}
#pragma endregion

		};
	}
}