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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using BitCollectors.PlinkService.Remoting.Entities;
using BitCollectors.PlinkService.Remoting.Enums;
using BitCollectors.PlinkService.Remoting.Helpers;
using BitCollectors.PlinkService.Remoting.Interfaces;
using BitCollectors.PlinkService.Service.Helpers;

namespace BitCollectors.PlinkService.Service.Remoting
{
    public class ServiceRemoting : IServiceRemoting
    {
        #region Public Methods 
        public void ChangeTunnelStatus(TunnelStatuses tunnelStatus)
        {
            TunnelHelper.ChangeTunnelStatus(tunnelStatus);
        }

        public ServiceSettings GetSettings()
        {
            ServiceSettings serviceSettings = SettingsHelper.GetSettings();

            if (string.IsNullOrEmpty(serviceSettings.PlinkExecutable))
            {
                string assemblyLocation = Assembly.GetEntryAssembly().Location;
                FileInfo assemblyFileInfo = new FileInfo(assemblyLocation);
                string plinkPath = Path.Combine(assemblyFileInfo.DirectoryName, "plink.exe");

                if (File.Exists(plinkPath))
                {
                    serviceSettings.PlinkExecutable = plinkPath;
                }
            }

            return serviceSettings;
        }

        public TunnelStatuses GetTunnelStatus()
        {
            return TunnelHelper.CurrentTunnelStatus;
        }

        public void Ping()
        {
        }

        public void RegisterCallbackClient()
        {
            if (WcfServerHelper.CallbackList == null)
            {
                WcfServerHelper.CallbackList = new List<IServiceRemotingCallback>();
            }

            IServiceRemotingCallback callback = OperationContext.Current.GetCallbackChannel<IServiceRemotingCallback>();

            if (!WcfServerHelper.CallbackList.Contains(callback))
            {
                WcfServerHelper.CallbackList.Add(callback);
            }

            Task.Factory.StartNew(() =>
                {
                    try
                    {
                        callback.TunnelStatusChanged(TunnelHelper.CurrentTunnelStatus);
                        if (TunnelHelper.CurrentTunnelStatus == TunnelStatuses.Started)
                        {
                            Thread.Sleep(70);

                            callback.Ping(
                                ProcessHelper.PlinkRunning,
                                (TunnelPingHelper.TunnelValidatorLastPingSuccessful && TunnelPingHelper.TunnelValidatorRunning),
                                TunnelPingHelper.TunnelValidatorLastSuccessfulPingTime);
                        }
                    }
                    catch
                    {
                    }
                });
        }

        public void SaveSettings(ServiceSettings serviceSettings, bool restartPlink)
        {
            SettingsHelper.SaveSettings(serviceSettings);

            WcfServerHelper.BroadcastRemoteCallback(x => x.SaveSettingsCallback(SettingsHelper.GetSettings()), true);

            if (restartPlink && TunnelHelper.CurrentTunnelStatus == TunnelStatuses.Started)
            {
                TunnelHelper.ChangeTunnelStatus(TunnelStatuses.Paused, false);
                Thread.Sleep(400);
                TunnelHelper.ChangeTunnelStatus(TunnelStatuses.Started, false);
            }
        }

        public void TestConnection(ServiceSettings serviceSettings)
        {
            Task.Factory.StartNew(() =>
            {
                ProcessHelper.StartPlink(serviceSettings, true);
            });
        }

        public void UnregisterCallbackClient()
        {
            IServiceRemotingCallback callback = OperationContext.Current.GetCallbackChannel<IServiceRemotingCallback>();

            if (WcfServerHelper.CallbackList != null && WcfServerHelper.CallbackList.Contains(callback))
            {
                WcfServerHelper.CallbackList.Remove(callback);
            }
        }
        #endregion   
    }
}
