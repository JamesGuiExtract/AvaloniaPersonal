#pragma once

using namespace System;
using namespace NUnit::Framework;
using namespace Extract::Testing::Utilities;

namespace FAMUtils {
	namespace Test {

		[TestFixture()]
		ref class PasswordComplexityTest
		{
		private:

		public:

			[OneTimeSetUp]
			static void Setup()
			{
				GeneralMethods::TestSetup();
			}

			[OneTimeTearDown]
			static void FinalCleanup()
			{

			}

			[Test]
			[Category("Automated")]
			[Category("Cpp")]
			[TestCase("", "", TestName = "CheckPassword: Empty password should fail with empty requirements")]
			[TestCase("", "U", TestName = "CheckPassword: Empty password should fail with invalid requirements")]
			[TestCase("1234567", "U3P", TestName = "CheckPassword: Seven character password should fail with invalid requirements")]
			[TestCase("1234567", "0D", TestName = "CheckPassword: Seven character password should fail with invalid requirements")]
			[TestCase("Abc?efgh", "U", TestName = "CheckPassword: Password missing a digit should fail with invalid requirements")]
			[TestCase("AB3?5678", "U", TestName = "CheckPassword: Password missing lower case should fail with invalid requirements")]
			[TestCase("ab3?5678", "U", TestName = "CheckPassword: Password missing upper case should fail with invalid requirements")]
			[TestCase("A", "U", TestName = "CheckPassword: A single char password should fail with invalid requirements")]
			[TestCase("A", "2", TestName = "CheckPassword: A single char password should fail when required length is 2")]
			[TestCase("AA", "3", TestName = "CheckPassword: A two-char password should fail when required length is 3")]
			[TestCase("A", "1L", TestName = "CheckPassword: An upper case letter should fail when lower case is required")]
			[TestCase("a", "1LU", TestName = "CheckPassword: Missing upper case requirement should fail")]
			[TestCase("aA", "1LUD", TestName = "CheckPassword: Missing digit requirement should fail")]
			[TestCase("a1A", "1LUDP", TestName = "CheckPassword: Missing punctuation requirement should fail")]
			[TestCase("a1A.", "5LUDP", TestName = "CheckPassword: Too short should fail")]
			static void CheckPasswordComplexity_ConfirmFailures(String ^password, String ^complexityRequirements);

			[Test]
			[Category("Automated")]
			[Category("Cpp")]
			[TestCase(".", "", TestName = "CheckPassword: Single char should be OK with empty requirements")]
			[TestCase(".", "1", TestName = "CheckPassword: A single char password should be OK when required length is 1")]
			[TestCase("Ab3?5678", "U", TestName = "CheckPassword: Eight character password with all categories should be OK with invalid requirements 1")]
			[TestCase("Ab3?5678", "W", TestName = "CheckPassword: Eight character password with all categories should be OK with invalid requirements 2")]
			[TestCase("Ab3?5678", "U4P", TestName = "CheckPassword: Eight character password with all categories should be OK with invalid requirements 3")]
			[TestCase("Ab3?5678", "0P", TestName = "CheckPassword: Eight character password with all categories should be OK with invalid requirements 4")]
			[TestCase("Ab3?5678", "8ULDP", TestName = "CheckPassword: Eight character password with all categories should be OK with valid requirements")]
			[TestCase("AA", "2", TestName = "CheckPassword: A two-char password should be OK when required length is 2")]
			[TestCase("A", "1U", TestName = "CheckPassword: An upper case letter should be OK when that is all that is required")]
			[TestCase("a", "1L", TestName = "CheckPassword: An lower case letter should be OK when that is all that is required")]
			[TestCase("aA", "1LU", TestName = "CheckPassword: Missing digit and punct should be OK")]
			[TestCase("aA3", "1LUD", TestName = "CheckPassword: Missing punct should be OK")]
			[TestCase("a1A.", "4LUDP", TestName = "CheckPassword: Exact length should be OK")]
			[TestCase("a1A.5678", "4LUDP", TestName = "CheckPassword: Longer than needed should be OK")]
			static void CheckPasswordComplexity_ConfirmSuccesses(String ^password, String ^complexityRequirements);

			[Test]
			[Category("Automated")]
			[Category("Cpp")]
			[TestCase("", "1", TestName = "DecodeEncode: Empty requirements turn into lax requirements")]
			[TestCase((String^)nullptr, "1", TestName = "DecodeEncode: Null requirements turn into lax requirements")]
			[TestCase("D", "8ULDP", TestName = "DecodeEncode: Invalid requirements turn into strict requirements 1")]
			[TestCase("Z", "8ULDP", TestName = "DecodeEncode: Invalid requirements turn into strict requirements 2")]
			[TestCase("L4U", "8ULDP", TestName = "DecodeEncode: Invalid requirements turn into strict requirements 3")]
			[TestCase("0D", "8ULDP", TestName = "DecodeEncode: Invalid requirements turn into strict requirements 4")]
			[TestCase("3", "3", TestName = "DecodeEncode: Length comes back OK")]
			[TestCase("5U", "5U", TestName = "DecodeEncode: Upper comes back OK")]
			[TestCase("6LU", "6UL", TestName = "DecodeEncode: Upper and lower come back OK")]
			[TestCase("7DU", "7UD", TestName = "DecodeEncode: Upper and digit come back OK")]
			[TestCase("8PL", "8LP", TestName = "DecodeEncode: Lower and punct come back OK")]
			[TestCase("9PDLU", "9ULDP", TestName = "DecodeEncode: All out of order come back in normal form")]
			[TestCase("10PL", "10LP", TestName = "DecodeEncode: Two-digit length is OK")]
			[TestCase("100PL", "100LP", TestName = "DecodeEncode: Three-digit length is OK")]
			static void PasswordComplexityRequirements_ConfirmDecodeEncodeRoundTrip(String ^input, String ^output);
		};
	}
}

