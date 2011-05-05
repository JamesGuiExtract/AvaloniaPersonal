#include <string>

#include <TextFunctionExpander.h>
#include <ComUtils.h>
#include <QuickMenuChooser.h>
#include <VectorOperations.h>

using namespace std;

class CFileProcessorsUtils
{
public:
	CFileProcessorsUtils();
	~CFileProcessorsUtils();

	// Expand tags using FAM Tag manager and expand utility function
	static const string ExpandTagsAndTFE(IFAMTagManager * pFAMTM, const string& strFile, 
		const string& strSourceDocName);
};