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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BitCollectors.PlinkService.Remoting.Entities;
using BitCollectors.PlinkService.Remoting.Enums;
using BitCollectors.PlinkService.Remoting.Helpers;
using BitCollectors.PlinkService.Service.JobObjects;

namespace BitCollectors.PlinkService.Service.Helpers
{
    public static class ProcessHelper
    {
        #region Fields
        private static Process _process;
        private static ProcessStartInfo _processInfo;
        #endregion

        #region Properties
        public static bool PlinkRunning { get; set; }
        #endregion

        #region Public Methods
        public static string GetPlinkArguments(ServiceSettings serviceSettings)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("-ssh -2 -N -v -P ");
            sb.Append(serviceSettings.SshPort);
            sb.Append(" ");

            if (serviceSettings.EnableCompression)
            {
                sb.Append("-C ");
            }

            if (serviceSettings.EnableDynamicSocks)
            {
                sb.Append("-D ");
                sb.Append("\"");
                if (!string.IsNullOrEmpty(serviceSettings.DynamicSocksHost))
                {
                    sb.Append(serviceSettings.DynamicSocksHost);
                    sb.Append(":");
                }
                sb.Append(serviceSettings.DynamicSocksPort);
                sb.Append("\" ");
            }

            if (serviceSettings.LocalTunnels != null && serviceSettings.LocalTunnels.Trim().Length > 0)
            {
                string[] localTunnels = StringHelper.ParseStringLines(serviceSettings.LocalTunnels);

                foreach (string line in localTunnels)
                {
                    sb.Append("-L \"");
                    sb.Append(line.Trim());
                    sb.Append("\" ");
                }
            }

            if (serviceSettings.RemoteTunnels != null && serviceSettings.RemoteTunnels.Trim().Length > 0)
            {
                string[] remoteTunnels = StringHelper.ParseStringLines(serviceSettings.RemoteTunnels);

                foreach (string line in remoteTunnels)
                {
                    sb.Append("-R \"");
                    sb.Append(line.Trim());
                    sb.Append("\" ");
                }
            }

            if (serviceSettings.EnableTunnelValidation && serviceSettings.TunnelValidationLocalPort > 0 && serviceSettings.TunnelValidationRemotePort > 0)
            {
                sb.Append("-L \"localhost:");
                sb.Append(serviceSettings.TunnelValidationLocalPort);
                sb.Append(":localhost:");
                sb.Append(serviceSettings.TunnelValidationLocalPort);
                sb.Append("\" -R \"localhost:");
                sb.Append(serviceSettings.TunnelValidationLocalPort);
                sb.Append(":localhost:");
                sb.Append(serviceSettings.TunnelValidationRemotePort);
                sb.Append("\" ");
            }

            sb.Append("\"");
            sb.Append(serviceSettings.SshUsername);
            sb.Append("@");
            sb.Append(serviceSettings.SshHostname);
            sb.Append("\"");

            return sb.ToString();
        }

        public static void StopPlink()
        {
            if (TunnelHelper.CurrentTunnelStatus == TunnelStatuses.Paused)
            {
                TunnelPingHelper.CloseTunnelPingListener();
            }

            if (_process != null && !_process.HasExited)
            {
                _process.Kill();
            }

            PlinkRunning = false;

            ClientHelper.PingClient();
        }

        public static bool StartPlink(ServiceSettings serviceSettings = null, bool testConnection = false)
        {
            if (serviceSettings == null)
            {
                serviceSettings = SettingsHelper.GetSettings();
            }

            if (string.IsNullOrEmpty(serviceSettings.PlinkExecutable) || !File.Exists(serviceSettings.PlinkExecutable))
            {
                WcfServerHelper.BroadcastRemoteCallback(x => x.TestConnectionCallback(PlinkStatus.ExecutableNotFound));
                return false;
            }

            serviceSettings.EnableVerbose = true;

            _processInfo = new ProcessStartInfo();
            _processInfo.FileName = serviceSettings.PlinkExecutable;
            _processInfo.Arguments = GetPlinkArguments(serviceSettings);
            _processInfo.RedirectStandardOutput = true;
            _processInfo.RedirectStandardError = true;
            _processInfo.RedirectStandardInput = true;
            _processInfo.UseShellExecute = false;
            _processInfo.CreateNoWindow = true;

            _process = new Process();
            _process.StartInfo = _processInfo;
            _process.Start();
            _process.StandardInput.NewLine = Environment.NewLine;

            if (!testConnection)
            {
                _process.Exited += process_Exited;
                PlinkRunning = true;

                try
                {
                    PlinkJobObject plinkJobObject = new PlinkJobObject();
                    plinkJobObject.AddProcess(_process.Id);
                }
                catch (Exception ex)
                {
                    WcfServerHelper.BroadcastRemoteCallback(x => x.EventToLog(ex.Message + " ::: " + ex.StackTrace, DateTime.Now));
                    return false;
                }

                ClientHelper.PingClient();
            }

            Task.Factory.StartNew(() => ProcessOutputCharacters(_process.StandardError, serviceSettings, testConnection, true));

            Task.Factory.StartNew(() => ProcessOutputCharacters(_process.StandardOutput, serviceSettings, testConnection, false));

            return true;
        }
        #endregion

        #region Private Methods
        private static void process_Exited(object sender, EventArgs e)
        {
            WcfServerHelper.BroadcastRemoteCallback(x => x.EventToLog("Plink Exited", DateTime.Now));

            StopPlink();

            if (TunnelHelper.CurrentTunnelStatus == TunnelStatuses.Started)
            {
                Thread.Sleep(500);
                WcfServerHelper.BroadcastRemoteCallback(x => x.EventToLog("Plink Restarting", DateTime.Now));
                StartPlink();
            }
        }

        private static void ProcessOutputCharacters(StreamReader streamReader, ServiceSettings serviceSettings, bool testConnection, bool errorLine)
        {
            int outputCharInt;
            string line = string.Empty;

            while (-1 != (outputCharInt = streamReader.Read()))
            {
                char outputChar = (char)outputCharInt;
                if (outputChar == '\n' || outputChar == '\r')
                {
                    if (line != string.Empty)
                    {
                        if (errorLine)
                        {
                            line = "E: " + line;
                        }

                        ProcessLine(line, testConnection);
                    }

                    line = string.Empty;
                }
                else
                {
                    line += outputChar;

                    if (line.Contains("'s password:"))
                    {
                        _process.StandardInput.WriteLine(serviceSettings.SshPassword);
                        _process.StandardInput.Flush();

                        ProcessLine(line, testConnection);
                        line = string.Empty;
                    }
                }
            }
        }

        private static void ProcessLine(string line, bool testConnection)
        {
            if (line == null) 
                return;

            WcfServerHelper.BroadcastRemoteCallback(x => x.PlinkTextOutput(line, DateTime.Now));

            if (line.Contains("If you do not trust this host, press Return to abandon the"))
            {
                _process.StandardInput.Write("y");
                _process.StandardInput.Write("\n");
                _process.StandardInput.Flush();
            }

            if (testConnection && line.Contains("Access granted"))
            {
                if (!_process.HasExited)
                    _process.Kill();

                WcfServerHelper.BroadcastRemoteCallback(x => x.TestConnectionCallback(PlinkStatus.Success));
            }
            else if (line.Contains("Access denied") || line.Contains("Password authentication failed"))
            {
                HandlePlinkError(PlinkStatus.InvalidUserOrPass, testConnection);
            }
            else if (line.Contains("Host does not exist"))
            {
                HandlePlinkError(PlinkStatus.InvalidHostname, testConnection);
            }
            else if (line.Contains("Connection timed out"))
            {
                HandlePlinkError(PlinkStatus.TimedOut, testConnection);
            }
        }

        private static void HandlePlinkError(PlinkStatus plinkStatus, bool testConnection)
        {
            if (!testConnection)
            {
                TunnelHelper.CurrentTunnelStatus = TunnelStatuses.Paused;
                WcfServerHelper.BroadcastRemoteCallback(x => x.TunnelStatusChanged(TunnelStatuses.Paused));
                StopPlink();
            }
            else if (!_process.HasExited)
            {
                _process.Kill();
            }

            WcfServerHelper.BroadcastRemoteCallback(x => x.TestConnectionCallback(plinkStatus));
        }
        #endregion
    }
}
