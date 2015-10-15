#pragma once

namespace Logging
{
	void CreateLog( const char* fullPathFilename );

	void WriteToLog( const char* formatSpec, ... );
}