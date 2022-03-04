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
		document.Parse<0>(strJson.data());

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
					attributeSetName->value.SetString(attributeSetNameDescription.data(), document.GetAllocator());
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
			long id = getLongLongField(ipFields, "ID");
			std::string description = getStringField(ipFields, "Description");

			attributeSetNames.insert(std::pair<long, std::string>(id, description));

			attributeSetNamesRecordSet->MoveNext();
		}

		return attributeSetNames;
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
		auto& services = getServices(ipConnection);
		auto& attributeSetNames = getAttributeSetNames(ipConnection);

		for (auto& service: services)
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
						{"@Settings", json.data()}
					}));
			}
		}
	}
}
