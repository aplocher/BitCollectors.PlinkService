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
using BitCollectors.PlinkService.Remoting.Helpers;

namespace BitCollectors.PlinkService.Service.Helpers
{
    public static class ClientHelper
    {
        public static void PingClient()
        {
            WcfServerHelper.BroadcastRemoteCallback(x => x.Ping(
                ProcessHelper.PlinkRunning,
                (TunnelPingHelper.TunnelValidatorLastPingSuccessful && TunnelPingHelper.TunnelValidatorRunning),
                TunnelPingHelper.TunnelValidatorLastSuccessfulPingTime));

            WcfServerHelper.LastPingToClient = DateTime.Now;
        }
    }
}
