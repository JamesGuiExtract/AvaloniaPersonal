
#pragma once

#include <string>
#include <vector>

using namespace std;

// Given a vector of queries used to create the tables in a database, a vector of names of tables
// created by the queries is returned.
vector<string> getTableNamesFromCreationQueries(vector<string> vecCreationQueries);

// Creates a vector of SQL queries that can be used to populate the Feature table with the default
// settings.
vector<string> getFeatureDefinitionQueries();