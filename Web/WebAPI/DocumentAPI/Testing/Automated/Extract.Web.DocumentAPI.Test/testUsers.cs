using DocumentAPI.Models;
using NUnit.Framework;
using System;

namespace Extract.Web.DocumentAPI.Test
{
    [TestFixture]
    [NUnit.Framework.Category("WebAPI")]
    public class TestUsers
    {
        #region Setup and Teardown

        [TestFixtureSetUp]
        public static void Setup()
        {
            // TODO - when the fileProcessingDB has an API to retrieve user login info, remove this.
            var user = new User()
            {
                Username = "admin",
                Password = "a"
            };

            UserData.AddMockUser(user);
        }

        [TestFixtureTearDown]
        public static void FinalCleanup()
        {
        }

        #endregion Setup and Teardown

        #region Public Test Functions

        /// <summary>
        /// basic test of login funcitonality - login as admin
        /// </summary>
        [Test, Category("Automated")]
        public static void Test_Login()
        {
            try
            {
                var user = new User()
                {
                    Username = "admin",
                    Password = "a"
                };

                Assert.IsTrue(UserData.MatchUser(user), "User did not match");
            }
            catch (Exception ex)
            {
                Assert.Fail("Failed: {0}", ex.Message);
            }
        }

        #endregion Public Test Functions
    }
}
