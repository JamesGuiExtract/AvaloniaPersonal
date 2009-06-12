//
// NOTE: The contents of this file were downloaded from www.codeproject.com
// DOWNLOAD DATE: 04-12-2002
//
// Arguments.cpp: implementation of the Arguments class.
//
// (C) 2001 NOVACOM GmbH, Berlin, www.novacom.net
// Author: Patrick Hoffmann, 03/21/2001
//
//////////////////////////////////////////////////////////////////////


#include "stdafx.h"
#include "Arguments.h"
#include "UCLIDException.h"

#include <iostream>

#ifdef WIN32

//==========================================
// TINYCRT - Matt Pietrek 1996
// Microsoft Systems Journal, October 1996
// FUNCTION: _ConvertCommandLineToArgcArgv
//==========================================

#define _MAX_CMD_LINE_ARGS  128

LPTSTR _ppszArgv[_MAX_CMD_LINE_ARGS+1];

int __cdecl _ConvertCommandLineToArgcArgv( LPTSTR pszSysCmdLine )
{
    int cbCmdLine;
    int argc;
    LPTSTR pszCmdLine;
    
    // Set to no argv elements, in case we have to bail out
    _ppszArgv[0] = 0;

    // First get a pointer to the system's version of the command line, and
    // figure out how long it is.
    cbCmdLine = lstrlen( pszSysCmdLine );

    // Allocate memory to store a copy of the command line.  We'll modify
    // this copy, rather than the original command line.  Yes, this memory
    // currently doesn't explicitly get freed, but it goes away when the
    // process terminates.
    pszCmdLine = (LPTSTR)HeapAlloc( GetProcessHeap(), 0, (cbCmdLine+1)*sizeof(TCHAR) );
    if ( !pszCmdLine )
        return 0;

    // Copy the system version of the command line into our copy
    lstrcpy( pszCmdLine, pszSysCmdLine );

    if ( _T('"') == *pszCmdLine )   // If command line starts with a quote ("),
    {                           // it's a quoted filename.  Skip to next quote.
        pszCmdLine++;
    
        _ppszArgv[0] = pszCmdLine;  // argv[0] == executable name
    
        while ( *pszCmdLine && (*pszCmdLine != _T('"')) )
            pszCmdLine++;

        if ( *pszCmdLine )      // Did we see a non-NULL ending?
            *pszCmdLine++ = 0;  // Null terminate and advance to next char
        else
            return 0;           // Oops!  We didn't see the end quote
    }
    else    // A regular (non-quoted) filename
    {
        _ppszArgv[0] = pszCmdLine;  // argv[0] == executable name

        while ( *pszCmdLine && (_T(' ') != *pszCmdLine) && (_T('\t') != *pszCmdLine) )
            pszCmdLine++;

        if ( *pszCmdLine )
            *pszCmdLine++ = 0;  // Null terminate and advance to next char
    }

    // Done processing argv[0] (i.e., the executable name).  Now do th
    // actual arguments

    argc = 1;

    while ( 1 )
    {
        // Skip over any whitespace
        while ( *pszCmdLine && (_T(' ') == *pszCmdLine) || (_T('\t') == *pszCmdLine) )
            pszCmdLine++;

        if ( 0 == *pszCmdLine ) // End of command line???
            return argc;

        if ( _T('"') == *pszCmdLine )   // Argument starting with a quote???
        {
            pszCmdLine++;   // Advance past quote character

            _ppszArgv[ argc++ ] = pszCmdLine;
            _ppszArgv[ argc ] = 0;

            // Scan to end quote, or NULL terminator
            while ( *pszCmdLine && (*pszCmdLine != _T('"')) )
                pszCmdLine++;
                
            if ( 0 == *pszCmdLine )
                return argc;
            
            if ( *pszCmdLine )
                *pszCmdLine++ = 0;  // Null terminate and advance to next char
        }
        else                        // Non-quoted argument
        {
            _ppszArgv[ argc++ ] = pszCmdLine;
            _ppszArgv[ argc ] = 0;

            // Skip till whitespace or NULL terminator
            while ( *pszCmdLine && (_T(' ')!=*pszCmdLine) && (_T('\t')!=*pszCmdLine) )
                pszCmdLine++;
            
            if ( 0 == *pszCmdLine )
                return argc;
            
            if ( *pszCmdLine )
                *pszCmdLine++ = 0;  // Null terminate and advance to next char
        }

        if ( argc >= (_MAX_CMD_LINE_ARGS) )
            return argc;
    }
}
#endif

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

using namespace std;

Arguments::Option	Arguments::Option::Empty(_T('\0'));
tstring				Arguments::UnknownArgument(_T("<UnKnOwN>"));

Arguments::Arguments(tstring strCommandName, tstring strDescription, tstring strOptionmarkers)
: m_strCommandName( strCommandName ) 
, m_strDescription( strDescription )
, m_strOptionmarkers( strOptionmarkers )
{
}

Arguments::~Arguments()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16373");
}

#ifdef WIN32
bool Arguments::Parse(LPTSTR pszCommandLine)
{
	int argc = _ConvertCommandLineToArgcArgv(pszCommandLine);
	
	return Parse( argc, _ppszArgv );
}

bool Arguments::Parse()
{
	int argc = _ConvertCommandLineToArgcArgv(GetCommandLine());
	
	return Parse( argc, _ppszArgv );
}
#endif
bool Arguments::Parse(int argc, TCHAR *argv[])
{
	if( m_strCommandName.empty() )
		m_strCommandName = argv[0];

	unsigned int nArg = 0;

	for (int i = 1; i < argc; i++)
	{
		tstring strArgument = argv[i];

		// Option...?
		if( m_strOptionmarkers.find(strArgument.substr(0,1)) != tstring::npos )
		{
			TCHAR chOptionName = strArgument[1];

			OptionMap::iterator it = m_mOptions.find(chOptionName);

			if( it == m_mOptions.end() )
			{
				cerr << m_strCommandName << " error: Unknown option " << strArgument << "." << endl;
				Usage();
				return false;
			}
			else
			{
				it->second.m_bSet = true;

				i++;
				{ 
					unsigned int nNonOptionalArgs = 0;
					
					{
						for( ArgVector::iterator itOptArg = it->second.m_vArguments.begin(); itOptArg != it->second.m_vArguments.end(); itOptArg++ ) 
						{
							if( !itOptArg->m_bOptional )
								nNonOptionalArgs++;
						}
					}
					
					for(unsigned int nOptArg = 0; nOptArg < it->second.m_vArguments.size(); i++, nOptArg++ )
					{
						if( i >= argc || m_strOptionmarkers.find(tstring(argv[i]).substr(0,1)) != tstring::npos )
						{
							if( nOptArg < nNonOptionalArgs )
							{
								cerr << m_strCommandName << " error: Too few arguments for option " << strArgument << "." << endl;
								Usage();
								return false;
							}
							else
							{
								break;
							}
						}
						
						it->second.m_vArguments[nOptArg].m_strValue = argv[i];
					}
				}
				i--;
			}
		}
		else	// ...oder Argument
		{
			if( nArg >= m_vArguments.size() )
			{
				cerr << m_strCommandName << " error: Too much arguments. " << endl;
				Usage();
				return false;
			}

			m_vArguments[nArg++].m_strValue = strArgument;
		}
	}

	{
		unsigned int nNonOptionalArgs = 0;
	
		{
			for( ArgVector::iterator it = m_vArguments.begin(); it != m_vArguments.end(); it++ ) 
			{
				if( !it->m_bOptional )
					nNonOptionalArgs++;
			}
		}
		
		if( nNonOptionalArgs > nArg )
		{
			cerr << m_strCommandName << " error: Too few arguments." << endl;
			Usage();
			return false;
		}
	}
	
	return true;
}

bool Arguments::AddOption(TCHAR chOption, tstring strDescription)
{
	m_mOptions.insert( pair<TCHAR,Option>(chOption,Option(chOption,strDescription)) );

	return true;
}

bool Arguments::AddOption( Option &option )
{
	m_mOptions.insert( pair<TCHAR,Option>(option.m_chName,option) );
	
	return true;
}

bool Arguments::Usage()
{
	cerr << "Usage: " << m_strCommandName;
	
	for( OptionMap::iterator it = m_mOptions.begin(); it != m_mOptions.end(); it++ )
	{
		cerr << " [" << m_strOptionmarkers[0] << it->second.GetName();
		
		for( ArgVector::iterator itArg = it->second.m_vArguments.begin(); itArg != it->second.m_vArguments.end(); itArg++ )
		{
			if( itArg->m_bOptional )
				cerr << " [" << itArg->m_strName << "]";
			else
				cerr << " " << itArg->m_strName;
		}
		cerr << "]";
	}

	ArgVector::iterator itArg;
	for (itArg = m_vArguments.begin(); itArg != m_vArguments.end(); itArg++ )
	{
		if( itArg->m_bOptional )
			cerr << " [" << itArg->m_strName << "]";
		else
			cerr << " " << itArg->m_strName;
	}
	
	cerr << endl;

	if (!m_mOptions.empty())
		cerr << endl << "Options:" << endl;
	
	for (OptionMap::iterator it = m_mOptions.begin(); it != m_mOptions.end(); it++)
	{
		cerr << "\t-" << it->second.GetName() << "\t  " << it->second.m_strDescription << endl;
		
		for( ArgVector::iterator itArg = it->second.m_vArguments.begin(); itArg != it->second.m_vArguments.end(); itArg++ )
		{
			cerr << "\t " << itArg->m_strName << "\t= " << itArg->m_strDescription << endl;

			if( itArg->m_bOptional )
				cerr << "\t\t  optional argument (default='" << itArg->m_strDefault << "')" << endl;
		}
	}
	
	if( !m_vArguments.empty() )
		cerr << endl << "Arguments:" << endl;

	for( itArg = m_vArguments.begin(); itArg != m_vArguments.end(); itArg++ )
	{
		cerr << "\t" << itArg->m_strName << "\t= " << itArg->m_strDescription << endl;

		if( itArg->m_bOptional )
			cerr << "\t\t  optional argument (default='" << itArg->m_strDefault << "')" << endl;
	}
	
	cerr << endl;
	
	cerr << m_strDescription << endl;

	return true;
}

Arguments::Option::Option( TCHAR chName, tstring strDescription )
: m_chName( chName )
, m_strDescription( strDescription )
, m_bSet( false )
{
}

bool Arguments::AddArgument( tstring strName, tstring strDescription, tstring strDefault )
{
	m_vArguments.push_back( Argument( strName, strDescription, strDefault ) );
	return true;
}

bool Arguments::Option::AddArgument( tstring strName, tstring strDescription, tstring strDefault )
{
	m_vArguments.push_back( Argument( strName, strDescription, strDefault ) );
	return true;
}

Arguments::Argument::Argument( tstring strName, tstring strDescription, tstring strDefault )
: m_strName( strName )
, m_strDescription( strDescription )
, m_strValue( strDefault )
, m_strDefault( strDefault )
, m_bOptional( !strDefault.empty() )
{

}

bool Arguments::IsOption(TCHAR chOptionName)
{
	OptionMap::iterator it = m_mOptions.find(chOptionName);
	
	if( it == m_mOptions.end() )
		return false;
	else 
		return it->second.m_bSet;
}

Arguments::Option::operator bool()
{
	return m_bSet;
}

void Arguments::Option::Set( bool bSet )
{
	m_bSet = bSet;
}

tstring &Arguments::operator[]( int n )
{
	return m_vArguments[n].m_strValue;
}

tstring &Arguments::operator[]( tstring strArgumentName )
{
	for( ArgVector::iterator it = m_vArguments.begin(); it != m_vArguments.end(); it++ ) 
	{
		if( it->m_strName == strArgumentName )
			return it->m_strValue;
	}

	return UnknownArgument;
}

tstring &Arguments::Option::operator[]( int n )
{
	return m_vArguments[n].m_strValue;
}

tstring &Arguments::Option::operator[]( const TCHAR *pszArgumentName )
{
	return operator[]( (tstring)pszArgumentName );
}

tstring &Arguments::Option::operator[]( tstring strArgumentName )
{
	for( ArgVector::iterator it = m_vArguments.begin(); it != m_vArguments.end(); it++ ) 
	{
		if( it->m_strName == strArgumentName )
			return it->m_strValue;
	}

	return UnknownArgument;
}

Arguments::Option &Arguments::operator[]( TCHAR chOptionName )
{
	OptionMap::iterator it = m_mOptions.find(chOptionName);
	
	if( it == m_mOptions.end() )
		return Option::Empty;
	else 
		return it->second;
}

tstring Arguments::Option::GetName()
{
	tstring str = _T(" ");

	str[0] = m_chName;

	return str;
}
