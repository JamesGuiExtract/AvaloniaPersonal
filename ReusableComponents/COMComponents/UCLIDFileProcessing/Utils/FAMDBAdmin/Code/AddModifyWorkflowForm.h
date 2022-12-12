#pragma once

#include "RedactionVerificationSettings.h"

namespace Extract {
	namespace FAMDBAdmin {

		using namespace System;
		using namespace System::ComponentModel;
		using namespace System::Collections;
		using namespace System::Data;
		using namespace System::Drawing;
		using namespace	System::Runtime::Serialization;
		using namespace	System::Runtime::Serialization::Json;
		using namespace System::Windows::Forms;
		using namespace UCLID_FILEPROCESSINGLib;

		/// <summary>
		/// Summary for AddModifyWorkflowForm
		/// </summary>
		public ref class AddModifyWorkflowForm : public System::Windows::Forms::Form
		{
		public:

#pragma region Constructors
			// Constructor
			//		famDatabase: FAM database to use to manage workflows and actions
			//		workflowID : The ID of the current workflow modify - if this is -1 
			//					 the form will assume it will be creating a new workflow
			AddModifyWorkflowForm(IFileProcessingDBPtr famDatabase, Int32 workflowID);

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
			~AddModifyWorkflowForm()
			{
				if (components)
				{
					delete components;
				}
				// Release the memory for the workflow definition 
				if (_pipWorkflowDefinition)
				{
					*_pipWorkflowDefinition = NULL;
					delete _pipWorkflowDefinition;
					_pipWorkflowDefinition = __nullptr;
				}

				if (_pipfamDatabase)
				{
					*_pipfamDatabase = NULL;
					delete _pipfamDatabase;
					_pipfamDatabase = __nullptr;
				}
			}

#pragma endregion

		private:

#pragma region Event handlers

			Void HandleAddModifyWorkflowForm_Load(System::Object^  sender, System::EventArgs^  e);
			Void HandleOkButton_Click(System::Object^  sender, System::EventArgs^  e);

#pragma endregion

#pragma region Helper methods

			// Loads the workflow definition values into the form
			Void loadWorkflow();

			// Load the workflow type combo list
			Void loadWorkFlowTypeCombo();

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

			// The settings for web-based redaction verification for the current workflow, or
			// __nullptr if redaction verification is not configured for the current workflow.
			RedactionVerificationSettings ^_redactionWebAppSettings;

#pragma endregion

#pragma region Windows Form Designer generated code	
		
		private: System::Windows::Forms::Label^  workflowNameLabel;
		private: System::Windows::Forms::TextBox^  workflowNameTextBox;
		private: System::Windows::Forms::Label^  label1;
		private: System::Windows::Forms::ComboBox^  workFlowTypeComboBox;
		private: System::Windows::Forms::Label^  descriptionLabel;
		private: System::Windows::Forms::TextBox^  descriptionTextBox;














		private: System::Windows::Forms::Button^  okButton;
		private: System::Windows::Forms::Button^  cancelButton;






		private: System::Windows::Forms::ComboBox^  _loadBalanceWeightComboBox;
		private: System::Windows::Forms::Label^  label9;








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
				this->workflowNameLabel = (gcnew System::Windows::Forms::Label());
				this->workflowNameTextBox = (gcnew System::Windows::Forms::TextBox());
				this->label1 = (gcnew System::Windows::Forms::Label());
				this->workFlowTypeComboBox = (gcnew System::Windows::Forms::ComboBox());
				this->descriptionLabel = (gcnew System::Windows::Forms::Label());
				this->descriptionTextBox = (gcnew System::Windows::Forms::TextBox());
				this->okButton = (gcnew System::Windows::Forms::Button());
				this->cancelButton = (gcnew System::Windows::Forms::Button());
				this->_loadBalanceWeightComboBox = (gcnew System::Windows::Forms::ComboBox());
				this->label9 = (gcnew System::Windows::Forms::Label());
				this->SuspendLayout();
				// 
				// workflowNameLabel
				// 
				this->workflowNameLabel->AutoSize = true;
				this->workflowNameLabel->Location = System::Drawing::Point(10, 13);
				this->workflowNameLabel->Name = L"workflowNameLabel";
				this->workflowNameLabel->Size = System::Drawing::Size(35, 13);
				this->workflowNameLabel->TabIndex = 0;
				this->workflowNameLabel->Text = L"Name";
				// 
				// workflowNameTextBox
				// 
				this->workflowNameTextBox->Location = System::Drawing::Point(123, 9);
				this->workflowNameTextBox->Name = L"workflowNameTextBox";
				this->workflowNameTextBox->Size = System::Drawing::Size(349, 20);
				this->workflowNameTextBox->TabIndex = 1;
				// 
				// label1
				// 
				this->label1->AutoSize = true;
				this->label1->Location = System::Drawing::Point(10, 39);
				this->label1->Name = L"label1";
				this->label1->Size = System::Drawing::Size(75, 13);
				this->label1->TabIndex = 2;
				this->label1->Text = L"Workflow type";
				// 
				// workFlowTypeComboBox
				// 
				this->workFlowTypeComboBox->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
				this->workFlowTypeComboBox->FormattingEnabled = true;
				this->workFlowTypeComboBox->Location = System::Drawing::Point(123, 35);
				this->workFlowTypeComboBox->Name = L"workFlowTypeComboBox";
				this->workFlowTypeComboBox->Size = System::Drawing::Size(119, 21);
				this->workFlowTypeComboBox->TabIndex = 3;
				// 
				// descriptionLabel
				// 
				this->descriptionLabel->AutoSize = true;
				this->descriptionLabel->Location = System::Drawing::Point(10, 89);
				this->descriptionLabel->Name = L"descriptionLabel";
				this->descriptionLabel->Size = System::Drawing::Size(60, 13);
				this->descriptionLabel->TabIndex = 4;
				this->descriptionLabel->Text = L"Description";
				// 
				// descriptionTextBox
				// 
				this->descriptionTextBox->Anchor = static_cast<System::Windows::Forms::AnchorStyles>((((System::Windows::Forms::AnchorStyles::Top | System::Windows::Forms::AnchorStyles::Bottom)
					| System::Windows::Forms::AnchorStyles::Left)
					| System::Windows::Forms::AnchorStyles::Right));
				this->descriptionTextBox->Location = System::Drawing::Point(123, 89);
				this->descriptionTextBox->Multiline = true;
				this->descriptionTextBox->Name = L"descriptionTextBox";
				this->descriptionTextBox->Size = System::Drawing::Size(350, 90);
				this->descriptionTextBox->TabIndex = 5;
				// 
				// okButton
				// 
				this->okButton->Anchor = static_cast<System::Windows::Forms::AnchorStyles>((System::Windows::Forms::AnchorStyles::Bottom | System::Windows::Forms::AnchorStyles::Right));
				this->okButton->Location = System::Drawing::Point(317, 193);
				this->okButton->Name = L"okButton";
				this->okButton->Size = System::Drawing::Size(75, 23);
				this->okButton->TabIndex = 25;
				this->okButton->Text = L"O&K";
				this->okButton->UseVisualStyleBackColor = true;
				this->okButton->Click += gcnew System::EventHandler(this, &AddModifyWorkflowForm::HandleOkButton_Click);
				// 
				// cancelButton
				// 
				this->cancelButton->Anchor = static_cast<System::Windows::Forms::AnchorStyles>((System::Windows::Forms::AnchorStyles::Bottom | System::Windows::Forms::AnchorStyles::Right));
				this->cancelButton->CausesValidation = false;
				this->cancelButton->DialogResult = System::Windows::Forms::DialogResult::Cancel;
				this->cancelButton->Location = System::Drawing::Point(398, 193);
				this->cancelButton->Name = L"cancelButton";
				this->cancelButton->Size = System::Drawing::Size(75, 23);
				this->cancelButton->TabIndex = 26;
				this->cancelButton->Text = L"&Cancel";
				this->cancelButton->UseVisualStyleBackColor = true;
				// 
				// _loadBalanceWeightComboBox
				// 
				this->_loadBalanceWeightComboBox->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
				this->_loadBalanceWeightComboBox->FormattingEnabled = true;
				this->_loadBalanceWeightComboBox->Items->AddRange(gcnew cli::array< System::Object^  >(10) {
					L"1", L"2", L"3", L"4", L"5",
						L"6", L"7", L"8", L"9", L"10"
				});
				this->_loadBalanceWeightComboBox->Location = System::Drawing::Point(123, 62);
				this->_loadBalanceWeightComboBox->Name = L"_loadBalanceWeightComboBox";
				this->_loadBalanceWeightComboBox->Size = System::Drawing::Size(52, 21);
				this->_loadBalanceWeightComboBox->TabIndex = 21;
				// 
				// label9
				// 
				this->label9->AutoSize = true;
				this->label9->Location = System::Drawing::Point(11, 65);
				this->label9->Name = L"label9";
				this->label9->Size = System::Drawing::Size(106, 13);
				this->label9->TabIndex = 20;
				this->label9->Text = L"Load balance weight";
				// 
				// AddModifyWorkflowForm
				// 
				this->AcceptButton = this->okButton;
				this->AutoScaleDimensions = System::Drawing::SizeF(6, 13);
				this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
				this->CancelButton = this->cancelButton;
				this->ClientSize = System::Drawing::Size(484, 226);
				this->Controls->Add(this->_loadBalanceWeightComboBox);
				this->Controls->Add(this->label9);
				this->Controls->Add(this->cancelButton);
				this->Controls->Add(this->okButton);
				this->Controls->Add(this->workFlowTypeComboBox);
				this->Controls->Add(this->label1);
				this->Controls->Add(this->descriptionTextBox);
				this->Controls->Add(this->descriptionLabel);
				this->Controls->Add(this->workflowNameTextBox);
				this->Controls->Add(this->workflowNameLabel);
				this->MaximizeBox = false;
				this->MinimizeBox = false;
				this->MinimumSize = System::Drawing::Size(500, 265);
				this->Name = L"AddModifyWorkflowForm";
				this->ShowIcon = false;
				this->ShowInTaskbar = false;
				this->StartPosition = System::Windows::Forms::FormStartPosition::CenterParent;
				this->Text = L"Add/Modify Workflow";
				this->Load += gcnew System::EventHandler(this, &AddModifyWorkflowForm::HandleAddModifyWorkflowForm_Load);
				this->ResumeLayout(false);
				this->PerformLayout();

			}
#pragma endregion
};
	}
}