
#pragma once

#include <string>
#include <vector>

using namespace std;
using namespace ADODB;

// Given a vector of queries used to create the tables in a database, a vector of names of tables
// created by the queries is returned.
vector<string> getTableNamesFromCreationQueries(vector<string> vecCreationQueries);

// Creates a vector of SQL queries that can be used to populate the Feature table with the default
// settings. If nSchemaVersion is specified only the feature definitions that were present as of
// the specified FAM DB schema version are returned.
vector<string> getFeatureDefinitionQueries(int nSchemaVersion = -1);

// Helper function for the FAMDB schema 201 upgrade; identifies any web app configurations that specify
// an inactivity timeout so that it can be applied as the default for the new DBInfo VerificationSessionTimeout
long getDefaultSessionTimeoutFromWebConfig(_ConnectionPtr ipConnection);