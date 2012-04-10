using Extract.Utilities;
using System;

namespace Extract.DataEntry
{
    /// <summary>
    /// A <see cref="QueryNode"/> to insert the solution directory name.
    /// </summary>
    internal class SolutionDirectoryQueryNode : QueryNode
    {
        /// <summary>
        /// Cache the solution directory name after it is calculated the first time since
        /// ConvertToNetworkPath can be slow to run.
        /// </summary>
        static string _solutionDirectory;

        /// <summary>
        /// Used to ensure calculation of _solutionDirectory happens on one thread only.
        /// </summary>
        object _lock = new object();

        /// <summary>
        /// Initializes a new <see cref="SolutionDirectoryQueryNode"/> instance.
        /// </summary>
        public SolutionDirectoryQueryNode()
            : base()
        {
        }

        /// <summary>
        /// Evaluates the query using the solution directory (as a UNC path, if possible).
        /// </summary>
        /// <returns>The query using the solution directory (as a UNC path, if possible).
        /// </returns>
        public override QueryResult Evaluate()
        {
            try
            {
                if (_solutionDirectory == null)
                {
                    // Calculate the solution directory only once for all instances of
                    // SolutionDirectoryQueryNode to minimize the performance hit of
                    // ConvertToNetworkPath.
                    lock (_lock)
                    {
                        if (_solutionDirectory == null)
                        {
                            _solutionDirectory = DataEntryMethods.ResolvePath(".");
                            FileSystemMethods.ConvertToNetworkPath(ref _solutionDirectory, true);
                        }
                    }
                }

                return new QueryResult(this, _solutionDirectory);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28931", ex);
                ee.AddDebugData("Query node type", GetType().Name, false);
                ee.AddDebugData("Query", QueryText ?? "null", false);
                throw ee;
            }
        }
    }
}
