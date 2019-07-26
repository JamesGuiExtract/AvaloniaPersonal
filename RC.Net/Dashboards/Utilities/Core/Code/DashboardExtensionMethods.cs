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
                while (axisPoint.Parent?.Parent != null)
                {
                    axisPoint = axisPoint.Parent;
                }

                AddAllNodesInList(axisPoint, ref dict);
                return dict;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47054");
            }
        }

        #region Private methods

        static void AddAllNodesInList(AxisPoint axisPoint, ref Dictionary<string, object> dictionary)
        {
            if (axisPoint == null)
            {
                return;
            }

            dictionary[axisPoint.Dimension.DataMember] = axisPoint.GetDimensionValue(axisPoint.Dimension).Value;

            // The ChildItems for a row is expected to have either 0 or 1 items
            if (axisPoint.ChildItems.Count == 0)
            {
                return;
            }
            if (axisPoint.ChildItems.Count == 1)
            {
                AddAllNodesInList(axisPoint.ChildItems[0], ref dictionary);
            }
            else
            {
                ExtractException ee = new ExtractException("ELI47055", "Unable to get data from Row.");
                throw ee;
            }
            return;
        }

        #endregion
    }
}
