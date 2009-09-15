
#pragma once

class UCLIDException;

#include <Upromeps.h>

#include <string>

using namespace std;

void loadSafeNetErrInfo(UCLIDException& ue, SP_STATUS snStatus);

string getSafeNetErrorDescription(SP_STATUS snStatus);
string getSafeNetErrorAsString(SP_STATUS snStatus);
