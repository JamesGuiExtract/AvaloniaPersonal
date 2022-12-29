#pragma once
#include "stdafx.h"

#include <ADOUtils.h>

#include <string>
#include <map>

#include <rapidjson/document.h>
#include <rapidjson/pointer.h>
#include <rapidjson/prettywriter.h>
#include <rapidjson/stringbuffer.h>

// Local functions
namespace
{
	//-------------------------------------------------------------------------------------------------
	// Update ExpandAttributes ETL service json to version 3:
	// - Update DashboardAttributes:
	//   - Change AttributeSetNameID (int) to AttributeSetName (string)
	// Returns: true if the json was modified
	//-------------------------------------------------------------------------------------------------
	bool updateExpandAttributesDatabaseServiceToV3(
		ADODB::_ConnectionPtr ipConnection,
		std::string& strJson,
		std::map<long long, const std::string>& attributeSetNames)
	{
		rapidjson::Document document;
		document.Parse<0>(strJson.c_str());

		// Ignore invalid json
		if (document.HasParseError())
		{
			return false;
		}

		// Do nothing if this isn't an ExpandAttributes object
		auto& type = document.FindMember("$type");
		if (type == document.MemberEnd() || strcmp(type->value.GetString(), "Extract.ETL.ExpandAttributes, Extract.ETL") != 0)
		{
			return false;
		}

		// Do nothing if the version is unknown or > 2
		auto& version = document.FindMember("Version");
		if (version == document.MemberEnd() || !version->value.IsInt() || version->value.GetInt() >= 3)
		{
			return false;
		}

		// Update the version
		version->value.SetInt(3);

		// Update each dashboard attribute
		auto& dashboardAttributes = document.FindMember("DashboardAttributes");
		if (dashboardAttributes != document.MemberEnd() && dashboardAttributes->value.IsArray())
		{
			for (auto& dashboardAttribute : dashboardAttributes->value.GetArray())
			{
				auto& attributeSetName = dashboardAttribute.FindMember("AttributeSetNameID");
				if (attributeSetName != dashboardAttribute.MemberEnd() && attributeSetName->value.IsInt64())
				{
					long long attributeSetNameID = attributeSetName->value.GetInt64();
					const std::string& attributeSetNameDescription = attributeSetNames[attributeSetNameID];

					// Rename
					attributeSetName->name.SetString("AttributeSetName", document.GetAllocator());

					// Change the value from the ID to the Description
					attributeSetName->value.SetString(attributeSetNameDescription.c_str(), document.GetAllocator());
				}
			}
		}

		rapidjson::StringBuffer buffer;
		rapidjson::PrettyWriter<rapidjson::StringBuffer> writer(buffer);
		writer.SetIndent(' ', 2);
		document.Accept(writer);

		strJson = buffer.GetString();

		return true;
	}

	//-------------------------------------------------------------------------------------------------
	// Get all database services
	//-------------------------------------------------------------------------------------------------
	std::map<long, std::string> getServices(ADODB::_ConnectionPtr ipConnection)
	{
		ADODB::_RecordsetPtr servicesRecordSet =
			ipConnection->Execute("SELECT ID, Settings FROM dbo.DatabaseService", NULL, adCmdText);

		std::map<long, std::string> services;
		while (servicesRecordSet->adoEOF == VARIANT_FALSE)
		{
			ADODB::FieldsPtr ipFields = servicesRecordSet->Fields;
			long serviceID = getLongField(ipFields, "ID");
			std::string settings = getStringField(ipFields, "Settings");

			services.insert(std::pair<long, std::string>(serviceID, settings));

			servicesRecordSet->MoveNext();
		}

		return services;
	}

	//-------------------------------------------------------------------------------------------------
	// Get all attribute set name records
	//-------------------------------------------------------------------------------------------------
	std::map<long long, const std::string> getAttributeSetNames(ADODB::_ConnectionPtr ipConnection)
	{
		ADODB::_RecordsetPtr attributeSetNamesRecordSet =
			ipConnection->Execute("SELECT ID, Description FROM dbo.AttributeSetName", NULL, adCmdText);

		std::map<long long, const std::string> attributeSetNames;
		while (attributeSetNamesRecordSet->adoEOF == VARIANT_FALSE)
		{
			ADODB::FieldsPtr ipFields = attributeSetNamesRecordSet->Fields;
			long long id = getLongLongField(ipFields, "ID");
			std::string description = getStringField(ipFields, "Description");

			attributeSetNames.insert(std::pair<long long, std::string>(id, description));

			attributeSetNamesRecordSet->MoveNext();
		}

		return attributeSetNames;
	}

	//-------------------------------------------------------------------------------------------------
	// WebAppConfig + relevant Workflow data
	//-------------------------------------------------------------------------------------------------
	struct WebAppWorkflowConfig {
		const std::string workflowName;
		const std::string webAppSettings;
		const std::string startActionName;
		const std::string endActionName;
		const std::string postWorkflowActionName;
		const std::string documentFolder;
		const std::string outputAttributeSetName;
		const std::string outputFileMetadataFieldName;
		const std::string outputFilePathInitializationFunction;
		const std::string editActionName;
		const std::string postEditActionName;
	};

	//-------------------------------------------------------------------------------------------------
	// Get WebAppConfig + relevant Workflow data
	//-------------------------------------------------------------------------------------------------
	std::vector<WebAppWorkflowConfig> getWebAppWorkflowConfigs(ADODB::_ConnectionPtr ipConnection)
	{
		try
		{
			std::string selectQuery =
				" IF (EXISTS (SELECT * "
				" FROM INFORMATION_SCHEMA.TABLES "
				" WHERE TABLE_SCHEMA = 'dbo' "
				" AND  TABLE_NAME = 'WebAppConfig')) "
				"  SELECT"
				"        [Name]"
				"        ,[Settings]"
				"        ,(SELECT ASCName FROM [dbo].[Action] WHERE [dbo].[Action].ID = [StartActionID]) AS [StartActionName]"
				"        ,(SELECT ASCName FROM [dbo].[Action] WHERE [dbo].[Action].ID = [EndActionID]) AS [EndActionName]"
				"        ,(SELECT ASCName FROM [dbo].[Action] WHERE [dbo].[Action].ID = [PostWorkflowActionID]) AS [PostWorkflowActionName]"
				"        ,[DocumentFolder]"
				"        ,(SELECT [Description] FROM [dbo].[AttributeSetName] WHERE [dbo].[AttributeSetName].ID = [OutputAttributeSetID]) AS [OutputAttributeSetName]"
				"        ,(SELECT [Name] FROM [dbo].[MetadataField] WHERE [dbo].[MetadataField].ID = [OutputFileMetadataFieldID]) AS [OutputFileMetadataFieldName]"
				"        ,[OutputFilePathInitializationFunction]"
				"        ,(SELECT ASCName FROM [dbo].[Action] WHERE [dbo].[Action].ID = [EditActionID]) AS [EditActionName]"
				"        ,(SELECT ASCName FROM [dbo].[Action] WHERE [dbo].[Action].ID = [PostEditActionID]) AS [PostEditActionName]"
				"    FROM [dbo].[Workflow]"
				"    LEFT JOIN [dbo].[WebAppConfig] ON [dbo].[Workflow].ID = [dbo].[WebAppConfig].WorkflowID"
				" ELSE "
				" SELECT NULL FROM dbo.DBInfo where DBInfo.Name = 'SettingThatShouldNeverExist' ";

			ADODB::_RecordsetPtr workflowConfigRecordSet =
				ipConnection->Execute(selectQuery.c_str(), NULL, adCmdText);

			std::vector<WebAppWorkflowConfig> configs;
			while (workflowConfigRecordSet->adoEOF == VARIANT_FALSE)
			{
				ADODB::FieldsPtr ipFields = workflowConfigRecordSet->Fields;
				configs.push_back(WebAppWorkflowConfig{
					getStringField(ipFields, "Name"), // .workflowName
					getStringField(ipFields, "Settings"), // .webAppSettings
					getStringField(ipFields, "StartActionName"), // .startActionName
					getStringField(ipFields, "EndActionName"), // .endActionName
					getStringField(ipFields, "PostWorkflowActionName"), // .postWorkflowActionName
					getStringField(ipFields, "DocumentFolder"), // .documentFolder
					getStringField(ipFields, "OutputAttributeSetName"), // .outputAttributeSetName
					getStringField(ipFields, "OutputFileMetadataFieldName"), // .outputFileMetadataFieldName
					getStringField(ipFields, "OutputFilePathInitializationFunction"), // outputFilePathInitializationFunction
					getStringField(ipFields, "EditActionName"), // .editActionName
					getStringField(ipFields, "PostEditActionName") // .postEditActionName
					});

				workflowConfigRecordSet->MoveNext();
			}

			return configs;
		}
		catch (...)
		{
			throw uex::fromCurrent("ELI53721");
		}
	}

	//-------------------------------------------------------------------------------------------------
	// Redaction configuration from WebAppConfig
	//-------------------------------------------------------------------------------------------------
	struct RedactionSettings {
		std::string docTypesPath;
		std::vector<std::string> redactionTypes;
		bool enableAllPendingQueue;
	};

	//-------------------------------------------------------------------------------------------------
	// Create one or more json strings for the new WebApiConfiguration table from a WebAppWorkflowConfig
	//-------------------------------------------------------------------------------------------------
	RedactionSettings getRedactionSettingsFromWebAppSettings(const WebAppWorkflowConfig& config)
	{
		try
		{
			RedactionSettings result;

			rapidjson::Document document;
			document.Parse<0>(config.webAppSettings.c_str());

			// Ignore invalid json but log an error
			if (document.HasParseError())
			{
				UCLIDException ue("ELI53718", "Parse error reading web app settings. No settings will be migrated");
				ue.addDebugInfo("Workflow", config.workflowName);
				ue.addDebugInfo("Settings", config.webAppSettings);
				ue.log();

				return RedactionSettings{};
			}

			// Get the DocumentTypes property, log an error if missing
			auto& docTypesPathIterator = document.FindMember("DocumentTypes");
			if (docTypesPathIterator != document.MemberEnd())
			{
				result.docTypesPath = docTypesPathIterator->value.GetString();
			}
			else
			{
				UCLIDException ue("ELI53719", "Missing web app setting: DocumentTypes");
				ue.addDebugInfo("Workflow", config.workflowName);
				ue.addDebugInfo("Settings", config.webAppSettings);
				ue.log();
			}

			// Get the new 'enable all users' flag property, assume true if missing
			auto& enableAllPendingQueueIterator = document.FindMember("EnableAllPendingQueue");
			if (enableAllPendingQueueIterator != document.MemberEnd() && enableAllPendingQueueIterator->value.IsBool())
			{
				result.enableAllPendingQueue = enableAllPendingQueueIterator->value.GetBool();
			}
			else
			{
				result.enableAllPendingQueue = true; // Legacy behavior is enabled
			}

			// Get the RedactionTypes property, log an error if missing
			auto& redactionTypesIterator = document.FindMember("RedactionTypes");
			if (redactionTypesIterator != document.MemberEnd() && redactionTypesIterator->value.IsArray())
			{
				for (auto& redactionType : redactionTypesIterator->value.GetArray())
				{
					if (redactionType.IsString())
					{
						result.redactionTypes.push_back(redactionType.GetString());
					}
				}
			}
			else
			{
				UCLIDException ue("ELI53720", "Missing web app setting: RedactionTypes");
				ue.addDebugInfo("Workflow", config.workflowName);
				ue.addDebugInfo("Settings", config.webAppSettings);
				ue.log();
			}

			return result;
		}
		catch (...)
		{
			throw uex::fromCurrent("ELI53722");
		}
	}

	//-------------------------------------------------------------------------------------------------
	// Add IWebConfiguration properties to a document
	//-------------------------------------------------------------------------------------------------
	void addCommonPropertiesToDto(
		rapidjson::Value& dto,
		const WebAppWorkflowConfig& config,
		std::string configName,
		rapidjson::MemoryPoolAllocator<>& allocator)
	{
		try
		{
			dto.AddMember("ConfigurationName", rapidjson::Value(configName.c_str(), allocator).Move(), allocator);
			dto.AddMember("IsDefault", true, allocator);
			dto.AddMember("WorkflowName", rapidjson::Value(config.workflowName.c_str(), allocator).Move(), allocator);
			dto.AddMember("AttributeSet", rapidjson::Value(config.outputAttributeSetName.c_str(), allocator).Move(), allocator);
			dto.AddMember("ProcessingAction", rapidjson::Value(config.editActionName.c_str(), allocator).Move(), allocator);
			dto.AddMember("PostProcessingAction", rapidjson::Value(config.postEditActionName.c_str(), allocator).Move(), allocator);
		}
		catch (...)
		{
			throw uex::fromCurrent("ELI53723");
		}
	}

	//-------------------------------------------------------------------------------------------------
	// Write a JSON Document as a string
	//-------------------------------------------------------------------------------------------------
	std::string getJsonString(const rapidjson::Document& document)
	{
		try
		{
			rapidjson::StringBuffer buffer;
			rapidjson::PrettyWriter<rapidjson::StringBuffer> writer(buffer);
			writer.SetIndent(' ', 2);
			document.Accept(writer);

			return buffer.GetString();
		}
		catch (...)
		{
			throw uex::fromCurrent("ELI53724");
		}
	}

	//-------------------------------------------------------------------------------------------------
	// Name + JSON string
	//-------------------------------------------------------------------------------------------------
	struct ApiConfiguration {
		const std::string configName;
		const std::string settings;
	};

	//-------------------------------------------------------------------------------------------------
	// Create a json document from a WebAppWorkflowConfig
	//-------------------------------------------------------------------------------------------------
	ApiConfiguration getDocumentApiWebConfiguration(const WebAppWorkflowConfig& config)
	{
		try
		{
			rapidjson::Document document;
			document.SetObject();
			auto& allocator = document.GetAllocator();

			// Set the type of the DTO wrapped by this document object
			document.AddMember("TypeName", "DocumentApiWebConfigurationV1", allocator);

			// Create the DTO
			rapidjson::Value dto(rapidjson::kObjectType);

			std::string configName = "Workflow: " + config.workflowName + " Type: DocumentAPI";
			addCommonPropertiesToDto(dto, config, configName, allocator);

			dto.AddMember("DocumentFolder",
				rapidjson::Value(config.documentFolder.c_str(), allocator).Move(), allocator);
			dto.AddMember("StartWorkflowAction",
				rapidjson::Value(config.startActionName.c_str(), allocator).Move(), allocator);
			dto.AddMember("EndWorkflowAction",
				rapidjson::Value(config.endActionName.c_str(), allocator).Move(), allocator);
			dto.AddMember("PostWorkflowAction",
				rapidjson::Value(config.postWorkflowActionName.c_str(), allocator).Move(), allocator);
			dto.AddMember("OutputFileNameMetadataField",
				rapidjson::Value(config.outputFileMetadataFieldName.c_str(), allocator).Move(), allocator);
			dto.AddMember("OutputFileNameMetadataInitialValueFunction",
				rapidjson::Value(config.outputFilePathInitializationFunction.c_str(), allocator).Move(), allocator);

			// Move the DTO object into to the document
			document.AddMember("DataTransferObject", dto, allocator);

			return ApiConfiguration{ configName, getJsonString(document) };
		}
		catch (...)
		{
			throw uex::fromCurrent("ELI53725");
		}
	}

	//-------------------------------------------------------------------------------------------------
	// Create a json string for a RedactionWebConfiguration object from a WebAppWorkflowConfig
	//-------------------------------------------------------------------------------------------------
	ApiConfiguration getRedactionWebConfiguration(const WebAppWorkflowConfig& config)
	{
		try
		{
			RedactionSettings redactionSettings = getRedactionSettingsFromWebAppSettings(config);
			rapidjson::Document document;
			document.SetObject();
			auto& allocator = document.GetAllocator();

			// Set the type of the DTO wrapped by this document object
			document.AddMember("TypeName", "RedactionWebConfigurationV1", allocator);

			// Create the DTO
			rapidjson::Value dto(rapidjson::kObjectType);

			std::string configName = "Workflow: " + config.workflowName + " Type: Redaction";
			addCommonPropertiesToDto(dto, config, configName, allocator);

			dto.AddMember("ActiveDirectoryGroups", rapidjson::Value(rapidjson::kArrayType).Move(), allocator);
			dto.AddMember("EnableAllUserPendingQueue",
				rapidjson::Value().SetBool(redactionSettings.enableAllPendingQueue), allocator);
			dto.AddMember("DocumentTypeFileLocation",
				rapidjson::Value(redactionSettings.docTypesPath.c_str(), allocator).Move(), allocator);

			// Redaction type array
			rapidjson::Value redactionTypes(rapidjson::kArrayType);
			for (auto& redactionType : redactionSettings.redactionTypes)
			{
				redactionTypes.PushBack(rapidjson::Value(redactionType.c_str(), allocator).Move(), allocator);
			}
			dto.AddMember("RedactionTypes", redactionTypes, allocator);

			// Move the DTO object into to the document
			document.AddMember("DataTransferObject", dto, allocator);

			return ApiConfiguration{ configName, getJsonString(document) };
		}
		catch (...)
		{
			throw uex::fromCurrent("ELI53726");
		}
	}

	//-------------------------------------------------------------------------------------------------
	// Create zero or more json strings for the new WebApiConfiguration table from a WebAppWorkflowConfig
	//-------------------------------------------------------------------------------------------------
	std::vector<ApiConfiguration> getJsonFromWebAppWorkflowConfig(const WebAppWorkflowConfig& config)
	{
		try
		{
			std::vector<ApiConfiguration> results;

			// If neither start action nor web app settings are specified then nothing to migrate
			if (config.startActionName.empty() && config.webAppSettings.empty())
			{
				return results;
			}

			// Create a DocumentApiConfiguration document
			results.push_back(getDocumentApiWebConfiguration(config));

			if (config.webAppSettings.empty())
			{
				return results;
			}

			// Create a RedactionWebApiConfiguration document
			results.push_back(getRedactionWebConfiguration(config));

			return results;
		}
		catch (...)
		{
			throw uex::fromCurrent("ELI53727");
		}
	}
}

// Public functions
namespace JsonSchemaUpdates
{
	//-------------------------------------------------------------------------------------------------
	// Update any ExpandAttributes ETL services with version < 3 to version 3
	//-------------------------------------------------------------------------------------------------
	void UpdateExpandAttributesDatabaseServicesToV3(ADODB::_ConnectionPtr ipConnection)
	{
		try
		{
			auto& services = getServices(ipConnection);
			auto& attributeSetNames = getAttributeSetNames(ipConnection);

			for (auto& service : services)
			{
				std::string& json = service.second;
				bool updated = updateExpandAttributesDatabaseServiceToV3(ipConnection, json, attributeSetNames);

				if (updated)
				{

					executeCmd(buildCmd(
						ipConnection,
						"UPDATE dbo.DatabaseService SET Settings = @Settings WHERE ID = @ID",
						{
							{"@ID", service.first},
							{"@Settings", json.c_str()}
						}));
				}
			}
		}
		catch (...)
		{
			throw uex::fromCurrent("ELI53728");
		}
	}

	//-------------------------------------------------------------------------------------------------
	// Copy settings from the Workflow and WebAppConfig tables into the new configuration table
	//-------------------------------------------------------------------------------------------------
	void CopyToWebApiConfiguration(ADODB::_ConnectionPtr ipConnection)
	{
		try
		{
			for (WebAppWorkflowConfig& oldConfig : getWebAppWorkflowConfigs(ipConnection))
			{
				std::vector<ApiConfiguration> configs = getJsonFromWebAppWorkflowConfig(oldConfig);
				for (auto& config : configs)
				{
					executeCmd(buildCmd(
						ipConnection,
						"INSERT INTO [dbo].[WebApiConfiguration] ([Name], [Settings]) Values (@Name, @Settings)",
						{
							{"@Name", config.configName.c_str()},
							{"@Settings", config.settings.c_str()}
						}));
				}
			}
		}
		catch (...)
		{
			throw uex::fromCurrent("ELI53729");
		}
	}
}
