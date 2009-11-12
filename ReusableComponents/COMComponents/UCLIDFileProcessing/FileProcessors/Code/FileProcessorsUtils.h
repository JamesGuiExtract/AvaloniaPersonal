#include <string>

using namespace std;

class CFileProcessorsUtils
{
public:
	CFileProcessorsUtils();
	~CFileProcessorsUtils();
	// Set tags for File Processors configuration dialog
	static const string ChooseDocTag(HWND hwnd, long x, long y, bool bIncludeSourceDocName=true);
	// Expand tags using FAM Tag manager and expand utility function
	static const string ExpandTagsAndTFE(IFAMTagManager * pFAMTM, const string& strFile, 
		const string& strSourceDocName);

private:
	// return IFAMTagManager pointer
	static IFAMTagManagerPtr getFAMTagManager();
};