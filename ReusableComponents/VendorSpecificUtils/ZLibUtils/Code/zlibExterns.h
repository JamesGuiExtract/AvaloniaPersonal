#pragma once
#include "zlibUtils.h"
#include <zlib.h>
#include <vector>

class EXT_ZLIB_UTILS_DLL Deflater
{
public:
	Deflater( int level, std::vector<unsigned char>& buffer );
	~Deflater();

	void Deflate();

	std::vector<unsigned char> GetCompressedData();

private:
	z_stream	m_strm;
	std::vector<unsigned char> m_compressed;
	std::vector<unsigned char>& m_buffer;
};

class EXT_ZLIB_UTILS_DLL Inflater
{
public:
	explicit Inflater( std::vector<unsigned char>& compressedData );
	~Inflater();

	void Inflate();

	std::vector<unsigned char> GetDecompressedData();

private:
	z_stream	m_strm;
	std::vector<unsigned char>& m_compressedData;
	std::vector<unsigned char> m_buffer;
	std::vector<unsigned char> m_result;
};
