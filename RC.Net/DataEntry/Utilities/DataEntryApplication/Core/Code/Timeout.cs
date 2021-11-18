using Extract.Utilities.Forms;
using System;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;
using Timer = System.Threading.Timer;

namespace Extract.DataEntry.Utilities.DataEntryApplication
{
    public interface IApplicationWithInactivityTimeout
    {
        IFileProcessingDB FileProcessingDB { get; }
        Action EndProcessingAction { get; }
        Control HostControl { get; }
    }

    internal class Timeout : IMessageFilter, IDisposable
    {
        /// <summary>
        /// Represents how much time a user must sit idle before a timeout.
        /// </summary>
        private readonly TimeSpan SessionTimeout;
        private TimeSpan TimeToDisplayTimeoutWarning = TimeSpan.FromSeconds(20);
        private TimeSpan TimeRemainingUntilTimeout = TimeSpan.FromSeconds(60);
        private DateTime LastUserInputDetected = DateTime.Now;
        private Point LastMousePosition;
        private CancellationTokenSource WarningOverlayCanceller;
        private bool TimeoutWarningOverlayShowing;
        private bool TimeoutTriggering;
        private Timer Timer;

        private readonly IApplicationWithInactivityTimeout _ApplicationWithInactivityTimeout;
        

        /// <summary>
        /// A base constructor for the Timeout class.
        /// </summary>
        /// <param name="fileProcessingDB">An instance of the file processing db.</param>
        /// <param name="endProcessingAction">This action should call anything necessary to end processing from where it is called.</param>
        /// <param name="controlToDisplayTimeoutWarning">This will be the control for invoking changes on, and overlaying text on.</param>
        public Timeout(IApplicationWithInactivityTimeout applicationWithInactivityTimeout)
        {
            Application.AddMessageFilter(this);
            _ApplicationWithInactivityTimeout = applicationWithInactivityTimeout;
            this.SessionTimeout = TimeSpan.FromMinutes(Int32.Parse(applicationWithInactivityTimeout.FileProcessingDB.GetDBInfoSetting("VerificationSessionTimeout", true), CultureInfo.InvariantCulture));
            SetupInactivityTimeout();
        }

        /// <summary>
        /// Creates and starts the timeout timer.
        /// </summary>
        private void SetupInactivityTimeout()
        {
            // If a user cannot be timed out, there is no need for this task.
            if (this.SessionTimeout.Minutes <= 0)
            {
                return;
            }

            Timer = new Timer(CheckInactivityTimeout, null, 1000, 1000);
        }

        /// <summary>
        /// Times a user out if activity is not detected for a time specified in the database.
        /// </summary>
        private void CheckInactivityTimeout(object stateInfo)
        {
            DateTime timeoutTrigger = LastUserInputDetected.AddMinutes(SessionTimeout.TotalMinutes);
            TimeRemainingUntilTimeout = timeoutTrigger.Subtract(DateTime.Now);

            _ApplicationWithInactivityTimeout.HostControl.SafeBeginInvoke("ELI52979", () =>
            {
                if(TimeoutTriggering)
                {
                    return;
                }
                else if (TimeRemainingUntilTimeout.TotalSeconds <= 0)
                {
                    TimeoutTriggering = true;
                    _ApplicationWithInactivityTimeout.EndProcessingAction();
                }
                else if(TimeoutWarningOverlayShowing)
                {
                    _ApplicationWithInactivityTimeout.HostControl.Refresh();
                }
                else if (TimeRemainingUntilTimeout <= TimeToDisplayTimeoutWarning)
                {
                    ShowTimeoutWarning(CalculateTimeoutText, (int)Math.Round(TimeRemainingUntilTimeout.TotalSeconds));
                }
            });
        }

        /// <summary>
        /// Calculates to the nearest second how long a user has before timeout.
        /// </summary>
        /// <param name="timeoutTrigger">The datetime that a timeout will trigger.</param>
        /// <returns></returns>
        private string CalculateTimeoutText()
        {
            return $"Closing session in {Math.Round(TimeRemainingUntilTimeout.TotalSeconds,0)} seconds.";
        }

        /// <summary>
        /// Shows the timeout warning text overlay.
        /// </summary>
        /// <param name="calculateTimeoutText">A function that calculates the number of seconds remaining untill timeout.</param>
        /// <param name="secondsToDisplayTimeoutWarning">How long the timeout warning should be displayed.</param>
        private void ShowTimeoutWarning(Func<string> calculateTimeoutText, int secondsToDisplayTimeoutWarning)
        {
            TimeoutWarningOverlayShowing = true;
            WarningOverlayCanceller = OverlayText.ShowText(
                     target: _ApplicationWithInactivityTimeout.HostControl
                     , textProvider: calculateTimeoutText
                     , font: _ApplicationWithInactivityTimeout.HostControl.Font
                     , color: Color.Red
                     , stringFormat: null
                     , displayTime: secondsToDisplayTimeoutWarning);
            
        }

        public bool PreFilterMessage(ref Message m)
        {
            try
            {
                switch (m.Msg)
                {
                    case WindowsMessage.MouseMove:
                        Point currentMousePosition = Control.MousePosition;
                        if (LastMousePosition != currentMousePosition)
                        {
                            LastMousePosition = currentMousePosition;
                            UserInputDetected();
                        }
                        break;
                    case WindowsMessage.KeyDown:
                    case WindowsMessage.SystemKeyDown:
                    case WindowsMessage.LeftButtonDown:
                    case WindowsMessage.RightButtonDown:
                    case WindowsMessage.MiddleButtonDown:
                    case WindowsMessage.MouseWheel:
                    case WindowsMessage.NonClientLeftButtonDown:
                    case WindowsMessage.NonClientRightButtonDown:
                    case WindowsMessage.NonClientMiddleButtonDown:
                        UserInputDetected();
                        break;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI52981");
            }
            return false;
        }

        public void Dispose()
        {
            Application.RemoveMessageFilter(this);
            Timer.Dispose();
            RemoveWarningOverlay();
        }

        private void RemoveWarningOverlay()
        {
            if (this.WarningOverlayCanceller != null)
            {
                this.WarningOverlayCanceller.Cancel();
                this.WarningOverlayCanceller.Dispose();
                this.WarningOverlayCanceller = null;
            }
        }

        /// <summary>
        /// Logs a user input being detected, and cancels any timeout warnings.
        /// </summary>
        private void UserInputDetected()
        {
            try
            {
                TimeoutWarningOverlayShowing = false;
                LastUserInputDetected = DateTime.Now;
                RemoveWarningOverlay();
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI52987");
            }
        }
    }
}
