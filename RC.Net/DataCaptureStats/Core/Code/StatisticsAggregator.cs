using System;
using System.Collections.Generic;
using System.Linq;

namespace Extract.DataCaptureStats
{
    /// <summary>
    /// Class to hold a method to aggregate data capture stats
    /// </summary>
    public static class StatisticsAggregator
    {
        /// <summary>
        /// Aggregates the statistics data from the <see cref="AttributeTreeComparer"/>.
        /// </summary>
        /// <remarks>
        /// Only one instance of each label/path combination will be output. However, contrary to the initial design,
        /// there could be paths where there is an item with a label of <see cref="AccuracyDetailLabel.ContainerOnly"/>
        /// as well as items with other labels, e.g., Expected. This situation will be dealt with by the summarizing method.
        /// </remarks>
        /// <param name="statisticsToAggregate">The collection of <see cref="AccuracyDetail"/> items to aggregate.</param>
        /// <returns>An <see cref="IEnumerable{AccuracyDetail}"/> where each label/path pair in the input has been summed.</returns>
        public static IEnumerable<AccuracyDetail> AggregateStatistics(this
            IEnumerable<AccuracyDetail> statisticsToAggregate)
        {
            try
            {
                return statisticsToAggregate.GroupBy(a => new { a.Path, a.Label })
                    .Select(g => new AccuracyDetail(g.Key.Label, g.Key.Path, g.Sum(a => a.Value)));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41526");
            }
        }
    }
}
