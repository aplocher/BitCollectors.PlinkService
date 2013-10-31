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
using BitCollectors.PlinkService.Service.Properties;

namespace BitCollectors.PlinkService.Service.Helpers
{
    public static class SettingsHelper
    {
        public static ServiceSettings GetSettings()
        {
            Settings settings = new Settings();
            ServiceSettings serviceSettings = new ServiceSettings();

            serviceSettings.DynamicSocksHost = settings.DynamicSocksHost;
            serviceSettings.DynamicSocksPort = settings.DynamicSocksPort;
            serviceSettings.EnableCompression = settings.EnableCompression;
            serviceSettings.EnableDynamicSocks = settings.EnableDynamicSocks;
            serviceSettings.EnableTunnelValidation = settings.EnableTunnelValidation;
            serviceSettings.EnableVerbose = settings.EnableVerbose;
            serviceSettings.PlinkExecutable = settings.PlinkExecutable;
            serviceSettings.SshHostname = settings.SshHostname;
            serviceSettings.SshPassword = CryptoHelper.ToInsecureString(CryptoHelper.DecryptString(settings.SshPassword));
            serviceSettings.SshPort = settings.SshPort;
            serviceSettings.SshUsername = settings.SshUsername;
            serviceSettings.TunnelValidationLocalPort = settings.TunnelValidationLocalPort;
            serviceSettings.TunnelValidationPingInterval = settings.TunnelValidationPingInterval;
            serviceSettings.TunnelValidationPingTimeout = settings.TunnelValidationPingTimeout;
            serviceSettings.TunnelValidationRemotePort = settings.TunnelValidationRemotePort;
            serviceSettings.EnableIcmpValidation = settings.EnableIcmpValidation;
            serviceSettings.IcmpValidationHostnames = settings.IcmpValidationHostnames;
            serviceSettings.IcmpValidationPingSshServer = settings.IcmpValidationPingSshServer;
            serviceSettings.UiMinimizedToSysTray = settings.UiMinimizedToSysTray;
            serviceSettings.UiShowSysTrayNotifications = settings.UiShowSysTrayNotifications;
            serviceSettings.UiStartMinimized = settings.UiStartMinimized;
            serviceSettings.UiStartWithWindows = settings.UiStartWithWindows;
            serviceSettings.LocalTunnels = settings.LocalTunnels;
            serviceSettings.RemoteTunnels = settings.RemoteTunnels;

            return serviceSettings;
        }

        public static void SaveSettings(ServiceSettings serviceSettings)
        {
            try
            {
                Settings settings = new Settings();

                settings.DynamicSocksHost = serviceSettings.DynamicSocksHost;
                settings.DynamicSocksPort = serviceSettings.DynamicSocksPort;
                settings.EnableCompression = serviceSettings.EnableCompression;
                settings.EnableDynamicSocks = serviceSettings.EnableDynamicSocks;
                settings.EnableTunnelValidation = serviceSettings.EnableTunnelValidation;
                settings.EnableVerbose = serviceSettings.EnableVerbose;
                settings.PlinkExecutable = serviceSettings.PlinkExecutable;
                settings.SshHostname = serviceSettings.SshHostname;
                settings.SshPassword = CryptoHelper.EncryptString(CryptoHelper.ToSecureString(serviceSettings.SshPassword));
                settings.SshPort = serviceSettings.SshPort;
                settings.SshUsername = serviceSettings.SshUsername;
                settings.TunnelValidationLocalPort = serviceSettings.TunnelValidationLocalPort;
                settings.TunnelValidationPingInterval = serviceSettings.TunnelValidationPingInterval;
                settings.TunnelValidationPingTimeout = serviceSettings.TunnelValidationPingTimeout;
                settings.TunnelValidationRemotePort = serviceSettings.TunnelValidationRemotePort;
                settings.EnableIcmpValidation = serviceSettings.EnableIcmpValidation;
                settings.IcmpValidationHostnames = serviceSettings.IcmpValidationHostnames;
                settings.IcmpValidationPingSshServer = serviceSettings.IcmpValidationPingSshServer;
                settings.UiMinimizedToSysTray = serviceSettings.UiMinimizedToSysTray;
                settings.UiShowSysTrayNotifications = serviceSettings.UiShowSysTrayNotifications;
                settings.UiStartMinimized = serviceSettings.UiStartMinimized;
                settings.UiStartWithWindows = serviceSettings.UiStartWithWindows;
                settings.LocalTunnels = serviceSettings.LocalTunnels;
                settings.RemoteTunnels = serviceSettings.RemoteTunnels;

                settings.Save();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
