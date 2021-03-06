﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TroyLab.JRPC
{
    public interface IServiceDiscovery
    {
        void RegisterService(string serviceName, string serviceId, int port);
        void DeregisterService(string serviceId);
    }
}
