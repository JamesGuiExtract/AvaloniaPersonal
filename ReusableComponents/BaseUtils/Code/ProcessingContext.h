#pragma once

class ProcessingContext
{
	friend class UCLIDException;
	friend class AccessUCLIDExceptionPrivate;
private:
	string m_strDatabaseServer;
	string m_strDatabaseName;
	string m_strFpsContext;
	// Note: on the C# side these need to be Int since long is Int32 on the c# side
	long m_lFileID;
	long m_lActionID;

public:
	ProcessingContext()
		:m_strDatabaseServer(""),
		m_strDatabaseName(""),
		m_strFpsContext(""),
		m_lFileID(-1),
		m_lActionID(-1)
	{};

	ProcessingContext(const ProcessingContext& context)
	{
		m_strDatabaseName = context.m_strDatabaseName;
		m_strDatabaseServer = context.m_strDatabaseServer;
		m_strFpsContext = context.m_strFpsContext;
		m_lFileID = context.m_lFileID;
		m_lActionID = context.m_lActionID;
	};

	ProcessingContext(const string& databaseServer, const string& databaseName, const string& fpsContext, long actionID)
	{
		m_strDatabaseName = databaseName;
		m_strDatabaseServer = databaseServer;
		m_strFpsContext = fpsContext;
		m_lActionID = actionID;
		m_lFileID = -1;
	};

	~ProcessingContext()
	{};

	void SetDatabaseContext(const string& server, const string& database, long actionID)
	{
		m_strDatabaseServer = server;
		m_strDatabaseName = database;
		m_lActionID = actionID;
	}

	void SetFileID(long fileID)
	{
		m_lFileID = fileID;
	}
};

