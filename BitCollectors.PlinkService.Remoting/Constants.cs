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

namespace BitCollectors.PlinkService.Remoting
{
    public class Constants
    {
        #region Constants 
        public const int ClientPingInterval = 12;
        public const int ClientPingTimeout = 30;
        public const int ServerPingInterval = 12;
        public const int ServerPingTimeout = 30;

        public const string ValidationPortInvalid = "Port must be a numeric value between 1 - 65535";
        #endregion Constants 
    }
}
