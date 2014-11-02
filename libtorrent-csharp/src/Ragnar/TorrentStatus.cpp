#include "stdafx.h"
#include "SHA1Hash.h"
#include "TorrentState.h"
#include "TorrentStatus.h"
#include "Utils.h"

#include <libtorrent\torrent_handle.hpp>

namespace Ragnar
{
    TorrentStatus::TorrentStatus(const libtorrent::torrent_status &status)
    {
        this->_status = new libtorrent::torrent_status(status);
    }

    TorrentStatus::~TorrentStatus()
    {
        delete this->_status;
    }

    System::String^ TorrentStatus::Error::get()
    {
        return gcnew System::String(this->_status->error.c_str());
    }

    System::String^ TorrentStatus::SavePath::get()
    {
        return gcnew System::String(this->_status->save_path.c_str());
    }

    System::String^ TorrentStatus::Name::get()
    {
        return gcnew System::String(this->_status->name.c_str());
    }

    long long TorrentStatus::TotalDownload::get()
    {
        return this->_status->total_download;
    }

    long long TorrentStatus::TotalUpload::get()
    {
        return this->_status->total_upload;
    }

    long long TorrentStatus::TotalPayloadDownload::get()
    {
        return this->_status->total_payload_download;
    }

    long long TorrentStatus::TotalPayloadUpload::get()
    {
        return this->_status->total_payload_upload;
    }

    long long TorrentStatus::TotalFailedBytes::get()
    {
        return this->_status->total_failed_bytes;
    }

    long long TorrentStatus::TotalReduntantBytes::get()
    {
        return this->_status->total_redundant_bytes;
    }

    long long TorrentStatus::TotalDone::get()
    {
        return this->_status->total_done;
    }

    long long TorrentStatus::TotalWantedDone::get()
    {
        return this->_status->total_wanted_done;
    }

    long long TorrentStatus::TotalWanted::get()
    {
        return this->_status->total_wanted;
    }

    long long TorrentStatus::AllTimeUpload::get()
    {
        return this->_status->all_time_upload;
    }

    long long TorrentStatus::AllTimeDownload::get()
    {
        return this->_status->all_time_download;
    }

    System::DateTime TorrentStatus::AddedTime::get()
    {
        return Utils::GetDateTimeFromTimeT(this->_status->added_time);
    }

    System::Nullable<System::DateTime> TorrentStatus::CompletedTime::get()
    {
        if (this->_status->completed_time == 0)
        {
            return Nullable<DateTime>();
        }

        return Nullable<System::DateTime>(Utils::GetDateTimeFromTimeT(this->_status->completed_time));
    }

    System::Nullable<System::DateTime> TorrentStatus::LastSeenComplete::get()
    {
        if (this->_status->last_seen_complete == 0)
        {
            return Nullable<DateTime>();
        }

        return Nullable<System::DateTime>(Utils::GetDateTimeFromTimeT(this->_status->last_seen_complete));
    }

    float TorrentStatus::Progress::get()
    {
        return this->_status->progress;
    }

    int TorrentStatus::QueuePosition::get()
    {
        return this->_status->queue_position;
    }

    int TorrentStatus::DownloadRate::get()
    {
        return this->_status->download_rate;
    }

    int TorrentStatus::UploadRate::get()
    {
        return this->_status->upload_rate;
    }

    int TorrentStatus::DownloadPayloadRate::get()
    {
        return this->_status->download_payload_rate;
    }

    int TorrentStatus::UploadPayloadRate::get()
    {
        return this->_status->upload_payload_rate;
    }

    int TorrentStatus::NumSeeds::get()
    {
        return this->_status->num_seeds;
    }

    int TorrentStatus::NumPeers::get()
    {
        return this->_status->num_peers;
    }

    int TorrentStatus::NumComplete::get()
    {
        return this->_status->num_complete;
    }

    int TorrentStatus::NumIncomplete::get()
    {
        return this->_status->num_incomplete;
    }

    int TorrentStatus::ListSeeds::get()
    {
        return this->_status->list_seeds;
    }

    int TorrentStatus::ListPeers::get()
    {
        return this->_status->list_peers;
    }

    int TorrentStatus::ConnectCandidates::get()
    {
        return this->_status->connect_candidates;
    }

    int TorrentStatus::NumPieces::get()
    {
        return this->_status->num_pieces;
    }

    int TorrentStatus::DistributedFullCopies::get()
    {
        return this->_status->distributed_full_copies;
    }

    int TorrentStatus::DistributedFraction::get()
    {
        return this->_status->distributed_fraction;
    }

    float TorrentStatus::DistributedCopies::get()
    {
        return this->_status->distributed_copies;
    }

    int TorrentStatus::BlockSize::get()
    {
        return this->_status->block_size;
    }

    int TorrentStatus::NumUploads::get()
    {
        return this->_status->num_uploads;
    }

    int TorrentStatus::NumConnections::get()
    {
        return this->_status->num_connections;
    }

    int TorrentStatus::UploadsLimit::get()
    {
        return this->_status->uploads_limit;
    }

    int TorrentStatus::ConnectionsLimit::get()
    {
        return this->_status->connections_limit;
    }

    int TorrentStatus::UpBandwidthQueue::get()
    {
        return this->_status->up_bandwidth_queue;
    }

    int TorrentStatus::DownBandwidthQueue::get()
    {
        return this->_status->down_bandwidth_queue;
    }

    System::TimeSpan TorrentStatus::TimeSinceUpload::get()
    {
        return System::TimeSpan::FromSeconds(this->_status->time_since_upload);
    }

    System::TimeSpan TorrentStatus::TimeSinceDownload::get()
    {
        return System::TimeSpan::FromSeconds(this->_status->time_since_download);
    }

    System::TimeSpan TorrentStatus::ActiveTime::get()
    {
        return System::TimeSpan::FromSeconds(this->_status->active_time);
    }

    System::TimeSpan TorrentStatus::FinishedTime::get()
    {
        return System::TimeSpan::FromSeconds(this->_status->finished_time);
    }

    System::TimeSpan TorrentStatus::SeedingTime::get()
    {
        return System::TimeSpan::FromSeconds(this->_status->seeding_time);
    }

    int TorrentStatus::SeedRank::get()
    {
        return this->_status->seed_rank;
    }

    System::Nullable<System::TimeSpan> TorrentStatus::LastScrape::get()
    {
        if (this->_status->last_scrape == -1)
        {
            return Nullable<TimeSpan>();
        }

        return Nullable<TimeSpan>(TimeSpan::FromSeconds(this->_status->last_scrape));
    }

    int TorrentStatus::SparseRegions::get()
    {
        return this->_status->sparse_regions;
    }

    int TorrentStatus::Priority::get()
    {
        return this->_status->priority;
    }

    TorrentState TorrentStatus::State::get()
    {
        return static_cast<TorrentState>(this->_status->state);
    }

    bool TorrentStatus::NeedSaveResume::get()
    {
        return this->_status->need_save_resume;
    }

    bool TorrentStatus::IPFilterApplies::get()
    {
        return this->_status->ip_filter_applies;
    }

    bool TorrentStatus::UploadMode::get()
    {
        return this->_status->upload_mode;
    }

    bool TorrentStatus::ShareMode::get()
    {
        return this->_status->share_mode;
    }

    bool TorrentStatus::SuperSeeding::get()
    {
        return this->_status->super_seeding;
    }

    bool TorrentStatus::Paused::get()
    {
        return this->_status->paused;
    }

    bool TorrentStatus::AutoManaged::get()
    {
        return this->_status->auto_managed;
    }

    bool TorrentStatus::SequentialDownload::get()
    {
        return this->_status->sequential_download;
    }

    bool TorrentStatus::IsSeeding::get()
    {
        return this->_status->is_seeding;
    }

    bool TorrentStatus::IsFinished::get()
    {
        return this->_status->is_finished;
    }

    bool TorrentStatus::HasMetadata::get()
    {
        return this->_status->has_metadata;
    }

    bool TorrentStatus::HasIncoming::get()
    {
        return this->_status->has_incoming;
    }

    bool TorrentStatus::SeedMode::get()
    {
        return this->_status->seed_mode;
    }

    bool TorrentStatus::MovingStorage::get()
    {
        return this->_status->moving_storage;
    }

    SHA1Hash^ TorrentStatus::InfoHash::get()
    {
        return gcnew SHA1Hash(this->_status->info_hash);
    }
}
