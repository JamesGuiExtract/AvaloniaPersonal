using DynamicData.Kernel;
using LabDEOrderMappingInvestigator.Models;
using LinqToDB;
using System.Collections.Generic;
using System.Linq;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;

namespace LabDEOrderMappingInvestigator.Services
{
    /// <summary>
    /// Service with methods to read order/test data from a file
    /// </summary>
    public interface ILabOrderFileService
    {
        /// <summary>
        /// Load actual customer lab orders from a data file
        /// </summary>
        IList<LabOrderActual> LoadLabOrdersFromFile(string attributesFileName, string customerOMDBPath);
    }

    /// <summary>
    /// Service with methods to read order/test data from a VOA file
    /// </summary>
    public class VOALabOrderFileService : ILabOrderFileService
    {
        readonly IAFUtility _afutil;

        public VOALabOrderFileService(IAFUtility afutil)
        {
            _afutil = afutil;
        }

        /// <summary>
        /// Load lab orders/tests from a VOA file
        /// </summary>
        /// <remarks>Order/test definitions need to be updated from the database later</remarks>
        public IList<LabOrderActual> LoadLabOrdersFromFile(string attributesFileName, string customerOMDBPath)
        {
            var voa = _afutil.GetAttributesFromFile(attributesFileName);

            (voa != null).Assert("Unexpected null vector of attributes!");

            return voa.ToIEnumerable<IAttribute>()
                .Select(a =>
                {
                    if (a.Name == "Test")
                    {
                        Optional<string> orderCode = GetFirstValueFromVoa(a.SubAttributes, "OrderCode");
                        Optional<string> orderName = GetFirstValueFromVoa(a.SubAttributes, "Name");

                        var labTests = _afutil.QueryAttributes(a.SubAttributes, "Component", false)
                            .ToIEnumerable<IAttribute>()
                            .Select(test =>
                            {
                                var testCode = GetFirstValueFromVoa(test.SubAttributes, "TestCode");
                                return new LabTestActual(null, test.Value.String, testCode,
                                    GetFirstValueFromVoa(test.SubAttributes, "Value").ValueOrDefault() ?? "",
                                    GetFirstValueFromVoa(test.SubAttributes, "Range").ValueOrDefault() ?? "",
                                    GetFirstValueFromVoa(test.SubAttributes, "Units").ValueOrDefault() ?? "",
                                    GetFirstValueFromVoa(test.SubAttributes, "Flag").ValueOrDefault() ?? "");
                            })
                            .ToList();

                        return Optional<LabOrderActual>.Create(new LabOrderActual(orderName, orderCode, null, labTests));
                    }
                    return null;
                })
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .ToList();
        }

        // Query for an attribute name and return the string value, or none if not found
        Optional<string> GetFirstValueFromVoa(IUnknownVector voa, string name)
        {
            Optional<string> value = null;
            var values = _afutil.QueryAttributes(voa, name, false);
            if (values.Size() > 0)
            {
                value = ((IAttribute)values.At(0)).Value.String;
            }

            return value;
        }
    }
}
