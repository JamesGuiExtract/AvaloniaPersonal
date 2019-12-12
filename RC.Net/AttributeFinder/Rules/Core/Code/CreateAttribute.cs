using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using YamlDotNet.Serialization;
using SpatialString = UCLID_RASTERANDOCRMGMTLib.SpatialString;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// This class creates attributes using (optionally) xpath expressions.
    /// </summary>
    [ComVisible(true)]
    [Guid("2C83DD61-85B3-4CBC-A9B2-D36B2E615C1B")]
    [CLSCompliant(false)]
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public class CreateAttribute : IdentifiableObject, IOutputHandler, IIdentifiableObject,
        ICategorizedComponent, IConfigurableObject, ICopyableObject, ILicensedComponent,
        IPersistStream
    {

        #region Constants

        /// <summary>
        /// The description of the rule
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Create attribute";

        /// <summary>
        /// Current version.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.RuleWritingCoreObjects;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The "root" of the subattributes.
        /// </summary>
        string _root;

        /// <summary>
        /// The subattributes to create, added from the dialog
        /// </summary>
        List<AttributeNameAndTypeAndValue> _subattributesToCreate = new List<AttributeNameAndTypeAndValue>();

        /// <summary>
        /// <see langword="true"/> if changes have been made to <see cref="CreateAttribute"/>
        /// since it was created; <see langword="false"/> if no changes have been made since it was
        /// created.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <overloads>
        /// Initializes a new instance of the <see cref="CreateAttribute"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAttribute"/> class.
        /// </summary>
        public CreateAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAttribute"/> class.
        /// </summary>
        /// <param name="createAttribute">The <see cref="CreateAttribute"/> from which settings
        /// should be copied.</param>
        public CreateAttribute(CreateAttribute createAttribute)
        {
            try
            {
                CopyFrom(createAttribute);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39400");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the subattribute root.
        /// </summary>
        /// <value>
        /// The root.
        /// </value>
        public string Root
        {
            get
            {
                return _root;
            }
            set
            {
                try
                {
                    if (value != _root)
                    {
                        _root = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI39440");
                }
            }
        }

        /// <summary>
        /// Gets the subattribute component count.
        /// </summary>
        /// <value>
        /// The subattribute component count.
        /// </value>
        public int SubAttributeComponentCount
        {
            get
            {
                return _subattributesToCreate.Count;
            }
        }

        /// <summary>
        /// The collection of attributes to create
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<AttributeNameAndTypeAndValue> SubAttributesToCreate
        {
            get => _subattributesToCreate;
            set => _subattributesToCreate = value;
        }

        #endregion Properties

        #region Public Methods

        public void AddSubAttributeComponents(AttributeNameAndTypeAndValue components)
        {
            _subattributesToCreate.Add(components);
            _dirty = true;
        }

        /// <summary>
        /// Adds the subattribute (component set) to the internal list.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="type">The type.</param>
        /// <param name="nameContainsXPath">true iff name is an xpath expression</param>
        /// <param name="valueContainsXPath">true iff value is an xpath expression</param>
        /// <param name="typeContainsXPath">true iff type is an xpath expression</param>
        /// <param name="doNotCreateIfNameIsEmpty">true or false</param>
        /// <param name="doNotCreateIfValueIsEmpty">true or false</param>
        /// <param name="doNotCreateIfTypeIsEmpty">true or false</param>
        public void AddSubAttributeComponents(string name, 
                                              string value,
                                              string type,
                                              bool nameContainsXPath,
                                              bool valueContainsXPath,
                                              bool typeContainsXPath,
                                              bool doNotCreateIfNameIsEmpty,
                                              bool doNotCreateIfValueIsEmpty,
                                              bool doNotCreateIfTypeIsEmpty)
        {
            try
            {
                _subattributesToCreate.Add(new AttributeNameAndTypeAndValue(name, 
                                                                            type, 
                                                                            value,
                                                                            nameContainsXPath,
                                                                            typeContainsXPath,
                                                                            valueContainsXPath,
                                                                            doNotCreateIfNameIsEmpty,
                                                                            doNotCreateIfTypeIsEmpty,
                                                                            doNotCreateIfValueIsEmpty));
                _dirty = true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39433");
            }
        }

        /// <summary>
        /// Copies the subattribute. Used to make deep copy.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        private void DeepCopySubattribute(AttributeNameAndTypeAndValue attribute)
        {
            _subattributesToCreate.Add(new AttributeNameAndTypeAndValue(attribute.Name,
                                                                        attribute.TypeOfAttribute,
                                                                        attribute.Value,
                                                                        attribute.NameContainsXPath,
                                                                        attribute.TypeContainsXPath,
                                                                        // https://extract.atlassian.net/browse/ISSUE-16448
                                                                        attribute.ValueContainsXPath,
                                                                        attribute.DoNotCreateIfNameIsEmpty,
                                                                        attribute.DoNotCreateIfTypeIsEmpty,
                                                                        attribute.DoNotCreateIfValueIsEmpty));
        }

        /// <summary>
        /// Duplicates the subattribute.
        /// </summary>
        /// <param name="index">The index of the subattribute to duplicate.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Subattribute")]
        public void DuplicateSubattribute(int index)
        {
            try
            {
                var subattr = GetComponents(index);
                AddSubAttributeComponents(subattr.Name,
                                          subattr.Value,
                                          subattr.TypeOfAttribute,
                                          subattr.NameContainsXPath,
                                          subattr.ValueContainsXPath,
                                          subattr.TypeContainsXPath,
                                          subattr.DoNotCreateIfNameIsEmpty,
                                          subattr.DoNotCreateIfValueIsEmpty,
                                          subattr.DoNotCreateIfTypeIsEmpty);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39498");
            }
        }

        /// <summary>
        /// Updates the subattribute (component set) already in the internal list.
        /// Note that the input is first checked against the current values, and if they match,
        /// then the update is not processed (and _dirty is not set).
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="type">The type.</param>
        /// <param name="nameContainsXPath">true iff name is an xpath expression</param>
        /// <param name="valueContainsXPath">true iff value is an xpath expression</param>
        /// <param name="typeContainsXPath">true iff type is an xpath expression</param>
        /// <param name="doNotCreateIfNameIsEmpty">true or false</param>
        /// <param name="doNotCreateIfValueIsEmpty">true or false</param>
        /// <param name="doNotCreateIfTypeIsEmpty">true or false</param>
        /// <param name="index">Index of the data element to update.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Subattribute")]
        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object,System.Object)")]
        public void UpdateSubattributeComponents(string name,
                                                 string value,
                                                 string type,
                                                 bool nameContainsXPath,
                                                 bool valueContainsXPath,
                                                 bool typeContainsXPath,
                                                 bool doNotCreateIfNameIsEmpty,
                                                 bool doNotCreateIfValueIsEmpty,
                                                 bool doNotCreateIfTypeIsEmpty,
                                                 int index)
        {
            try
            {
                if (index >= _subattributesToCreate.Count)
                {
                    return;
                }

                bool match = _subattributesToCreate[index]
                    .ComponentsAreEquivalent(name,
                                             type,
                                             value,
                                             nameContainsXPath,
                                             typeContainsXPath,
                                             valueContainsXPath,
                                             doNotCreateIfNameIsEmpty,
                                             doNotCreateIfTypeIsEmpty,
                                             doNotCreateIfValueIsEmpty);
                if (match)
                {
                    return;
                }

                _subattributesToCreate[index].Name = name;
                _subattributesToCreate[index].Value = value;
                _subattributesToCreate[index].TypeOfAttribute = type;

                _subattributesToCreate[index].NameContainsXPath = nameContainsXPath;
                _subattributesToCreate[index].TypeContainsXPath = typeContainsXPath;
                _subattributesToCreate[index].ValueContainsXPath = valueContainsXPath;

                _subattributesToCreate[index].DoNotCreateIfNameIsEmpty = doNotCreateIfNameIsEmpty;
                _subattributesToCreate[index].DoNotCreateIfTypeIsEmpty = doNotCreateIfTypeIsEmpty;
                _subattributesToCreate[index].DoNotCreateIfValueIsEmpty = doNotCreateIfValueIsEmpty;

                _dirty = true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40368");
            }
        }

        /// <summary>
        /// Deletes the subattribute components object at the specifed index.
        /// </summary>
        /// <param name="index">The index of the object to remove.</param>
        public void DeleteSubAttributeComponents(int index)
        {
            try
            {
                ExtractException.Assert("ELI39451",
                        String.Format(CultureInfo.InvariantCulture, "Index: {0}, is out-of-range: 0 to {1}",
                                      index,
                                      _subattributesToCreate.Count - 1),
                        index >= 0 && index < _subattributesToCreate.Count);

                _subattributesToCreate.RemoveAt(index);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39459");
            }

            _dirty = true;
        }

        /// <summary>
        /// Swaps the attribute components. For use with the dialog up/down buttons.
        /// </summary>
        /// <param name="index1">The index1.</param>
        /// <param name="index2">The index2.</param>
        public void SwapAttributeComponents(int index1, int index2)
        {
            try
            {
                ExtractException.Assert("ELI39452",
                        String.Format(CultureInfo.InvariantCulture, "Index1: {0}, is out-of-range: 0 to {1}",
                                      index1,
                                      _subattributesToCreate.Count - 1),
                        index1 >= 0 && index1 < _subattributesToCreate.Count);

                ExtractException.Assert("ELI39453",
                                        String.Format(CultureInfo.InvariantCulture, "Index2: {0}, is out-of-range: 0 to {1}",
                                                      index2,
                                                      _subattributesToCreate.Count - 1),
                                        index2 >= 0 && index2 < _subattributesToCreate.Count);

                var tmp = _subattributesToCreate[index1];
                _subattributesToCreate[index1] = _subattributesToCreate[index2];
                _subattributesToCreate[index2] = tmp;

                _dirty = true;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39460");
            }
        }

        /// <summary>
        /// Gets the specified components by index.
        /// </summary>
        /// <param name="index">The index of the component to retrieve.</param>
        /// <returns></returns>
        [SuppressMessage("ExtractRules", "ES0001:PublicMethodsContainTryCatch")]
        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)")]
        public AttributeNameAndTypeAndValue GetComponents(int index)
        {
            try
            {
                ExtractException.Assert("ELI39430",
                                        String.Format("input index: {0}, is out-of-range: 0 to {1}",
                                                      index,
                                                      _subattributesToCreate.Count),
                                        index < _subattributesToCreate.Count);

                return _subattributesToCreate[index];
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39632");
            }
        }

        /// <summary>
        /// Determines whether the Name component of the NameAndValueAndType is valid.
        /// </summary>
        /// <param name="text">The text to validate.</param>
        /// <param name="isXPath">if set to <c>true</c> [x path enabled].</param>
        static internal bool NameIsValid(string text, bool isXPath)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(text))
                {
                    return false;
                }

                if (isXPath)
                {
                    return UtilityMethods.IsValidXPathExpression(text);
                }
                else
                {
                    return UtilityMethods.IsValidIdentifier(text);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46880");
            }
        }

        /// <summary>
        /// Determines whether the Value component of the NameAndValueAndType is valid.
        /// </summary>
        /// <param name="text">The text to validate.</param>
        /// <param name="isXPath">if set to <c>true</c> [x path enabled].</param>
        static internal bool ValueIsValid(string text, bool isXPath)
        {
            try
            {
                if (isXPath)
                {
                    return UtilityMethods.IsValidXPathExpression(text);
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46881");
            }
        }

        /// <summary>
        /// Determines whether the Type component of the NameAndValueAndType is valid.
        /// </summary>
        /// <param name="text">The text to validate.</param>
        /// <param name="isXPath">if set to <c>true</c> [x path enabled].</param>
        static internal bool TypeIsValid(string text, bool isXPath)
        {
            try
            {
                if (isXPath)
                {
                    return UtilityMethods.IsValidXPathExpression(text);
                }
                else if (string.IsNullOrEmpty(text))
                {
                    return true;
                }
                else
                {
                    return text.Split('+').All(id => UtilityMethods.IsValidIdentifier(id));
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46882");
            }
        }

        /// <summary>
        /// Part of the return type of AttributeIsValid(), this enum specifies the failed component
        /// of the attribute, for advanced error handling. Note that the first invalid component is 
        /// flagged - no attempt is made to deal with multiple validation errors.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1034:DoNotNestPublicEnum")]        
        public enum AttributeComponentId
        {
            /// <summary>
            /// Used when the attribute components are all valid
            /// </summary>
            NotApplicable,

            /// <summary>
            /// Used when the name portion of the attribute is invalid
            /// </summary>
            Name,

            /// <summary>
            /// Used when the value portion of the attribute is invalid
            /// </summary>
            Value,

            /// <summary>
            /// Used when the type portion of the attribute is invalid
            /// </summary>
            Type        
        }

        /// <summary>
        /// Is the attribute valid?
        /// </summary>
        /// <param name="index">index of the specified attribute</param>
        /// <returns>Tuple of bool, AttributeComponentId. .Item1 = true if the specified attribute is valid (complete).
        /// Item2 is set to NotApplicable if Item1 is true (attribute is valid), otherwise it is set to 
        /// indicate the component of the attribute that is at fault.
        /// </returns>
        [SuppressMessage("ExtractRules", "ES0001:PublicMethodsContainTryCatch")]
        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)")]
        public Tuple<bool, AttributeComponentId> AttributeIsValid(int index)
        {
            try
            {
                ExtractException.Assert("ELI39553",
                                        String.Format("input index: {0}, is out-of-range: 0 to {1}",
                                                      index,
                                                      _subattributesToCreate.Count),
                                        index < _subattributesToCreate.Count);

                var subAttr = _subattributesToCreate[index];

                bool bName = NameIsValid(subAttr.Name, subAttr.NameContainsXPath);
                bool bValue = ValueIsValid(subAttr.Value, subAttr.ValueContainsXPath);
                bool bType = TypeIsValid(subAttr.TypeOfAttribute, subAttr.TypeContainsXPath);
                bool bRet = bName && bValue && bType;

                AttributeComponentId aci = new AttributeComponentId();
                if (false == bRet)
                {
                    if (!bName)
                    {
                        aci = AttributeComponentId.Name;
                    }
                    else if (!bValue)
                    {
                        aci = AttributeComponentId.Value;
                    }
                    else if (!bType)
                    {
                        aci = AttributeComponentId.Type;
                    }
                }
                else
                {
                    aci = AttributeComponentId.NotApplicable;
                }

                return new Tuple<bool, AttributeComponentId>(bRet, aci);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39633");
            }
        }

        /// <summary>
        /// Determines if the states of all of the defined subattributes, and the _root, are valid.
        /// </summary>
        /// <returns>true iff all states are valid</returns>
        public bool StateIsValid()
        {
            try
            {
                var stateIsValid = UtilityMethods.IsValidXPathExpression(_root);
                for (int i = 0; i < SubAttributeComponentCount; ++i)
                {
                    if (!AttributeIsValid(i).Item1)
                    {
                        stateIsValid = false;
                    }
                }

                return stateIsValid;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39630");
            }
        }

        /// <summary>
        /// Saves this object to a YAML file
        /// </summary>
        /// <param name="fileName">The file to save to</param>
        public void LoadFromYAML(string fileName)
        {
            try
            {
                DeserializerBuilder db = new DeserializerBuilder();
                db.WithTagMapping("!CreateAttribute", typeof(CreateAttribute));
                var deserializer = db.Build();
                using (var reader = new StreamReader(fileName))
                {
                    var loaded = deserializer.Deserialize<CreateAttribute>(reader);
                    CopyFrom(loaded);
                }

                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46885");
            }
        }

        /// <summary>
        /// Saves this object to a YAML file
        /// </summary>
        /// <param name="fileName">The file to save to</param>
        public void SaveToYAML(string fileName)
        {
            try
            {
                var sb = new SerializerBuilder();
                sb.EmitDefaults();
                sb.EnsureRoundtrip();
                sb.WithTagMapping("!CreateAttribute", typeof(CreateAttribute));
                var serializer = sb.Build();
                using (var stream = new StreamWriter(fileName))
                {
                    serializer.Serialize(stream, this);
                }
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46886");
            }
        }

        #endregion Public Methods

        #region IOutputHandler

        /// <summary>
        /// helper class to simplify evaluation of Name, Type, and Value xpath or literal text expressions.
        /// </summary>
        class Evaluator
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Evaluator"/> class.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="iter">The iter.</param>
            public Evaluator(XPathContext context, XPathContext.XPathIterator iter)
            {
                _xpathContext = context;
                _iter = iter;
            }

            /// <summary>
            /// Evaluates the name or type query. Not for use with the Value query, which is more complex.
            /// </summary>
            /// <param name="query">The query.</param>
            /// <param name="containsXPath">if set to <c>true</c> iff contains xpath.</param>
            /// <param name="doNotCreateIfEmpty">if set to <c>true</c> [do not create if empty].</param>
            /// <param name="evalForSpatialString">if true, evaulate to result in a spatial string, if false,
            /// evaluate as a text string</param>
            public void EvaluateQuery(string query,
                                      bool containsXPath,
                                      bool doNotCreateIfEmpty,
                                      bool evalForSpatialString = false)
            {
                try
                {
                    Reset();

                    if (!containsXPath)
                    {
                        QueryResult = query;
                        return;
                    }

                    var result = _xpathContext.Evaluate(_iter, query);
                    if (null == result)
                    {
                        Failed = doNotCreateIfEmpty;
                        return;
                    }

                    var objectList = result as List<object>;
                    var attrResult = objectList != null ? objectList.OfType<IAttribute>().ToList() : null;
                    if (null == attrResult)
                    {
                        QueryResult = result.ToString();
                        Failed = doNotCreateIfEmpty && string.IsNullOrEmpty(QueryResult);
                        return;
                    }

                    if (attrResult.Count == 0)
                    {
                        if (objectList.Count != 0)
                        {
                            QueryResult = objectList[0].ToString();
                        }

                        Failed = doNotCreateIfEmpty && string.IsNullOrEmpty(QueryResult);
                        return;
                    }

                    var tempAttr = ((ICopyableObject)attrResult[0].Value).Clone();
                    EvaluatedSpatialString = (SpatialString)tempAttr;
                    if (evalForSpatialString)
                    {
                        Failed = String.IsNullOrWhiteSpace(EvaluatedSpatialString.String) && doNotCreateIfEmpty;
                        IsSpatialString = true;
                    }
                    else
                    {
                        QueryResult = EvaluatedSpatialString.String;
                        Failed = String.IsNullOrWhiteSpace(QueryResult) && doNotCreateIfEmpty;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI39462");
                }
            }

            /// <summary>
            /// Resets the properties (which get re-used repeatedly) of this instance.
            /// </summary>
            void Reset()
            {
                Failed = false;
                IsSpatialString = false;
                EvaluatedSpatialString = null;
                QueryResult = String.Empty;
            }

            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="Evaluator"/> is failed.
            /// In this context, failed means that a subattribute should not be created, i.e. fail 
            /// the operation.
            /// </summary>
            /// <value>
            ///   <c>true</c> if failed; otherwise, <c>false</c>.
            /// </value>
            public bool Failed { get; set; }

            /// <summary>
            /// Gets or sets the query result iff it si a string value; otherwise the value is in
            /// QueryResultAsSpatialString;
            /// </summary>
            /// <value>
            /// The query result.
            /// </value>
            public string QueryResult { get; set; }

            public SpatialString EvaluatedSpatialString { get; set; }

            public bool IsSpatialString { get; set; }

            XPathContext _xpathContext;
            XPathContext.XPathIterator _iter;
        }

        /// <summary>
        /// Processes the output <see paramref="pAttributes"/>.
        /// </summary>
        /// <param name="pAttributes">The <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s
        /// on which this task is to be run.</param>
        /// <param name="pDoc">The <see cref="AFDocument"/> the attributes are associated with.
        /// </param>
        /// <param name="pProgressStatus">The <see cref="ProgressStatus"/> displaying the progress.
        /// </param>
        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)")]
        public void ProcessOutput(IUnknownVector pAttributes, AFDocument pDoc, ProgressStatus pProgressStatus)
        {
            try
            {
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI39404", _COMPONENT_DESCRIPTION);

                List<ExtractException> exceptions = new List<ExtractException>();

                var xPathContext = new XPathContext(pAttributes);
                var iter = xPathContext.GetIterator(_root);
                Evaluator eval = new Evaluator(xPathContext, iter);

                while (iter.MoveNext())
                {
                    IUnknownVector vectorToAddTo = null;
                    if (iter.IsAtRootElement)
                    {
                        vectorToAddTo = pAttributes;
                    }
                    else
                    {
                        vectorToAddTo = iter.CurrentAttribute?.SubAttributes;

                        ExtractException.Assert("ELI39681",
                            "No Attribute was found for the XPath expression",
                            vectorToAddTo != null,
                            "PossibleCause",
                            String.Format(CultureInfo.CurrentCulture,
                                "XPath root query: {0}",
                                _root));
                    }

                    foreach (var subAttr in _subattributesToCreate)
                    {
                        IAttribute newAttr = new AttributeClass();

                        eval.EvaluateQuery(subAttr.Name, subAttr.NameContainsXPath, subAttr.DoNotCreateIfNameIsEmpty);
                        if (eval.Failed)
                        {
                            continue;
                        }

                        try
                        {
                            newAttr.Name = eval.QueryResult;
                        }
                        catch (Exception ex)
                        {
                            var ee = new ExtractException("ELI39464", "Failed to set subattribute name", ex);
                            ee.AddDebugData("Name", eval.QueryResult, encrypt: false);
                            ee.AddDebugData("Reason", ex.AsExtract("").Message, false);
                            exceptions.Add(ee);
                            continue;
                        }

                        eval.EvaluateQuery(subAttr.Value, 
                                           subAttr.ValueContainsXPath, 
                                           subAttr.DoNotCreateIfValueIsEmpty, 
                                           evalForSpatialString: true);
                        if (eval.Failed)
                        {
                            continue;
                        }

                        if (eval.IsSpatialString)
                        {
                            newAttr.Value = eval.EvaluatedSpatialString;
                        }
                        else
                        {
                            newAttr.Value.ReplaceAndDowngradeToNonSpatial(eval.QueryResult);
                        }

                        eval.EvaluateQuery(subAttr.TypeOfAttribute, 
                                           subAttr.TypeContainsXPath, 
                                           subAttr.DoNotCreateIfTypeIsEmpty);
                        if (eval.Failed)
                        {
                            continue;
                        }

                        try
                        {
                            newAttr.Type = eval.QueryResult;
                        }
                        catch (Exception ex)
                        {
                            var ee = new ExtractException("ELI39625", "Failed to set subattribute Type", ex);
                            ee.AddDebugData("TypeName", eval.QueryResult, encrypt: false);
                            ee.AddDebugData("Reason", ex.AsExtract("").Message, false);
                            exceptions.Add(ee);
                            continue;
                        }

                        vectorToAddTo.PushBack(newAttr);
                    }
                }

                // Report memory usage of hierarchy after processing to ensure all COM objects
                // referenced in final result are reported.
                pAttributes.ReportMemoryUsage();

                if (exceptions.Count > 0)
                {
                    throw ExtractException.AsAggregateException(exceptions);
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI39405", "Create attribute task failed.");
            }
        }

        #endregion IOutputHandler

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="CreateAttribute"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI39406", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                CreateAttribute cloneOfThis = (CreateAttribute)Clone();

                using (CreateAttributeSettingsDialog dlg = new CreateAttributeSettingsDialog(cloneOfThis))
                {
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        CopyFrom(dlg.Settings);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI39407", "Error running configuration.");
            }
        }

        #endregion IConfigurableObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="CreateAttribute"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="CreateAttribute"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new CreateAttribute(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI39401",
                    "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="CreateAttribute"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as CreateAttribute;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to " + _COMPONENT_DESCRIPTION);
                }

                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI39402", "Failed to copy '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        #endregion ICopyableObject Members

        #region ICategorizedComponent Members

        /// <summary>
        /// Gets the name of the COM object.
        /// </summary>
        /// <returns>The name of the COM object.</returns>
        public string GetComponentDescription()
        {
            return _COMPONENT_DESCRIPTION;
        }

        #endregion ICategorizedComponent Members

        #region ILicensedComponent Members

        /// <summary>
        /// Gets whether this component is licensed.
        /// </summary>
        /// <returns><see langword="true"/> if the component is licensed; <see langword="false"/>
        /// if the component is not licensed.</returns>
        public bool IsLicensed()
        {
            return LicenseUtilities.IsLicensed(_LICENSE_ID);
        }

        #endregion ILicensedComponent Members

        #region IPersistStream Members

        /// <summary>
        /// Returns the class identifier (CLSID) <see cref="Guid"/> for the component object.
        /// </summary>
        /// <param name="classID">Pointer to the location of the CLSID <see cref="Guid"/> on 
        /// return.</param>
        public void GetClassID(out Guid classID)
        {
            classID = GetType().GUID;
        }

        /// <summary>
        /// Checks the object for changes since it was last saved.
        /// </summary>
        /// <returns><see cref="HResult.Ok"/> if changes have been made;
        /// <see cref="HResult.False"/> if changes have not been made.
        /// </returns>
        public int IsDirty()
        {
            return HResult.FromBoolean(_dirty);
        }

        /// <summary>
        /// Initializes an object from the IStream where it was previously saved.
        /// </summary>
        /// <param name="stream">IStream from which the object should be loaded.</param>
        public void Load(System.Runtime.InteropServices.ComTypes.IStream stream)
        {
            try
            {
                if (null == _subattributesToCreate)
                {
                    _subattributesToCreate = new List<AttributeNameAndTypeAndValue>();
                }

                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    _root = reader.ReadString();

                    int numberOfSubAttrs = (int)reader.ReadInt32();
                    for (int i = 0; i < numberOfSubAttrs; ++i)
                    {
                        var antv = new AttributeNameAndTypeAndValue();
                        antv.ReadSubAttributeComponents(reader);
                        _subattributesToCreate.Add(antv);
                    }

                    // Load the GUID for the IIdentifiableObject interface.
                    LoadGuid(stream);
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI39403",
                    "Failed to load '" + _COMPONENT_DESCRIPTION + "'.");
            }
        }

        /// <summary>
        /// Saves an object into the specified IStream and indicates whether the object should reset
        /// its dirty flag.
        /// </summary>
        /// <param name="stream">IStream into which the object should be saved.</param>
        /// <param name="clearDirty">Value that indicates whether to clear the dirty flag after the
        /// save is complete. If <see langword="true"/>, the flag should be cleared. If
        /// <see langword="false"/>, the flag should be left unchanged.</param>
        public void Save(System.Runtime.InteropServices.ComTypes.IStream stream, bool clearDirty)
        {
            try
            {
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    writer.Write(_root);

                    writer.Write(_subattributesToCreate.Count);

                    // now write each subattr...
                    foreach (var subattr in _subattributesToCreate)
                    {
                        subattr.WriteSubAttributeComponents(writer);                    
                    }

                    // Write to the provided IStream.
                    writer.WriteTo(stream);
                }

                // Save the GUID for the IIdentifiableObject interface.
                SaveGuid(stream);

                if (clearDirty)
                {
                    _dirty = false;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI39408",
                    "Failed to save '" + _COMPONENT_DESCRIPTION + "'.");
            }
        }

        /// <summary>
        /// Returns the size in bytes of the stream needed to save the object.
        /// </summary>
        /// <param name="size">Pointer to a 64-bit unsigned integer value indicating the size, in
        /// bytes, of the stream needed to save this object.</param>
        public void GetSizeMax(out long size)
        {
            throw new NotImplementedException();
        }

        #endregion IPersistStream Members

        #region Private Members

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// appropriate COM categories.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.OutputHandlersGuid);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// appropriate COM categories.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.OutputHandlersGuid);
        }

        /// <summary>
        /// Copies the specified <see cref="CreateAttribute"/> instance into this one.
        /// </summary><param name="source">The <see cref="CreateAttribute"/> from which to copy.
        /// </param>
        void CopyFrom(CreateAttribute source)
        {
            _root = source._root;

            _subattributesToCreate = new List<AttributeNameAndTypeAndValue>();
            foreach (var attr in source._subattributesToCreate)
            {
                DeepCopySubattribute(attr);
            }

            _dirty = true;
        }

        #endregion Private Members
    }


    /// <summary>
    /// This class is used to contain a set of user-defined subattributes to create.
    /// </summary>
    public class AttributeNameAndTypeAndValue
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string TypeOfAttribute { get; set; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether name contains an xpath expression.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [name contains x path]; otherwise, <c>false</c>.
        /// </value>
        public bool NameContainsXPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether value contains an xpath expression.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [value contains x path]; otherwise, <c>false</c>.
        /// </value>
        public bool ValueContainsXPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether type contains an xpath expression.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [type contains x path]; otherwise, <c>false</c>.
        /// </value>
        public bool TypeContainsXPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to create the attribute if name is empty.
        /// </summary>
        /// <value>
        /// <c>true</c> if [do not create if name is empty]; otherwise, <c>false</c>.
        /// </value>
        public bool DoNotCreateIfNameIsEmpty { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to create the attribute if value is empty.
        /// </summary>
        /// <value>
        /// <c>true</c> if [do not create if value is empty]; otherwise, <c>false</c>.
        /// </value>
        public bool DoNotCreateIfValueIsEmpty { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to create the attribute if type is empty.
        /// </summary>
        /// <value>
        /// <c>true</c> if [do not creaet if type is empty]; otherwise, <c>false</c>.
        /// </value>
        public bool DoNotCreateIfTypeIsEmpty { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeNameAndTypeAndValue"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <param name="nameContainsXPath">true iff name is an xpath expression</param>
        /// <param name="valueContainsXPath">true iff value is an xpath expression</param>
        /// <param name="typeContainsXPath">true iff type is an xpath expression</param>
        /// <param name="doNotCreateIfNameIsEmpty">true or false</param>
        /// <param name="doNotCreateIfTypeIsEmpty">true or false</param>
        /// <param name="doNotCreateIfValueIsEmpty">true or false</param>
        public AttributeNameAndTypeAndValue(string name, 
                                            string type, 
                                            string value, 
                                            bool nameContainsXPath,
                                            bool typeContainsXPath,
                                            bool valueContainsXPath,
                                            bool doNotCreateIfNameIsEmpty,
                                            bool doNotCreateIfTypeIsEmpty,
                                            bool doNotCreateIfValueIsEmpty)
        {
            ExtractException.Assert("ELI39431", "Name argument is null", null != name);
            ExtractException.Assert("ELI39432", "type argument is null", null != type);
            ExtractException.Assert("ELI39427", "value argument is null", null != value);

            Name = name;
            TypeOfAttribute = type;
            Value = value;

            NameContainsXPath = nameContainsXPath;
            ValueContainsXPath = valueContainsXPath;
            TypeContainsXPath = typeContainsXPath;

            DoNotCreateIfNameIsEmpty = doNotCreateIfNameIsEmpty;
            DoNotCreateIfTypeIsEmpty = doNotCreateIfTypeIsEmpty;
            DoNotCreateIfValueIsEmpty = doNotCreateIfValueIsEmpty;
        }

        /// <summary>
        /// Initializes a new empty instance of the <see cref="AttributeNameAndTypeAndValue"/> class.
        /// </summary>
        public AttributeNameAndTypeAndValue()
        {
        }

        /// <summary>
        /// Writes the subattribute components.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void WriteSubAttributeComponents(IStreamWriter writer)
        {
            try
            {
                writer.Write(Name);
                writer.Write(Value);
                writer.Write(TypeOfAttribute);

                writer.Write(NameContainsXPath);
                writer.Write(ValueContainsXPath);
                writer.Write(TypeContainsXPath);

                writer.Write(DoNotCreateIfNameIsEmpty);
                writer.Write(DoNotCreateIfValueIsEmpty);
                writer.Write(DoNotCreateIfTypeIsEmpty);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39458");
            }
        }
        
        /// <summary>
        /// Reads the sub attribute components (intended to be used with a default constructed instance of this class).
        /// </summary>
        /// <param name="reader">The reader.</param>
        public void ReadSubAttributeComponents(IStreamReader reader)
        {
            try
            {
                Name = reader.ReadString();
                Value = reader.ReadString();
                TypeOfAttribute = reader.ReadString();

                NameContainsXPath = reader.ReadBoolean();
                ValueContainsXPath = reader.ReadBoolean();
                TypeContainsXPath = reader.ReadBoolean();

                DoNotCreateIfNameIsEmpty = reader.ReadBoolean();
                DoNotCreateIfValueIsEmpty = reader.ReadBoolean();
                DoNotCreateIfTypeIsEmpty = reader.ReadBoolean();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39457");
            }
        }

        /// <summary>
        /// Test whether components are equivalent to this instance (to aid determination of _dirty flag)
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <param name="nameContainsXPath">if set to <c>true</c> [name contains x path].</param>
        /// <param name="typeContainsXPath">if set to <c>true</c> [type contains x path].</param>
        /// <param name="valueContainsXPath">if set to <c>true</c> [value contains x path].</param>
        /// <param name="doNotCreateIfNameIsEmpty">true or false</param>
        /// <param name="doNotCreateIfTypeIsEmpty">true or false</param>
        /// <param name="doNotCreateIfValueIsEmpty">true or false</param>
        /// <returns></returns>
        [SuppressMessage("ExtractRules", "ES0001:PublicMethodsContainTryCatch")]
        public bool ComponentsAreEquivalent(string name, 
                                            string type, 
                                            string value, 
                                            bool nameContainsXPath,
                                            bool typeContainsXPath,
                                            bool valueContainsXPath,
                                            bool doNotCreateIfNameIsEmpty,
                                            bool doNotCreateIfTypeIsEmpty,
                                            bool doNotCreateIfValueIsEmpty)
        {
            if (name != Name ||
                type != TypeOfAttribute ||
                value != Value ||
                nameContainsXPath != NameContainsXPath ||
                typeContainsXPath != TypeContainsXPath ||
                valueContainsXPath != ValueContainsXPath ||
                doNotCreateIfNameIsEmpty != DoNotCreateIfNameIsEmpty ||
                doNotCreateIfTypeIsEmpty != DoNotCreateIfTypeIsEmpty ||
                doNotCreateIfValueIsEmpty != DoNotCreateIfValueIsEmpty)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Tests the spcified character to determine if it is an allowable xpath character.
        /// </summary>
        /// <param name="character">The character to test</param>
        /// <returns>true if the specified character is an xpath character.</returns>
        [SuppressMessage("ExtractRules", "ES0001:PublicMethodsContainTryCatch")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Xpath")]
        static public bool AllowableXpathChar(char character)
        {
            return character == '.' || character == '/' || character == '*' || character == '@';
        }
    }
} 