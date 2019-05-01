#include "StdAfx.h"
#include "zlibExterns.h"
#include <UCLIDException.h>
#include <cpputil.h>



Deflater::Deflater( int level, std::vector<unsigned char>& buffer ):
m_compressed( buffer.size(), '\0' ),	// buffer size is MUCH larger than needed, max size for NO compression!
m_buffer( buffer )
{
	m_strm.opaque = Z_NULL;
	m_strm.zalloc = Z_NULL;
	m_strm.zfree = Z_NULL;

	int iRet = deflateInit(&m_strm, level);
	ASSERT_RUNTIME_CONDITION( "ELI38943", 
							  Z_OK == iRet, 
							  Util::Format( "In: %s, deflateInit failed, error: %d",
											__FUNCTION__,
											iRet ) );
}


Deflater::~Deflater()
{
	try
	{
		int iRet = deflateEnd(&m_strm);
		ASSERT_RUNTIME_CONDITION("ELI38939",
			Z_OK == iRet,
			Util::Format("In: %s, Compression end failed: %s",
				__FUNCTION__,
				Z_DATA_ERROR == iRet
				? "unflushed data present in stream"
				: "inconsistent stream state").c_str());
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI44674");
}

void Deflater::Deflate()
{
	m_strm.next_in = m_buffer.data();		// next input byte (byte stream starts here)
	m_strm.avail_in = m_buffer.size();		// number of available bytes to read
	m_strm.total_in = 0;					// input bytes read so far

	m_strm.next_out = m_compressed.data();	// next output byte (output stream starts here)
	m_strm.avail_out = m_compressed.size();	// remaining free space at next_out
	m_strm.total_out = 0;					// total bytes output so far

	auto ret = deflate(&m_strm, Z_FINISH);
	ASSERT_RUNTIME_CONDITION( "ELI38848", 
						  Z_STREAM_END == ret, 
						  Util::Format("In: %s, deflate() returned error code: %d", 
									   __FUNCTION__, 
									   ret).c_str() );
}

std::vector<unsigned char> Deflater::GetCompressedData()
{
	try
	{
		size_t compressedSize = m_compressed.size() - m_strm.avail_out;
		std::vector<unsigned char> result( m_compressed.begin(), m_compressed.begin() + compressedSize );
		return std::move( result );
	}
	CATCH_UNEXPECTED_EXCEPTION( "ELI38952" );

	std::vector<unsigned char> empty;
	return empty;
}


namespace
{
	const size_t inflateBufferSize = 64 * 1024;
}

Inflater::Inflater( std::vector<unsigned char>& compressedData ):
m_compressedData( compressedData ),
m_buffer( inflateBufferSize, '\0' )
{
	try
	{
		try
		{
			m_strm.opaque = Z_NULL;
			m_strm.zalloc = Z_NULL;
			m_strm.zfree = Z_NULL;
		
			m_strm.next_in = compressedData.data();		// start of input bytes
			m_strm.avail_in = compressedData.size();	// number of input bytes
		
			auto ret = inflateInit( &m_strm );
			ASSERT_RUNTIME_CONDITION( "ELI38855", 
									  ret == Z_OK,
									  Util::Format("In: %s, inflateInit() returned error code: %d", 
												   __FUNCTION__, 
												   ret).c_str() );
		}
		CATCH_UCLID_EXCEPTION( "ELI38953" );
	}
	CATCH_UNEXPECTED_EXCEPTION( "ELI38954" );
}

Inflater::~Inflater()
{
	try
	{
		try
		{
			auto ret = inflateEnd( &m_strm );
			ASSERT_RUNTIME_CONDITION( "ELI38884", 
									  ret == Z_OK, 
									  Util::Format("In: %s, inflateEnd() returned error code: %d", 
												   __FUNCTION__, 
												   ret).c_str() );
		}
		CATCH_UCLID_EXCEPTION( "ELI38955" );
	}
	CATCH_UNEXPECTED_EXCEPTION( "ELI38956" );
}

void Inflater::Inflate()
{
	try
	{
		try
		{
			while ( true )
			{
				m_strm.next_out = m_buffer.data();			// m_buffer is temp buffer to inflate each chunk into
				m_strm.avail_out = m_buffer.size();			// max size to inflate into m_buffer

				auto ret = inflate( &m_strm, Z_NO_FLUSH );
				ASSERT_RUNTIME_CONDITION( "ELI38883", 
										  ret == Z_OK || ret == Z_STREAM_END,
										  Util::Format("In: %s, inflate() returned error code: %d", 
													   __FUNCTION__, 
													   ret).c_str() );

				size_t numberOfBytesProcessed = m_buffer.size() - m_strm.avail_out;
				unsigned char* pStart = m_buffer.data();
				unsigned char* pEnd= pStart + numberOfBytesProcessed;
				m_result.insert( std::end(m_result), pStart, pEnd );	// accumulate inflated bytes into result, adding to end

				if ( Z_STREAM_END == ret )
					return;
			}
		}
		CATCH_UCLID_EXCEPTION( "ELI38957" );
	}
	CATCH_UNEXPECTED_EXCEPTION( "ELI38958" );

}

std::vector<unsigned char> Inflater::GetDecompressedData()
{
	return std::move( m_result );
}



