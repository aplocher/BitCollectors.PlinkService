// This file is part of BitCollectors Plink Service.
// Copyright 2013 Adam Plocher (BitCollectors)
// 
// BitCollectors Plink Service is free software: you can redistribute it and/or 
// modify it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// BitCollectors Plink Service is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for 
// more details.
// 
// You should have received a copy of the GNU General Public License along with 
// BitCollectors Plink Service.  If not, see <http://www.gnu.org/licenses/>.

using System;
using BitCollectors.PlinkService.Remoting.Entities;
using BitCollectors.PlinkService.Remoting.Enums;
using BitCollectors.PlinkService.Remoting.Interfaces;

namespace BitCollectors.PlinkService.UI.Remoting
{
    public class ServiceRemotingCallback : IServiceRemotingCallback
    {
        public delegate void ServerStoppingEventDelegate(object sender, EventArgs eventArgs);
        public delegate void EventToLogEventDelegate(object sender, string eventText, DateTime eventTime);
        public delegate void PingEventDelegate(object sender, bool plinkRunning, bool tunnelValidationRunning, DateTime tunnelValidationLastRan);
        public delegate void TestConnectionCallbackDelegate(object sender, PlinkStatus plinkStatus);
        public delegate void TunnelStatusChangedCallbackDelegate(object sender, TunnelStatuses tunnelStatus);
        public delegate void PlinkTextOutputEventDelegate(object sender, string eventText, DateTime eventTime);
        public delegate void SaveSettingsCallbackEventDelegate(object sender, ServiceSettings serviceSettings);

        public static event ServerStoppingEventDelegate ServerStoppingEvent;
        public static event EventToLogEventDelegate EventToLogEvent;
        public static event PingEventDelegate PingEvent;
        public static event TestConnectionCallbackDelegate TestConnectionCallbackEvent;
        public static event TunnelStatusChangedCallbackDelegate TunnelStatusChangedEvent;
        public static event PlinkTextOutputEventDelegate PlinkTextOutputEvent;
        public static event SaveSettingsCallbackEventDelegate SaveSettingsCallbackEvent;

        public static DateTime LastPingDateTime = new DateTime();

        public static void ClearAllEventHandlers()
        {
            if (ServerStoppingEvent != null)
            {
                foreach (Delegate d in ServerStoppingEvent.GetInvocationList())
                {
                    ServerStoppingEvent -= (ServerStoppingEventDelegate)d;
                }
            }

            if (EventToLogEvent != null)
            {
                foreach (Delegate d in EventToLogEvent.GetInvocationList())
                {
                    EventToLogEvent -= (EventToLogEventDelegate)d;
                }
            }

            if (PingEvent != null)
            {
                foreach (Delegate d in PingEvent.GetInvocationList())
                {
                    PingEvent -= (PingEventDelegate)d;
                }
            }

            if (TestConnectionCallbackEvent != null)
            {
                foreach (Delegate d in TestConnectionCallbackEvent.GetInvocationList())
                {
                    TestConnectionCallbackEvent -= (TestConnectionCallbackDelegate)d;
                }
            }

            if (TunnelStatusChangedEvent != null)
            {
                foreach (Delegate d in TunnelStatusChangedEvent.GetInvocationList())
                {
                    TunnelStatusChangedEvent -= (TunnelStatusChangedCallbackDelegate)d;
                }
            }

            if (PlinkTextOutputEvent != null)
            {
                foreach (Delegate d in PlinkTextOutputEvent.GetInvocationList())
                {
                    PlinkTextOutputEvent -= (PlinkTextOutputEventDelegate)d;
                }
            }

            if (SaveSettingsCallbackEvent != null)
            {
                foreach (Delegate d in SaveSettingsCallbackEvent.GetInvocationList())
                {
                    SaveSettingsCallbackEvent -= (SaveSettingsCallbackEventDelegate)d;
                }
            }
        }

        public void EventToLog(string eventText, DateTime eventTime)
        {
            if (EventToLogEvent != null)
            {
                EventToLogEvent(this, eventText, eventTime);
            }
        }

        public void ServerStopping()
        {
            if (ServerStoppingEvent != null)
            {
                ServerStoppingEvent(this, new EventArgs());
            }
        }

        public void TestConnectionCallback(PlinkStatus plinkStatus)
        {
            if (TestConnectionCallbackEvent != null)
            {
                TestConnectionCallbackEvent(this, plinkStatus);
            }
        }

        public void TunnelStatusChanged(TunnelStatuses tunnelStatus)
        {
            if (TunnelStatusChangedEvent != null)
            {
                TunnelStatusChangedEvent(this, tunnelStatus);
            }
        }

        public void Ping(bool plinkRunning, bool tunnelValidationRunning, DateTime tunnelValidationLastRan)
        {
            LastPingDateTime = DateTime.Now;

            if (PingEvent != null)
            {
                PingEvent(this, plinkRunning, tunnelValidationRunning, tunnelValidationLastRan);
            }
        }


        public void PlinkTextOutput(string eventText, DateTime eventTime)
        {
            if (PlinkTextOutputEvent != null)
            {
                PlinkTextOutputEvent(this, eventText, DateTime.Now);
            }
        }

        public void SaveSettingsCallback(ServiceSettings serviceSettings)
        {
            if (SaveSettingsCallbackEvent != null)
            {
                SaveSettingsCallbackEvent(this, serviceSettings);
            }
        }
    }
}
