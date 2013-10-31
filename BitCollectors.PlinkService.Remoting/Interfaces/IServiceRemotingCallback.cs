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
using BitCollectors.PlinkService.Remoting.Entities;
using BitCollectors.PlinkService.Remoting.Enums;

namespace BitCollectors.PlinkService.Remoting.Interfaces
{
    public interface IServiceRemotingCallback
    {
        [OperationContract(IsOneWay=true)]
        void EventToLog(string eventText, DateTime eventTime);

        [OperationContract(IsOneWay = true)]
        void PlinkTextOutput(string eventText, DateTime eventTime);

        [OperationContract(IsOneWay = true)]
        void Ping(bool plinkRunning, bool tunnelValidationRunning, DateTime tunnelValidationLastRan);

        [OperationContract(IsOneWay = true)]
        void ServerStopping();

        [OperationContract(IsOneWay = true)]
        void TestConnectionCallback(PlinkStatus plinkStatus);

        [OperationContract(IsOneWay = true)]
        void TunnelStatusChanged(TunnelStatuses tunnelStatus);

        [OperationContract(IsOneWay = true)]
        void SaveSettingsCallback(ServiceSettings serviceSettings);
    }
}
