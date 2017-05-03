#pragma once
namespace Extract {
	namespace FAMDBAdmin {

		using namespace System;
		using namespace System::ComponentModel;
		using namespace System::Collections;
		using namespace System::Windows::Forms;
		using namespace System::Data;
		using namespace System::Drawing;
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
				this->label2->Location = System::Drawing::Point(10, 162);
				this->label2->Name = L"label2";
				this->label2->Size = System::Drawing::Size(61, 13);
				this->label2->TabIndex = 6;
				this->label2->Text = L"Start action";
				// 
				// label3
				// 
				this->label3->AutoSize = true;
				this->label3->Location = System::Drawing::Point(10, 189);
				this->label3->Name = L"label3";
				this->label3->Size = System::Drawing::Size(58, 13);
				this->label3->TabIndex = 8;
				this->label3->Text = L"End action";
				// 
				// label4
				// 
				this->label4->AutoSize = true;
				this->label4->Location = System::Drawing::Point(10, 216);
				this->label4->Name = L"label4";
				this->label4->Size = System::Drawing::Size(105, 13);
				this->label4->TabIndex = 10;
				this->label4->Text = L"Post workflow action";
				// 
				// startActionComboBox
				// 
				this->startActionComboBox->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
				this->startActionComboBox->FormattingEnabled = true;
				this->startActionComboBox->Location = System::Drawing::Point(142, 158);
				this->startActionComboBox->Name = L"startActionComboBox";
				this->startActionComboBox->Size = System::Drawing::Size(239, 21);
				this->startActionComboBox->TabIndex = 7;
				// 
				// endActionComboBox
				// 
				this->endActionComboBox->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
				this->endActionComboBox->FormattingEnabled = true;
				this->endActionComboBox->Location = System::Drawing::Point(142, 185);
				this->endActionComboBox->Name = L"endActionComboBox";
				this->endActionComboBox->Size = System::Drawing::Size(239, 21);
				this->endActionComboBox->TabIndex = 9;
				// 
				// postWorkflowActionComboBox
				// 
				this->postWorkflowActionComboBox->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
				this->postWorkflowActionComboBox->FormattingEnabled = true;
				this->postWorkflowActionComboBox->Location = System::Drawing::Point(142, 212);
				this->postWorkflowActionComboBox->Name = L"postWorkflowActionComboBox";
				this->postWorkflowActionComboBox->Size = System::Drawing::Size(239, 21);
				this->postWorkflowActionComboBox->TabIndex = 11;
				// 
				// label5
				// 
				this->label5->AutoSize = true;
				this->label5->Location = System::Drawing::Point(10, 243);
				this->label5->Name = L"label5";
				this->label5->Size = System::Drawing::Size(85, 13);
				this->label5->TabIndex = 12;
				this->label5->Text = L"Document folder";
				// 
				// documentFolderTextBox
				// 
				this->documentFolderTextBox->Anchor = static_cast<System::Windows::Forms::AnchorStyles>(((System::Windows::Forms::AnchorStyles::Top | System::Windows::Forms::AnchorStyles::Left)
					| System::Windows::Forms::AnchorStyles::Right));
				this->documentFolderTextBox->Location = System::Drawing::Point(142, 239);
				this->documentFolderTextBox->Name = L"documentFolderTextBox";
				this->documentFolderTextBox->Size = System::Drawing::Size(428, 20);
				this->documentFolderTextBox->TabIndex = 13;
				// 
				// label6
				// 
				this->label6->AutoSize = true;
				this->label6->Location = System::Drawing::Point(10, 270);
				this->label6->Name = L"label6";
				this->label6->Size = System::Drawing::Size(97, 13);
				this->label6->TabIndex = 14;
				this->label6->Text = L"Output attribute set";
				// 
				// okButton
				// 
				this->okButton->Anchor = static_cast<System::Windows::Forms::AnchorStyles>((System::Windows::Forms::AnchorStyles::Bottom | System::Windows::Forms::AnchorStyles::Right));
				this->okButton->Location = System::Drawing::Point(414, 393);
				this->okButton->Name = L"okButton";
				this->okButton->Size = System::Drawing::Size(75, 23);
				this->okButton->TabIndex = 17;
				this->okButton->Text = L"O&K";
				this->okButton->UseVisualStyleBackColor = true;
				this->okButton->Click += gcnew System::EventHandler(this, &AddModifyWorkflowForm::HandleOkButton_Click);
				// 
				// cancelButton
				// 
				this->cancelButton->Anchor = static_cast<System::Windows::Forms::AnchorStyles>((System::Windows::Forms::AnchorStyles::Bottom | System::Windows::Forms::AnchorStyles::Right));
				this->cancelButton->CausesValidation = false;
				this->cancelButton->DialogResult = System::Windows::Forms::DialogResult::Cancel;
				this->cancelButton->Location = System::Drawing::Point(495, 393);
				this->cancelButton->Name = L"cancelButton";
				this->cancelButton->Size = System::Drawing::Size(75, 23);
				this->cancelButton->TabIndex = 18;
				this->cancelButton->Text = L"&Cancel";
				this->cancelButton->UseVisualStyleBackColor = true;
				// 
				// outputAttributeSetComboBox
				// 
				this->outputAttributeSetComboBox->Anchor = static_cast<System::Windows::Forms::AnchorStyles>(((System::Windows::Forms::AnchorStyles::Top | System::Windows::Forms::AnchorStyles::Left)
					| System::Windows::Forms::AnchorStyles::Right));
				this->outputAttributeSetComboBox->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
				this->outputAttributeSetComboBox->FormattingEnabled = true;
				this->outputAttributeSetComboBox->Location = System::Drawing::Point(142, 266);
				this->outputAttributeSetComboBox->Name = L"outputAttributeSetComboBox";
				this->outputAttributeSetComboBox->Size = System::Drawing::Size(408, 21);
				this->outputAttributeSetComboBox->TabIndex = 15;
				// 
				// label7
				// 
				this->label7->AutoSize = true;
				this->label7->Location = System::Drawing::Point(10, 25);
				this->label7->Name = L"label7";
				this->label7->Size = System::Drawing::Size(74, 13);
				this->label7->TabIndex = 10;
				this->label7->Text = L"Metadata field";
				// 
				// outputFileMetadataFieldComboBox
				// 
				this->outputFileMetadataFieldComboBox->Anchor = static_cast<System::Windows::Forms::AnchorStyles>(((System::Windows::Forms::AnchorStyles::Top | System::Windows::Forms::AnchorStyles::Left)
					| System::Windows::Forms::AnchorStyles::Right));
				this->outputFileMetadataFieldComboBox->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
				this->outputFileMetadataFieldComboBox->FormattingEnabled = true;
				this->outputFileMetadataFieldComboBox->Location = System::Drawing::Point(130, 22);
				this->outputFileMetadataFieldComboBox->Name = L"outputFileMetadataFieldComboBox";
				this->outputFileMetadataFieldComboBox->Size = System::Drawing::Size(414, 21);
				this->outputFileMetadataFieldComboBox->TabIndex = 11;
				// 
				// outputFilePathInitializationFunctionTextBox
				// 
				this->outputFilePathInitializationFunctionTextBox->Anchor = static_cast<System::Windows::Forms::AnchorStyles>(((System::Windows::Forms::AnchorStyles::Top | System::Windows::Forms::AnchorStyles::Left)
					| System::Windows::Forms::AnchorStyles::Right));
				this->outputFilePathInitializationFunctionTextBox->Location = System::Drawing::Point(130, 49);
				this->outputFilePathInitializationFunctionTextBox->Name = L"outputFilePathInitializationFunctionTextBox";
				this->outputFilePathInitializationFunctionTextBox->Size = System::Drawing::Size(415, 20);
				this->outputFilePathInitializationFunctionTextBox->TabIndex = 13;
				this->outputFilePathInitializationFunctionTextBox->Text = L"<SourceDocName>.voa";
				// 
				// label8
				// 
				this->label8->AutoSize = true;
				this->label8->Location = System::Drawing::Point(10, 52);
				this->label8->Name = L"label8";
				this->label8->Size = System::Drawing::Size(101, 13);
				this->label8->TabIndex = 12;
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
				this->groupBox1->Location = System::Drawing::Point(12, 297);
				this->groupBox1->Name = L"groupBox1";
				this->groupBox1->Size = System::Drawing::Size(557, 84);
				this->groupBox1->TabIndex = 16;
				this->groupBox1->TabStop = false;
				this->groupBox1->Text = L"Output file path configuration";
				// 
				// AddModifyWorkflowForm
				// 
				this->AcceptButton = this->okButton;
				this->AutoScaleDimensions = System::Drawing::SizeF(6, 13);
				this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
				this->CancelButton = this->cancelButton;
				this->ClientSize = System::Drawing::Size(581, 426);
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