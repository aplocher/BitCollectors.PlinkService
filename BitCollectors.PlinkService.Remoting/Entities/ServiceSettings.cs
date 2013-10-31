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

using System.Security;

namespace BitCollectors.PlinkService.Remoting.Entities
{
    public class ServiceSettings
    {
        private string _plinkExecutable;
        private string _sshUsername;
        //private SecureString _sshPassword;
        private string _sshPassword;
        private string _sshHostname;
        private int _sshPort;
        private bool _enableCompression;
        private bool _enableVerbose;
        private bool _enableDynamicSocks;
        private string _dynamicSocksHost;
        private int _dynamicSocksPort;
        private bool _enableTunnelValidation;
        private int _tunnelValidationLocalPort;
        private int _tunnelValidationRemotePort;
        private int _tunnelValidationPingInterval;
        private int _tunnelValidationPingTimeout;
        private string _localTunnels;
        private string _remoteTunnels;

        public bool IsDirty { get; set; }

        public string PlinkExecutable 
        {
            get { return _plinkExecutable; }
            set
            {
                if (_plinkExecutable != value)
                {
                    _plinkExecutable = value;
                    IsDirty = true;
                }
            }
        }

        public string SshUsername 
        {
            get { return _sshUsername; }
            set
            {
                if (_sshUsername != value)
                {
                    _sshUsername = value;
                    IsDirty = true;
                }
            }
        }

        //public SecureString SshPassword
        //{
        //    get { return _sshPassword; }
        //    set
        //    {
        //        if (_sshPassword != value)
        //        {
        //            _sshPassword = value;
        //            IsDirty = true;
        //        }
        //    }
        //}

        //public string SshPasswordDecrypted
        //{
        //    get
        //    {
        //        return CryptoHelper.EncryptString(SshPassword);
        //    }
        //    set
        //    {
        //        SshPassword = CryptoHelper.DecryptString(value);
        //    }
        //}

        public string SshPassword
        {
            get { return _sshPassword; }
            set
            {
                if (_sshPassword != value)
                {
                    _sshPassword = value;
                    IsDirty = true;
                }
            }
        }

        public string SshHostname
        {
            get { return _sshHostname; }
            set
            {
                if (_sshHostname != value)
                {
                    _sshHostname = value;
                    IsDirty = true;
                }
            }
        }

        public int SshPort
        {
            get { return _sshPort; }
            set
            {
                if (_sshPort != value)
                {
                    _sshPort = value;
                    IsDirty = true;
                }
            }
        }

        public bool EnableCompression
        {
            get { return _enableCompression; }
            set
            {
                if (_enableCompression != value)
                {
                    _enableCompression = value;
                    IsDirty = true;
                }
            }
        }

        public bool EnableVerbose
        {
            get { return _enableVerbose; }
            set
            {
                if (_enableVerbose != value)
                {
                    _enableVerbose = value;
                    IsDirty = true;
                }
            }
        }

        public bool EnableDynamicSocks
        {
            get { return _enableDynamicSocks; }
            set
            {
                if (_enableDynamicSocks != value)
                {
                    _enableDynamicSocks = value;
                    IsDirty = true;
                }
            }
        }

        public string DynamicSocksHost
        {
            get { return _dynamicSocksHost; }
            set
            {
                if (_dynamicSocksHost != value)
                {
                    _dynamicSocksHost = value;
                    IsDirty = true;
                }
            }
        }

        public int DynamicSocksPort
        {
            get { return _dynamicSocksPort; }
            set
            {
                if (_dynamicSocksPort != value)
                {
                    _dynamicSocksPort = value;
                    IsDirty = true;
                }
            }
        }

        public bool UiStartWithWindows { get; set; }

        public bool UiStartMinimized { get; set; }

        public bool UiMinimizedToSysTray { get; set; }

        public bool UiShowSysTrayNotifications { get; set; }

        public bool EnableTunnelValidation
        {
            get { return _enableTunnelValidation; }
            set
            {
                if (_enableTunnelValidation != value)
                {
                    _enableTunnelValidation = value;
                    IsDirty = true;
                }
            }
        }

        public int TunnelValidationLocalPort
        {
            get { return _tunnelValidationLocalPort; }
            set
            {
                if (_tunnelValidationLocalPort != value)
                {
                    _tunnelValidationLocalPort = value;
                    IsDirty = true;
                }
            }
        }

        public int TunnelValidationRemotePort
        {
            get { return _tunnelValidationRemotePort; }
            set
            {
                if (_tunnelValidationRemotePort != value)
                {
                    _tunnelValidationRemotePort = value;
                    IsDirty = true;
                }
            }
        }

        public int TunnelValidationPingInterval
        {
            get { return _tunnelValidationPingInterval; }
            set
            {
                if (_tunnelValidationPingInterval != value)
                {
                    _tunnelValidationPingInterval = value;
                    IsDirty = true;
                }
            }
        }

        public int TunnelValidationPingTimeout
        {
            get { return _tunnelValidationPingTimeout; }
            set
            {
                if (_tunnelValidationPingTimeout != value)
                {
                    _tunnelValidationPingTimeout = value;
                    IsDirty = true;
                }
            }
        }

        public bool EnableIcmpValidation { get; set; }

        public string IcmpValidationHostnames { get; set; }

        public bool IcmpValidationPingSshServer { get; set; }

        public string LocalTunnels
        {
            get { return _localTunnels; }
            set
            {
                if (_localTunnels != value)
                {
                    _localTunnels = value;
                    IsDirty = true;
                }
            }
        }

        public string RemoteTunnels
        {
            get { return _remoteTunnels; }
            set
            {
                if (_remoteTunnels != value)
                {
                    _remoteTunnels = value;
                    IsDirty = true;
                }
            }
        }
    }
}
