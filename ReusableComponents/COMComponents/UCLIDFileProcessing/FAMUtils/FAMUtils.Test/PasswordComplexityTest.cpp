#include "stdafx.h"
#include "PasswordComplexityTest.h"
#include <msclr\marshal_cppstd.h>
#include <string>
#include <cpputil.h>


using namespace msclr::interop;
using namespace Extract::Utilities;

namespace FAMUtils {
	namespace Test {

		void PasswordComplexityTest::CheckPasswordComplexity_ConfirmFailures(String^ password, String^ complexityRequirements)
		{
			std::string cppPassword = marshal_as<std::string>(password);
			std::string cppComplexityRequirements = marshal_as<std::string>(complexityRequirements);

			try
			{
				Util::checkPasswordComplexity(cppPassword, cppComplexityRequirements);
				Assert::Fail("Expected an exception but got none");
			}
			catch (...)
			{
			}
		}

		void PasswordComplexityTest::CheckPasswordComplexity_ConfirmSuccesses(String^ password, String^ complexityRequirements)
		{
			std::string cppPassword = marshal_as<std::string>(password);
			std::string cppComplexityRequirements = marshal_as<std::string>(complexityRequirements);
			Util::checkPasswordComplexity(cppPassword, cppComplexityRequirements);
		}

		void PasswordComplexityTest::PasswordComplexityRequirements_ConfirmDecodeEncodeRoundTrip(String ^input, String ^expectedOutput)
		{
			Assert::AreEqual(expectedOutput, (gcnew PasswordComplexityRequirements(input))->EncodeRequirements());
		}
	}
}
