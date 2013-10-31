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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using BitCollectors.PlinkService.Remoting.Entities;
using BitCollectors.PlinkService.Remoting.Enums;
using BitCollectors.PlinkService.Remoting.Helpers;

namespace BitCollectors.PlinkService.Service.Helpers
{
    public static class TunnelPingHelper
    {
        #region Fields
        private static System.Timers.Timer _pingTunnelTimer = null;
        private static bool _tunnelValidatorLastPingSuccessful = false;
        private static TcpListener _validationListener = null;
        #endregion   

        #region Properties
        public static int TunnelValidatorConsecutiveFailures 
        { 
            get; 
            set; 
        }

        public static bool TunnelValidatorLastPingSuccessful
        {
            get
            {
                return _tunnelValidatorLastPingSuccessful;
            }
            set
            {
                _tunnelValidatorLastPingSuccessful = value;

                if (_tunnelValidatorLastPingSuccessful)
                {
                    TunnelValidatorLastSuccessfulPingTime = DateTime.Now;
                    TunnelValidatorConsecutiveFailures = 0;
                }
            }
        }

        public static DateTime TunnelValidatorLastSuccessfulPingTime 
        { 
            get; 
            set; 
        }

        public static bool TunnelValidatorRunning
        {
            get;
            set;
        }
        #endregion   

        #region Public Methods
        public static void CloseTunnelPingListener()
        {
            if (TunnelPingHelper.TunnelValidatorRunning)
            {
                TunnelPingHelper.TunnelValidatorRunning = false;

                WcfServerHelper.BroadcastRemoteCallback((x) => x.EventToLog("Tunnel Validation Closing", DateTime.Now), true);

                try
                {
                    if (_pingTunnelTimer != null && _pingTunnelTimer.Enabled)
                    {
                        _pingTunnelTimer.Stop();
                        _pingTunnelTimer.Dispose();
                    }

                    if (_validationListener != null)
                    {
                        _validationListener.Stop();
                    }
                }
                catch (Exception ex)
                {
                    WcfServerHelper.BroadcastRemoteCallback((x) => x.EventToLog(ex.Message +" ::: "+ ex.StackTrace, DateTime.Now), true);
                }
            }
        }

        public static void OpenTunnelPingListener()
        {
            if (TunnelValidatorRunning)
                CloseTunnelPingListener();

            ServiceSettings serviceSettings = SettingsHelper.GetSettings();

            if (TunnelHelper.CurrentTunnelStatus == TunnelStatuses.Started && serviceSettings.EnableTunnelValidation)
            {
                TunnelPingHelper.TunnelValidatorLastPingSuccessful = false;

                _validationListener = new TcpListener(IPAddress.Any, serviceSettings.TunnelValidationRemotePort);

                Task.Factory.StartNew(() =>
                {
                    TunnelPingHelper.TunnelValidatorRunning = true;

                    _validationListener.Start();

                    try
                    {
                        while (true)
                        {
                            TcpClient tcpClient;

                            try
                            {
                                tcpClient = _validationListener.AcceptTcpClient();
                            }
                            catch (SocketException ex)
                            {
                                if (ex.SocketErrorCode == SocketError.Interrupted)
                                {
                                    TunnelPingHelper.TunnelValidatorRunning = false;
                                    break;
                                }
                                else
                                {
                                    throw;
                                }
                            }

                            Task.Factory.StartNew(() =>
                            {
                                try
                                {
                                    NetworkStream clientStream = tcpClient.GetStream();
                                    byte[] message = new byte[32];
                                    string validationString = "";

                                    while (true)
                                    {
                                        int bytesRead = clientStream.Read(message, 0, 32);

                                        if (bytesRead == 0)
                                        {
                                            break;
                                        }

                                        ASCIIEncoding encoding = new ASCIIEncoding();
                                        string str = encoding.GetString(message, 0, bytesRead);

                                        validationString += str;

                                        if (validationString == "-111-")
                                        {
                                            const string sendString = "-222-";
                                            byte[] sendData = Encoding.ASCII.GetBytes(sendString);
                                            clientStream.Write(sendData, 0, sendData.Length);

                                            break;
                                        }
                                        else if (validationString.Length > 5)
                                        {
                                            break;
                                        }
                                    }

                                    tcpClient.Close();
                                }
                                catch (Exception ex)
                                {
                                    WcfServerHelper.BroadcastRemoteCallback((x) => x.EventToLog("Tunnel Validation Exception (1): " + ex.Message + " ::: " + ex.StackTrace, DateTime.Now));
                                    HandleFailedTunnelPing();
                                }
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        WcfServerHelper.BroadcastRemoteCallback((x) => x.EventToLog("Tunnel Validation Exception (2): " + ex.Message + " ::: " + ex.StackTrace, DateTime.Now));
                        HandleFailedTunnelPing();
                    }
                }, TaskCreationOptions.LongRunning);
                
                _pingTunnelTimer = new System.Timers.Timer();
                _pingTunnelTimer.Interval = serviceSettings.TunnelValidationPingInterval * 1000;
                _pingTunnelTimer.AutoReset = true;
                _pingTunnelTimer.Elapsed += _pingTunnelTimer_Elapsed;

                Thread.Sleep(1200);
                SendTunnelPing();

                _pingTunnelTimer.Start();
            }
        }

        public static void SendTunnelPing()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    ServiceSettings serviceSettings = SettingsHelper.GetSettings();

                    TcpClient tcpClient = new TcpClient("localhost", serviceSettings.TunnelValidationLocalPort);

                    using (NetworkStream stream = tcpClient.GetStream())
                    {
                        byte[] sendData = Encoding.ASCII.GetBytes("-111-");
                        stream.Write(sendData, 0, sendData.Length);

                        byte[] responseData = new byte[32];

                        if (stream.CanRead)
                        {
                            int bytesReceived = stream.Read(responseData, 0, responseData.Length);

                            string responseString = Encoding.ASCII.GetString(responseData, 0, bytesReceived);

                            if (responseString.Trim() == "-222-")
                            {
                                WcfServerHelper.BroadcastRemoteCallback((x) => x.EventToLog("Tunnel Validation: Complete", DateTime.Now));

                                TunnelValidatorLastPingSuccessful = true;
                                TunnelValidatorLastSuccessfulPingTime = DateTime.Now;
                            }
                            else
                            {
                                HandleFailedTunnelPing();
                            }
                        }
                        else
                        {
                            WcfServerHelper.BroadcastRemoteCallback((x) => x.EventToLog("Tunnel Validation: Sender Error (1)", DateTime.Now));
                            HandleFailedTunnelPing();
                        }

                        stream.Close();
                    }

                    tcpClient.Close();
                }
                catch (Exception ex)
                {
                    WcfServerHelper.BroadcastRemoteCallback((x) => x.EventToLog("Tunnel Validation Sender Exception (1): " + ex.Message + " ::: " + ex.StackTrace, DateTime.Now));
                    HandleFailedTunnelPing();
                }
                finally
                {
                    ClientHelper.PingClient();
                }
            });
        }
        #endregion   

        #region Private Methods 
        private static void _pingTunnelTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SendTunnelPing();
        }

        private static void HandleFailedTunnelPing()
        {
            WcfServerHelper.BroadcastRemoteCallback(x => x.EventToLog("Tunnel ping failed! (consecutive failures: " + TunnelValidatorConsecutiveFailures.ToString() + ")", DateTime.Now));
            TunnelValidatorLastPingSuccessful = false;

            if (TunnelValidatorConsecutiveFailures < 3)
            {
                CloseTunnelPingListener();
                WcfServerHelper.BroadcastRemoteCallback(x => x.EventToLog("Restarting Tunnel Validation", DateTime.Now));
                Thread.Sleep(800);
                OpenTunnelPingListener();
            }
            else if (TunnelValidatorConsecutiveFailures < 6)
            {
                WcfServerHelper.BroadcastRemoteCallback(x => x.EventToLog("Restarting Plink.exe", DateTime.Now));

                Thread.Sleep(1000);
                ProcessHelper.StopPlink();
            }
            else
            {
                WcfServerHelper.BroadcastRemoteCallback(x => x.EventToLog("Restarting Plink.exe AND Tunnel Validation", DateTime.Now));

                CloseTunnelPingListener();
                Thread.Sleep(500);

                ProcessHelper.StopPlink();
                Thread.Sleep(1500);
                
                OpenTunnelPingListener();
            }

            TunnelValidatorConsecutiveFailures++;
        }
        #endregion   
    }
}
