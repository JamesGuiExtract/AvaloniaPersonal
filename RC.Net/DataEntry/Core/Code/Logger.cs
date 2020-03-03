using Extract.Licensing;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UCLID_AFCORELib;

namespace Extract.DataEntry
{
    /// <summary>
    /// The different categories of events that may be logged by the <see cref="Logger"/> class.
    /// </summary>
    [Flags]
    public enum LogCategories
    {
        /// <summary>
        /// No category specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// An event relating to data being loaded into the UI.
        /// </summary>
        DataLoad = 0x0001,

        /// <summary>
        /// An event relating to data being saved.
        /// </summary>
        DataSave = 0x0002,

        /// <summary>
        /// An event relating to an attribute being initialized (either from loaded data or a newly
        /// created attribute)
        /// </summary>
        AttributeInitialized = 0x0004,

        /// <summary>
        /// An event relating to an attribute value being updated.
        /// </summary>
        AttributeUpdated = 0x0008,

        /// <summary>
        /// An event relating to an attribute value being deleted.
        /// </summary>
        AttributeDeleted = 0x0010,

        /// <summary>
        /// An event relating to the execution of an auto-update query.
        /// </summary>
        AutoUpdateQuery = 0x0020,

        /// <summary>
        /// An event to report the results of an auto-update query.
        /// </summary>
        AutoUpdateResult = 0x0040,

        /// <summary>
        /// An event relating to the execution of a validation query.
        /// </summary>
        ValidationQuery = 0x0080,

        /// <summary>
        /// An event to report the results of a validation query.
        /// </summary>
        ValidationResult = 0x0100,

        /// <summary>
        /// An event relating to data having been swiped from the document image.
        /// </summary>
        SwipedText = 0x0200,

        /// <summary>
        /// An event reporting a tooltip message that was displayed.
        /// </summary>
        TooltipNotification = 0x0400,

        /// <summary>
        /// An event reporting the results of a formatting rule.
        /// </summary>
        FormattingRuleResult = 0x0800,

        /// <summary>
        /// An undo operation.
        /// </summary>
        Undo = 0x1000,

        /// <summary>
        /// A redo operation.
        /// </summary>
        Redo = 0x2000,

        /// <summary>
        /// An event reporting a focus change.
        /// </summary>
        Focus = 0x4000,

        /// <summary>
        /// An event reporting mouse or keyboard input.
        /// </summary>
        InputEvent = 0x8000,
    }

    /// <summary>
    /// Allows logging input and events in the data entry verification UI.
    /// </summary>
    public class Logger : MessageFilterBase
    {
        #region Fields

        /// <summary>
        /// <see langword="true"/> to store logged events to memory (to be saved later);
        /// <see langword="false"/> to generate trace output.
        /// </summary>
        bool _logToMemory;

        /// <summary>
        /// A <see cref="LogCategories"/> value indicating which events should be logged.
        /// </summary>
        LogCategories _logCategories;

        /// <summary>
        /// The <see cref="WindowsMessage"/> IDs that should be logged for LogCategories.InputEvent
        /// if that event is enabled.
        /// </summary>
        HashSet<int> _inputEventsToTrack;

        /// <summary>
        /// The default buffer size to use store logged input when <see cref="LogToMemory"/> is
        /// <see langword="true"/>.
        /// </summary>
        StringBuilder _loggedInput = new StringBuilder(0x10000); // 1MB

        /// <summary>
        /// The number of events that have been logged to _loggedInput.
        /// </summary>
        int _loggedEventCount = 0;

        #endregion Fields

        #region Constructors

        /// <overloads>
        /// Creates a new instance of the <see cref="Logger"/> class.
        /// </overloads>
        /// <summary>
        /// Creates a new instance of the <see cref="Logger"/> class.
        /// </summary>
        /// <param name="logToMemory"><see langword="true"/> to store logged events to memory (to be
        /// saved later); <see langword="false"/> to generate trace output.</param>
        /// <param name="logCategories">A comma-delimited list representing the
        /// <see cref="LogCategories"/> to be logged or "*" to log all categories.</param>
        /// <param name="inputEventsToTrack">A comma-delimited list representing the
        /// <see cref="WindowsMessageCode"/> that should be recorded when LogCategories.InputEvent
        /// is enabled or "*" to log all <see cref="WindowsMessage"/> codes.</param>
        /// <param name="controls">The <see cref="Control"/>(s) for which InputEvents is to be
        /// tracked.</param>
        public static Logger CreateLogger(bool logToMemory, string logCategories,
            string inputEventsToTrack, params Control[] controls)
        {
            try
            {
                // Convert the string logCategories parameter to a LogCategories value.
                LogCategories categoryFilter = LogCategories.None;
                if (logCategories.Trim() == "*")
                {
                    categoryFilter = Enum.GetValues(typeof(LogCategories))
                        .Cast<LogCategories>()
                        .Aggregate((a, b) => a | b);
                }
                else
                {
                    categoryFilter = logCategories
                        .Split(',', ';')
                        .Where(inputType => !string.IsNullOrWhiteSpace(inputType))
                        .Select(inputType =>
                            Enum.Parse(typeof(LogCategories), inputType.Trim()))
                        .Cast<LogCategories>()
                        .Aggregate((a, b) => a | b);
                }

                // Convert the string inputEventsToTrack parameter to a enumerable of WindowMessage
                // codes.
                IEnumerable<int> inputFilter = null;
                if (categoryFilter.HasFlag(LogCategories.InputEvent))
                {
                    if (inputEventsToTrack.Trim() == "*")
                    {
                        inputFilter = Enum.GetValues(typeof(WindowsMessageCode)).Cast<int>();
                    }
                    else
                    {
                        inputFilter = inputEventsToTrack
                            .Split(',', ';')
                            .Where(eventName => !string.IsNullOrWhiteSpace(eventName))
                            .Select(eventName =>
                                (int)Enum.Parse(typeof(WindowsMessageCode), eventName.Trim()));
                    }
                }

                return new Logger(logToMemory, categoryFilter, inputFilter, controls);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38346");
            }
        }

        /// <overloads>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        /// <param name="logToMemory"><see langword="true"/> to store logged events to memory (to be
        /// saved later); <see langword="false"/> to generate trace output.</param>
        /// <param name="logCategories">A <see cref="LogCategories"/> value representing the event
        /// categories to be logged.</param>
        /// <param name="eventsToTrack">An <see cref="int"/> <see cref="IEnumerable{T}"/> of
        /// the <see cref="T:WindowsMessageCode"/> that should be recorded when
        /// LogCategories.InputEvent is enabled.</param>
        /// <param name="controls">The <see cref="Control"/>(s) for which InputEvents is to be
        /// tracked.</param>
        public Logger(bool logToMemory, LogCategories logCategories,
            IEnumerable<int> eventsToTrack, params Control[] controls)
            : base(
                (logCategories.HasFlag(LogCategories.InputEvent) && eventsToTrack.Any()) 
                ? controls 
                : null)
        {
            try
            {
                _logToMemory = logToMemory;
                _logCategories = logCategories;

                if (eventsToTrack == null)
                {
                    _inputEventsToTrack = new HashSet<int>();
                }
                else
                {
                    _inputEventsToTrack = new HashSet<int>(eventsToTrack);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38348");
            }
        }

        #endregion Constructors

        /// <summary>
        /// Gets or sets a value indicating whether to store logged events to memory or to generate
        /// trace output.
        /// </summary>
        /// <value><see langword="true"/> to store logged events to memory (to be saved later);
        /// <see langword="false"/> to generate trace output.</value>
        public bool LogToMemory
        {
            get
            {
                return _logToMemory;
            }

            set
            {
                _logToMemory = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether 
        /// </summary>
        /// <value><see langword="true"/>A <see cref="LogCategories"/> value representing the event
        /// categories to be logged.</value>
        public LogCategories LogCategories
        {
            get
            {
                return _logCategories;
            }

            set
            {
                _logCategories = value;
            }
        }

        #region Methods

        /// <summary>
        /// Logs the <see paramref="loglines"/> for the specified <see paramref="category"/> if
        /// logging for the category is enabled.
        /// </summary>
        /// <param name="category">The category with which <see paramref="loglines"/> are
        /// associated.</param>
        /// <param name="attribute">The <see cref="IAttribute"/> the event pertains to or
        /// <see langword="null"/> if the event doesn't pertain to any single attribute.</param>
        /// <param name="loglines">The lines to log for this event.</param>
        public void LogEvent(LogCategories category, IAttribute attribute,
            params string[] loglines)
        {
            LogEvent(category, attribute, null, loglines);
        }

        /// <summary>
        /// Logs the <see paramref="loglines"/> for the specified <see paramref="category"/> if
        /// logging for the category is enabled.
        /// </summary>
        /// <param name="category">The category with which <see paramref="loglines"/> are
        /// associated.</param>
        /// <param name="attribute">The <see cref="IAttribute"/> the event pertains to or
        /// <see langword="null"/> if the event doesn't pertain to any single attribute.</param>
        /// <param name="control">The <see cref="Control"/> the event pertains to or
        /// <see langword="null"/> if the event doesn't pertain to any single control.</param>
        /// <param name="loglines">The lines to log for this event.</param>
        public void LogEvent(LogCategories category, IAttribute attribute, Control control,
            params string[] loglines)
        {
            try
            {
                if (LogCategories.HasFlag(category))
                {
                    // If a control was now explicitly specified, use the attribute's owning control
                    // if there is one.
                    Control owningControl = control ??
                        ((attribute != null) 
                            ? AttributeStatusInfo.GetOwningControl(attribute) as Control
                            : null);

                    string categoryName = category.ToString();

                    categoryName += (owningControl != null)
                        ? " [" + owningControl.Name + "]"
                        : " []";

                    categoryName += (attribute != null)
                        ? " (" + attribute.Name + ")"
                        : " ()";

                    if (loglines != null)
                    {
                        // Ensure each supplied line is actually a separate line.
                        foreach (string logLine in loglines
                            .SelectMany(line => line.Split('\n')))
                        {
                            if (string.IsNullOrEmpty(logLine))
                            {
                                LogLine(categoryName, "[EMPTY]");
                            }
                            else
                            {
                                LogLine(categoryName, logLine);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38349");
            }
        }

        /// <summary>
        /// Notify this instance of a message that was intercepted/handled outside of what this
        /// instance is able to track. One example is tab key events that are intercepted at a
        /// higher level.
        /// </summary>
        /// <param name="e"></param>
        public void NotifyMessageHandled(MessageHandledEventArgs e)
        {
            try
            {
                HandleMessage(e.Message);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38351");
            }
        }

        /// <summary>
        /// Outputs all events logged to memory to the specified <see paramref="fileName"/>.
        /// </summary>
        /// <param name="fileName">The filename to which the events logged to memory should be
        /// saved.</param>
        public void SaveLoggedData(string fileName)
        {
            try
            {
                if (_loggedInput.Length > 0)
                {
                    File.WriteAllText(fileName, _loggedInput.ToString());
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38352");
            }
        }

        /// <summary>
        /// Clears all events logged to memory.
        /// </summary>
        public void ClearLoggedData()
        {
            try
            {
                _loggedEventCount = 0;
                _loggedInput.Clear();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38353");
            }
        }

        #endregion Methods

        #region MessageFilterBase Overrides

        /// <summary>
        /// Called from <see cref="IMessageFilter.PreFilterMessage"/>.  This method will examine
        /// the <paramref name="message"/> and increment the inputCount if the message
        /// is an input message 
        /// </summary>
        /// <param name="message">The message to be dispatched. You cannot modify this message.</param>
        /// <returns>
        /// <see langword="false"/> to allow the message to continue to the next filter or control.
        /// </returns>
        protected override bool HandleMessage(Message message)
        {
            try
            {
                // Only check the message if event tracking is on
                if (_inputEventsToTrack.Contains(message.Msg))
                {
                    string line = ((WindowsMessageCode)message.Msg).ToString();

                    switch (message.Msg)
                    {
                        case WindowsMessage.KeyUp:
                        case WindowsMessage.KeyDown:
                            {
                                Keys key = KeyMethods.GetKeyFromMessage(message, false);

                                line += ": " + key.ToString();
                            }
                            break;

                        case WindowsMessage.LeftButtonDown:
                        case WindowsMessage.MiddleButtonDown:
                        case WindowsMessage.RightButtonDown:
                        case WindowsMessage.NonClientLeftButtonDown:
                        case WindowsMessage.NonClientMiddleButtonDown:
                        case WindowsMessage.NonClientRightButtonDown:
                            {
                                line += " " + Cursor.Position.ToString();
                            }
                            break;
                    }

                    LogLine("InputEvent", line);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI38354", ex);
            }

            return false;
        }

        #endregion MessageFilterBase Overrides

        #region Private Members

        /// <summary>
        /// Logs the specified <see paramref="logline"/> under the specified
        /// <see paramref="category"/>.
        /// </summary>
        /// <param name="category">The name of the category the line is associated with.</param>
        /// <param name="logline">The log line to log (may contain multiple lines of text).</param>
        void LogLine(string category, string logline)
        {
            if (_logToMemory)
            {
                // Output: [log line #]   [Time]    [Log lines]
                _loggedEventCount++;
                _loggedInput.Append(_loggedEventCount.ToString("D", CultureInfo.InvariantCulture));
                _loggedInput.Append("\t");
                _loggedInput.Append(
                    new DateTime(DateTime.Now.Ticks, DateTimeKind.Unspecified).ToString(
                        "o", CultureInfo.InvariantCulture));
                _loggedInput.Append("\t");
                _loggedInput.AppendLine(category + ": " + logline);
            }
            else
            {
                System.Diagnostics.Trace.WriteLine(logline, category);
            }
        }

        #endregion Private Members
    }
}
