#pragma once

#include "stdafx.h"
#include <ADOUtils.h>
#include <string>

namespace FAMUtils
{
	namespace Test
	{
		namespace DatabaseTestingUtils
		{
			inline void RemoveDB(std::string dbName)
			{
				ADODB::_ConnectionPtr connection;
				connection.CreateInstance(__uuidof(Connection));
				connection->Open(createConnectionString("(local)", "master").c_str(), "", "", adConnectUnspecified);
				std::string sql = "IF DB_ID('" + dbName + "') IS NOT NULL DROP DATABASE " + dbName;
				connection->Execute(sql.c_str(), NULL, adCmdText);
				connection->Close();
			}

			inline IFileProcessingDBPtr CreateDB(std::string dbName)
			{
				// Remove existing db by the same name
				RemoveDB(dbName);

				IFileProcessingDBPtr FamDB;
				FamDB.CreateInstance(__uuidof(FileProcessingDB));
				FamDB->DatabaseName = dbName.c_str();
				FamDB->DatabaseServer = "(local)";
				FamDB->CreateNewDB(dbName.c_str(), "a");

				return FamDB;
			}
		}
	}
}
