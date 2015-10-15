#include "StdAfx.h"
#include <string>
#include <vector>
#include "cpputil.h"
#include "UCLIDException.h"

#include "Logging.h"


namespace
{
	std::string CurrentDateTimeAsString()
	{
		SYSTEMTIME st;
		GetLocalTime( &st );

		return Util::Format( "%04d-%02d-%02d_%02d-%02d", 
							 st.wYear,
							 st.wMonth,
							 st.wDay,
							 st.wHour, 
							 st.wMinute );
	}

	std::string MakeFilename( const char* filename )
	{
		return Util::Format( "%s_%s.txt", filename, CurrentDateTimeAsString().c_str() );
	}

}		// end of anonymous namespace

namespace Logging
{
	class Log
	{
		friend void WriteToLog( const char*, ... );
		friend void CreateLog( const char* );

		explicit Log( const char* filename ):
		_file( MakeFilename(filename).c_str() )
		{
			ASSERT_RUNTIME_CONDITION( "ELI38787", _file.is_open(), "Log open failed" );
		}

		static Log& Instance( const char* filename = nullptr )
		{
			static Log instance( filename );
			return instance;
		}

		void Write( const std::string& msg )
		{
			_file.write( msg.c_str(), msg.size() );
			_file.flush();
		}

		~Log()
		{
			_file.flush();
			_file.close();
		}

		std::ofstream _file;
	};

	void WriteToLog( const char* formatSpec, ... )
	{
#ifdef LOGGING_ENABLED
#pragma message("Logging is enabled...")
#elseif
#pragma message("Logging is disabled...")
		return;
#endif
		size_t size = 8 * 1024;
		std::vector<char> buffer( size, '\0' );

		std::string format = Util::Format( "%s\n", formatSpec );

		va_list args;
		va_start( args, formatSpec );
		::vsnprintf_s( &buffer[0], size, size, format.c_str(), args );

		std::string msg( buffer.data() );
		Log::Instance().Write( msg );
	}

	void CreateLog( const char* fullPathFilenameWithoutExtension )
	{
#ifndef LOGGING_ENABLED
		return;
#endif
	
		Log::Instance( fullPathFilenameWithoutExtension );
	}


}	// end of namespace Logging