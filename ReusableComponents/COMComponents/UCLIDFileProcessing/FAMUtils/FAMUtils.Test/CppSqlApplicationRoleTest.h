#pragma once

using namespace System;
using namespace NUnit::Framework;
using namespace Extract::Testing::Utilities;


namespace FAMUtils {
	namespace Test {

		[TestFixture()]
		public ref class CppSqlApplicationRoleTest
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
			void CreateApplicationRoleTest();

			[Test]
			[Category("Automated")]
			[Category("Cpp")]
			[TestCase(0/*SqlApplicationRole.AppRoleAccess.NoAccess*/, "TestCppUseApplicationRoleTest_NoAccess", Description = "Sql Application role for no access")]
			[TestCase(1/*SqlApplicationRole.AppRoleAccess.SelectAccess*/, "TestCppUseApplicationRoleTest_SelectAccess", Description = "Sql Application role for Select access")]
			[TestCase(3/*SqlApplicationRole.AppRoleAccess.InsertAccess*/, "TestCppUseApplicationRoleTest_InsertAccess", Description = "Sql Application role for Insert access")]
			[TestCase(5/*SqlApplicationRole.AppRoleAccess.UpdateAccess*/, "TestCppUseApplicationRoleTest_UpdateAccess", Description = "Sql Application role for Update access")]
			[TestCase(9/*SqlApplicationRole.AppRoleAccess.DeleteAccess*/, "TestCppUseApplicationRoleTest_DeleteAccess", Description = "Sql Application role for Delete access")]
			[TestCase(15 /*SqlApplicationRole.AppRoleAccess.AllAccess*/, "TestCppUseApplicationRoleTest_AllAccess", Description = "Sql Application role for All access")]
			static void UseApplicationRoleTest(int access, System::String^ testDBName);
		};
	}
}
