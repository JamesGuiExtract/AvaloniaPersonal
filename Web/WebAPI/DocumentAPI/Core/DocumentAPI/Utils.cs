using AttributeDbMgrComponentsLib;
using DocumentAPI.Models;
using Microsoft.AspNetCore.Hosting;     // for IHostingEnvironment
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UCLID_FILEPROCESSINGLib;

namespace DocumentAPI
{
    /// <summary>
    /// static Utils are kept here for global use
    /// </summary>
    public static class Utils
    {
        private static FileProcessingDB _fileProcessingDB = null;
        private static AttributeDBMgr _attributeDbMgr = null;
        private static IHostingEnvironment _environment = null;
        private static string _databaseServer;
        private static string _databaseName;
        private static string _attributeSetName = "Attr";

        /// <summary>
        /// Inv - short form of Invariant. Normally I would use the full name, but in this case the 
        /// full name is just noise, a distraction from the more important functionality. All this
        /// function does is prevent FXCop warnings!
        /// </summary>
        /// <param name="strings">strings - one or more strings to format</param>
        /// <returns></returns>
        public static string Inv(params FormattableString[] strings)
        {
            return string.Join("", strings.Select(str => FormattableString.Invariant(str)));
        }

        /// <summary>
        /// make an error info instance
        /// </summary>
        /// <param name="isError"></param>
        /// <param name="message"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static ErrorInfo MakeError(bool isError, string message = "", int code = 0)
        {
            return new ErrorInfo
            {
                ErrorOccurred = isError,
                Message = message,
                Code = code
            };
        }

        /// <summary>
        /// Make a list of Processing status, with one ProcessingStatus element.
        /// </summary>
        /// <param name="isError"></param>
        /// <param name="message"></param>
        /// <param name="status"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static List<ProcessingStatus> MakeListProcessingStatus(bool isError, 
                                                                      string message, 
                                                                      DocumentProcessingStatus status,
                                                                      int code = 0)
        {
            var ps = new ProcessingStatus
            {
                Error = MakeError(isError, message, code: 0),
                DocumentStatus = status
            };

            List<ProcessingStatus> lps = new List<ProcessingStatus>();
            lps.Add(ps);

            return lps;
        }

        /// <summary>
        /// makes a document attribute set for returning an error
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static DocumentAttributeSet MakeDocumentAttributeSetError(string message)
        {
            return new DocumentAttributeSet
            {
                Error = new ErrorInfo
                {
                    ErrorOccurred = true,
                    Message = message,
                    Code = -1
                },

                Attributes = null
            };
        }

        /// <summary>
        /// make a document attribute set for a successful return case
        /// </summary>
        /// <param name="documentAttribute"></param>
        /// <returns></returns>
        public static DocumentAttributeSet MakeDocumentAttributeSet(DocumentAttribute documentAttribute)
        {
            var lda = new List<DocumentAttribute>();
            lda.Add(documentAttribute);

            return new DocumentAttributeSet
            {
                Error = MakeError(isError: false),
                Attributes = lda
            };
        }

        /// <summary>
        /// constructs (if necessary) and returns a fileProcessingDB instance
        /// </summary>
        public static FileProcessingDB FileDbMgr
        {
            get
            {
                if (_fileProcessingDB == null)
                {
                    try
                    {
                        FAMDBUtils dbUtils = new FAMDBUtils();
                        Type mgrType = Type.GetTypeFromProgID(dbUtils.GetFAMDBProgId());
                        _fileProcessingDB = (FileProcessingDB)Activator.CreateInstance(mgrType);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(Inv($"Exception creating FileProcessingDB instance: {ex.Message}"));
                        throw;
                    }

                    Contract.Assert(_fileProcessingDB != null, "Failed to create FileProcessingDB instance");

                    // TODO - get these values from configuration
                    try
                    {
                        _fileProcessingDB.DatabaseServer = DatabaseServer;
                        _fileProcessingDB.DatabaseName = DatabaseName;
                    }
                    catch (Exception exp)
                    {
                        Log.WriteLine(Inv($"Exception setting FileProcessingDB DB context: {exp.Message}"));
                        throw;
                    }
                }

                return _fileProcessingDB;
            }
        }

        /// <summary>
        /// In case of a critical error using the file processing db interface, reset it.
        /// The intent is to allow the Web API to recover from a critical FAM error.
        /// </summary>
        public static void ResetFileProcessingDB()
        {
            _fileProcessingDB = null;
        }

        /// <summary>
        /// constructs (if necessary) and returns an AttributeDBMgr instance
        /// </summary>
        public static AttributeDBMgr AttrDbMgr
        {
            get
            {
                if (_attributeDbMgr == null)
                {
                    try
                    {
                        _attributeDbMgr = new AttributeDBMgr();
                        Contract.Assert(_attributeDbMgr != null, "Failure to create attributeDbMgr!");

                        _attributeDbMgr.FAMDB = FileDbMgr;
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(Inv($"Exception creating AttributeDBMgr: {ex.Message}"));
                        throw;
                    }
                }

                return _attributeDbMgr;
            }
        }

        /// <summary>
        /// reset the attribute manager - intended to allow recovery on critical error using interface...
        /// </summary>
        public static void ResetAttributeMgr()
        {
            _attributeDbMgr = null;
        }

        /// <summary>
        /// the name of the database server
        /// </summary>
        public static string DatabaseServer
        {   get
            {
                Contract.Assert(!String.IsNullOrEmpty(_databaseServer), "DatabaseServer cannot be empty.");
                return _databaseServer;
            }
            set
            {
                _databaseServer = value;
            }
        }

        /// <summary>
        /// The name of the database to use
        /// </summary>
        public static string DatabaseName
        {
            get
            {
                Contract.Assert(!String.IsNullOrEmpty(_databaseName), "DatabaseName cannot be empty");
                return _databaseName;
            }
            set
            {
                _databaseName = value;
            }
        }
        
        /// <summary>
        /// environment getter/setter
        /// </summary>
        public static IHostingEnvironment environment
        {
            get
            {
                Contract.Assert(_environment != null, "environment is null");
                return _environment;
            }
            
            set
            {
                Contract.Assert(value != null, "environment is being set to null");
                _environment = value;
            }
        }
        

        /// <summary>
        /// String compare made easier to use and read...
        /// Note that this always uses CultureInfo.InvariantCulture
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        public static bool IsEquivalent(this string s1, 
                                        string s2, 
                                        bool ignoreCase = true)
        {
            if (String.Compare(s1, s2, ignoreCase, CultureInfo.InvariantCulture) == 0)
                return true;

            return false;
        }

        /// <summary>
        /// make a default initialized SpatialLineZone
        /// </summary>
        /// <returns></returns>
        public static SpatialLineZone MakeDefaultSpatialLineZone()
        {
            return new SpatialLineZone()
            {
                PageNumber = -1
            };
        }

        /// <summary>
        /// TODO - remove this...
        /// A place to hold the attribute set name - this is temporary until workflows are implemented...
        /// </summary>
        public static string AttributeSetName
        {
            get
            {
                return _attributeSetName;
            }

            set
            {
                _attributeSetName = value;
            }
        }
    }
}
