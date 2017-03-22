using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Xml;

using NUnit.Framework;
using IO.Swagger.Api;
using IO.Swagger.Model;

using Extract.FileActionManager.Database.Test;
using Extract.Testing.Utilities;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Web.DocumentAPI.Test
{
    [TestFixture]
    [NUnit.Framework.Category("WebAPI")]
    class testUsers
    {

        #region Fields

        // When run from VS, DocumentAPI uses the port 58926 (as configured). When exec'd using dotnet,
        // port 5000 is the default port.
        static string WebApiURL = "http://localhost:5000";

        /// <summary>
        /// tracks whether the web service was invoked or not
        /// </summary>
        static bool APIInvoked;

        #endregion Fields

        #region Setup and Teardown

        [TestFixtureSetUp]
        public static void Setup()
        {
            APIInvoked = Utils.StartWebServer(workingDirectory: Utils.GetWebApiFolder, webApiURL: WebApiURL);
        }


        [TestFixtureTearDown]
        public static void FinalCleanup()
        {
            if (APIInvoked)
            {
                Utils.ShutdownWebServer(args: "/f /im DocumentAPI.exe");
            }
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
                var usersApi = new IO.Swagger.Api.UsersApi(basePath: WebApiURL);
                var user = new IO.Swagger.Model.User()
                {
                    Username = "admin",
                    Password = "a"
                };

                usersApi.ApiUsersLoginPost(user);
            }
            catch (Exception ex)
            {
                Assert.Fail("Failed: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Basic test of logout functionality
        /// </summary>
        [Test, Category("Automated")]
        public static void Test_Logout()
        {
            try
            {
                Test_Login();

                var usersApi = new IO.Swagger.Api.UsersApi(basePath: WebApiURL);
                usersApi.ApiUsersLogoutDelete();
            }
            catch (Exception ex)
            {
                Assert.Fail("Failed: {0}", ex.Message);
            }
        }

        #endregion Public Test Functions
    }
}
