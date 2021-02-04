using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using Extract;
using Microsoft.Deployment.WindowsInstaller;

namespace WebInstallerCustomActions
{
    public static class CustomActions
    {
        [CustomAction]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        public static ActionResult UpdateAppSettings(Session session)
        {
            ExtractException.Assert("ELI51533", "Session cannot be null", session != null);

            try
            {
                string appBackendJson = File.ReadAllText(@"C:\Program Files (x86)\Extract Systems\APIs\AppBackendAPI\appsettings.json");
                string docAPIJson = File.ReadAllText(@"C:\Program Files (x86)\Extract Systems\APIs\DocumentAPI\appsettings.json");
                dynamic appBackendJsonDeserial = Newtonsoft.Json.JsonConvert.DeserializeObject(appBackendJson);
                dynamic docAPIJsonDeserial = Newtonsoft.Json.JsonConvert.DeserializeObject(docAPIJson);
                appBackendJsonDeserial["DatabaseName"] = session["DATABASE_NAME"];
                appBackendJsonDeserial["DatabaseServer"] = session["DATABASE_SERVER"];
                appBackendJsonDeserial["DefaultWorkflow"] = session["DATABASE_WORKFLOW"];
                docAPIJsonDeserial["DatabaseName"] = session["DATABASE_NAME"];
                docAPIJsonDeserial["DatabaseServer"] = session["DATABASE_SERVER"];
                docAPIJsonDeserial["DefaultWorkflow"] = session["DATABASE_WORKFLOW"];


                string appBackendOutput = Newtonsoft.Json.JsonConvert.SerializeObject(appBackendJsonDeserial, Newtonsoft.Json.Formatting.Indented);
                string docAPIOutput = Newtonsoft.Json.JsonConvert.SerializeObject(appBackendJsonDeserial, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(@"C:\Program Files (x86)\Extract Systems\APIs\AppBackendAPI\appsettings.json", appBackendOutput);
                File.WriteAllText(@"C:\Program Files (x86)\Extract Systems\APIs\DocumentAPI\appsettings.json", docAPIOutput);
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI51501").Log();
                throw;
            }
        }

        [CustomAction]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        public static ActionResult ModifyWebConfig(Session session)
        {
            ExtractException.Assert("ELI51531", "Session cannot be null", session != null);
            
            try
            {
                var appBackendconfig = File.ReadAllText(@"C:\Program Files (x86)\Extract Systems\APIs\AppBackendAPI\web.config");
                var docAPIconfig = File.ReadAllText(@"C:\Program Files (x86)\Extract Systems\APIs\DocumentAPI\web.config");

                if (System.Environment.OSVersion.Version.Major >= 10)
                {
                    appBackendconfig = appBackendconfig.Replace("<mimeMap fileExtension=\".json\" mimeType=\"application/json\" />", string.Empty);
                    docAPIconfig = docAPIconfig.Replace("<mimeMap fileExtension=\".json\" mimeType=\"application/json\" />", string.Empty);
                }
                appBackendconfig = appBackendconfig.Replace(@"..\..\CommonComponents\AppBackendAPI.exe", @"C:\Program Files (x86)\Extract Systems\CommonComponents\AppBackendAPI.exe");
                docAPIconfig = docAPIconfig.Replace(@"..\..\CommonComponents\DocumentAPI.exe", @"C:\Program Files (x86)\Extract Systems\CommonComponents\DocumentAPI.exe");

                File.WriteAllText(@"C:\Program Files (x86)\Extract Systems\APIs\AppBackendAPI\web.config", appBackendconfig);
                File.WriteAllText(@"C:\Program Files (x86)\Extract Systems\APIs\DocumentAPI\web.config", docAPIconfig);
                AddCorsToAuthorizationAPI(session);
                return ActionResult.Success;
            }
            catch(Exception ex)
            {
                ex.AsExtract("ELI51500").Log();
                throw;
            }
        }

        [CustomAction]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        public static ActionResult UpdateAngularSettings(Session session)
        {
            ExtractException.Assert("ELI51532", "Session cannot be null", session != null);

            try
            {
                string angularJson = File.ReadAllText(@$"{session["INSTALLLOCATION"].ToString()}IDSVerify\json\settings.json");
                dynamic angularJsonDeserial = Newtonsoft.Json.JsonConvert.DeserializeObject(angularJson);
                angularJsonDeserial["appBackendUrl"] = "http://" + session["APPBACKEND_DNS_ENTRY"];
                angularJsonDeserial["WindowsAuthenticationUrl"] = "http://" + session["WINDOWSAUTHORIZATION_DNS_ENTRY"];
                angularJsonDeserial["EnablePasswordLogin"] = !session["CREATE_WINDOWS_AUTHORIZATION_SITE"].Equals("1", StringComparison.OrdinalIgnoreCase);
                angularJsonDeserial["UseWindowsAuthentication"] = session["CREATE_WINDOWS_AUTHORIZATION_SITE"].Equals("1", StringComparison.OrdinalIgnoreCase);
                angularJsonDeserial["ForceRedactionTypeToBeSet"] = !session["FORCE_REDACTION_TYPE_TO_BE_SET"].Equals("1", StringComparison.OrdinalIgnoreCase);

                string angularOutput = Newtonsoft.Json.JsonConvert.SerializeObject(angularJsonDeserial, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(@$"{session["INSTALLLOCATION"].ToString()}IDSVerify\json\settings.json", angularOutput);
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI51500").Log();
                throw;
            }
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        private static void AddCorsToAuthorizationAPI(Session session)
        {
            XDocument doc = XDocument.Load(@"C:\Program Files (x86)\Extract Systems\APIs\AuthorizationAPI\web.config");
            XElement webServerNode = doc.Element("configuration").Element("system.webServer");
            webServerNode.Add(
                new XElement("cors",
                    new XAttribute("enabled", "true"),
                    new XAttribute("failUnlistedOrigins", "true"),
                    new XElement("add",
                        new XAttribute("origin", "http://" + session["IDSVERIFY_DNS_ENTRY"].ToLowerInvariant()),
                        new XAttribute("allowCredentials", "true"),
                        new XAttribute("maxAge", "120"),
                        new XElement("allowHeaders",
                            new XAttribute("allowAllRequestedHeaders", "true")))));



            doc.Save(@"C:\Program Files (x86)\Extract Systems\APIs\AuthorizationAPI\web.config");
        }
    }
}
