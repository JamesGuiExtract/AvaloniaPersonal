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
		






















		private: System::Windows::Forms::Button^  okButton;
		private: System::Windows::Forms::Button^  cancelButton;








private: System::Windows::Forms::Label^  label1;
private: System::Windows::Forms::DataGridView^  _redactionTypesDataGridView;


private: System::Windows::Forms::DataGridViewTextBoxColumn^  _docTypeColumn;


private: System::Windows::Forms::Label^ label3;
private: System::Windows::Forms::TextBox^ _docTypeListFilename;
private: System::Windows::Forms::Label^ _sessionTimeoutLabel;
private: System::Windows::Forms::Label^ label2;
private: System::Windows::Forms::CheckBox^ enableAllPendingQueueCheckBox;












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
				this->okButton = (gcnew System::Windows::Forms::Button());
				this->cancelButton = (gcnew System::Windows::Forms::Button());
				this->label1 = (gcnew System::Windows::Forms::Label());
				this->_redactionTypesDataGridView = (gcnew System::Windows::Forms::DataGridView());
				this->_docTypeColumn = (gcnew System::Windows::Forms::DataGridViewTextBoxColumn());
				this->label3 = (gcnew System::Windows::Forms::Label());
				this->_docTypeListFilename = (gcnew System::Windows::Forms::TextBox());
				this->_sessionTimeoutLabel = (gcnew System::Windows::Forms::Label());
				this->label2 = (gcnew System::Windows::Forms::Label());
				this->enableAllPendingQueueCheckBox = (gcnew System::Windows::Forms::CheckBox());
				(cli::safe_cast<System::ComponentModel::ISupportInitialize^>(this->_redactionTypesDataGridView))->BeginInit();
				this->SuspendLayout();
				// 
				// okButton
				// 
				this->okButton->Anchor = static_cast<System::Windows::Forms::AnchorStyles>((System::Windows::Forms::AnchorStyles::Bottom | System::Windows::Forms::AnchorStyles::Right));
				this->okButton->Location = System::Drawing::Point(228, 423);
				this->okButton->Name = L"okButton";
				this->okButton->Size = System::Drawing::Size(75, 23);
				this->okButton->TabIndex = 7;
				this->okButton->Text = L"O&K";
				this->okButton->UseVisualStyleBackColor = true;
				this->okButton->Click += gcnew System::EventHandler(this, &WorkflowVerifySettingsForm::HandleOkButton_Click);
				// 
				// cancelButton
				// 
				this->cancelButton->Anchor = static_cast<System::Windows::Forms::AnchorStyles>((System::Windows::Forms::AnchorStyles::Bottom | System::Windows::Forms::AnchorStyles::Right));
				this->cancelButton->CausesValidation = false;
				this->cancelButton->DialogResult = System::Windows::Forms::DialogResult::Cancel;
				this->cancelButton->Location = System::Drawing::Point(309, 423);
				this->cancelButton->Name = L"cancelButton";
				this->cancelButton->Size = System::Drawing::Size(75, 23);
				this->cancelButton->TabIndex = 8;
				this->cancelButton->Text = L"&Cancel";
				this->cancelButton->UseVisualStyleBackColor = true;
				// 
				// label1
				// 
				this->label1->AutoSize = true;
				this->label1->Location = System::Drawing::Point(13, 17);
				this->label1->Name = L"label1";
				this->label1->Size = System::Drawing::Size(84, 13);
				this->label1->TabIndex = 0;
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
				this->_redactionTypesDataGridView->Location = System::Drawing::Point(125, 12);
				this->_redactionTypesDataGridView->Name = L"_redactionTypesDataGridView";
				this->_redactionTypesDataGridView->Size = System::Drawing::Size(258, 281);
				this->_redactionTypesDataGridView->TabIndex = 1;
				// 
				// _docTypeColumn
				// 
				this->_docTypeColumn->AutoSizeMode = System::Windows::Forms::DataGridViewAutoSizeColumnMode::Fill;
				this->_docTypeColumn->HeaderText = L"Doc Type";
				this->_docTypeColumn->Name = L"_docTypeColumn";
				// 
				// label3
				// 
				this->label3->Anchor = static_cast<System::Windows::Forms::AnchorStyles>((System::Windows::Forms::AnchorStyles::Bottom | System::Windows::Forms::AnchorStyles::Left));
				this->label3->AutoSize = true;
				this->label3->Location = System::Drawing::Point(13, 323);
				this->label3->Name = L"label3";
				this->label3->Size = System::Drawing::Size(245, 13);
				this->label3->TabIndex = 2;
				this->label3->Text = L"Specify a file containing available document types:";
				// 
				// _docTypeListFilename
				// 
				this->_docTypeListFilename->Anchor = static_cast<System::Windows::Forms::AnchorStyles>(((System::Windows::Forms::AnchorStyles::Bottom | System::Windows::Forms::AnchorStyles::Left)
					| System::Windows::Forms::AnchorStyles::Right));
				this->_docTypeListFilename->Location = System::Drawing::Point(16, 340);
				this->_docTypeListFilename->Name = L"_docTypeListFilename";
				this->_docTypeListFilename->Size = System::Drawing::Size(368, 20);
				this->_docTypeListFilename->TabIndex = 3;
				// 
				// _sessionTimeoutLabel
				// 
				this->_sessionTimeoutLabel->Anchor = static_cast<System::Windows::Forms::AnchorStyles>((System::Windows::Forms::AnchorStyles::Bottom | System::Windows::Forms::AnchorStyles::Left));
				this->_sessionTimeoutLabel->AutoSize = true;
				this->_sessionTimeoutLabel->Location = System::Drawing::Point(13, 371);
				this->_sessionTimeoutLabel->Name = L"_sessionTimeoutLabel";
				this->_sessionTimeoutLabel->Size = System::Drawing::Size(112, 13);
				this->_sessionTimeoutLabel->TabIndex = 6;
				this->_sessionTimeoutLabel->Text = L"[Session timeout label]";
				// 
				// label2
				// 
				this->label2->Anchor = static_cast<System::Windows::Forms::AnchorStyles>((System::Windows::Forms::AnchorStyles::Bottom | System::Windows::Forms::AnchorStyles::Left));
				this->label2->AutoSize = true;
				this->label2->Location = System::Drawing::Point(13, 388);
				this->label2->Name = L"label2";
				this->label2->Size = System::Drawing::Size(321, 13);
				this->label2->TabIndex = 9;
				this->label2->Text = L"(Configure the session timeout in the database options security tab)";
				// 
				// enableAllPendingQueueCheckBox
				// 
				this->enableAllPendingQueueCheckBox->Anchor = static_cast<System::Windows::Forms::AnchorStyles>((System::Windows::Forms::AnchorStyles::Bottom | System::Windows::Forms::AnchorStyles::Left));
				this->enableAllPendingQueueCheckBox->AutoSize = true;
				this->enableAllPendingQueueCheckBox->Location = System::Drawing::Point(16, 303);
				this->enableAllPendingQueueCheckBox->Name = L"enableAllPendingQueueCheckBox";
				this->enableAllPendingQueueCheckBox->Size = System::Drawing::Size(169, 17);
				this->enableAllPendingQueueCheckBox->TabIndex = 10;
				this->enableAllPendingQueueCheckBox->Text = L"Enable all user pending queue";
				this->enableAllPendingQueueCheckBox->UseVisualStyleBackColor = true;
				// 
				// WorkflowVerifySettingsForm
				// 
				this->AcceptButton = this->okButton;
				this->AutoScaleDimensions = System::Drawing::SizeF(6, 13);
				this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
				this->CancelButton = this->cancelButton;
				this->ClientSize = System::Drawing::Size(395, 456);
				this->Controls->Add(this->enableAllPendingQueueCheckBox);
				this->Controls->Add(this->label2);
				this->Controls->Add(this->_docTypeListFilename);
				this->Controls->Add(this->label3);
				this->Controls->Add(this->_sessionTimeoutLabel);
				this->Controls->Add(this->_redactionTypesDataGridView);
				this->Controls->Add(this->label1);
				this->Controls->Add(this->cancelButton);
				this->Controls->Add(this->okButton);
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