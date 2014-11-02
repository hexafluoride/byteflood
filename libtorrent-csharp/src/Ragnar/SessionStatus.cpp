#include "stdafx.h"
#include "SessionStatus.h"

#include <libtorrent\session_status.hpp>

namespace Ragnar
{
    SessionStatus::SessionStatus(const libtorrent::session_status &status)
    {
        this->_status = new libtorrent::session_status(status);
    }

    SessionStatus::~SessionStatus()
    {
        delete this->_status;
    }

    bool SessionStatus::HasIncomingConnections::get()
    {
        return this->_status->has_incoming_connections;
    }

    int SessionStatus::UploadRate::get()
    {
        return this->_status->upload_rate;
    }

    int SessionStatus::DownloadRate::get()
    {
        return this->_status->download_rate;
    }

    long long SessionStatus::TotalDownload::get()
    {
        return this->_status->total_download;
    }

    long long SessionStatus::TotalUpload::get()
    {
        return this->_status->total_upload;
    }

    int SessionStatus::PayloadUploadRate::get()
    {
        return this->_status->payload_upload_rate;
    }

    int SessionStatus::PayloadDownloadRate::get()
    {
        return this->_status->payload_download_rate;
    }

    long long SessionStatus::TotalPayloadUpload::get()
    {
        return this->_status->total_payload_upload;
    }

    long long SessionStatus::TotalPayloadDownload::get()
    {
        return this->_status->total_payload_download;
    }

    int SessionStatus::IPOverheadUploadRate::get()
    {
        return this->_status->ip_overhead_upload_rate;
    }

    int SessionStatus::IPOverheadDownloadRate::get()
    {
        return this->_status->ip_overhead_download_rate;
    }

    long long SessionStatus::TotalIPOverheadDownload::get()
    {
        return this->_status->total_ip_overhead_download;
    }

    long long SessionStatus::TotalIPOverheadUpload::get()
    {
        return this->_status->total_ip_overhead_upload;
    }

    int SessionStatus::DhtUploadRate::get()
    {
        return this->_status->dht_upload_rate;
    }

    int SessionStatus::DhtDownloadRate::get()
    {
        return this->_status->dht_download_rate;
    }

    long long SessionStatus::TotalDhtDownload::get()
    {
        return this->_status->total_dht_download;
    }

    long long SessionStatus::TotalDhtUpload::get()
    {
        return this->_status->total_dht_upload;
    }

    int SessionStatus::TrackerUploadRate::get()
    {
        return this->_status->tracker_upload_rate;
    }

    int SessionStatus::TrackerDownloadRate::get()
    {
        return this->_status->tracker_download_rate;
    }

    long long SessionStatus::TotalTrackerDownload::get()
    {
        return this->_status->total_tracker_download;
    }

    long long SessionStatus::TotalTrackerUpload::get()
    {
        return this->_status->total_tracker_upload;
    }

    long long SessionStatus::TotalRedundantBytes::get()
    {
        return this->_status->total_redundant_bytes;
    }

    long long SessionStatus::TotalFailedBytes::get()
    {
        return this->_status->total_failed_bytes;
    }

    int SessionStatus::NumPeers::get()
    {
        return this->_status->num_peers;
    }

    int SessionStatus::NumUnchoked::get()
    {
        return this->_status->num_unchoked;
    }

    int SessionStatus::AllowedUploadSlots::get()
    {
        return this->_status->allowed_upload_slots;
    }

    int SessionStatus::UpBandwidthQueue::get()
    {
        return this->_status->up_bandwidth_queue;
    }

    int SessionStatus::DownBandwidthQueue::get()
    {
        return this->_status->down_bandwidth_queue;
    }

    int SessionStatus::UpBandwidthBytesQueue::get()
    {
        return this->_status->up_bandwidth_bytes_queue;
    }

    int SessionStatus::DownBandwidthBytesQueue::get()
    {
        return this->_status->down_bandwidth_bytes_queue;
    }

    int SessionStatus::OptimisticUnchokeCounter::get()
    {
        return this->_status->optimistic_unchoke_counter;
    }

    int SessionStatus::UnchokeCounter::get()
    {
        return this->_status->unchoke_counter;
    }

    int SessionStatus::DiskWriteQueue::get()
    {
        return this->_status->disk_write_queue;
    }

    int SessionStatus::DiskReadQueue::get()
    {
        return this->_status->disk_read_queue;
    }

    int SessionStatus::DhtNodes::get()
    {
        return this->_status->dht_nodes;
    }

    int SessionStatus::DhtNodeCache::get()
    {
        return this->_status->dht_node_cache;
    }

    int SessionStatus::DhtTorrents::get()
    {
        return this->_status->dht_torrents;
    }

    long long SessionStatus::DhtGlobalNodes::get()
    {
        return this->_status->dht_global_nodes;
    }

    int SessionStatus::DhtTotalAllocations::get()
    {
        return this->_status->dht_total_allocations;
    }

    int SessionStatus::PeerlistSize::get()
    {
        return this->_status->peerlist_size;
    }
}