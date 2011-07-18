#pragma once

#include <string>

#include <IConfigurationSettingsPersistenceMgr.h>

// String constant for Feedback Manager Index file
//const std::string	gstrINDEXFILE = "UCLID_FeedbackIndex.dat";
const std::string	gstrINDEXFILE = "Feedback.mdb";

// String constant for Feedback Manager Database DSN
const std::string	gstrDSNFILE = "Feedback.dsn";

/////////////////////////////////////////////////
// Constants for Feedback Manager Database fields
/////////////////////////////////////////////////

// Rule Execution table
const _variant_t	gstrRuleIDField			= L"RuleID";
const _variant_t	gstrStartTimeField		= L"StartTime";
const _variant_t	gstrDurationField		= L"Duration";
const _variant_t	gstrSourceDocField		= L"SourceDoc";
const _variant_t	gstrPackageSrcDocField	= L"PackageSourceDoc";
const _variant_t	gstrRSDFileField		= L"RSDFile";
const _variant_t	gstrCorrectTimeField	= L"CorrectTime";
const _variant_t	gstrComputerField		= L"Computer";

// Counter table
const _variant_t	gstrCounterNameField	= L"CounterName";
const _variant_t	gstrCounterValueField	= L"CounterValue";

class PersistenceMgr
{
public:
	PersistenceMgr(IConfigurationSettingsPersistenceMgr* pConfigMgr, const std::string& strSectionName);

	// Gets/Sets enabled state (true = Feedback Collection enabled, false = not enabled)
	bool getFeedbackEnabled();
	void setFeedbackEnabled(bool bEnabled);

	// Gets/Sets the feedback folder
	std::string getFeedbackFolder(void);
	void setFeedbackFolder(const std::string& strFolder);

	// Gets/Sets enabled state for Automatically stopping feedback collection
	bool getAutoTurnOffEnabled();
	void setAutoTurnOffEnabled(bool bEnabled);

	// Gets/Sets the automatic turn-off date
	// Format: MM/DD/YYYY
	std::string getTurnOffDate(void);
	void setTurnOffDate(const std::string& strDate);

	// Gets/Sets the automatic turn-off Rule Execution count
	long getTurnOffCount(void);
	void setTurnOffCount(const long lCount);

	// Gets/Sets the Rule Execution skip count
	long getSkipCount(void);
	void setSkipCount(const long lCount);

	// Gets/Sets the document collection choice
	long getDocumentCollection(void);
	void setDocumentCollection(const long lCollect);

	// Gets/Sets choice for converting source documents to text
	bool getDocumentConversion();
	void setDocumentConversion(bool bConvert);

	// Gets/Sets the attribute selection choice
	bool getAttributeSelection(void);
	void setAttributeSelection(bool bNamed);

	// Clears the named attributes from the registry
	//   Attributes are saved as Attribute_n
	//   for n = 1, 2, ..., N
//	void clearAttributeNames();

	// Gets/Sets the named attributes
//	IIUnknownVectorPtr getAttributeNames(void);
//	void setAttributeNames(IIUnknownVectorPtr ipNames);

	// Gets/Sets the package file
	std::string getPackageFile(void);
	void setPackageFile(const std::string& strFile);

	// Gets/Sets the Clear Data After Packaging choice
	bool getClearAfterPackage(void);
	void setClearAfterPackage(bool bClear);

	////////////////////
	// Defined as public
	////////////////////

private:
	// Registry keys for information persistence
	static const std::string FEEDBACK_FOLDER;
	static const std::string FEEDBACK_ENABLED;
	static const std::string AUTO_TURNOFF_ENABLED;
	static const std::string AUTO_TURNOFF_DATE;
	static const std::string AUTO_TURNOFF_COUNT;
	static const std::string SKIP_COUNT;
	static const std::string DOCUMENT_COLLECTION;
	static const std::string CONVERT_TO_TEXT;
	static const std::string ATTRIBUTE_SELECTION;
	static const std::string ATTRIBUTE_NAME;
	static const std::string PACKAGE_FILE;
	static const std::string CLEAR_AFTER_PACKAGE;

	static const std::string DEFAULT_FEEDBACK_FOLDER;
	static const std::string DEFAULT_FEEDBACK_ENABLED;
	static const std::string DEFAULT_AUTO_TURNOFF_ENABLED;
	static const std::string DEFAULT_AUTO_TURNOFF_DATE;
	static const std::string DEFAULT_AUTO_TURNOFF_COUNT;
	static const std::string DEFAULT_SKIP_COUNT;
	static const std::string DEFAULT_DOCUMENT_COLLECTION;
	static const std::string DEFAULT_CONVERT_TO_TEXT;
	static const std::string DEFAULT_ATTRIBUTE_SELECTION;
	static const std::string DEFAULT_ATTRIBUTE_NAME;
	static const std::string DEFAULT_PACKAGE_FILE;
	static const std::string DEFAULT_CLEAR_AFTER_PACKAGE;

	IConfigurationSettingsPersistenceMgr* m_pCfgMgr;

	std::string m_strSectionFolderName;
};
