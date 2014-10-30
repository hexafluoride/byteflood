#pragma once

namespace libtorrent
{
    struct torrent_status;
}

namespace Ragnar
{
    ref class SHA1Hash;
    enum class TorrentState;

    public ref class TorrentStatus
    {
    private:
        libtorrent::torrent_status* _status;

    internal:
        libtorrent::torrent_status* get_ptr() { return _status; }

        TorrentStatus(const libtorrent::torrent_status &status);
        ~TorrentStatus();

    public:
        property System::String^ Error
        {
            System::String^ get();
        }

        property System::String^ SavePath
        {
            System::String^ get();
        }

        property System::String^ Name
        {
            System::String^ get();
        }

        property long long TotalDownload
        {
            long long get();
        }

        property long long TotalUpload
        {
            long long get();
        }

        property long long TotalPayloadDownload
        {
            long long get();
        }

        property long long TotalPayloadUpload
        {
            long long get();
        }

        property long long TotalFailedBytes
        {
            long long get();
        }

        property long long TotalReduntantBytes
        {
            long long get();
        }

        //bitfield pieces;
        //bitfield verified_pieces;

        property long long TotalDone
        {
            long long get();
        }

        property long long TotalWantedDone
        {
            long long get();
        }

        property long long TotalWanted
        {
            long long get();
        }

        property long long AllTimeUpload
        {
            long long get();
        }

        property long long AllTimeDownload
        {
            long long get();
        }

        property System::DateTime AddedTime
        {
            System::DateTime get();
        }

        property System::Nullable<System::DateTime> CompletedTime
        {
            System::Nullable<System::DateTime> get();
        }

        property System::Nullable<System::DateTime> LastSeenComplete
        {
            System::Nullable<System::DateTime> get();
        }

        //storage_mode

        property float Progress
        {
            float get();
        }

        //    int progress_ppm;

        property int QueuePosition
        {
            int get();
        }

        property int DownloadRate
        {
            int get();
        }

        property int UploadRate
        {
            int get();
        }

        property int DownloadPayloadRate
        {
            int get();
        }

        property int UploadPayloadRate
        {
            int get();
        }

        property int NumSeeds
        {
            int get();
        }

        property int NumPeers
        {
            int get();
        }

        property int NumComplete
        {
            int get();
        }

        property int NumIncomplete
        {
            int get();
        }

        property int ListSeeds
        {
            int get();
        }

        property int ListPeers
        {
            int get();
        }

        property int ConnectCandidates
        {
            int get();
        }

        property int NumPieces
        {
            int get();
        }

        property int DistributedFullCopies
        {
            int get();
        }

        property int DistributedFraction
        {
            int get();
        }

        property float DistributedCopies
        {
            float get();
        }

        property int BlockSize
        {
            int get();
        }

        property int NumUploads
        {
            int get();
        }

        property int NumConnections
        {
            int get();
        }

        property int UploadsLimit
        {
            int get();
        }

        property int ConnectionsLimit
        {
            int get();
        }

        property int UpBandwidthQueue
        {
            int get();
        }

        property int DownBandwidthQueue
        {
            int get();
        }

        property System::TimeSpan TimeSinceUpload
        {
            System::TimeSpan get();
        }

        property System::TimeSpan TimeSinceDownload
        {
            System::TimeSpan get();
        }

        property System::TimeSpan ActiveTime
        {
            System::TimeSpan get();
        }

        property System::TimeSpan FinishedTime
        {
            System::TimeSpan get();
        }

        property System::TimeSpan SeedingTime
        {
            System::TimeSpan get();
        }

        property int SeedRank
        {
            int get();
        }

        property System::Nullable<System::TimeSpan> LastScrape
        {
            System::Nullable<System::TimeSpan> get();
        }

        property int SparseRegions
        {
            int get();
        }

        property int Priority
        {
            int get();
        }

        property TorrentState State
        {
            TorrentState get();
        }

        property bool NeedSaveResume
        {
            bool get();
        }

        property bool IPFilterApplies
        {
            bool get();
        }

        property bool UploadMode
        {
            bool get();
        }

        property bool ShareMode
        {
            bool get();
        }

        property bool SuperSeeding
        {
            bool get();
        }

        property bool Paused
        {
            bool get();
        }

        property bool AutoManaged
        {
            bool get();
        }

        property bool SequentialDownload
        {
            bool get();
        }

        property bool IsSeeding
        {
            bool get();
        }

        property bool IsFinished
        {
            bool get();
        }

        property bool HasMetadata
        {
            bool get();
        }

        property bool HasIncoming
        {
            bool get();
        }

        property bool SeedMode
        {
            bool get();
        }

        property bool MovingStorage
        {
            bool get();
        }

        property SHA1Hash^ InfoHash
        {
            SHA1Hash^ get();
        }
    };
}
