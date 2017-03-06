#pragma once

namespace Extract
{
	namespace FAMDBAdmin
	{

		using namespace System;
		using namespace System::ComponentModel;
		using namespace System::Collections;
		using namespace System::Windows::Forms;
		using namespace System::Data;
		using namespace System::Drawing;

		/// <summary>
		/// Summary for AddActionForm
		/// </summary>
		public ref class AddActionForm : public System::Windows::Forms::Form
		{
		public:

#pragma region Constructors

			AddActionForm()
			{
				InitializeComponent();
			}

#pragma endregion	
			

#pragma region Properties

			// Property to return the action name to add
			property String ^ActionNameToAdd
			{
				String ^get()
				{
					return actionNameTextBox->Text;
				}
			}

#pragma endregion

#pragma region Destructors

		protected:
			/// <summary>
			/// Clean up any resources being used.
			/// </summary>
			~AddActionForm()
			{
				if (components)
				{
					delete components;
				}
			}
#pragma endregion

		private:

#pragma region Event Handlers
			
			Void HandleAddActionFormActionName_Validating(System::Object^  sender, System::ComponentModel::CancelEventArgs^  e);

#pragma endregion

#pragma region Windows Form Designer generated code

		protected:
		private: System::Windows::Forms::Button^  okButton;
		private: System::Windows::Forms::Label^  label1;
		private: System::Windows::Forms::TextBox^  actionNameTextBox;
		private: System::Windows::Forms::Button^  cancelButton;

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
				this->cancelButton = (gcnew System::Windows::Forms::Button());
				this->okButton = (gcnew System::Windows::Forms::Button());
				this->label1 = (gcnew System::Windows::Forms::Label());
				this->actionNameTextBox = (gcnew System::Windows::Forms::TextBox());
				this->SuspendLayout();
				// 
				// cancelButton
				// 
				this->cancelButton->Anchor = static_cast<System::Windows::Forms::AnchorStyles>((System::Windows::Forms::AnchorStyles::Bottom | System::Windows::Forms::AnchorStyles::Right));
				this->cancelButton->CausesValidation = false;
				this->cancelButton->DialogResult = System::Windows::Forms::DialogResult::Cancel;
				this->cancelButton->Location = System::Drawing::Point(302, 59);
				this->cancelButton->Name = L"cancelButton";
				this->cancelButton->Size = System::Drawing::Size(75, 23);
				this->cancelButton->TabIndex = 3;
				this->cancelButton->Text = L"Cancel";
				this->cancelButton->UseVisualStyleBackColor = true;
				// 
				// okButton
				// 
				this->okButton->Anchor = static_cast<System::Windows::Forms::AnchorStyles>((System::Windows::Forms::AnchorStyles::Bottom | System::Windows::Forms::AnchorStyles::Right));
				this->okButton->DialogResult = System::Windows::Forms::DialogResult::OK;
				this->okButton->Location = System::Drawing::Point(221, 59);
				this->okButton->Name = L"okButton";
				this->okButton->Size = System::Drawing::Size(75, 23);
				this->okButton->TabIndex = 2;
				this->okButton->Text = L"OK";
				this->okButton->UseVisualStyleBackColor = true;
				// 
				// label1
				// 
				this->label1->AutoSize = true;
				this->label1->Location = System::Drawing::Point(13, 13);
				this->label1->Name = L"label1";
				this->label1->Size = System::Drawing::Size(99, 13);
				this->label1->TabIndex = 4;
				this->label1->Text = L"Action name to add";
				// 
				// actionNameTextBox
				// 
				this->actionNameTextBox->Anchor = static_cast<System::Windows::Forms::AnchorStyles>(((System::Windows::Forms::AnchorStyles::Top | System::Windows::Forms::AnchorStyles::Left)
					| System::Windows::Forms::AnchorStyles::Right));
				this->actionNameTextBox->Location = System::Drawing::Point(16, 30);
				this->actionNameTextBox->Name = L"actionNameTextBox";
				this->actionNameTextBox->Size = System::Drawing::Size(360, 20);
				this->actionNameTextBox->TabIndex = 1;
				this->actionNameTextBox->Validating += gcnew System::ComponentModel::CancelEventHandler(this, &AddActionForm::HandleAddActionFormActionName_Validating);
				// 
				// AddActionForm
				// 
				this->AcceptButton = this->okButton;
				this->AutoScaleDimensions = System::Drawing::SizeF(6, 13);
				this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
				this->CancelButton = this->cancelButton;
				this->ClientSize = System::Drawing::Size(389, 94);
				this->ControlBox = false;
				this->Controls->Add(this->actionNameTextBox);
				this->Controls->Add(this->label1);
				this->Controls->Add(this->cancelButton);
				this->Controls->Add(this->okButton);
				this->MaximizeBox = false;
				this->MinimizeBox = false;
				this->Name = L"AddActionForm";
				this->ShowIcon = false;
				this->StartPosition = System::Windows::Forms::FormStartPosition::CenterParent;
				this->Text = L"Add Action";
				this->ResumeLayout(false);
				this->PerformLayout();

			}
#pragma endregion

		};
	}
}