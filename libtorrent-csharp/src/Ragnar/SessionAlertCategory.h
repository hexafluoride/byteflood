#pragma once

namespace Ragnar
{
    [System::FlagsAttribute]
    public enum class SessionAlertCategory : unsigned int
    {
        Error = 1,
        Peer = 2,
        PortMapping = 4,
        Storage = 8,
        Tracker = 16,
        Debug = 32,
        Status = 64,
        Progress = 128,
        IPBlock = 256,
        Performance = 512,
        Dht = 1024,
        Stats = 2048,
        Rss = 4096,
        All = 2147483647
    };
}
