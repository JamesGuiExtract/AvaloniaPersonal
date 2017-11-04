#include "StdAfx.h"
#include "AddModifyWorkflowForm.h"
#include "WorkflowVerifySettingsForm.h"
#include "ListCtrlHelper.h"

#include <UCLIDException.h>

// Disable all the warnings that generated by including the msclr header files
#pragma warning(disable : 4945)

// includes to allow simple syntax for marshaling between c++ types and .net types
#include <msclr\marshal_cppstd.h>
#include <msclr\marshal_windows.h>

using namespace msclr::interop;
using namespace AttributeDbMgrComponentsLib;

namespace Extract
{
	namespace FAMDBAdmin
	{

#pragma region Constructors

		AddModifyWorkflowForm::AddModifyWorkflowForm(IFileProcessingDBPtr famDatabase, Int32 workflowID)
		{
			try
			{
				InitializeComponent();

				_pipfamDatabase = new IFileProcessingDBPtr();
				*_pipfamDatabase = famDatabase;

				_workflowID = workflowID;

				// Set the title 
				if (_workflowID < 0)
				{
					Text = "Add workflow";
				}
				else
				{
					Text = "Modify workflow";
				}

				// Create the pointer to the smart pointer for the WorkflowDefinition
				_pipWorkflowDefinition = new IWorkflowDefinitionPtr;
				if (_workflowID >= 0)
				{
					ipWorkflowDefinition = _ipfamDatabase->GetWorkflowDefinition(_workflowID);
				}
				else
				{
					_pipWorkflowDefinition->CreateInstance(CLSID_WorkflowDefinition);
					ASSERT_RESOURCE_ALLOCATION("ELI41983", ipWorkflowDefinition != __nullptr);
				}

			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI41982");
		}

#pragma endregion

#pragma region Event handlers

		Void AddModifyWorkflowForm::HandleRedactionVerifySettingsButton_Click(System::Object^  sender, System::EventArgs^  e)
		{
			try
			{
				WorkflowVerifySettingsForm ^verifySettingsForm;
				try
				{
					if (_redactionWebAppSettings == __nullptr)
					{
						_redactionWebAppSettings = dynamic_cast<RedactionVerificationSettings ^>(
							loadWebAppSettings(RedactionVerificationSettings::typeid));
					}

					verifySettingsForm = gcnew WorkflowVerifySettingsForm(_ipfamDatabase, _workflowID, _redactionWebAppSettings);
					if (verifySettingsForm->ShowDialog() == System::Windows::Forms::DialogResult::OK)
					{
						_redactionWebAppSettings = verifySettingsForm->Settings;
					}
				}
				finally
				{
					delete verifySettingsForm;
				}
			}
			CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI45064");
		}

		Void AddModifyWorkflowForm::HandleRedactionVerifyCheckBox_CheckedChanged(System::Object^  sender, System::EventArgs^  e)
		{
			try
			{
				_redactionVerifySettingsButton->Enabled = _redactionVerifyCheckBox->Checked;
			}
			CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI45072");
		}

		Void AddModifyWorkflowForm::HandleAddModifyWorkflowForm_Load(System::Object ^ sender, System::EventArgs ^ e)
		{
			try
			{
				// Load data into the form				
				loadActionComboLists();
				loadWorkFlowTypeCombo();
				loadOutputAttributeSetCombo();	
				loadOutputFileMetadataFieldCombo();
				loadWorkflow();
				_loadBalanceWeightComboBox->SelectedItem =
					ipWorkflowDefinition->LoadBalanceWeight.ToString();

				_redactionVerifyCheckBox->Checked =
					(loadWebAppSettings(RedactionVerificationSettings::typeid) != __nullptr);
			}
			CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI41952");
		}

		Void AddModifyWorkflowForm::HandleOkButton_Click(System::Object ^ sender, System::EventArgs ^ e)
		{
			try
			{
				// Set the result so that if an exception is thrown the dialog will stay open
				DialogResult = System::Windows::Forms::DialogResult::None;

				marshal_context context;

				// Trim leading and trailing whitespace from the workflow name
				String ^workflowName = workflowNameTextBox->Text->Trim();

				// Verify that the name is not empty
				if (String::IsNullOrWhiteSpace(workflowName))
				{
					System::Windows::Forms::MessageBox::Show("Workflow name cannot be empty.");
					workflowNameTextBox->Focus();
					return;
				}

				if (_redactionVerifyCheckBox->Checked && _redactionWebAppSettings == __nullptr)
				{
					MessageBox::Show("Redaction verification settings have not been configured.");
					_redactionVerifySettingsButton->Focus();
					return;
				}

				ipWorkflowDefinition->Name = context.marshal_as<BSTR>(workflowName);

				// Set the description
				ipWorkflowDefinition->Description = context.marshal_as<BSTR>(descriptionTextBox->Text);

				// Used to add the actions that are part of the definition to the workflow
				IIUnknownVectorPtr ipActions(CLSID_IUnknownVector);

				EWorkflowType workflowType = kUndefined;
				if (workFlowTypeComboBox->SelectedIndex >= 0)
				{
					ListItemPair ^selected = safe_cast<ListItemPair^>(workFlowTypeComboBox->SelectedItem);
					workflowType = safe_cast<EWorkflowType>(selected->ID);
					ipWorkflowDefinition->Type = workflowType;
				}

				String^ startAction;
				if (startActionComboBox->SelectedIndex >= 0)
				{
					startAction = (String^)startActionComboBox->SelectedItem;
					ipWorkflowDefinition->StartAction = context.marshal_as<BSTR>(startAction);
					if (!String::IsNullOrWhiteSpace(startAction))
					{
						IVariantVectorPtr ipActionInfo(CLSID_VariantVector);
						ipActionInfo->PushBack(ipWorkflowDefinition->StartAction);
						ipActionInfo->PushBack(VARIANT_TRUE);
						
						ipActions->PushBack(ipActionInfo);
					}
				}
				String^ endAction;
				if (endActionComboBox->SelectedIndex >= 0)
				{
					endAction = (String^)endActionComboBox->SelectedItem;
					ipWorkflowDefinition->EndAction = context.marshal_as<BSTR>(endAction);
					if (!String::IsNullOrWhiteSpace(endAction))
					{
						IVariantVectorPtr ipActionInfo(CLSID_VariantVector);
						ipActionInfo->PushBack(ipWorkflowDefinition->EndAction);
						ipActionInfo->PushBack(VARIANT_TRUE);

						ipActions->PushBack(ipActionInfo);
					}
				}
				// 0 is a blank value so the post action should always be able to be empty
				if (postWorkflowActionComboBox->SelectedIndex > 0)
				{
					String^ postWorkflowAction = (String^)postWorkflowActionComboBox->SelectedItem;
					if (postWorkflowAction == startAction || postWorkflowAction == endAction)
					{
						System::Windows::Forms::MessageBox::Show(
							"Post workflow action cannot be same as start or end action.");
						postWorkflowActionComboBox->Focus();
						return;
					}

					ipWorkflowDefinition->PostWorkflowAction = context.marshal_as<BSTR>(postWorkflowAction);
					if (!String::IsNullOrWhiteSpace(postWorkflowAction))
					{
						IVariantVectorPtr ipActionInfo(CLSID_VariantVector);
						ipActionInfo->PushBack(ipWorkflowDefinition->PostWorkflowAction);
						ipActionInfo->PushBack(VARIANT_FALSE);

						ipActions->PushBack(ipActionInfo);
					}
				}

				if (outputAttributeSetComboBox->SelectedIndex >= 0)
				{
					ListItemPair^ selected = safe_cast<ListItemPair^>(outputAttributeSetComboBox->SelectedItem);
					ipWorkflowDefinition->OutputAttributeSet = context.marshal_as<BSTR>(selected->Name);
				}
				ipWorkflowDefinition->DocumentFolder = context.marshal_as<BSTR>(documentFolderTextBox->Text);

				ipWorkflowDefinition->LoadBalanceWeight = Int32::Parse((String^)_loadBalanceWeightComboBox->SelectedItem);

				if (outputFileMetadataFieldComboBox->SelectedIndex >= 0)
				{
					ipWorkflowDefinition->OutputFileMetadataField = context.marshal_as<BSTR>((String^)outputFileMetadataFieldComboBox->SelectedItem);
				}
				
				// This is a new workflow so add it to the database and set actions to what ever is selected
				// in the combo boxes
				if (_workflowID == -1)
				{
					_workflowID = _ipfamDatabase->AddWorkflow(ipWorkflowDefinition->Name, workflowType);
					ipWorkflowDefinition->ID = _workflowID;

					if (ipActions->Size() > 0)
					{
						// Need to add the actions that are in the definition to the workflow
						_ipfamDatabase->SetWorkflowActions(_workflowID, ipActions);
					}
				}

				ipWorkflowDefinition->OutputFilePathInitializationFunction =
					context.marshal_as<BSTR>(outputFilePathInitializationFunctionTextBox->Text);

				// Update the workflow definition
				_ipfamDatabase->SetWorkflowDefinition(ipWorkflowDefinition);

				if (_redactionVerifyCheckBox->Checked)
				{
					saveWebAppSettings(_redactionWebAppSettings);
				}
				else
				{
					deleteWebAppSettings(RedactionVerificationSettings::typeid);
				}

				// Successfully added/updated the workflow definition so set result to ok
				DialogResult = System::Windows::Forms::DialogResult::OK;
			}
			CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI41981");			
		}

#pragma endregion

#pragma region Helper methods

		Void AddModifyWorkflowForm::loadWorkflow()
		{
			// Load the form from the loaded workflow definition if this is not a new workflow
			if (_workflowID >= 0)
			{
				workflowNameTextBox->Text = marshal_as<String ^>(ipWorkflowDefinition->Name);
				descriptionTextBox->Text = marshal_as<String ^>(ipWorkflowDefinition->Description);
				
				String^ value = marshal_as<String^>(ipWorkflowDefinition->StartAction);
				startActionComboBox->SelectedIndex = startActionComboBox->FindStringExact(value);
				
				value = marshal_as<String^>(ipWorkflowDefinition->EndAction);
				endActionComboBox->SelectedIndex = endActionComboBox->FindStringExact(value);
				
				value = marshal_as<String^>(ipWorkflowDefinition->PostWorkflowAction);
				postWorkflowActionComboBox->SelectedIndex = postWorkflowActionComboBox->FindStringExact(value);
				
				documentFolderTextBox->Text = marshal_as<String^>(ipWorkflowDefinition->DocumentFolder);

				value = marshal_as<String^>(ipWorkflowDefinition->OutputAttributeSet);
				outputAttributeSetComboBox->SelectedIndex = outputAttributeSetComboBox->FindStringExact(value);

				value = marshal_as<String^>(ipWorkflowDefinition->OutputFileMetadataField);
				outputFileMetadataFieldComboBox->SelectedIndex = outputFileMetadataFieldComboBox->FindStringExact(value);

				outputFilePathInitializationFunctionTextBox->Text =
					marshal_as<String ^>(ipWorkflowDefinition->OutputFilePathInitializationFunction);
			}
		}

		Void AddModifyWorkflowForm::loadActionComboLists()
		{
			IVariantVectorPtr mainSequenceActions(CLSID_VariantVector);
			IVariantVectorPtr otherActions(CLSID_VariantVector);

			// If this is a new workflow load all the actions
			// if this is an existing workflow just load the actions associated with the workflow
			if (_workflowID < 0)
			{
				// Get All actions from the database
				IStrToStrMapPtr actions = _ipfamDatabase->GetAllActions();
				mainSequenceActions->Append(actions->GetKeys());
				otherActions->Append(actions->GetKeys());
			}
			else
			{
				IIUnknownVectorPtr actions = _ipfamDatabase->GetWorkflowActions(_workflowID);
				long count = actions->Size();
				for (long i = 0; i < count; i++)
				{
					IVariantVectorPtr properties = actions->At(i);
					if (properties->Item[2].boolVal == VARIANT_TRUE)
					{
						mainSequenceActions->PushBack(properties->Item[1]);
					}
					else
					{
						otherActions->PushBack(properties->Item[1]);
					}
				}
			}


			// Load each of the action combo boxes
			ListCtrlHelper::LoadListCtrl(startActionComboBox, mainSequenceActions);
			// Empty item at the first index
			startActionComboBox->Items->Insert(0, "");

			ListCtrlHelper::LoadListCtrl(endActionComboBox, mainSequenceActions);
			// Empty item at the first index
			endActionComboBox->Items->Insert(0, "");

			ListCtrlHelper::LoadListCtrl(postWorkflowActionComboBox, otherActions);
			// Empty item at the first index
			postWorkflowActionComboBox->Items->Insert(0, "");
		}

		Void Extract::FAMDBAdmin::AddModifyWorkflowForm::loadWorkFlowTypeCombo()
		{
			workFlowTypeComboBox->Items->Clear();
			workFlowTypeComboBox->DisplayMember = "Name";
			workFlowTypeComboBox->ValueMember = "ID";

			workFlowTypeComboBox->Items->Add(gcnew ListItemPair("Redaction", kRedaction));
			workFlowTypeComboBox->Items->Add(gcnew ListItemPair("Extraction",kExtraction));
			workFlowTypeComboBox->Items->Add(gcnew ListItemPair("Classification",kClassification));
			
			switch (ipWorkflowDefinition->Type)
			{
			case kRedaction:
				workFlowTypeComboBox->SelectedIndex = workFlowTypeComboBox->FindStringExact("Redaction");
				break;
			case kExtraction:
				workFlowTypeComboBox->SelectedIndex = workFlowTypeComboBox->FindStringExact("Extraction");
				break;
			case kClassification:
				workFlowTypeComboBox->SelectedIndex = workFlowTypeComboBox->FindStringExact("Classification");
				break;
			}
		}

		Void Extract::FAMDBAdmin::AddModifyWorkflowForm::loadOutputAttributeSetCombo()
		{
			outputAttributeSetComboBox->Items->Clear();
			workFlowTypeComboBox->DisplayMember = "Name";
			workFlowTypeComboBox->ValueMember = "ID";

			IAttributeDBMgrPtr ipAttributeDB(CLSID_AttributeDBMgr);
			ASSERT_RESOURCE_ALLOCATION("ELI42057", ipAttributeDB != __nullptr);

			ipAttributeDB->FAMDB = _ipfamDatabase;

			IStrToStrMapPtr ipAttributeSets = ipAttributeDB->GetAllAttributeSetNames();
			ListCtrlHelper::LoadListCtrl(outputAttributeSetComboBox, ipAttributeSets);
			
			// Empty item at the first index
			outputAttributeSetComboBox->Items->Insert(0, gcnew ListItemPair("", 0));
		
		}

		Void Extract::FAMDBAdmin::AddModifyWorkflowForm::loadOutputFileMetadataFieldCombo()
		{
			outputFileMetadataFieldComboBox->Items->Clear();

			IVariantVectorPtr ipMetadataFieldNames = _ipfamDatabase->GetMetadataFieldNames();
			ListCtrlHelper::LoadListCtrl(outputFileMetadataFieldComboBox, ipMetadataFieldNames);

			// Empty item at the first index
			outputFileMetadataFieldComboBox->Items->Insert(0, "");
		}

		Object ^ AddModifyWorkflowForm::loadWebAppSettings(Type^ type)
		{
			marshal_context context;

			BSTR bstrType = context.marshal_as<BSTR>(type->Name);

			String ^jsonSettings = marshal_as<String ^>(_ipfamDatabase->LoadWebAppSettings(_workflowID, bstrType));

			if (String::IsNullOrWhiteSpace(jsonSettings))
			{
				return __nullptr;
			}
			else
			{
				MemoryStream ^stream;
				StreamWriter ^writer;
				try
				{
					stream = gcnew MemoryStream();
					writer = gcnew StreamWriter(stream);
					writer->Write(jsonSettings);
					writer->Flush();
					stream->Position = 0;

					auto serializer = gcnew DataContractJsonSerializer(type);
					return serializer->ReadObject(stream);
				}
				finally
				{
					delete writer;
					delete stream;
				}
			}
		}

		Void AddModifyWorkflowForm::deleteWebAppSettings(Type^ type)
		{
			marshal_context context;

			BSTR bstrType = context.marshal_as<BSTR>(type->Name);

			_ipfamDatabase->SaveWebAppSettings(_workflowID, bstrType, "");
		}

		Void AddModifyWorkflowForm::saveWebAppSettings(Object^ settings)
		{
			marshal_context context;

			MemoryStream ^stream;
			StreamReader ^reader;
			try
			{
				Type ^type = settings->GetType();
				auto serializer = gcnew DataContractJsonSerializer(type);
				stream = gcnew MemoryStream();
				serializer->WriteObject(stream, settings);
				stream->Position = 0;
				reader = gcnew StreamReader(stream);

				BSTR bstrType = context.marshal_as<BSTR>(type->Name);
				BSTR bstrSettings = context.marshal_as<BSTR>(reader->ReadToEnd());

				_ipfamDatabase->SaveWebAppSettings(_workflowID, bstrType, bstrSettings);
			}
			finally
			{
				delete reader;
				delete stream;
			}
		}

#pragma endregion

	}
}