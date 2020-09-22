using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UCLID_AFCORELib;
using Newtonsoft.Json;

namespace Extract.AttributeFinder.Rules.Json
{
    /// <summary>
    /// Class used to load/save rulesets to JSON and check dirty state
    /// </summary>
    [ComVisible(true)]
    [Guid("7D9D3356-415F-48CC-AAF3-15596ACFB6A7")]
    [ProgId("Extract.AttributeFinder.Rules.Json.RuleSetJsonSerializer")]
    public class RuleSetJsonSerializer : IRuleSetSerializer
    {
        #region Fields

        static readonly Dto.RuleSet _defaultRuleSet = (Dto.RuleSet)
            Domain.RuleObjectConverter.ConvertToDto(new RuleSetClass());

        Dto.RuleSet _unmodifiedRuleSet = _defaultRuleSet;

        #endregion

        #region Constructors

        public RuleSetJsonSerializer()
        {
        }

        #endregion

        #region IRuleSetJsonSerializer

        /// <summary>
        /// Save a ruleset to a JSON file
        /// </summary>
        /// <param name="pRuleSet">The <see cref="RuleSet"/> to save</param>
        /// <param name="bstrFileName">The path to save the file to</param>
		/// <param name="bUpdateUnmodified">Whether to store the intermediate DTO object for later comparison</param>
        public void SaveRuleSet(RuleSet pRuleSet, string bstrFileName, bool bUpdateUnmodified)
        {
            try
            {
                var dto = (Dto.RuleSet)Domain.RuleObjectConverter.ConvertToDto(pRuleSet);
                var container = new Dto.ObjectWithType(typeof(Dto.RuleSet).Name, dto);
                var json = JsonConvert.SerializeObject(container, Formatting.Indented, RuleObjectJsonSerializer.Settings);

                var fullPath = Path.GetFullPath(bstrFileName);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                File.WriteAllText(fullPath, json);

                if (bUpdateUnmodified)
                {
                    _unmodifiedRuleSet = dto;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI49667", "Failed to save ruleset");
            }
        }

        /// <summary>
        /// Load a ruleset from a JSON file
        /// </summary>
        /// <param name="bstrFileName">The path of the JSON file</param>
        /// <param name="pRuleSet">The deserialized <see cref="RuleSet"/> or <c>null</c> if the file content is invalid</param>
        /// <returns><c>true</c>If the file might be a JSON file (has no null bytes in the decrypted content) else <c>false</c></returns>
		/// <remarks>This will have the side-effect of storing the loaded, intermediate DTO object for later comparison</remarks>
        public bool TryLoadRuleSet(string bstrFileName, out RuleSet pRuleSet)
        {
            try
            {
                var str = FileDerivedResourceCache.ThreadLocalMiscUtils.GetBase64StringFromFile(bstrFileName);
                var bytes = Convert.FromBase64String(str);
                bool isLongEnough = bytes.Length > 200;

                // Log an exception to help with debugging
                // https://extract.atlassian.net/browse/ISSUE-17229
                if (!isLongEnough)
                {
                    ExtractException.Log("ELI50381", "Application trace: Insufficient RSD bytes loaded!");
                }

                // Can this be JSON?
                bool isValidTextFile =
                    isLongEnough
                    && bytes.Contains((byte)'{')
                    && bytes.All(b => b > 0);

                // This input is probably COM structured storage, not JSON
                if (!isValidTextFile)
                {
                    pRuleSet = null;
                    return false;
                }

                var json = Encoding.UTF8.GetString(bytes);
                var dto = (Dto.ObjectWithType)JsonConvert.DeserializeObject(json, typeof(Dto.ObjectWithType), RuleObjectJsonSerializer.Settings);
                var domain = (RuleSet)Domain.RuleObjectConverter.ConvertFromDto(dto);

                // If current version changes to Dto.RuleSetV2 then change all Dto.RuleSet to Dto.RuleSetV2 in this file...
                if (!(dto.Object is Dto.RuleSet rulesetDto))
                {
                    // ...and this will do the upgrading
                    rulesetDto = (Dto.RuleSet)Domain.RuleObjectConverter.ConvertToDto(domain);
                }

                // Something is screwed up. At least provide an exception with the json that this method is seeing
                // https://extract.atlassian.net/browse/ISSUE-17229
                if (rulesetDto == null)
                {
                    var uex = new ExtractException("ELI50382", "RuleSet from JSON is null!");
                    uex.AddDebugData("JSON", json);
                    throw uex;
                }

                _unmodifiedRuleSet = Dto.RuleSetModule.GetWithCurrentSoftwareVersion(rulesetDto);

                pRuleSet = domain;

                return true;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI49668", "Failed to load ruleset");
            }
        }

        /// <summary>
        /// Converts a <see cref="RuleSet"/> to a <see cref="Dto.RuleSet"/> and compares it to the DTO last loaded or saved with this object
        /// </summary>
        /// <param name="pRuleSet">The <see cref="RuleSet"/> to compare</param>
        /// <returns><c>true</c> if there are no differences between the last loaded/saved ruleset and the input, else <c>false</c></returns>
        /// <remarks>The SavedInSoftwareVersion of the loaded ruleset is updated to match the current version for comparison purposes so this will not count as a modification</remarks>
        public bool EqualsUnmodified(RuleSet pRuleSet)
        {
            try
            {
                var dto = (Dto.RuleSet)Domain.RuleObjectConverter.ConvertToDto(pRuleSet);
                return dto.Equals(_unmodifiedRuleSet);
            }
            catch (Exception ex)
            {
                new ExtractException("ELI49685", "Failed to convert ruleset to DTO", ex).Log();
                return false;
            }
        }

        /// <summary>
		/// Store the DTO version of a <see cref="RuleSet"/> for later comparison (for IsDirty calculations)
        /// </summary>
        /// <param name="pRuleSet">The <see cref="RuleSet"/> to store</param>
        public void InitializeUnmodified(RuleSet pRuleSet)
        {
            try
            {
                _unmodifiedRuleSet = (Dto.RuleSet)Domain.RuleObjectConverter.ConvertToDto(pRuleSet);
            }
            catch (Exception ex)
            {
                new ExtractException("ELI49695", "Failed to convert ruleset to DTO", ex).Log();
                _unmodifiedRuleSet = _defaultRuleSet;
            }
        }

        #endregion
    }
}
