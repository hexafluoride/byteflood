#pragma once

namespace libtorrent
{
    struct announce_entry;
}

namespace Ragnar
{
    using namespace System;

    public ref class AnnounceEntry
    {
    private:
        bool _disposed;
        libtorrent::announce_entry* _entry;

    internal:
        libtorrent::announce_entry* get_ptr() { return this->_entry; }

        AnnounceEntry(const libtorrent::announce_entry &entry);

    public:
        AnnounceEntry(String^ url);

        ~AnnounceEntry();

        !AnnounceEntry();

        property String^ Url { String^ get(); }

        property String^ TrackerId { String^ get(); }

        property String^ Message { String^ get(); }

        // TODO: error_code last_error;
        // TODO: ptime next_announce;
        // TODO: ptime min_announce;

        property int ScrapeIncomplete { int get(); }

        property int ScrapeComplete { int get(); }

        property int ScrapeDownloaded { int get(); }

        property int Tier { int get(); }

        property int FailLimit { int get(); }

        property int Fails { int get(); }

        property bool Updating { bool get(); }

        // TODO:    boost::uint8_t source:4;

        property bool Verified { bool get(); }

        property bool StartSent { bool get(); }

        property bool CompleteSent { bool get(); }

        property bool SendStats { bool get(); }
    };
}
