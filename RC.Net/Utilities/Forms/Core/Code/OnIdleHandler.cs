using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// This class is used to simplify executing code in UI when it next goes idle.
    /// This differs from using BeginInvoke to execute actions via the message pump since messages
    /// already in the queue may result in further messages getting queued. This class executes only
    /// after all other messages that are to occur as part of the current message chain complete.
    /// </summary>
    public class OnIdleHandler : MessageFilterBase
    {
        /// <summary>
        /// Indicates whether the host is currently idle (message pump is empty)
        /// </summary>
        bool _isIdle = true;

        /// <summary>
        /// The target control for which scheduled actions are run.
        /// </summary>
        Control _control;

        /// <summary>
        /// Commands that should be executed the next time the host is idle along with ELI codes
        /// that should be attributed to any exceptions.
        /// </summary>
        Queue<Tuple<Action, string>> _idleCommands = new Queue<Tuple<Action, string>>();

        public OnIdleHandler(Control control)
            : base(control)
        {
            _control = control;

            Application.Idle += HandleApplicationIdle;
        }

        /// <summary>
        /// Executes the provided <see paramref="action"/> only after the DEP's message pump is
        /// completely empty. This differs from using BeginInvoke to execute it via the message
        /// pump since messages already in the queue may result in further messages getting queued.
        /// Therefore, this call ensures that all other messages that are to occur as part of the
        /// current message chain occur before the specified <see paramref="action"/>.
        /// </summary>
        /// <param name="eliCode">The ELI code to attribute to any exception.</param>
        /// <param name="action">The action to execute once the DEP's message pump is empty.</param>
        public void Execute(string eliCode, Action action)
        {
            try
            {
                if (_isIdle)
                {
                    _control.SafeBeginInvoke(eliCode, () => action());
                }
                else
                {
                    _idleCommands.Enqueue(new Tuple<Action, string>(action, eliCode));
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49892");
            }
        }

        /// <summary>
        /// Handles the <see cref="Application.Idle"/> event in order to execute any pending
        /// commands from <see cref="ExecuteOnIdle"/>.
        /// </summary>
        void HandleApplicationIdle(object sender, EventArgs e)
        {
            try
            {
                if (_idleCommands.Count > 0)
                {
                    Tuple<Action, string> command = _idleCommands.Dequeue();
                    _control.SafeBeginInvoke(command.Item2, () => command.Item1());
                }
                else
                {
                    _isIdle = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI49893");
            }
        }

        /// <summary>
        /// Override of MessageFilterBase to notify of messages processed.
        /// </summary>
        protected override bool HandleMessage(Message message)
        {
            if (_isIdle)
            {
                _isIdle = false;
            }

            return false;
        }
    }
}
