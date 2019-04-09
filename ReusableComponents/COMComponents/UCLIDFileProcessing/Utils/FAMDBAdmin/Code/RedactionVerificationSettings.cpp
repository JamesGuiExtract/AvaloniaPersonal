#include "StdAfx.h"
#include "RedactionVerificationSettings.h"

namespace Extract
{
	namespace FAMDBAdmin
	{
		RedactionVerificationSettings::RedactionVerificationSettings()
		{
			InitializeDefaults();
		}

		RedactionVerificationSettings::~RedactionVerificationSettings()
		{
		}

		void RedactionVerificationSettings::InitializeDefaults()
		{
			RedactionTypes = gcnew List<String^>();
			InactivityTimeout = 5;
		}

		void RedactionVerificationSettings::OnDeserializing(StreamingContext context)
		{
			InitializeDefaults();
		}
	}
}
