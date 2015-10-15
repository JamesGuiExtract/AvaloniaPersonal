#include "stdafx.h"
#include <vector>
#include <string>
#include <UCLIDException.h>
#include <zlibExterns.h>
#include <atlsafe.h>
#include <COMUtils.h>

#include "DefinedTypes.h"


namespace Internal
{
	VectorOfByte SafeArrayToByteVector( CComSafeArray<BYTE>& saData )
	{
		ulong numberElements = saData.GetCount();
		VectorOfByte data;
		LPSAFEARRAY* ppArray = saData.GetSafeArrayPtr();

		uchar* pStart = (uchar*)(*ppArray)->pvData;
		uchar* pEnd = pStart + numberElements;
		data.insert( std::end(data), pStart, pEnd );

		saData.Destroy();
		return data;
	}
	
	VectorOfByte SafeArrayToByteVector( SAFEARRAY* pSA )
	{
		VectorOfByte data;

		uchar* pStart = (uchar*)pSA->pvData;

		auto saSize = pSA->rgsabound[0].cElements;
		uchar* pEnd = pStart + saSize;
		data.insert( std::end(data), pStart, pEnd );

		return data;
	}
	

	VectorOfByte PersistStreamToByteVector( const IPersistStreamPtr& ipPersistObj )
	{
		CComSafeArray<BYTE> saData;
		saData.CopyFrom( writeObjToSAFEARRAY( ipPersistObj ) );
		ASSERT_RUNTIME_CONDITION( "ELI38825", 
								  1 == saData.GetDimensions(),  
								  Util::Format("In: %s, SAFEARRAY assumed to have 1 dimension, has: %d dimensions",
											   __FUNCTION__,
											   saData.GetDimensions() ).c_str() );
		return SafeArrayToByteVector( saData );
	}

	LPSAFEARRAY ConvertVectorOfByteToSafeArray( const VectorOfByte& data )
	{
		CComSafeArray<BYTE> saData( data.size(), 0 );
		for ( ulong i = 0; i < data.size(); ++i )
		{
			saData.SetAt( i, data[i] );
		}

		auto sa = saData.Detach();				
		return sa;
	}
	
	VectorOfByte Compress( VectorOfByte& buffer )
	{
		const int level9 = 9;
		Deflater deflater( level9, buffer );
		deflater.Deflate();

		return deflater.GetCompressedData();
	}


	// The input std::vector isn't const because this would force use of const_cast<>,
	// because the zstream.next_in isn't a const*. Damn!
	VectorOfByte Decompress( VectorOfByte& compressedData )
	{
		Inflater inflater( compressedData );
		inflater.Inflate();

		return inflater.GetDecompressedData();
	}

}		// end of namespace Internal


namespace ZipUtil
{
	SAFEARRAY* CompressAttributes( IPersistStreamPtr ipStream )
	{
		VectorOfByte buffer = Internal::PersistStreamToByteVector( ipStream );
		VectorOfByte compressed = Internal::Compress( buffer );
		return Internal::ConvertVectorOfByteToSafeArray( compressed );
	}

	SAFEARRAY* DecompressAttributes( SAFEARRAY* pSA )
	{
		VectorOfByte buffer = Internal::SafeArrayToByteVector( pSA );
		VectorOfByte outBuf = Internal::Decompress( buffer );

		return Internal::ConvertVectorOfByteToSafeArray( outBuf );
	}

#if 0	
	// Used to log the byte stream before compression and after decompression
	// (into two separate text files), so that the results can be diff'ed
	// and verified easily.
	void LogByteStream( const char* filename, const VectorOfByte& buffer )
	{
		std::ofstream ofile( filename );
		if ( !ofile.is_open() )
			return;

		ofile << Util::Format( "Size of byte stream: %d\n", buffer.size() );
		size_t elementsOnLine = 0;
		for ( size_t i = 0; i < buffer.size(); ++i, ++elementsOnLine )
		{
			if ( elementsOnLine == 20 )
			{
				elementsOnLine = 0;
				ofile << "\n";
			}

			ofile << Util::Format( "%02x ", buffer[i] );
		}

		ofile.flush();
		ofile.close();
	}

	void LogByteStream( const char* filename, IPersistStreamPtr ipStream )
	{
		VectorOfByte buffer = Internal::PersistStreamToByteVector( ipStream );
		LogByteStream( filename, buffer );
	}
#endif

}		// end of namespace ZipUtil


