#include <string>

class CAFFileProcessorsUtils
{
public:
	CAFFileProcessorsUtils();
	~CAFFileProcessorsUtils();
	
	// Expand tags using FAM Tag manager and expand utility function
	static const std::string ExpandTagsAndTFE(IFAMTagManagerPtr ipFAMTM, 
		const std::string& strFile, const std::string& strSourceDocName);
};