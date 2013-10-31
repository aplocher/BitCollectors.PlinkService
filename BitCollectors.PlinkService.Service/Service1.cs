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
using System.ServiceProcess;
using System.Threading;
using System.Timers;
using BitCollectors.PlinkService.Remoting;
using BitCollectors.PlinkService.Remoting.Enums;
using BitCollectors.PlinkService.Remoting.Helpers;
using BitCollectors.PlinkService.Service.Helpers;
using BitCollectors.PlinkService.Service.Properties;
using BitCollectors.PlinkService.Service.Remoting;

namespace BitCollectors.PlinkService.Service
{
    public partial class Service1 : ServiceBase
    {
        private System.Timers.Timer _pingClientTimer;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }

            WcfServerHelper.Start(typeof(ServiceRemoting));

            _pingClientTimer = new System.Timers.Timer(Constants.ServerPingInterval * 1000);
            _pingClientTimer.AutoReset = true;
            _pingClientTimer.Elapsed += pingTimer_Elapsed;
            _pingClientTimer.Start();

            Settings settings = new Settings();
            if (settings.CurrentTunnelStatus == (int)TunnelStatuses.Started)
            {
                TunnelHelper.ChangeTunnelStatus(TunnelStatuses.Started, false);
            }
        }

        protected override void OnStop()
        {
            TunnelHelper.ChangeTunnelStatus(TunnelStatuses.Paused, false);
            Thread.Sleep(300);

            WcfServerHelper.BroadcastRemoteCallback(x => x.ServerStopping(), true);
            Thread.Sleep(300);

            WcfServerHelper.Stop();
        }

        private void pingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (WcfServerHelper.LastPingToClient.AddSeconds(5) < DateTime.Now)
            {
                ClientHelper.PingClient();
            }
        }
    }
}
