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
using System.ServiceModel;
using System.Threading.Tasks;
using BitCollectors.PlinkService.Remoting.Interfaces;

namespace BitCollectors.PlinkService.Remoting.Helpers
{
    public static class WcfClientHelper
    {
        public static string FaultMessage { get; set; }
        public static IServiceRemoting RemotingObject { get; set; }
        public static ChannelFactory<IServiceRemoting> PipeFactory { get; set; }

        public static bool IsConnected
        {
            get
            {
                if (RemotingObject != null && PipeFactory != null && PipeFactory.State == CommunicationState.Opened)
                {
                    try
                    {
                        RemotingObject.Ping();

                        return true;
                    }
                    catch(Exception)
                    {
                        return false;
                    }
                }

                return false;
            }
        }

        public static void ConnectIpc(IServiceRemotingCallback serviceRemotingCallback)
        {
            InstanceContext instanceContext = new InstanceContext(serviceRemotingCallback);

            PipeFactory = new DuplexChannelFactory<IServiceRemoting>(instanceContext, new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/BitCollectors.PlinkService/PlinkService"));
            RemotingObject = PipeFactory.CreateChannel();
        }

        public static void DisconnectIpc(bool unregister = true)
        {
            if (unregister && RemotingObject != null)
            {
                RemotingObject.UnregisterCallbackClient();
            }

            if (unregister && PipeFactory != null)
            {
                PipeFactory.Close();
            }

            RemotingObject = null;
            PipeFactory = null;
        }

        public static void ExecuteRemoteAction(Action<IServiceRemoting> actionDelegate, bool newThread = false)
        {
            if (newThread)
            {
                Task.Factory.StartNew(() =>
                {
                    ExecuteRemoteAction(actionDelegate, false);
                });
            }
            else
            {
                try
                {
                    actionDelegate(WcfClientHelper.RemotingObject);
                }
                catch (CommunicationException e)
                {
                    FaultMessage = e.GetType().ToString() + " - " + e.Message;
                    WcfClientHelper.PipeFactory.Abort();
                }
                catch (TimeoutException e)
                {
                    FaultMessage = e.GetType().ToString() + " - " + e.Message;
                    WcfClientHelper.PipeFactory.Abort();
                }
                catch (Exception)
                {
                    WcfClientHelper.PipeFactory.Abort();
                    throw;
                }
            }
        }
    }
}
