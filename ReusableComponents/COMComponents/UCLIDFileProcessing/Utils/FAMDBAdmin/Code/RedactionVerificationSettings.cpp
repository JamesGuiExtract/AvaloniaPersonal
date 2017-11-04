#include "StdAfx.h"
#include "RedactionVerificationSettings.h"

namespace Extract
{
	namespace FAMDBAdmin
	{
		RedactionVerificationSettings::RedactionVerificationSettings()
		{
			RedactionTypes = gcnew List<String^>();
		}

		RedactionVerificationSettings::~RedactionVerificationSettings()
		{
		}
	}
}
