#pragma once

#include <UCLIDException.h>

namespace Extract {
	namespace FAMDBAdmin {

		using namespace System;
		using namespace System::ComponentModel;
		using namespace System::Collections;
		using namespace System::Windows::Forms;
		using namespace System::Data;
		using namespace System::Drawing;

		/// <summary>
		/// Summary for MoveToWorkflowForm
		/// </summary>
		public ref class MoveToWorkflowForm : public System::Windows::Forms::Form
		{
		public:

#pragma region Constructors

			// Constructor
			//		famDatabase : FAM database to use to manage workflows and actions
			MoveToWorkflowForm(IFileProcessingDBPtr famDatabase)
			{
				InitializeComponent();

				// Create the pointer to the smart pointer for the database
				_pipfamDatabase = new IFileProcessingDBPtr();
				ASSERT_RESOURCE_ALLOCATION("ELI43394", _pipfamDatabase != __nullptr);

				// Set the fam database to the smart pointer
				*_pipfamDatabase = famDatabase;

				_pipFileSelector = new IFAMFileSelectorPtr();
				ASSERT_RESOURCE_ALLOCATION("ELI43396", _pipFileSelector != __nullptr);

				(*_pipFileSelector).CreateInstance(CLSID_FAMFileSelector);
				ASSERT_RESOURCE_ALLOCATION("ELI43397", *_pipFileSelector != __nullptr);
			}

#pragma endregion

		private:
#pragma region Event Handlers

			Void HandleMoveToWorkflowForm_Load(System::Object^  sender, System::EventArgs^  e);
			Void HandleMoveToWorkflowForm_SelectFilesButton_Click(System::Object^  sender, System::EventArgs^  e);
			Void HandleMoveToWorkflowForm_OKButton_Click(System::Object^ sender, System::EventArgs^ e);
			Void HandleMoveToWorkflowForm_ApplyButton_Click(System::Object^ sender, System::EventArgs^ e);

#pragma endregion
#pragma region Helper methods

			// Loads the workflow combo which also checks the actions for the workflow
			Void loadWorkflowCombos();

			Void applyWorkflowChanges(bool closeDialog);

			bool areSettingsValid();

#pragma endregion

		protected:

#pragma region Destructors

			/// <summary>
			/// Clean up any resources being used.
			/// </summary>
			~MoveToWorkflowForm()
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
				
				if (_pipFileSelector)
				{
					*_pipFileSelector = __nullptr;
					delete _pipFileSelector;
					_pipFileSelector = __nullptr;
				}
			}

#pragma endregion

		private:

#pragma region Private variables


			property IFAMFileSelectorPtr _ipFileSelector
			{
				IFAMFileSelectorPtr get()
				{
					return *_pipFileSelector;
				}
			}

			IFAMFileSelectorPtr *_pipFileSelector;

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
			
			System::Windows::Forms::Button^  _okButton;
			System::Windows::Forms::Button^  _cancelButton;
			System::Windows::Forms::Button^  _applyButton;
			System::Windows::Forms::GroupBox^  groupBox3;
			System::Windows::Forms::ComboBox^  _sourceWorkflowComboBox;
			System::Windows::Forms::Label^  label2;
			System::Windows::Forms::GroupBox^  groupBox1;
			System::Windows::Forms::Button^  _buttonSelectFiles;
			System::Windows::Forms::TextBox^  _selectFilesSummaryTextBox;

			System::Windows::Forms::GroupBox^  groupBox2;
			System::Windows::Forms::ComboBox^  _destinationWorkflowComboBox;

			System::Windows::Forms::Label^  label1;


			/// <summary>
			/// Required designer variable.
			/// </summary>
			System::ComponentModel::Container ^components;

#pragma endregion

#pragma region Windows Form Designer generated code
			/// <summary>
			/// Required method for Designer support - do not modify
			/// the contents of this method with the code editor.
			/// </summary>
			void InitializeComponent(void)
			{
				this->groupBox1 = (gcnew System::Windows::Forms::GroupBox());
				this->_buttonSelectFiles = (gcnew System::Windows::Forms::Button());
				this->_selectFilesSummaryTextBox = (gcnew System::Windows::Forms::TextBox());
				this->groupBox2 = (gcnew System::Windows::Forms::GroupBox());
				this->_destinationWorkflowComboBox = (gcnew System::Windows::Forms::ComboBox());
				this->label1 = (gcnew System::Windows::Forms::Label());
				this->_okButton = (gcnew System::Windows::Forms::Button());
				this->_cancelButton = (gcnew System::Windows::Forms::Button());
				this->_applyButton = (gcnew System::Windows::Forms::Button());
				this->groupBox3 = (gcnew System::Windows::Forms::GroupBox());
				this->_sourceWorkflowComboBox = (gcnew System::Windows::Forms::ComboBox());
				this->label2 = (gcnew System::Windows::Forms::Label());
				this->groupBox1->SuspendLayout();
				this->groupBox2->SuspendLayout();
				this->groupBox3->SuspendLayout();
				this->SuspendLayout();
				// 
				// groupBox1
				// 
				this->groupBox1->Controls->Add(this->_buttonSelectFiles);
				this->groupBox1->Controls->Add(this->_selectFilesSummaryTextBox);
				this->groupBox1->Location = System::Drawing::Point(12, 12);
				this->groupBox1->Name = L"groupBox1";
				this->groupBox1->Size = System::Drawing::Size(436, 107);
				this->groupBox1->TabIndex = 0;
				this->groupBox1->TabStop = false;
				this->groupBox1->Text = L"Select files to move to workflow";
				// 
				// _buttonSelectFiles
				// 
				this->_buttonSelectFiles->Location = System::Drawing::Point(341, 19);
				this->_buttonSelectFiles->Name = L"_buttonSelectFiles";
				this->_buttonSelectFiles->Size = System::Drawing::Size(88, 23);
				this->_buttonSelectFiles->TabIndex = 2;
				this->_buttonSelectFiles->Text = L"Select files...";
				this->_buttonSelectFiles->UseVisualStyleBackColor = true;
				this->_buttonSelectFiles->Click += gcnew System::EventHandler(this, &MoveToWorkflowForm::HandleMoveToWorkflowForm_SelectFilesButton_Click);
				// 
				// _selectFilesSummaryTextBox
				// 
				this->_selectFilesSummaryTextBox->Location = System::Drawing::Point(13, 20);
				this->_selectFilesSummaryTextBox->Multiline = true;
				this->_selectFilesSummaryTextBox->Name = L"_selectFilesSummaryTextBox";
				this->_selectFilesSummaryTextBox->ReadOnly = true;
				this->_selectFilesSummaryTextBox->Size = System::Drawing::Size(322, 81);
				this->_selectFilesSummaryTextBox->TabIndex = 1;
				this->_selectFilesSummaryTextBox->TabStop = false;
				// 
				// groupBox2
				// 
				this->groupBox2->Controls->Add(this->_destinationWorkflowComboBox);
				this->groupBox2->Controls->Add(this->label1);
				this->groupBox2->Location = System::Drawing::Point(13, 190);
				this->groupBox2->Name = L"groupBox2";
				this->groupBox2->Size = System::Drawing::Size(435, 60);
				this->groupBox2->TabIndex = 5;
				this->groupBox2->TabStop = false;
				this->groupBox2->Text = L"Destination";
				// 
				// _destinationWorkflowComboBox
				// 
				this->_destinationWorkflowComboBox->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
				this->_destinationWorkflowComboBox->FormattingEnabled = true;
				this->_destinationWorkflowComboBox->Location = System::Drawing::Point(66, 20);
				this->_destinationWorkflowComboBox->Name = L"_destinationWorkflowComboBox";
				this->_destinationWorkflowComboBox->Size = System::Drawing::Size(362, 21);
				this->_destinationWorkflowComboBox->TabIndex = 6;
				// 
				// label1
				// 
				this->label1->AutoSize = true;
				this->label1->Location = System::Drawing::Point(8, 23);
				this->label1->Name = L"label1";
				this->label1->Size = System::Drawing::Size(52, 13);
				this->label1->TabIndex = 0;
				this->label1->Text = L"Workflow";
				// 
				// _okButton
				// 
				this->_okButton->Location = System::Drawing::Point(214, 256);
				this->_okButton->Name = L"_okButton";
				this->_okButton->Size = System::Drawing::Size(75, 23);
				this->_okButton->TabIndex = 7;
				this->_okButton->Text = L"OK";
				this->_okButton->UseVisualStyleBackColor = true;
				this->_okButton->Click += gcnew System::EventHandler(this, &MoveToWorkflowForm::HandleMoveToWorkflowForm_OKButton_Click);
				// 
				// _cancelButton
				// 
				this->_cancelButton->DialogResult = System::Windows::Forms::DialogResult::Cancel;
				this->_cancelButton->Location = System::Drawing::Point(293, 256);
				this->_cancelButton->Name = L"_cancelButton";
				this->_cancelButton->Size = System::Drawing::Size(75, 23);
				this->_cancelButton->TabIndex = 8;
				this->_cancelButton->Text = L"Cancel";
				this->_cancelButton->UseVisualStyleBackColor = true;
				// 
				// _applyButton
				// 
				this->_applyButton->Location = System::Drawing::Point(372, 256);
				this->_applyButton->Name = L"_applyButton";
				this->_applyButton->Size = System::Drawing::Size(75, 23);
				this->_applyButton->TabIndex = 9;
				this->_applyButton->Text = L"Apply";
				this->_applyButton->UseVisualStyleBackColor = true;
				this->_applyButton->Click += gcnew System::EventHandler(this, &MoveToWorkflowForm::HandleMoveToWorkflowForm_ApplyButton_Click);
				// 
				// groupBox3
				// 
				this->groupBox3->Controls->Add(this->_sourceWorkflowComboBox);
				this->groupBox3->Controls->Add(this->label2);
				this->groupBox3->Location = System::Drawing::Point(13, 120);
				this->groupBox3->Name = L"groupBox3";
				this->groupBox3->Size = System::Drawing::Size(435, 60);
				this->groupBox3->TabIndex = 3;
				this->groupBox3->TabStop = false;
				this->groupBox3->Text = L"Source";
				// 
				// _sourceWorkflowComboBox
				// 
				this->_sourceWorkflowComboBox->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
				this->_sourceWorkflowComboBox->FormattingEnabled = true;
				this->_sourceWorkflowComboBox->Location = System::Drawing::Point(66, 20);
				this->_sourceWorkflowComboBox->Name = L"_sourceWorkflowComboBox";
				this->_sourceWorkflowComboBox->Size = System::Drawing::Size(362, 21);
				this->_sourceWorkflowComboBox->TabIndex = 4;
				// 
				// label2
				// 
				this->label2->AutoSize = true;
				this->label2->Location = System::Drawing::Point(8, 23);
				this->label2->Name = L"label2";
				this->label2->Size = System::Drawing::Size(52, 13);
				this->label2->TabIndex = 0;
				this->label2->Text = L"Workflow";
				// 
				// MoveToWorkflowForm
				// 
				this->AcceptButton = this->_okButton;
				this->AutoScaleDimensions = System::Drawing::SizeF(6, 13);
				this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
				this->CancelButton = this->_cancelButton;
				this->ClientSize = System::Drawing::Size(460, 291);
				this->Controls->Add(this->groupBox3);
				this->Controls->Add(this->_applyButton);
				this->Controls->Add(this->_cancelButton);
				this->Controls->Add(this->_okButton);
				this->Controls->Add(this->groupBox2);
				this->Controls->Add(this->groupBox1);
				this->FormBorderStyle = System::Windows::Forms::FormBorderStyle::FixedDialog;
				this->MaximizeBox = false;
				this->MinimizeBox = false;
				this->Name = L"MoveToWorkflowForm";
				this->ShowIcon = false;
				this->StartPosition = System::Windows::Forms::FormStartPosition::CenterParent;
				this->Text = L"Move files to workflow";
				this->Load += gcnew System::EventHandler(this, &MoveToWorkflowForm::HandleMoveToWorkflowForm_Load);
				this->groupBox1->ResumeLayout(false);
				this->groupBox1->PerformLayout();
				this->groupBox2->ResumeLayout(false);
				this->groupBox2->PerformLayout();
				this->groupBox3->ResumeLayout(false);
				this->groupBox3->PerformLayout();
				this->ResumeLayout(false);

			}
#pragma endregion
};
	}
}