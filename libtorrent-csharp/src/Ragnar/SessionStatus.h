#pragma once

namespace libtorrent
{
    struct session_status;
}

namespace Ragnar
{
    public ref class SessionStatus
    {
    private:
        libtorrent::session_status* _status;

    internal:
        SessionStatus(const libtorrent::session_status &status);

    public:
        ~SessionStatus();

        property bool HasIncomingConnections { bool get(); }

        property int UploadRate { int get(); }

        property int DownloadRate { int get(); }

        property long long TotalDownload { long long get(); }

        property long long TotalUpload { long long get(); }

        property int PayloadUploadRate { int get(); }

        property int PayloadDownloadRate { int get(); }

        property long long TotalPayloadDownload { long long get(); }

        property long long TotalPayloadUpload { long long get(); }

        property int IPOverheadUploadRate { int get(); }

        property int IPOverheadDownloadRate { int get(); }

        property long long TotalIPOverheadDownload { long long get(); }

        property long long TotalIPOverheadUpload { long long get(); }

        property int DhtUploadRate { int get(); }

        property int DhtDownloadRate { int get(); }

        property long long TotalDhtDownload { long long get(); }

        property long long TotalDhtUpload { long long get(); }

        property int TrackerUploadRate { int get(); }

        property int TrackerDownloadRate { int get(); }

        property long long TotalTrackerDownload { long long get(); }

        property long long TotalTrackerUpload { long long get(); }

        property long long TotalRedundantBytes { long long get(); }

        property long long TotalFailedBytes { long long get(); }

        property int NumPeers { int get(); }

        property int NumUnchoked { int get(); }

        property int AllowedUploadSlots { int get(); }

        property int UpBandwidthQueue { int get(); }

        property int DownBandwidthQueue { int get(); }

        property int UpBandwidthBytesQueue { int get(); }

        property int DownBandwidthBytesQueue { int get(); }

        property int OptimisticUnchokeCounter { int get(); }

        property int UnchokeCounter { int get(); }

        property int DiskWriteQueue { int get(); }

        property int DiskReadQueue { int get(); }

        property int DhtNodes { int get(); }

        property int DhtNodeCache { int get(); }

        property int DhtTorrents { int get(); }

        property long long DhtGlobalNodes { long long get(); }

        // TODO: std::vector<dht_lookup> active_requests;
        // TODO: std::vector<dht_routing_bucket> dht_routing_table;

        property int DhtTotalAllocations { int get(); }

        // TODO:    utp_status utp_stats;

        property int PeerlistSize { int get(); }
    };
}
