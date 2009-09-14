//
// NOTE: The contents of this file were downloaded from www.codeproject.com
// DOWNLOAD DATE: 04-12-2002
//
// Arguments.h: interface for the Arguments class.
//
// (C) 2001 NOVACOM GmbH, Berlin, www.novacom.net
// Author: Patrick Hoffmann, 03/21/2001
//
//////////////////////////////////////////////////////////////////////

#pragma once

#include "BaseUtils.h"

#include <string>
#include <vector>
#include <map>

using namespace std;

typedef string tstring;

inline ostream &operator<<(ostream& ostr, tstring& tstr )
{
	ostr << tstr.c_str();
	return ostr;
}

class EXPORT_BaseUtils Arguments  
{
	tstring	m_strOptionmarkers;
	tstring	m_strDescription;
	tstring	m_strCommandName;

public:
	static tstring	UnknownArgument;

	class Option;

	class EXPORT_BaseUtils Argument
	{
		friend class Arguments;
		friend class Option;
		
		tstring	m_strName;
		tstring	m_strDescription;
		tstring	m_strValue;
		tstring	m_strDefault;
		bool	m_bOptional;

	public:
		Argument( tstring strName, tstring strDescription=_T(""), tstring strDefault=_T("") );
	};

	typedef vector<Argument>	ArgVector;

	class EXPORT_BaseUtils Option
	{
		friend class Arguments;
		static Option	Empty;
		
		TCHAR			m_chName;
		ArgVector		m_vArguments;
		tstring			m_strDescription;
		bool			m_bSet;
		
	public:
		Option( TCHAR chName, tstring strDescription=_T("") );
		bool AddArgument( tstring strName, tstring strDescription=_T(""), tstring strDefault = _T("") );
		tstring &operator[]( int n );
		tstring &operator[]( tstring strArgumentName );
		tstring &operator[]( const TCHAR *pszArgumentName );
		operator bool();
		void Set( bool bSet = true );
		tstring GetName();
	};

private:
	typedef map<TCHAR,Option,less<TCHAR>,allocator<Option> > OptionMap;
	
	OptionMap			m_mOptions;
	ArgVector			m_vArguments;

public:
	bool IsOption( TCHAR chOptionName );
	bool Usage();
	bool AddOption( TCHAR chOptionName, tstring strDescription=_T("") );
	bool AddOption( Option &option );
	bool AddArgument( tstring strName, tstring strDescription=_T(""), tstring strDefault = _T("") );
	bool Parse(int argc, TCHAR* argv[]);
	bool Parse(TCHAR *pszCommandLine);
	bool Parse();
	
	tstring &operator[]( int n );
	tstring &operator[]( tstring strArgumentName );
	Option &operator[]( TCHAR chOptionName );
	
	Arguments( tstring strCommandName, tstring strDescription=_T(""), tstring strOptionmarkers=_T("-/") );
	virtual ~Arguments();
};