using Extract.Licensing.Internal;
using System;

namespace Extract.FAMDBCounterManager
{
    /// <summary>
    /// The operations available to apply to FAM DB secure counters via the FAM DB Counter Manager.
    /// </summary>
    internal enum CounterOperation
    {
        None = 0,
        Create = 1,
        Set = 2,
        Increment = 3,
        Decrement = 4,
        Delete = 5,
        Rename = 6
    }

    /// <summary>
    /// Represents a FAM DB secure counter (either existing or to be created), and information about
    /// any operation to execute on the counter.
    /// </summary>
    internal class CounterData
    {
        #region Fields

        /// <summary>
        /// The ID of the counter.
        /// </summary>
        int? _id;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Provides static initialization for the <see cref="CounterData"/> class.
        /// </summary>
        static CounterData()
        {
            try
            {
                CounterOperation.None.SetReadableValue("");
                CounterOperation.Create.SetReadableValue("Create");
                CounterOperation.Set.SetReadableValue("Set");
                CounterOperation.Increment.SetReadableValue("Increment");
                CounterOperation.Decrement.SetReadableValue("Decrement");
                CounterOperation.Delete.SetReadableValue("Delete");
            }
            catch (Exception ex)
            {
                ex.ShowMessageBox();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CounterData"/> class representing a new
        /// counter to be created in the FAM DB.
        /// </summary>
        public CounterData()
        {
            // If this instance is not being initialized with an ID, it must be a new counter.
            UserAdded = true;
            Operation = CounterOperation.Create;
            ApplyValue = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CounterData"/> class representing an
        /// existing counter in the FAM DB.
        /// <see paramref="id"/>.
        /// </summary>
        /// <param name="id">The ID of the counter.</param>
        public CounterData(int id)
        {
            // If this instance is not being initialized with an ID, it is not a new counter.
            UserAdded = false;
            ID = id;
            Operation = CounterOperation.None;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this is a new counter the user is specifying to
        /// be created.
        /// </summary>
        /// <value><see langword="true"/> if this is a new counter the user is specifying to
        /// be created; <see langword="false"/> if this is an existing counter in the FAM DB.
        /// </value>
        public bool UserAdded
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ID of the counter
        /// </summary>
        /// <value>
        /// The ID of the counter or <see langword="null"/> if an ID has not yet been assigned.
        /// </value>
        public int? ID
        {
            get
            {
                return _id;
            }

            set
            {
                UtilityMethods.Assert(!value.HasValue || value <= 5 || value >= 100,
                    "Counter values < 100 are reserved for standard counters");

                if (value.HasValue)
                {
                    string standardName;
                    if (FAMDBCounterManagerForm._standardCounterNames.TryGetValue(value.Value, out standardName))
                    {
                        Name = standardName;
                    }
                }

                _id = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the counter.
        /// </summary>
        /// <value>
        /// The name of the counter.
        /// </value>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value of the counter in the FAM DB as of the time of license string
        /// generation.
        /// </summary>
        /// <value>
        /// The value of the counter in the FAM DB as of the time of license string
        /// generation. This value will be <see langword="null"/> for new counters defined in this
        /// utility for creation.
        /// </value>
        public int? Value
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value of the counter as reported in the SecureCounterValueChange table.
        /// </summary>
        /// <value>
        /// The value of the counter in the SecureCounterValueChange table. This value will be
        /// <see langword="null"/> if there is no corresponding value in the
        /// SecureCounterValueChange table.
        /// </value>
        public int? ChangeLogValue
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <see cref="CounterOperation"/> to execute for this counter.
        /// </summary>
        /// <value>
        /// The <see cref="CounterOperation"/> to execute for this counter.
        /// </value>
        public CounterOperation Operation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value to apply to a counter in conjunction with the specified
        /// <see cref="Operation"/>.
        /// </summary>
        /// <value>
        /// The value to apply to a counter in conjunction with the specified
        /// <see cref="Operation"/>. Will be <see langword="null"/> for operation "None" or "Delete".
        /// </value>
        public int? ApplyValue
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a textual description of any counter-specific reason(s) the counter is
        /// invalid.
        /// </summary>
        /// <value>
        /// A textual counter-specific reason the counter is invalid
        /// </value>
        public string ValidityComment
        {
            get;
            set;
        }

        #endregion Properties
    }
}
