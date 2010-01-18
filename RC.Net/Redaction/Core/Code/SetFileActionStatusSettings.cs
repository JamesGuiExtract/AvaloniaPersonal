using Extract.Interop;
using System;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Redaction
{
    /// <summary>
    /// Represents settings for setting the action status of a file.
    /// </summary>
    public class SetFileActionStatusSettings
    {
        #region Fields

        /// <summary>
        /// <see langword="true"/> if the action status should be set after a document is 
        /// committed; <see langword="false"/> if the action status should not be set after a 
        /// document is committed.
        /// </summary>
        readonly bool _enabled;
	
        /// <summary>
        /// Name of the action to set.
        /// </summary>
        readonly string _actionName;
        
        /// <summary>
        /// Status to which the action should be set.
        /// </summary>
        readonly EActionStatus _actionStatus;
		
        #endregion Fields

        #region Constructors
	
        /// <overloads>Initializes a new instance of the <see cref="SetFileActionStatusSettings"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="SetFileActionStatusSettings"/> class with default 
        /// settings.
        /// </summary>
        public SetFileActionStatusSettings() 
            : this(false, null, EActionStatus.kActionPending)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetFileActionStatusSettings"/>.
        /// </summary>
        /// <param name="enabled"><see langword="true"/> if the action status should be set after 
        /// a document is committed; <see langword="false"/> if the action status should not be 
        /// set after a document is committed.</param>
        /// <param name="actionName">Name of the action to set.</param>
        /// <param name="actionStatus">Status to which the action should be set.</param>
        [CLSCompliant(false)]
        public SetFileActionStatusSettings(bool enabled, string actionName, EActionStatus actionStatus)
        {
            _enabled = enabled;
            _actionName = actionName ?? "";
            _actionStatus = actionStatus;
        }
		
        #endregion Constructors
        
        #region Properties

        /// <summary>
        /// Gets whether the action status should be set after a document is committed.
        /// </summary>
        /// <value><see langword="true"/> if the action status should be set after a document is 
        /// committed; <see langword="false"/> if the action status should not be set after a 
        /// document is committed.</value>
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
        }
	
        /// <summary>
        /// Gets the name of the action to set.
        /// </summary>
        /// <value>The name of the action to set.</value>
        public string ActionName
        {
            get
            {
                return _actionName;
            }
        }
		
        /// <summary>
        /// Gets the status to which the action should be set.
        /// </summary>
        /// <value>The status to which the action should be set.</value>
        [CLSCompliant(false)]
        public EActionStatus ActionStatus
        {
            get
            {
                return _actionStatus;
            }
        }
		
        #endregion Properties

        #region Methods

        /// <summary>
        /// Creates a <see cref="SetFileActionStatusSettings"/> from the specified 
        /// <see cref="IStreamReader"/>.
        /// </summary>
        /// <param name="reader">The reader from which to create the 
        /// <see cref="SetFileActionStatusSettings"/>.</param>
        /// <returns>A <see cref="SetFileActionStatusSettings"/> created from the specified 
        /// <see cref="IStreamReader"/>.</returns>
        public static SetFileActionStatusSettings ReadFrom(IStreamReader reader)
        {
            try
            {
                bool enabled = reader.ReadBoolean();
                string actionName = reader.ReadString();
                EActionStatus actionStatus = (EActionStatus)reader.ReadInt32();

                return new SetFileActionStatusSettings(enabled, actionName, actionStatus);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI29160",
                    "Unable to read file action status settings.", ex);
            }
        }

        /// <summary>
        /// Writes the <see cref="SetFileActionStatusSettings"/> to the specified 
        /// <see cref="IStreamWriter"/>.
        /// </summary>
        /// <param name="writer">The writer into which the 
        /// <see cref="SetFileActionStatusSettings"/> will be written.</param>
        public void WriteTo(IStreamWriter writer)
        {
            try
            {
                writer.Write(_enabled);
                writer.Write(_actionName);
                writer.Write((int)_actionStatus);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI29161",
                    "Unable to write file action status settings.", ex);
            }
        }

        #endregion Methods
    }
}
