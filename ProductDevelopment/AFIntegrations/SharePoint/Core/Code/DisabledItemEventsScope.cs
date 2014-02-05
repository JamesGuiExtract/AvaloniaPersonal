using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace Extract.SharePoint
{
    /// <summary>
    /// Class used to disable event firing within a scope on a thread
    /// Expected usage is in a using (new DisabledItemEventsScope) when performing update();
    /// </summary>
    public class DisabledItemEventsScope:SPItemEventReceiver, IDisposable
    {
        private bool eventFiringEnabledStatus;

        /// <summary>
        /// Constructor saves previous state
        /// </summary>
        public DisabledItemEventsScope()
        {
            eventFiringEnabledStatus = base.EventFiringEnabled;
            base.EventFiringEnabled = false;
        }

        /// <summary>
        /// Destructor restores previous stated
        /// </summary>
        public void Dispose()
        {
            base.EventFiringEnabled = eventFiringEnabledStatus;
        }
    }
}
