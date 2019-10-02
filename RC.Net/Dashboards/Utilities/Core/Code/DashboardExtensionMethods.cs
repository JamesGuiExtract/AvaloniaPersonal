using DevExpress.DashboardCommon.ViewerData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Extract.Dashboard.Utilities
{
    public static class DashboardExtensionMethods
    {

        /// <summary>
        ///  Returns a dictionary that has all Dimension values associated with the row represented by axistValueTuple
        /// </summary>
        /// <param name="axisValueTuple"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ToDictionary(this AxisPointTuple axisValueTuple)
        {
            try
            {
                var dict = new Dictionary<string, object>();

                var startingAxisPoint = axisValueTuple.GetAxisPoint();

                // If no starting point return null
                if (startingAxisPoint is null)
                {
                    return null;
                }

                // The axis points for the row are all in a linked list with the top level parent countaining all the
                // rows so want to go to the first child
                var axisPoint = startingAxisPoint;
                do
                {
                    dict[axisPoint.Dimension.DataMember] = axisPoint.GetDimensionValue(axisPoint.Dimension).Value;
                    axisPoint = axisPoint.Parent;
                }
                while (axisPoint?.Parent != null);

                return dict;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47054");
            }
        }
    }
}
