#pragma once

#include <string>
#include <KernelAPI.h>

// Releases memory allocated by RecAPI (Nuance) calls. Create this object after the RecAPI call has
// allocated space for the object. MemoryType is the data type of the object to release when 
// RecMemoryReleaser goes out of scope.
template<typename MemoryType>
class RecMemoryReleaser
{
public:
	RecMemoryReleaser(MemoryType* pMemoryType);
	~RecMemoryReleaser();

private:
	MemoryType* m_pMemoryType;
};

namespace ifc {
	//-------------------------------------------------------------------------------------------------
	// Supported file types
	//-------------------------------------------------------------------------------------------------
	typedef enum EConverterFileType
	{
		kFileType_None,
		kFileType_Tif,
		kFileType_Pdf,
		kFileType_Jpg
	}	EConverterFileType;

	//-------------------------------------------------------------------------------------------------
	// Image conversion functions
	//-------------------------------------------------------------------------------------------------
	void convertImage(
		const std::string strInputFileName,
		const std::string strOutputFileName,
		EConverterFileType eOutputType,
		bool bPreserveColor,
		std::string strPagesToRemove,
		IMF_FORMAT nExplicitFormat,
		int nCompressionLevel);

	void convertImagePage(
		const std::string strInputFileName,
		const std::string strOutputFileName,
		EConverterFileType eOutputType,
		bool bPreserveColor,
		int nPage,
		IMF_FORMAT nExplicitFormat,
		int nCompressionLevel);
}
