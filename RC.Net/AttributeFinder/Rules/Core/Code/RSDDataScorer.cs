using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using Spring.Expressions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;

using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// An interface for the <see cref="RSDDataScorer"/> class.
    /// </summary>
    [ComVisible(true)]
    [Guid("482709B1-DD80-4AB0-8AD7-7F35FCC223BD")]
    [CLSCompliant(false)]
    public interface IRSDDataScorer : IDataScorer, ICategorizedComponent,
        IConfigurableObject, ICopyableObject, ILicensedComponent, IPersistStream,
        IMustBeConfiguredObject, IIdentifiableRuleObject
    {
        /// <summary>
        /// Gets or sets the name of the RSD file used to generate the score.
        /// </summary>
        /// <value>
        /// The name of the RSD file used to generate the score.
        /// </value>
        string RSDFileName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the expression used to generate the score from the output of the RSD file.
        /// </summary>
        /// <value>
        /// The expression used to generate the score from the output of the RSD file.
        /// </value>
        string ScoreExpression
        {
            get;
            set;
        }
    }

    /// <summary>
    /// An <see cref="IDataScorer"/> instance that allows a score to be generated based on the
    /// results of an attribute value run against an RSD file.
    /// </summary>
    [ComVisible(true)]
    [Guid("BC49A13B-A88D-4853-B2D2-D1C6A3345379")]
    [CLSCompliant(false)]
    public class RSDDataScorer : IdentifiableRuleObject, IRSDDataScorer
    {
        #region Constants

        /// <summary>
        /// The description of the rule
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "RSD data scorer";

        /// <summary>
        /// Current version.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.FlexIndexIDShieldCoreObjects;

        /// <summary>
        /// Expression variables that reference string values will be suffixed with this.
        /// </summary>
        const string _STRING_VALUE = "__STRING__";

        #endregion Constants

        #region Fields

        /// <summary>
        /// An <see cref="FileActionManagerPathTags"/> instance with the AreaID tag added to expand
        /// the output filename.
        /// </summary>
        AttributeFinderPathTags _pathTags = new AttributeFinderPathTags();

        /// <value>
        /// The name of the RSD file used to generate the score.
        /// </value>
        string _rsdFileName;

        /// <summary>
        /// The expression used to generate the score from the output of the RSD file.
        /// </summary>
        string _scoreExpression;

        /// <summary>
        /// A modified version of <see cref="_scoreExpression"/> where variables prefixed with "$"
        /// to reference string values have been replaced with a spring framework expression
        /// compatible version prefixed with "#"
        /// </summary>
        string _preparedExpression;

        /// <summary>
        /// The variable names used in the expression.
        /// </summary>
        string[] _variableNames = new string[0];

        /// <summary>
        /// <see langword="true"/> if changes have been made to <see cref="RSDDataScorer"/>
        /// since it was created; <see langword="false"/> if no changes have been made since it was
        /// created.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RSDDataScorer"/> class.
        /// </summary>
        public RSDDataScorer()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33827");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RSDDataScorer"/> class as a copy of the
        /// specified <see paramref="source"/>.
        /// </summary>
        /// <param name="source">The <see cref="RSDDataScorer"/> from which settings should be
        /// copied.</param>
        public RSDDataScorer(RSDDataScorer source)
        {
            try
            {
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33828");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the name of the RSD file used to generate the score.
        /// </summary>
        /// <value>
        /// The name of the RSD file used to generate the score.
        /// </value>
        public string RSDFileName
        {
            get
            {
                return _rsdFileName;
            }

            set
            {
                try
                {
                    if (value != _rsdFileName)
                    {
                        _rsdFileName = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI33841");
                }
            }
        }

        /// <summary>
        /// Gets or sets the expression used to generate the score from the output of the RSD file.
        /// </summary>
        /// <value>
        /// The expression used to generate the score from the output of the RSD file.
        /// </value>
        public string ScoreExpression
        {
            get
            {
                return _scoreExpression;
            }

            set
            {
                try
                {
                    if (value != _scoreExpression)
                    {
                        PrepareExpression(value);

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI33842");
                }
            }
        }

        #endregion Properties

        #region IDataScorer Members

        /// <summary>
        /// Gets the data score for <see paramref="pAttribute"/>.
        /// </summary>
        /// <param name="pAttribute">The <see cref="UCLID_AFCORELib.Attribute"/> to score.</param>
        /// <returns>The score as an <see langword="int"/>.</returns>
        public int GetDataScore1(UCLID_AFCORELib.Attribute pAttribute)
        {
            try 
	        {
                RuleSet ruleSet = LoadRuleSet(pAttribute);

                IEnumerable<ComAttribute> foundAttributes = GetFoundAttributes(ruleSet, pAttribute);

                // Create the dictionary used to store the expression variables.
                Dictionary<string, object> variables = InitializeVariables();

                // Use the found attributes to populate the dictionary of expression variables.
                PopulateExpressionVariables(foundAttributes, ref variables);

                // Evaluate the expression, then convert the result to an int.
                object result = ExpressionEvaluator.GetValue(null, _preparedExpression, variables);
                int score = Convert.ToInt32(result, CultureInfo.CurrentCulture);

                return score;
	        }
	        catch (Exception ex)
	        {
                ExtractException ee = ex.AsExtract("ELI33857");
                ee.AddDebugData("RuleSet", _rsdFileName, true);
                ee.AddDebugData("Score expression", ScoreExpression, true);

                throw ee.CreateComVisible("ELI33844", _COMPONENT_DESCRIPTION + " failed.");
	        }
        }

        /// <summary>
        /// Gets the data score for <see paramref="pAttributes"/>.
        /// </summary>
        /// <param name="pAttributes">The <see cref="IUnknownVector"/> of
        /// <see cref="UCLID_AFCORELib.Attribute"/>s to score.</param>
        /// <returns>The score as an <see langword="int"/>.</returns>
        public int GetDataScore2(IUnknownVector pAttributes)
        {
            try
            {
                // Create the dictionary used to store the expression variables.
                Dictionary<string, object> variables = InitializeVariables();

                IEnumerable<ComAttribute> sourceAttributes = pAttributes.ToIEnumerable<ComAttribute>();
                if (sourceAttributes.Any())
                {
                    RuleSet ruleSet = LoadRuleSet(sourceAttributes.First());

                    List<ComAttribute> foundAttributes = new List<ComAttribute>();

                    foreach (ComAttribute attribute in sourceAttributes)
                    {
                        foundAttributes.AddRange(GetFoundAttributes(ruleSet, attribute));
                    }

                    // Use the found attributes to populate the dictionary of expression variables.
                    PopulateExpressionVariables(foundAttributes, ref variables);
                }

                // Evaluate the expression, then convert the result to an int.
                object result = ExpressionEvaluator.GetValue(null, _preparedExpression, variables);
                int score = Convert.ToInt32(result, CultureInfo.CurrentCulture);

                return score;
            }
            catch (Exception ex)
            {
                ExtractException ee = ex.AsExtract("ELI33858");
                ee.AddDebugData("RuleSet", _rsdFileName, true);
                ee.AddDebugData("Score expression", ScoreExpression, true);

                throw ex.CreateComVisible("ELI33843", _COMPONENT_DESCRIPTION + " failed.");
            }
        }

        #endregion IDataScorer Members

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="RSDDataScorer"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI33829", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                RSDDataScorer cloneOfThis = (RSDDataScorer)Clone();

                using (RSDDataScorerSettingsDialog dlg
                    = new RSDDataScorerSettingsDialog(cloneOfThis))
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
                throw ex.CreateComVisible("ELI33830", "Error running configuration.");
            }
        }

        #endregion IConfigurableObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="RSDDataScorer"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="RSDDataScorer"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new RSDDataScorer(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33831",
                    "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="RSDDataScorer"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as RSDDataScorer;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to RSDDataScorer");
                }
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33832",
                    "Failed to copy '" + _COMPONENT_DESCRIPTION + "' object.");
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
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    _rsdFileName = reader.ReadString();
                    _scoreExpression = reader.ReadString();
                    _preparedExpression = reader.ReadString();
                    _variableNames = reader.ReadStringArray();

                    // Load the GUID for the IIdentifiableRuleObject interface.
                    LoadGuid(stream);
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33833",
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
                    writer.Write(_rsdFileName);
                    writer.Write(_scoreExpression);
                    writer.Write(_preparedExpression);
                    writer.Write(_variableNames);

                    // Write to the provided IStream.
                    writer.WriteTo(stream);
                }

                // Save the GUID for the IIdentifiableRuleObject interface.
                SaveGuid(stream);

                if (clearDirty)
                {
                    _dirty = false;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33834",
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

        #region IMustBeConfiguredObject Members

        /// <summary>
        /// Determines whether this instance is configured.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if this instance is configured; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool IsConfigured()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_rsdFileName) ||
                    string.IsNullOrWhiteSpace(_scoreExpression) ||
                    string.IsNullOrWhiteSpace(_preparedExpression))
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33835",
                    "Error checking configuration of RSD data scorer.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region Private Members

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// "UCLID AF-API Data Scorers" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.DataScorersGuid);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// "UCLID AF-API Data Scorers" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.DataScorersGuid);
        }

        /// <summary>
        /// Copies the specified <see cref="RSDDataScorer"/> instance into this one.
        /// </summary><param name="source">The <see cref="RSDDataScorer"/> from which
        /// to copy.</param>
        void CopyFrom(RSDDataScorer source)
        {
            _rsdFileName = source._rsdFileName;
            _scoreExpression = source._scoreExpression;
            _preparedExpression = source._preparedExpression;
            _variableNames = new string[source._variableNames.Length];
            source._variableNames.CopyTo(_variableNames, 0);

            _dirty = true;
        }

        /// <summary>
        /// Loads the rule set configured to be run.
        /// </summary>
        /// <param name="pAttribute">A <see cref="ComAttribute"/> to provide context for expanding
        /// path tags.</param>
        /// <returns>The <see cref="RuleSet"/>.</returns>
        RuleSet LoadRuleSet(UCLID_AFCORELib.Attribute pAttribute)
        {
            AFDocument afDoc = new AFDocument();
            afDoc.Text = pAttribute.Value;

            _pathTags.Document = afDoc;
            string rsdFileName = _pathTags.Expand(_rsdFileName);

            RuleSet ruleSet = new RuleSet();
            ruleSet.LoadFrom(rsdFileName, false);

            return ruleSet;
        }

        /// <summary>
        /// Prepares the expression by extracting all variable names used in the expression and
        /// replacing variables prefixed with "$" to reference string values with a spring
        /// framework expression compatible version prefixed with "#"
        /// </summary>
        /// <param name="expression">The expression to prepare.</param>
        void PrepareExpression(string expression)
        {
            _scoreExpression = expression;
            _preparedExpression = _scoreExpression;

            // A regex to find all variables in the expression.
            Regex variableIdentifier = new Regex(@"(?<=#|\$)[_a-zA-Z]\w*(?=[\W])");

            // Use the regex to identify all variable names that are not special spring framework
            // variables
            _variableNames = variableIdentifier.Matches(_scoreExpression)
                .Cast<Match>()
                .Select(match => match.Value)
                .Where(variableName =>
                    !variableName.Equals("this", StringComparison.OrdinalIgnoreCase) &&
                    !variableName.Equals("root", StringComparison.OrdinalIgnoreCase))
                .Distinct()
                .ToArray();

            // Any of the variables that used "$" to reference the string version of the variable
            // should be replaced with a "#" version that will be recognized by the expression
            // evaluation engine.
            // [FlexIDSCore:5002]
            // Replaces variables in reverse order of length so that if one variable name is part
            // of another variable name, the shorter variable name doesn't get replaced where the
            // longer one should have been.
            foreach (string variableName in _variableNames
                .OrderBy(name => name.Length)
                .Reverse())
            {
                _preparedExpression = _preparedExpression.Replace("$" + variableName, 
                    "#" + variableName + _STRING_VALUE);
            }
        }

        /// <summary>
        /// Runs the specified <see paramref="ruleSet"/> against all attribute names referenced in
        /// the <see cref="ScoreExpression"/>.
        /// </summary>
        /// <param name="ruleSet">The <see cref="RuleSet"/> to run.</param>
        /// <param name="attribute">The <see cref="ComAttribute"/> whose value should be processed.
        /// </param>
        /// <returns>The <see cref="ComAttribute"/>s found by the <see paramref="ruleSet"/>.</returns>
        static IEnumerable<ComAttribute> GetFoundAttributes(RuleSet ruleSet, ComAttribute attribute)
        {
            // Use ComObjectReleaser on the RuleExecutionSession so that the current RSD file name is
            // handled correctly.
            using (ComObjectReleaser ComObjectReleaser = new ComObjectReleaser())
            {
                RuleExecutionSession session = new RuleExecutionSession();
                ComObjectReleaser.ManageObjects(session);
                session.SetRSDFileName(ruleSet.FileName);

                // Turn the supplied attribute into an AFDocument to run the rules against.
                AFDocument afDoc = new AFDocument();
                afDoc.Text = attribute.Value;

                IUnknownVector foundAttributes = ruleSet.ExecuteRulesOnText(afDoc, null, null);

                return foundAttributes.ToIEnumerable<ComAttribute>();
            }
        }

        /// <summary>
        /// Initializes the expression variables dictionary by adding 2 keys for every variable
        /// name; the first value is a list of <see langword="int"/>s and the second is a list of
        /// <see langword="string"/>s cast where the key has been suffixed to indicate it references
        /// strings.
        /// </summary>
        /// <returns>The initialized <see cref="Dictionary{T,T}"/>.</returns>
        Dictionary<string, object> InitializeVariables()
        {
            Dictionary<string, object> variables = new Dictionary<string, object>();
            foreach (string variableName in _variableNames)
            {
                variables[variableName] = new List<int>();
                variables[variableName + _STRING_VALUE] = new List<string>();
            }
            return variables;
        }

        /// <summary>
        /// Uses the <see paramref="attributes"/> to populate the
        /// <see paramref="expressionVariables"/> dictionary.
        /// </summary>
        /// <param name="attributes">The <see cref="ComAttribute"/>s whose values should be used to
        /// populate the variables.</param>
        /// <param name="expressionVariables">A dictionary that contains 2 entries for every
        /// variables name; the first value is the <see langword="int"/> cast of all attributes of
        /// the corresponding name; the second is the <see langword="string"/> cast where the name
        /// has been suffixed to indicate it references strings.</param>
        /// <returns></returns>
        static void PopulateExpressionVariables(IEnumerable<ComAttribute> attributes,
            ref Dictionary<string, object> expressionVariables)
        {
            foreach (ComAttribute attribute in attributes)
            {
                string name = attribute.Name;
                string value = attribute.Value.String;

                // expressionVariables will always contain keys in pairs-- one key for the numeric
                // value and one for the string value. We only need to check for one.
                if (expressionVariables.ContainsKey(name))
                {
                    // Add the string value.
                    ((List<string>)expressionVariables[name + _STRING_VALUE]).Add(value);

                    // If the value cannot be interpreted as an integer, use and int value of zero.
                    int intValue;
                    if (!Int32.TryParse(value, out intValue))
                    {
                        intValue = 0;
                    }

                    // Add the numeric value.
                    ((List<int>)expressionVariables[name]).Add(intValue);
                }
            }
        }

        #endregion Private Members
    }
}
