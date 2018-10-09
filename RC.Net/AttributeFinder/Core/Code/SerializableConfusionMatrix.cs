using LearningMachineTrainer;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using AccuracyData = Extract.Utilities.Union<Accord.Statistics.Analysis.GeneralConfusionMatrix, Accord.Statistics.Analysis.ConfusionMatrix>;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// Class to save confusion matrix data
    /// </summary>
    [CLSCompliant(false)]
    [Serializable]
    [Obfuscation(Feature = "renaming", Exclude = true)]
    public class SerializableConfusionMatrix
    {
        int[][] _data;
        int[] _rowTotals;
        int[] _columnTotals;
        string[] _labels;
        int[] _positiveIndexes;

        private SerializableConfusionMatrix(int[][] data, int[] rowTotals, int[] columnTotals, string[] labels, int[] positiveIndexes)
        {
            _data = data;
            _rowTotals = rowTotals;
            _columnTotals = columnTotals;
            _labels = labels;
            _positiveIndexes = positiveIndexes;
        }


        /// <summary>
        /// Create a confusion matrix from a <see cref="Accord.Statistics.Analysis.ConfusionMatrix"/>
        /// or <see cref="Accord.Statistics.Analysis.GeneralConfusionMatrix"/>
        /// </summary>
        /// <param name="encoder">The <see cref="LearningMachineDataEncoder"/> to use to obtain class names and codes</param>
        /// <param name="accuracyData">The confusion matrix</param>
        public SerializableConfusionMatrix(ILearningMachineDataEncoderModel encoder, AccuracyData accuracyData)
        {
            try
            {
                int[,] sourceData = null;

                // Clone the list so that the encoder isn't modified
                // https://extract.atlassian.net/browse/ISSUE-15642
                var labels = new List<string>(encoder.AnswerCodeToName);

                accuracyData.Match(gcm =>
                {
                    if (labels.Count < gcm.Classes)
                    {
                        labels.Add("<Low Probability>");
                    }
                    ExtractException.Assert("ELI45004", "Logic exception: too many classes", labels.Count == gcm.Classes);

                    _labels = labels.ToArray();
                    _positiveIndexes = Enumerable.Range(0, _labels.Length)
                        .Where(i => !_labels[i].Equals(encoder.NegativeClassName, StringComparison.OrdinalIgnoreCase))
                        .ToArray();

                    sourceData = gcm.Matrix;
                    _rowTotals = gcm.RowTotals;
                    _columnTotals = gcm.ColumnTotals;
                },
                cm =>
                {
                    // The ConfusionMatrix class puts class 0 at index 1 so reverse the labels
                    // but don't modify the encoder's list
                    // https://extract.atlassian.net/browse/ISSUE-15524
                    _labels = new string[2]
                    {
                        labels[1],
                        labels[0]
                    };
                    _positiveIndexes = new[] { 0 };

                    sourceData = cm.Matrix;
                    _rowTotals = cm.RowTotals;
                    _columnTotals = cm.ColumnTotals;
                });

                _data = new int[sourceData.GetLength(0)][];
                for (int row = 0; row < _data.Length; row++)
                {
                    _data[row] = new int[_data.Length];
                    for (int col = 0; col < _data.Length; col++)
                    {
                        _data[row][col] = sourceData[row, col];
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45003");
            }
        }

        /// <summary>
        /// Creates a deep clone of this instance
        /// </summary>
        public SerializableConfusionMatrix DeepClone()
        {
            try
            {
                int[][] data = new int[_data.Length][];
                for (int i = 0; i < _data.Length; i++)
                {
                    data[i] = (int[])_data[i].Clone();
                }
                int[] rowTotals = (int[])_rowTotals.Clone();
                int[] columnTotals = (int[])_columnTotals.Clone();
                string[] labels = (string[])_labels.Clone();
                int[] positiveIndexes = (int[])_positiveIndexes.Clone();

                return new SerializableConfusionMatrix(data, rowTotals, columnTotals, labels, positiveIndexes);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45012");
            }
        }


        /// <summary>
        /// The confusion matrix
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public int[][] Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
            }
        }

        /// <summary>
        /// The sums of the rows (expected values)
        /// </summary>
        /// <remarks>If any data is modified, this needs to be updated to match</remarks>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public int[] RowTotals
        {
            get
            {
                return _rowTotals;
            }
            set
            {
                _rowTotals = value;
            }
        }

        /// <summary>
        /// The sums of the columns (found values)
        /// </summary>
        /// <remarks>If any data is modified, this needs to be updated to match</remarks>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public int[] ColumnTotals
        {
            get
            {
                return _columnTotals;
            }
            set
            {
                _columnTotals = value;
            }
        }

        /// <summary>
        /// The class names corresponding to rows and columns of the <see cref="Data"/>
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public string[] Labels
        {
            get
            {
                return _labels;
            }
            set
            {
                _labels = value;
            }
        }

        /// <summary>
        /// Calculate the overall agreement of the predictions and the expected classes
        /// </summary>
        public double OverallAgreement()
        {
            try
            {
                var correct = Enumerable.Range(0, _rowTotals.Length).Sum(i => _data[i][i]);
                var predictions = _columnTotals.Sum(r => r);
                return (double)correct / predictions;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45009");
            }
        }

        /// <summary>
        /// The precision calculated using sums of predictions
        /// </summary>
        public double PrecisionMicroAverage()
        {
            try
            {
                var predictedPositives = _positiveIndexes.Sum(i => _columnTotals[i]);
                if (predictedPositives == 0)
                {
                    return 1;
                }

                var truePositives = _positiveIndexes.Sum(i => _data[i][i]);
                return (double)truePositives / predictedPositives;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45011");
            }
        }

        /// <summary>
        /// Recall calculated using sums of predictions and expected values
        /// </summary>
        public double RecallMicroAverage()
        {
            try
            {
                var expectedPositives = _positiveIndexes.Sum(i => _rowTotals[i]);
                if (expectedPositives == 0)
                {
                    return 0;
                }

                var truePositives = _positiveIndexes.Sum(i => _data[i][i]);
                return (double)truePositives / expectedPositives;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45010");
            }
        }

        /// <summary>
        /// The harmonic mean of PrecisionMicroAverage and RecallMicroAverage
        /// </summary>
        public double FScoreMicroAverage()
        {
            try
            {
                var p = PrecisionMicroAverage();
                var r = RecallMicroAverage();
                return 2 * p * r / (p + r);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45013");
            }
        }

        /// <summary>
        /// The indexes of classes that are considered negative (e.g., NotFirstPage)
        /// </summary>
        /// <returns></returns>
        public int[] NegativeClassIndexes()
        {
            return Enumerable.Range(0, _labels.Length).Except(_positiveIndexes).ToArray();
        }

        public IEnumerable<string> NegativeClasses()
        {
            return Enumerable.Range(0, _labels.Length)
                .Except(_positiveIndexes)
                .Select(i => _labels[i]);
        }

        public void SetNegativeClasses(IEnumerable<string> classes)
        {
            try
            {
                var negativeLabels = new HashSet<string>(classes);
                _positiveIndexes = Enumerable.Range(0, _labels.Length)
                    .Where(i => !negativeLabels.Contains(_labels[i]))
                    .ToArray();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45576");
            }
        }
    }
}