#pragma once

namespace libtorrent
{
    struct torrent_handle;
}

namespace Ragnar
{
    ref class AnnounceEntry;
    ref class SHA1Hash;
    ref class PartialPieceInfo;
    ref class PeerInfo;
    ref class TorrentInfo;
    ref class TorrentStatus;

    public ref class TorrentHandle
    {
    private:
        bool _disposed;
        libtorrent::torrent_handle* _handle;

    internal:
        libtorrent::torrent_handle* get_ptr() { return this->_handle; }

        TorrentHandle(const libtorrent::torrent_handle &handle);

    public:
        enum class MoveFlags : int
        {
            AlwaysReplaceFiles = 0,
            FailIfExist = 1,
            DontReplace = 2
        };

        ~TorrentHandle();

        !TorrentHandle();

        // add_piece
        
        void ReadPiece(int pieceIndex);

        bool HavePiece(int pieceIndex);

        System::Collections::Generic::IEnumerable<PeerInfo^>^ GetPeerInfo();

        property SHA1Hash^ InfoHash { SHA1Hash^ get(); }

        TorrentStatus^ GetStatus();

		TorrentStatus^ QueryStatus();

        System::Collections::Generic::IEnumerable<PartialPieceInfo^>^ GetDownloadQueue();

        void ResetPieceDeadline(int pieceIndex);

        void ClearPieceDeadlines();

        void SetPieceDeadline(int pieceIndex, int deadline); // TODO: flags

        void SetPriority(int priority);

        cli::array<long long>^ GetFileProgresses();

        void ClearError();

        void AddTracker(AnnounceEntry^ entry);

        // replace_trackers()

        System::Collections::Generic::IEnumerable<AnnounceEntry^>^ GetTrackers();

        // url_seeds()
        // add_url_seed()
        // remove_url_seed()

        // http_seeds()
        // remove_http_seed()
        // add_http_seed()

        // add_extension()

        // set_metadata()

        // is_valid();

        void Pause();

        void Resume();

        void SetUploadMode(bool value);

        void SetShareMode(bool value);

        void FlushCache();

        void ApplyIPFilter(bool value);

        void ForceRecheck();

        void SaveResumeData();

        bool NeedSaveResumeData();

        property bool AutoManaged { bool get(); void set(bool value); }

        void QueuePositionDown();

        void QueuePositionTop();

        void QueuePositionBottom();

        void QueuePositionUp();

        property int QueuePosition { int get(); }

        property bool ResolveCountries { bool get(); void set(bool value); }

        // set_ssl_certificate_buffer()
        // set_ssl_certificate()

        // get_storage_impl()

        property TorrentInfo^ TorrentFile { TorrentInfo^ get(); }

        // use_interface()

        // piece_availability()

        // int piece_priority (int index) const;
        // void piece_priority(int index, int priority) const;
        // void prioritize_pieces(std::vector<int> const& pieces) const;
        // std::vector<int> piece_priorities() const;

        int GetFilePriority(int fileIndex);

        void SetFilePriorities(cli::array<int>^ filePriorities);

        void SetFilePriority(int fileIndex, int priority);

        cli::array<int>^ GetFilePriorities();

        void ForceReannounce();

        void ForceReannounce(int seconds, int trackerIndex);

        void ForceDhtAnnounce();

        void ScrapeTracker();

        property int UploadLimit { int get(); void set(int value); }

        property int DownloadLimit { int get(); void set(int value); }

        property bool SequentialDownload { bool get(); void set(bool value); }

        // TODO: void connect_peer (tcp::endpoint const& adr, int source = 0) const;

        property int MaxUploads { int get(); void set(int value); }

        property int MaxConnections { int get(); void set(int value); }

        void SetTrackerLogin(System::String^ userName, System::String^ password);

        void MoveStorage(System::String^ savePath, MoveFlags flags);

        void RenameFile(int fileIndex, System::String^ fileName);

        property bool SuperSeeding { bool get(); void set(bool value); }

        property bool IsFinished { bool get(); }

        property bool IsPaused { bool get(); }

        property bool IsSeed { bool get(); }

        property bool HasMetadata { bool get(); }

		property bool IsValid { bool get(); }
    };
}
