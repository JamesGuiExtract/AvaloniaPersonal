// DotNetUtils.h : Header file for methods use dot net internally
#pragma once

#include "FAMUtils.h"

#include <string>
#include <vector>

using namespace std;

// PROMISE: To return a list of servers in the rvecServers parameter
void FAMUTILS_API getServerList(vector<string>& rvecServers);

// PROMISE: To return a list of database names in the rvecDBNames parameter that are on
// the server strServer
void FAMUTILS_API getDBNameList(const string& strServer, vector<string>&rvecDBNames);