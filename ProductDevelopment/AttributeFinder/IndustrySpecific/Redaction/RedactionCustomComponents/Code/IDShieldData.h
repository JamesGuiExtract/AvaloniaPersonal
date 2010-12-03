#pragma once

#include <string>
#include <set>

using namespace std;

class IDShieldData
{
public:
	IDShieldData(void);
	IDShieldData(IIUnknownVectorPtr ipAttributes);
	virtual ~IDShieldData(void);

	void clear();

	// Only adds attributes that have the name in the setRedactLabels set to the TotalRedactions
	void calculateFromVector(IIUnknownVectorPtr ipAttributes, const set<string>& setRedactLabels);

	// Assumes all attributes in the ipAttributes are redacted
	void calculateFromVector(IIUnknownVectorPtr ipAttributes);

	// Count attribute as redacted
	void countRedacted(IAttributePtr ipAttribute);

	// Count attribute as not redacted
	void countNotRedacted(IAttributePtr ipAttribute);

	// Variables
	long m_lNumHCDataFound;
	long m_lNumMCDataFound;
	long m_lNumLCDataFound;
	long m_lNumCluesFound;
	long m_lTotalRedactions;
	long m_lTotalManualRedactions;
	long m_lNumPagesAutoAdvanced;

private:

	// Adds to the appropriate count value based on the label
	// Will add to appropriate totals if bAddToTotals is true
	void addToCounts(const string& strLabel, bool bAddToTotals);
};

static const string gstrHCDATA_LABEL = "HCData";
static const string gstrMCDATA_LABEL = "MCData";
static const string gstrLCDATA_LABEL = "LCData";
static const string gstrCLUES_LABEL = "Clues";
static const string gstrMANUAL_LABEL = "Manual";

