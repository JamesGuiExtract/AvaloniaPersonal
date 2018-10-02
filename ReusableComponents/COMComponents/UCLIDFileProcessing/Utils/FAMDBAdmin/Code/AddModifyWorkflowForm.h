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

			Void HandleRedactionVerifySettingsButton_Click(System::Object^  sender, System::EventArgs^  e);
			Void HandleRedactionVerifyCheckBox_CheckedChanged(System::Object^  sender, System::EventArgs^  e);
			Void HandleAddModifyWorkflowForm_Load(System::Object^  sender, System::EventArgs^  e);
			Void HandleOkButton_Click(System::Object^  sender, System::EventArgs^  e);

#pragma endregion

#pragma region Helper methods

			// Loads the workflow definition values into the form
			Void loadWorkflow();

			// Loads the action combo lists - if the workflow ID was not -1
			// the combo boxes will be loaded with actions already associated with this workflow
			// if workflow ID was -1, all actions will be load in the action combo boxes
			Void loadActionComboLists();

			// Load the workflow type combo list
			Void loadWorkFlowTypeCombo();

			// Loads the attribute sets into the OutputAttributeSetCombo
			Void loadOutputAttributeSetCombo();

			// Loads the metadata fields into the outputFileMetadataFieldCombo
			Void loadOutputFileMetadataFieldCombo();

			// Loads web application settings of the specified type.
			Object ^ loadWebAppSettings(Type^ type);

			// Deletes existing web application settings of the specified type.
			Void deleteWebAppSettings(Type^ type);

			// Saves web application settings to the database.
			Void saveWebAppSettings(Object^ settings);

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
		private: System::Windows::Forms::Label^  label2;
		private: System::Windows::Forms::Label^  label3;
		private: System::Windows::Forms::Label^  label4;
		private: System::Windows::Forms::ComboBox^  startActionComboBox;
		private: System::Windows::Forms::ComboBox^  endActionComboBox;
		private: System::Windows::Forms::ComboBox^  postWorkflowActionComboBox;



		private: System::Windows::Forms::Label^  label5;
		private: System::Windows::Forms::TextBox^  documentFolderTextBox;

		private: System::Windows::Forms::Label^  label6;

		private: System::Windows::Forms::Button^  okButton;
		private: System::Windows::Forms::Button^  cancelButton;
		private: System::Windows::Forms::ComboBox^  outputAttributeSetComboBox;
		private: System::Windows::Forms::Label^  label7;
		private: System::Windows::Forms::ComboBox^  outputFileMetadataFieldComboBox;
		private: System::Windows::Forms::TextBox^  outputFilePathInitializationFunctionTextBox;
		private: System::Windows::Forms::Label^  label8;
		private: System::Windows::Forms::GroupBox^  groupBox1;
		private: System::Windows::Forms::ComboBox^  _loadBalanceWeightComboBox;
		private: System::Windows::Forms::Label^  label9;

		private: System::Windows::Forms::CheckBox^  _redactionVerifyCheckBox;
		private: System::Windows::Forms::Button^  _redactionVerifySettingsButton;
		private: System::Windows::Forms::Label^  label10;
		private: System::Windows::Forms::ComboBox^  editActionComboBox;
		private: System::Windows::Forms::ComboBox^  postEditActionComboBox;
		private: System::Windows::Forms::Label^  label11;

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
				this->label2 = (gcnew System::Windows::Forms::Label());
				this->label3 = (gcnew System::Windows::Forms::Label());
				this->label4 = (gcnew System::Windows::Forms::Label());
				this->startActionComboBox = (gcnew System::Windows::Forms::ComboBox());
				this->endActionComboBox = (gcnew System::Windows::Forms::ComboBox());
				this->postWorkflowActionComboBox = (gcnew System::Windows::Forms::ComboBox());
				this->label5 = (gcnew System::Windows::Forms::Label());
				this->documentFolderTextBox = (gcnew System::Windows::Forms::TextBox());
				this->label6 = (gcnew System::Windows::Forms::Label());
				this->okButton = (gcnew System::Windows::Forms::Button());
				this->cancelButton = (gcnew System::Windows::Forms::Button());
				this->outputAttributeSetComboBox = (gcnew System::Windows::Forms::ComboBox());
				this->label7 = (gcnew System::Windows::Forms::Label());
				this->outputFileMetadataFieldComboBox = (gcnew System::Windows::Forms::ComboBox());
				this->outputFilePathInitializationFunctionTextBox = (gcnew System::Windows::Forms::TextBox());
				this->label8 = (gcnew System::Windows::Forms::Label());
				this->groupBox1 = (gcnew System::Windows::Forms::GroupBox());
				this->_loadBalanceWeightComboBox = (gcnew System::Windows::Forms::ComboBox());
				this->label9 = (gcnew System::Windows::Forms::Label());
				this->_redactionVerifyCheckBox = (gcnew System::Windows::Forms::CheckBox());
				this->_redactionVerifySettingsButton = (gcnew System::Windows::Forms::Button());
				this->label10 = (gcnew System::Windows::Forms::Label());
				this->editActionComboBox = (gcnew System::Windows::Forms::ComboBox());
				this->postEditActionComboBox = (gcnew System::Windows::Forms::ComboBox());
				this->label11 = (gcnew System::Windows::Forms::Label());
				this->groupBox1->SuspendLayout();
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
				this->workflowNameTextBox->Location = System::Drawing::Point(142, 9);
				this->workflowNameTextBox->Name = L"workflowNameTextBox";
				this->workflowNameTextBox->Size = System::Drawing::Size(239, 20);
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
				this->workFlowTypeComboBox->Location = System::Drawing::Point(142, 35);
				this->workFlowTypeComboBox->Name = L"workFlowTypeComboBox";
				this->workFlowTypeComboBox->Size = System::Drawing::Size(119, 21);
				this->workFlowTypeComboBox->TabIndex = 3;
				// 
				// descriptionLabel
				// 
				this->descriptionLabel->AutoSize = true;
				this->descriptionLabel->Location = System::Drawing::Point(10, 62);
				this->descriptionLabel->Name = L"descriptionLabel";
				this->descriptionLabel->Size = System::Drawing::Size(60, 13);
				this->descriptionLabel->TabIndex = 4;
				this->descriptionLabel->Text = L"Description";
				// 
				// descriptionTextBox
				// 
				this->descriptionTextBox->Anchor = static_cast<System::Windows::Forms::AnchorStyles>(((System::Windows::Forms::AnchorStyles::Top | System::Windows::Forms::AnchorStyles::Left)
					| System::Windows::Forms::AnchorStyles::Right));
				this->descriptionTextBox->Location = System::Drawing::Point(142, 62);
				this->descriptionTextBox->Multiline = true;
				this->descriptionTextBox->Name = L"descriptionTextBox";
				this->descriptionTextBox->Size = System::Drawing::Size(428, 90);
				this->descriptionTextBox->TabIndex = 5;
				// 
				// label2
				// 
				this->label2->AutoSize = true;
				this->label2->Location = System::Drawing::Point(9, 188);
				this->label2->Name = L"label2";
				this->label2->Size = System::Drawing::Size(61, 13);
				this->label2->TabIndex = 8;
				this->label2->Text = L"Start action";
				// 
				// label3
				// 
				this->label3->AutoSize = true;
				this->label3->Location = System::Drawing::Point(9, 269);
				this->label3->Name = L"label3";
				this->label3->Size = System::Drawing::Size(58, 13);
				this->label3->TabIndex = 14;
				this->label3->Text = L"End action";
				// 
				// label4
				// 
				this->label4->AutoSize = true;
				this->label4->Location = System::Drawing::Point(10, 295);
				this->label4->Name = L"label4";
				this->label4->Size = System::Drawing::Size(105, 13);
				this->label4->TabIndex = 16;
				this->label4->Text = L"Post workflow action";
				// 
				// startActionComboBox
				// 
				this->startActionComboBox->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
				this->startActionComboBox->FormattingEnabled = true;
				this->startActionComboBox->Location = System::Drawing::Point(141, 184);
				this->startActionComboBox->Name = L"startActionComboBox";
				this->startActionComboBox->Size = System::Drawing::Size(239, 21);
				this->startActionComboBox->Sorted = true;
				this->startActionComboBox->TabIndex = 9;
				// 
				// endActionComboBox
				// 
				this->endActionComboBox->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
				this->endActionComboBox->FormattingEnabled = true;
				this->endActionComboBox->Location = System::Drawing::Point(141, 265);
				this->endActionComboBox->Name = L"endActionComboBox";
				this->endActionComboBox->Size = System::Drawing::Size(239, 21);
				this->endActionComboBox->Sorted = true;
				this->endActionComboBox->TabIndex = 15;
				// 
				// postWorkflowActionComboBox
				// 
				this->postWorkflowActionComboBox->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
				this->postWorkflowActionComboBox->FormattingEnabled = true;
				this->postWorkflowActionComboBox->Location = System::Drawing::Point(141, 292);
				this->postWorkflowActionComboBox->Name = L"postWorkflowActionComboBox";
				this->postWorkflowActionComboBox->Size = System::Drawing::Size(239, 21);
				this->postWorkflowActionComboBox->Sorted = true;
				this->postWorkflowActionComboBox->TabIndex = 17;
				// 
				// label5
				// 
				this->label5->AutoSize = true;
				this->label5->Location = System::Drawing::Point(9, 162);
				this->label5->Name = L"label5";
				this->label5->Size = System::Drawing::Size(85, 13);
				this->label5->TabIndex = 6;
				this->label5->Text = L"Document folder";
				// 
				// documentFolderTextBox
				// 
				this->documentFolderTextBox->Anchor = static_cast<System::Windows::Forms::AnchorStyles>(((System::Windows::Forms::AnchorStyles::Top | System::Windows::Forms::AnchorStyles::Left)
					| System::Windows::Forms::AnchorStyles::Right));
				this->documentFolderTextBox->Location = System::Drawing::Point(141, 158);
				this->documentFolderTextBox->Name = L"documentFolderTextBox";
				this->documentFolderTextBox->Size = System::Drawing::Size(428, 20);
				this->documentFolderTextBox->TabIndex = 7;
				// 
				// label6
				// 
				this->label6->AutoSize = true;
				this->label6->Location = System::Drawing::Point(10, 379);
				this->label6->Name = L"label6";
				this->label6->Size = System::Drawing::Size(97, 13);
				this->label6->TabIndex = 22;
				this->label6->Text = L"Output attribute set";
				// 
				// okButton
				// 
				this->okButton->Anchor = static_cast<System::Windows::Forms::AnchorStyles>((System::Windows::Forms::AnchorStyles::Bottom | System::Windows::Forms::AnchorStyles::Right));
				this->okButton->Location = System::Drawing::Point(414, 495);
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
				this->cancelButton->Location = System::Drawing::Point(495, 495);
				this->cancelButton->Name = L"cancelButton";
				this->cancelButton->Size = System::Drawing::Size(75, 23);
				this->cancelButton->TabIndex = 26;
				this->cancelButton->Text = L"&Cancel";
				this->cancelButton->UseVisualStyleBackColor = true;
				// 
				// outputAttributeSetComboBox
				// 
				this->outputAttributeSetComboBox->Anchor = static_cast<System::Windows::Forms::AnchorStyles>(((System::Windows::Forms::AnchorStyles::Top | System::Windows::Forms::AnchorStyles::Left)
					| System::Windows::Forms::AnchorStyles::Right));
				this->outputAttributeSetComboBox->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
				this->outputAttributeSetComboBox->FormattingEnabled = true;
				this->outputAttributeSetComboBox->Location = System::Drawing::Point(141, 375);
				this->outputAttributeSetComboBox->Name = L"outputAttributeSetComboBox";
				this->outputAttributeSetComboBox->Size = System::Drawing::Size(408, 21);
				this->outputAttributeSetComboBox->TabIndex = 23;
				// 
				// label7
				// 
				this->label7->AutoSize = true;
				this->label7->Location = System::Drawing::Point(10, 25);
				this->label7->Name = L"label7";
				this->label7->Size = System::Drawing::Size(74, 13);
				this->label7->TabIndex = 0;
				this->label7->Text = L"Metadata field";
				// 
				// outputFileMetadataFieldComboBox
				// 
				this->outputFileMetadataFieldComboBox->Anchor = static_cast<System::Windows::Forms::AnchorStyles>(((System::Windows::Forms::AnchorStyles::Top | System::Windows::Forms::AnchorStyles::Left)
					| System::Windows::Forms::AnchorStyles::Right));
				this->outputFileMetadataFieldComboBox->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
				this->outputFileMetadataFieldComboBox->FormattingEnabled = true;
				this->outputFileMetadataFieldComboBox->Location = System::Drawing::Point(129, 19);
				this->outputFileMetadataFieldComboBox->Name = L"outputFileMetadataFieldComboBox";
				this->outputFileMetadataFieldComboBox->Size = System::Drawing::Size(414, 21);
				this->outputFileMetadataFieldComboBox->TabIndex = 1;
				// 
				// outputFilePathInitializationFunctionTextBox
				// 
				this->outputFilePathInitializationFunctionTextBox->Anchor = static_cast<System::Windows::Forms::AnchorStyles>(((System::Windows::Forms::AnchorStyles::Top | System::Windows::Forms::AnchorStyles::Left)
					| System::Windows::Forms::AnchorStyles::Right));
				this->outputFilePathInitializationFunctionTextBox->Location = System::Drawing::Point(128, 49);
				this->outputFilePathInitializationFunctionTextBox->Name = L"outputFilePathInitializationFunctionTextBox";
				this->outputFilePathInitializationFunctionTextBox->Size = System::Drawing::Size(415, 20);
				this->outputFilePathInitializationFunctionTextBox->TabIndex = 3;
				// 
				// label8
				// 
				this->label8->AutoSize = true;
				this->label8->Location = System::Drawing::Point(10, 52);
				this->label8->Name = L"label8";
				this->label8->Size = System::Drawing::Size(101, 13);
				this->label8->TabIndex = 2;
				this->label8->Text = L"Initial value function";
				// 
				// groupBox1
				// 
				this->groupBox1->Anchor = static_cast<System::Windows::Forms::AnchorStyles>(((System::Windows::Forms::AnchorStyles::Top | System::Windows::Forms::AnchorStyles::Left)
					| System::Windows::Forms::AnchorStyles::Right));
				this->groupBox1->Controls->Add(this->label8);
				this->groupBox1->Controls->Add(this->outputFilePathInitializationFunctionTextBox);
				this->groupBox1->Controls->Add(this->outputFileMetadataFieldComboBox);
				this->groupBox1->Controls->Add(this->label7);
				this->groupBox1->Location = System::Drawing::Point(13, 402);
				this->groupBox1->Name = L"groupBox1";
				this->groupBox1->Size = System::Drawing::Size(557, 84);
				this->groupBox1->TabIndex = 24;
				this->groupBox1->TabStop = false;
				this->groupBox1->Text = L"Output file path configuration";
				// 
				// _loadBalanceWeightComboBox
				// 
				this->_loadBalanceWeightComboBox->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
				this->_loadBalanceWeightComboBox->FormattingEnabled = true;
				this->_loadBalanceWeightComboBox->Items->AddRange(gcnew cli::array< System::Object^  >(10) {
					L"1", L"2", L"3", L"4", L"5",
						L"6", L"7", L"8", L"9", L"10"
				});
				this->_loadBalanceWeightComboBox->Location = System::Drawing::Point(141, 348);
				this->_loadBalanceWeightComboBox->Name = L"_loadBalanceWeightComboBox";
				this->_loadBalanceWeightComboBox->Size = System::Drawing::Size(52, 21);
				this->_loadBalanceWeightComboBox->TabIndex = 21;
				// 
				// label9
				// 
				this->label9->AutoSize = true;
				this->label9->Location = System::Drawing::Point(10, 351);
				this->label9->Name = L"label9";
				this->label9->Size = System::Drawing::Size(106, 13);
				this->label9->TabIndex = 20;
				this->label9->Text = L"Load balance weight";
				// 
				// _redactionVerifyCheckBox
				// 
				this->_redactionVerifyCheckBox->AutoSize = true;
				this->_redactionVerifyCheckBox->Location = System::Drawing::Point(120, 324);
				this->_redactionVerifyCheckBox->Name = L"_redactionVerifyCheckBox";
				this->_redactionVerifyCheckBox->Size = System::Drawing::Size(15, 14);
				this->_redactionVerifyCheckBox->TabIndex = 18;
				this->_redactionVerifyCheckBox->UseVisualStyleBackColor = true;
				this->_redactionVerifyCheckBox->CheckedChanged += gcnew System::EventHandler(this, &AddModifyWorkflowForm::HandleRedactionVerifyCheckBox_CheckedChanged);
				// 
				// _redactionVerifySettingsButton
				// 
				this->_redactionVerifySettingsButton->Enabled = false;
				this->_redactionVerifySettingsButton->Location = System::Drawing::Point(141, 319);
				this->_redactionVerifySettingsButton->Name = L"_redactionVerifySettingsButton";
				this->_redactionVerifySettingsButton->Size = System::Drawing::Size(218, 23);
				this->_redactionVerifySettingsButton->TabIndex = 19;
				this->_redactionVerifySettingsButton->Text = L"Redaction Verification Settings";
				this->_redactionVerifySettingsButton->UseVisualStyleBackColor = true;
				this->_redactionVerifySettingsButton->Click += gcnew System::EventHandler(this, &AddModifyWorkflowForm::HandleRedactionVerifySettingsButton_Click);
				// 
				// label10
				// 
				this->label10->AutoSize = true;
				this->label10->Location = System::Drawing::Point(9, 215);
				this->label10->Name = L"label10";
				this->label10->Size = System::Drawing::Size(103, 13);
				this->label10->TabIndex = 10;
				this->label10->Text = L"Verify/update action";
				// 
				// editActionComboBox
				// 
				this->editActionComboBox->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
				this->editActionComboBox->FormattingEnabled = true;
				this->editActionComboBox->Location = System::Drawing::Point(141, 211);
				this->editActionComboBox->Name = L"editActionComboBox";
				this->editActionComboBox->Size = System::Drawing::Size(239, 21);
				this->editActionComboBox->Sorted = true;
				this->editActionComboBox->TabIndex = 11;
				// 
				// postEditActionComboBox
				// 
				this->postEditActionComboBox->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
				this->postEditActionComboBox->FormattingEnabled = true;
				this->postEditActionComboBox->Location = System::Drawing::Point(141, 238);
				this->postEditActionComboBox->Name = L"postEditActionComboBox";
				this->postEditActionComboBox->Size = System::Drawing::Size(239, 21);
				this->postEditActionComboBox->Sorted = true;
				this->postEditActionComboBox->TabIndex = 13;
				// 
				// label11
				// 
				this->label11->AutoSize = true;
				this->label11->Location = System::Drawing::Point(9, 242);
				this->label11->Name = L"label11";
				this->label11->Size = System::Drawing::Size(126, 13);
				this->label11->TabIndex = 12;
				this->label11->Text = L"Post-verify/update action";
				// 
				// AddModifyWorkflowForm
				// 
				this->AcceptButton = this->okButton;
				this->AutoScaleDimensions = System::Drawing::SizeF(6, 13);
				this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
				this->CancelButton = this->cancelButton;
				this->ClientSize = System::Drawing::Size(581, 528);
				this->Controls->Add(this->postEditActionComboBox);
				this->Controls->Add(this->label11);
				this->Controls->Add(this->editActionComboBox);
				this->Controls->Add(this->label10);
				this->Controls->Add(this->_redactionVerifySettingsButton);
				this->Controls->Add(this->_redactionVerifyCheckBox);
				this->Controls->Add(this->_loadBalanceWeightComboBox);
				this->Controls->Add(this->label9);
				this->Controls->Add(this->groupBox1);
				this->Controls->Add(this->cancelButton);
				this->Controls->Add(this->label6);
				this->Controls->Add(this->outputAttributeSetComboBox);
				this->Controls->Add(this->okButton);
				this->Controls->Add(this->documentFolderTextBox);
				this->Controls->Add(this->label5);
				this->Controls->Add(this->postWorkflowActionComboBox);
				this->Controls->Add(this->endActionComboBox);
				this->Controls->Add(this->startActionComboBox);
				this->Controls->Add(this->label2);
				this->Controls->Add(this->label4);
				this->Controls->Add(this->label3);
				this->Controls->Add(this->workFlowTypeComboBox);
				this->Controls->Add(this->label1);
				this->Controls->Add(this->descriptionTextBox);
				this->Controls->Add(this->descriptionLabel);
				this->Controls->Add(this->workflowNameTextBox);
				this->Controls->Add(this->workflowNameLabel);
				this->MaximizeBox = false;
				this->MinimizeBox = false;
				this->MinimumSize = System::Drawing::Size(597, 464);
				this->Name = L"AddModifyWorkflowForm";
				this->ShowIcon = false;
				this->ShowInTaskbar = false;
				this->StartPosition = System::Windows::Forms::FormStartPosition::CenterParent;
				this->Text = L"Add/Modify Workflow";
				this->Load += gcnew System::EventHandler(this, &AddModifyWorkflowForm::HandleAddModifyWorkflowForm_Load);
				this->groupBox1->ResumeLayout(false);
				this->groupBox1->PerformLayout();
				this->ResumeLayout(false);
				this->PerformLayout();

			}
#pragma endregion
};
	}
}