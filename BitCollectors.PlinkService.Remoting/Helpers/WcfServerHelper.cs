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
using System.ServiceModel;
using System.Threading.Tasks;
using BitCollectors.PlinkService.Remoting.Interfaces;

namespace BitCollectors.PlinkService.Remoting.Helpers
{
    public static class WcfServerHelper
    {
        public static ServiceHost RemoteServiceHost { get; set; }
        public static List<IServiceRemotingCallback> CallbackList { get; set; }
        public static DateTime LastPingToClient { get; set; }

        public static void Start(Type remotingType)
        {
            RemoteServiceHost = new ServiceHost(remotingType, new Uri[] {
                new Uri("net.pipe://localhost/BitCollectors.PlinkService")
            });

            RemoteServiceHost.AddServiceEndpoint(typeof(IServiceRemoting), new NetNamedPipeBinding(), "PlinkService");

            RemoteServiceHost.Open();
        }

        public static void Stop()
        {
            if (RemoteServiceHost != null && RemoteServiceHost.State == CommunicationState.Opened)
            {
                RemoteServiceHost.Close();
            }
        }

        public static void BroadcastRemoteCallback(Action<IServiceRemotingCallback> actionDelegate, bool newThread = false)
        {
            if (CallbackList != null && CallbackList.Count > 0)
            {
                if (newThread)
                {
                    Task.Factory.StartNew(() =>
                    {
                        BroadcastRemoteCallback(actionDelegate, false);
                    });
                }
                else
                {
                    Action<IServiceRemotingCallback> invoke =
                        (IServiceRemotingCallback x) =>
                        {
                            try
                            {
                                actionDelegate(x);
                            }
                            catch (CommunicationObjectAbortedException)
                            {
                                CallbackList.Remove(x);
                            }
                        };

                    try
                    {
                        CallbackList.ForEach(invoke);
                    }
                    catch (CommunicationObjectAbortedException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
    }
}
