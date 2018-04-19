using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Extract.ETL
{
    public abstract class MachineLearningService : DatabaseService
    {
        string _modelName;

        /// <summary>
        /// The name used to group training/testing data in FAMDB (table MLModel)
        /// </summary>
        [DataMember]
        public string ModelName
        {
            get
            {
                return _modelName;
            }
            set
            {
                if (value != _modelName)
                {
                    _modelName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the DatabaseServer of the <see cref="Container"/> if non-null, else returns base implementation
        /// </summary>
        public override string DatabaseServer
        {
            get
            {
                if (Container is DatabaseService parent)
                {
                    return parent.DatabaseServer;
                }
                else
                {
                    return base.DatabaseServer;
                }
            }
        }

        /// <summary>
        /// Gets the DatabaseName of the <see cref="Container"/> if non-null, else returns base implementation
        /// </summary>
        public override string DatabaseName
        {
            get
            {
                if (Container is DatabaseService parent)
                {
                    return parent.DatabaseName;
                }
                else
                {
                    return base.DatabaseName;
                }
            }
        }

        /// <summary>
        /// Used by the <see cref="ITrainingCoordinator"/> to save statuses for the contained
        /// instances
        /// </summary>
        [DataMember]
        public Guid Guid { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Whether enabled. Serialized to simplify keeping track of this information when
        /// contained in a <see cref="ITrainingCoordinator"/>.
        /// </summary>
        [DataMember]
        public override bool Enabled
        {
            get => base.Enabled;
            set => base.Enabled = value;
        }

        /// <summary>
        /// The instance that contains this service
        /// </summary>
        public ITrainingCoordinator Container { get; set; }

        /// <summary>
        /// Used as a namespace to prefix MLModel.Names in the database
        /// </summary>
        /// <remarks>This is empty if this instance is not contained in a <see cref="ITrainingCoordinator"/></remarks>
        public string ModelNamePrefix => string.IsNullOrEmpty(Container?.ProjectName)
            ? ""
            : Container.ProjectName + "::";

        /// <summary>
        /// The directory where referenced file's paths are based
        /// </summary>
        /// <remarks>This is null if this instance is not contained in a <see cref="ITrainingCoordinator"/></remarks>
        public string RootDir => Container?.RootDir;

        /// <summary>
        /// The <see cref="ModelName"/> prefixed by the <see cref="ModelNamePrefix"/>
        /// </summary>
        /// <remarks>The setter will update the <see cref="ModelName"/> by trimming the prefix
        /// from the new value and using what's left</remarks>
        public string QualifiedModelName
        {
            get
            {
                return string.IsNullOrEmpty(ModelName)
                    ? ""
                    : ModelNamePrefix + ModelName;
            }
            set
            {
                try
                {
                    if (string.IsNullOrEmpty(ModelNamePrefix)
                        || !value.StartsWith(ModelNamePrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        ModelName = value;
                    }
                    else
                    {
                        ModelName = value.Remove(0, ModelNamePrefix.Length);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI45809");
                }
            }
        }

        /// <summary>
        /// Clones with a new Guid
        /// </summary>
        public MachineLearningService Duplicate()
        {
            var clone = (MachineLearningService)Clone();
            clone.Guid = Guid.NewGuid();
            return clone;
        }

        /// <summary>
        /// When overridden by a derived class, will return the current status
        /// </summary>
        public abstract DatabaseServiceStatus Status { get; set; }

        /// <summary>
        /// When overridden by a derived class, will return the count of new MLData records
        /// since last processing occurred
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public abstract int GetUnprocessedRecordCount();
    }
}
