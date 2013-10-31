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
using BitCollectors.PlinkService.Remoting.Helpers;
using BitCollectors.PlinkService.Service.Properties;

namespace BitCollectors.PlinkService.Service.Helpers
{
    public static class TunnelHelper
    {
        public static TunnelStatuses CurrentTunnelStatus { get; set; }

        public static void ChangeTunnelStatus(TunnelStatuses tunnelStatus, bool saveTunnelStatus = true)
        {
            TunnelHelper.CurrentTunnelStatus = tunnelStatus;
            ServiceSettings serviceSettings = SettingsHelper.GetSettings();

            if (saveTunnelStatus)
            {
                Settings settings = new Settings();
                settings.CurrentTunnelStatus = (int)tunnelStatus;
                settings.Save();
            }

            if (tunnelStatus == TunnelStatuses.Started)
            {
                WcfServerHelper.BroadcastRemoteCallback(x => x.EventToLog("Plink Starting", DateTime.Now), true);
                if (ProcessHelper.StartPlink())
                {
                    TunnelPingHelper.OpenTunnelPingListener();
                    WcfServerHelper.BroadcastRemoteCallback(x => x.EventToLog("Plink Started: " + ProcessHelper.GetPlinkArguments(serviceSettings), DateTime.Now), true);
                }
                else
                {
                    ChangeTunnelStatus(TunnelStatuses.Paused);
                }
            }
            else
            {
                WcfServerHelper.BroadcastRemoteCallback(x => x.EventToLog("Plink Stopping", DateTime.Now), true);
                TunnelPingHelper.CloseTunnelPingListener();
                ProcessHelper.StopPlink();
            }

            WcfServerHelper.BroadcastRemoteCallback(x => x.TunnelStatusChanged(tunnelStatus), true);
        }
    }
}
