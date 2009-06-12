using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace ValidateXMLFile
{
    class Program
    {
        void displayUsage()
        {
            System.Console.WriteLine("\nApplication usage:");
            System.Console.WriteLine("ValidateXMLFile.exe <filename>");
            System.Console.WriteLine("  <filename>: the name of the XML file to validate!\n");
        }

        // Validation Error Count
        int m_iErrorsCount = 0;

        // Validation Error Message
        string m_strErrorMessage = "";

        public void ValidationHandler(object sender, ValidationEventArgs args)
        {
            m_strErrorMessage = m_strErrorMessage + args.Message + "\r\n";
            m_iErrorsCount++;
        }

        void validateFile(string strXMLFile)
        {
            // setup the XML validating reader
            XmlTextReader tr = new XmlTextReader(strXMLFile);
            XmlValidatingReader vr = new XmlValidatingReader(tr);
            vr.ValidationType = ValidationType.Schema;
            vr.ValidationEventHandler += new ValidationEventHandler(ValidationHandler);

            // read the file and accumulate error information
            while (vr.Read()) ;
            vr.Close();

            // Raise exception, if XML validation fails
            if (m_iErrorsCount > 0)
            {
                throw new Exception(m_strErrorMessage);
            }
        }
 
        static void Main(string[] args)
        {
            try
            {
                // create a local instance to call methods on so that
                // we don't have to declare them as static
                Program p = new Program() ;

                // ensure that only one argument was provided
                if (args.Length != 1)
                {
                    p.displayUsage();
                }
                else
                {
                    System.Console.WriteLine("\nFile being validated:");
                    System.Console.WriteLine(args[0]);

                    p.validateFile(args[0]);

                    System.Console.WriteLine("File is OK.");
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Validation errors were found!");
                System.Console.WriteLine(e.Message);
            }
        }
    }
}
