#include "stdafx.h"
#include "SessionSettings.h"

#include <libtorrent\session_settings.hpp>
#include <msclr\marshal_cppstd.h>

namespace Ragnar
{
    SessionSettings::SessionSettings(const libtorrent::session_settings &settings)
    {
        this->_settings = new libtorrent::session_settings(settings);
    }

    SessionSettings::~SessionSettings()
    {
        delete this->_settings;
    }

    String^ SessionSettings::UserAgent::get()
    {
        return gcnew String(this->_settings->user_agent.c_str());
    }

    void SessionSettings::UserAgent::set(String^ value)
    {
        this->_settings->user_agent = msclr::interop::marshal_as<std::string>(value);
    }

    int SessionSettings::TrackerCompletionTimeout::get()
    {
        return this->_settings->tracker_completion_timeout;
    }

    void SessionSettings::TrackerCompletionTimeout::set(int value)
    {
        this->_settings->tracker_completion_timeout = value;
    }

    int SessionSettings::TrackerReceiveTimeout::get()
    {
        return this->_settings->tracker_receive_timeout;
    }

    void SessionSettings::TrackerReceiveTimeout::set(int value)
    {
        this->_settings->tracker_receive_timeout = value;
    }

    int SessionSettings::StopTrackerTimeout::get()
    {
        return this->_settings->stop_tracker_timeout;
    }

    void SessionSettings::StopTrackerTimeout::set(int value)
    {
        this->_settings->stop_tracker_timeout = value;
    }

    int SessionSettings::TrackerMaximumResponseLength::get()
    {
        return this->_settings->tracker_maximum_response_length;
    }

    void SessionSettings::TrackerMaximumResponseLength::set(int value)
    {
        this->_settings->tracker_maximum_response_length = value;
    }

    int SessionSettings::PieceTimeout::get()
    {
        return this->_settings->piece_timeout;
    }

    void SessionSettings::PieceTimeout::set(int value)
    {
        this->_settings->piece_timeout = value;
    }

    int SessionSettings::RequestTimeout::get()
    {
        return this->_settings->request_timeout;
    }

    void SessionSettings::RequestTimeout::set(int value)
    {
        this->_settings->request_timeout = value;
    }

    int SessionSettings::RequestQueueTime::get()
    {
        return this->_settings->request_queue_time;
    }

    void SessionSettings::RequestQueueTime::set(int value)
    {
        this->_settings->request_queue_time = value;
    }

    int SessionSettings::MaxAllowedInRequestQueue::get()
    {
        return this->_settings->max_allowed_in_request_queue;
    }

    void SessionSettings::MaxAllowedInRequestQueue::set(int value)
    {
        this->_settings->max_allowed_in_request_queue = value;
    }

    int SessionSettings::MaxOutRequestQueue::get()
    {
        return this->_settings->max_out_request_queue;
    }

    void SessionSettings::MaxOutRequestQueue::set(int value)
    {
        this->_settings->max_out_request_queue = value;
    }

    int SessionSettings::WholePiecesThreshold::get()
    {
        return this->_settings->whole_pieces_threshold;
    }

    void SessionSettings::WholePiecesThreshold::set(int value)
    {
        this->_settings->whole_pieces_threshold = value;
    }

    int SessionSettings::PeerTimeout::get()
    {
        return this->_settings->peer_timeout;
    }

    void SessionSettings::PeerTimeout::set(int value)
    {
        this->_settings->peer_timeout = value;
    }

    int SessionSettings::UrlSeedTimeout::get()
    {
        return this->_settings->urlseed_timeout;
    }

    void SessionSettings::UrlSeedTimeout::set(int value)
    {
        this->_settings->urlseed_timeout = value;
    }

    int SessionSettings::UrlSeedPipelineSize::get()
    {
        return this->_settings->urlseed_pipeline_size;
    }

    void SessionSettings::UrlSeedPipelineSize::set(int value)
    {
        this->_settings->urlseed_pipeline_size = value;
    }

    int SessionSettings::UrlSeedWaitRetry::get()
    {
        return this->_settings->urlseed_wait_retry;
    }

    void SessionSettings::UrlSeedWaitRetry::set(int value)
    {
        this->_settings->urlseed_wait_retry = value;
    }

    int SessionSettings::FilePoolSize::get()
    {
        return this->_settings->file_pool_size;
    }

    void SessionSettings::FilePoolSize::set(int value)
    {
        this->_settings->file_pool_size = value;
    }

    bool SessionSettings::AllowMultipleConnectionsPerIP::get()
    {
        return this->_settings->allow_multiple_connections_per_ip;
    }

    void SessionSettings::AllowMultipleConnectionsPerIP::set(bool value)
    {
        this->_settings->allow_multiple_connections_per_ip = value;
    }

    int SessionSettings::MaxFailCount::get()
    {
        return this->_settings->max_failcount;
    }

    void SessionSettings::MaxFailCount::set(int value)
    {
        this->_settings->max_failcount = value;
    }

    int SessionSettings::MinReconnectTime::get()
    {
        return this->_settings->min_reconnect_time;
    }

    void SessionSettings::MinReconnectTime::set(int value)
    {
        this->_settings->min_reconnect_time = value;
    }

    int SessionSettings::PeerConnectTimeout::get()
    {
        return this->_settings->peer_connect_timeout;
    }

    void SessionSettings::PeerConnectTimeout::set(int value)
    {
        this->_settings->peer_connect_timeout = value;
    }

    bool SessionSettings::IgnoreLimitsOnLocalNetwork::get()
    {
        return this->_settings->ignore_limits_on_local_network;
    }

    void SessionSettings::IgnoreLimitsOnLocalNetwork::set(bool value)
    {
        this->_settings->ignore_limits_on_local_network = value;
    }

    int SessionSettings::ConnectionSpeed::get()
    {
        return this->_settings->connection_speed;
    }

    void SessionSettings::ConnectionSpeed::set(int value)
    {
        this->_settings->connection_speed = value;
    }

    bool SessionSettings::SendRedundantHave::get()
    {
        return this->_settings->send_redundant_have;
    }

    void SessionSettings::SendRedundantHave::set(bool value)
    {
        this->_settings->send_redundant_have = value;
    }

    bool SessionSettings::LazyBitfields::get()
    {
        return this->_settings->lazy_bitfields;
    }

    void SessionSettings::LazyBitfields::set(bool value)
    {
        this->_settings->lazy_bitfields = value;
    }

    int SessionSettings::InactivityTimeout::get()
    {
        return this->_settings->inactivity_timeout;
    }

    void SessionSettings::InactivityTimeout::set(int value)
    {
        this->_settings->inactivity_timeout = value;
    }

    int SessionSettings::UnchokeInterval::get()
    {
        return this->_settings->unchoke_interval;
    }

    void SessionSettings::UnchokeInterval::set(int value)
    {
        this->_settings->unchoke_interval = value;
    }

    int SessionSettings::OptimisticUnchokeInterval::get()
    {
        return this->_settings->optimistic_unchoke_interval;
    }

    void SessionSettings::OptimisticUnchokeInterval::set(int value)
    {
        this->_settings->optimistic_unchoke_interval = value;
    }

    String^ SessionSettings::AnnounceIP::get()
    {
        return gcnew String(this->_settings->announce_ip.c_str());
    }

    void SessionSettings::AnnounceIP::set(String^ value)
    {
        this->_settings->announce_ip = msclr::interop::marshal_as<std::string>(value);
    }

    int SessionSettings::NumWant::get()
    {
        return this->_settings->num_want;
    }

    void SessionSettings::NumWant::set(int value)
    {
        this->_settings->num_want = value;
    }

    int SessionSettings::InitialPickerThreshold::get()
    {
        return this->_settings->initial_picker_threshold;
    }

    void SessionSettings::InitialPickerThreshold::set(int value)
    {
        this->_settings->initial_picker_threshold = value;
    }

    int SessionSettings::AllowedFastSetSize::get()
    {
        return this->_settings->allowed_fast_set_size;
    }

    void SessionSettings::AllowedFastSetSize::set(int value)
    {
        this->_settings->allowed_fast_set_size = value;
    }

    int SessionSettings::SuggestMode::get()
    {
        return this->_settings->suggest_mode;
    }

    void SessionSettings::SuggestMode::set(int value)
    {
        this->_settings->suggest_mode = value;
    }

    int SessionSettings::MaxQueuedDiskBytes::get()
    {
        return this->_settings->max_queued_disk_bytes;
    }

    void SessionSettings::MaxQueuedDiskBytes::set(int value)
    {
        this->_settings->max_queued_disk_bytes = value;
    }

    int SessionSettings::MaxQueuedDiskBytesLowWatermark::get()
    {
        return this->_settings->max_queued_disk_bytes_low_watermark;
    }

    void SessionSettings::MaxQueuedDiskBytesLowWatermark::set(int value)
    {
        this->_settings->max_queued_disk_bytes_low_watermark = value;
    }

    int SessionSettings::HandshakeTimeout::get()
    {
        return this->_settings->handshake_timeout;
    }

    void SessionSettings::HandshakeTimeout::set(int value)
    {
        this->_settings->handshake_timeout = value;
    }

    bool SessionSettings::UseDhtAsFallback::get()
    {
        return this->_settings->use_dht_as_fallback;
    }

    void SessionSettings::UseDhtAsFallback::set(bool value)
    {
        this->_settings->use_dht_as_fallback = value;
    }

    bool SessionSettings::FreeTorrentHashes::get()
    {
        return this->_settings->free_torrent_hashes;
    }

    void SessionSettings::FreeTorrentHashes::set(bool value)
    {
        this->_settings->free_torrent_hashes = value;
    }

    bool SessionSettings::UpnpIgnoreNonRouters::get()
    {
        return this->_settings->upnp_ignore_nonrouters;
    }

    void SessionSettings::UpnpIgnoreNonRouters::set(bool value)
    {
        this->_settings->upnp_ignore_nonrouters = value;
    }

    int SessionSettings::SendBufferLowWatermark::get()
    {
        return this->_settings->send_buffer_low_watermark;
    }

    void SessionSettings::SendBufferLowWatermark::set(int value)
    {
        this->_settings->send_buffer_low_watermark = value;
    }

    int SessionSettings::SendBufferWatermark::get()
    {
        return this->_settings->send_buffer_watermark;
    }

    void SessionSettings::SendBufferWatermark::set(int value)
    {
        this->_settings->send_buffer_watermark = value;
    }

    int SessionSettings::SendBufferWatermarkFactor::get()
    {
        return this->_settings->send_buffer_watermark_factor;
    }

    void SessionSettings::SendBufferWatermarkFactor::set(int value)
    {
        this->_settings->send_buffer_watermark_factor = value;
    }

    int SessionSettings::ChokingAlgorithm::get()
    {
        return this->_settings->choking_algorithm;
    }

    void SessionSettings::ChokingAlgorithm::set(int value)
    {
        this->_settings->choking_algorithm = value;
    }

    int SessionSettings::SeedChokingAlgorithm::get()
    {
        return this->_settings->seed_choking_algorithm;
    }

    void SessionSettings::SeedChokingAlgorithm::set(int value)
    {
        this->_settings->seed_choking_algorithm = value;
    }

    bool SessionSettings::UseParoleMode::get()
    {
        return this->_settings->use_parole_mode;
    }

    void SessionSettings::UseParoleMode::set(bool value)
    {
        this->_settings->use_parole_mode = value;
    }

    int SessionSettings::CacheSize::get()
    {
        return this->_settings->cache_size;
    }

    void SessionSettings::CacheSize::set(int value)
    {
        this->_settings->cache_size = value;
    }

    int SessionSettings::CacheBufferChunkSize::get()
    {
        return this->_settings->cache_buffer_chunk_size;
    }

    void SessionSettings::CacheBufferChunkSize::set(int value)
    {
        this->_settings->cache_buffer_chunk_size = value;
    }

    int SessionSettings::CacheExpiry::get()
    {
        return this->_settings->cache_expiry;
    }

    void SessionSettings::CacheExpiry::set(int value)
    {
        this->_settings->cache_expiry = value;
    }

    bool SessionSettings::UseReadCache::get()
    {
        return this->_settings->use_read_cache;
    }

    void SessionSettings::UseReadCache::set(bool value)
    {
        this->_settings->use_read_cache = value;
    }

    bool SessionSettings::ExplicitReadCache::get()
    {
        return this->_settings->explicit_read_cache;
    }

    void SessionSettings::ExplicitReadCache::set(bool value)
    {
        this->_settings->explicit_read_cache = value;
    }

    int SessionSettings::ExplicitCacheInterval::get()
    {
        return this->_settings->explicit_cache_interval;
    }

    void SessionSettings::ExplicitCacheInterval::set(int value)
    {
        this->_settings->explicit_cache_interval = value;
    }

    int SessionSettings::DiskIOWriteMode::get()
    {
        return this->_settings->disk_io_write_mode;
    }

    void SessionSettings::DiskIOWriteMode::set(int value)
    {
        this->_settings->disk_io_write_mode = value;
    }

    int SessionSettings::DiskIOReadMode::get()
    {
        return this->_settings->disk_io_read_mode;
    }

    void SessionSettings::DiskIOReadMode::set(int value)
    {
        this->_settings->disk_io_read_mode = value;
    }

    bool SessionSettings::CoalesceReads::get()
    {
        return this->_settings->coalesce_reads;
    }

    void SessionSettings::CoalesceReads::set(bool value)
    {
        this->_settings->coalesce_reads = value;
    }

    bool SessionSettings::CoalesceWrites::get()
    {
        return this->_settings->coalesce_writes;
    }

    void SessionSettings::CoalesceWrites::set(bool value)
    {
        this->_settings->coalesce_writes = value;
    }

    int SessionSettings::ActiveDownloads::get()
    {
        return this->_settings->active_downloads;
    }

    void SessionSettings::ActiveDownloads::set(int value)
    {
        this->_settings->active_downloads = value;
    }

    int SessionSettings::ActiveSeeds::get()
    {
        return this->_settings->active_seeds;
    }

    void SessionSettings::ActiveSeeds::set(int value)
    {
        this->_settings->active_seeds = value;
    }

    int SessionSettings::ActiveDhtLimit::get()
    {
        return this->_settings->active_dht_limit;
    }

    void SessionSettings::ActiveDhtLimit::set(int value)
    {
        this->_settings->active_dht_limit = value;
    }

    int SessionSettings::ActiveTrackerLimit::get()
    {
        return this->_settings->active_tracker_limit;
    }

    void SessionSettings::ActiveTrackerLimit::set(int value)
    {
        this->_settings->active_tracker_limit = value;
    }

    int SessionSettings::ActiveLsdLimit::get()
    {
        return this->_settings->active_lsd_limit;
    }

    void SessionSettings::ActiveLsdLimit::set(int value)
    {
        this->_settings->active_lsd_limit = value;
    }

    int SessionSettings::ActiveLimit::get()
    {
        return this->_settings->active_limit;
    }

    void SessionSettings::ActiveLimit::set(int value)
    {
        this->_settings->active_limit = value;
    }

    bool SessionSettings::AutoManagePreferSeeds::get()
    {
        return this->_settings->auto_manage_prefer_seeds;
    }

    void SessionSettings::AutoManagePreferSeeds::set(bool value)
    {
        this->_settings->auto_manage_prefer_seeds = value;
    }

    bool SessionSettings::DontCountSlowTorrents::get()
    {
        return this->_settings->dont_count_slow_torrents;
    }

    void SessionSettings::DontCountSlowTorrents::set(bool value)
    {
        this->_settings->dont_count_slow_torrents = value;
    }

    int SessionSettings::AutoManageInterval::get()
    {
        return this->_settings->auto_manage_interval;
    }

    void SessionSettings::AutoManageInterval::set(int value)
    {
        this->_settings->auto_manage_interval = value;
    }

    float SessionSettings::ShareRatioLimit::get()
    {
        return this->_settings->share_ratio_limit;
    }

    void SessionSettings::ShareRatioLimit::set(float value)
    {
        this->_settings->share_ratio_limit = value;
    }

    float SessionSettings::SeedTimeRatioLimit::get()
    {
        return this->_settings->seed_time_ratio_limit;
    }

    void SessionSettings::SeedTimeRatioLimit::set(float value)
    {
        this->_settings->seed_time_ratio_limit = value;
    }

    int SessionSettings::SeedTimeLimit::get()
    {
        return this->_settings->seed_time_limit;
    }

    void SessionSettings::SeedTimeLimit::set(int value)
    {
        this->_settings->seed_time_limit = value;
    }

    int SessionSettings::PeerTurnoverInterval::get()
    {
        return this->_settings->peer_turnover_interval;
    }

    void SessionSettings::PeerTurnoverInterval::set(int value)
    {
        this->_settings->peer_turnover_interval = value;
    }

    float SessionSettings::PeerTurnover::get()
    {
        return this->_settings->peer_turnover;
    }

    void SessionSettings::PeerTurnover::set(float value)
    {
        this->_settings->peer_turnover = value;
    }

    float SessionSettings::PeerTurnoverCutoff::get()
    {
        return this->_settings->peer_turnover_cutoff;
    }

    void SessionSettings::PeerTurnoverCutoff::set(float value)
    {
        this->_settings->peer_turnover_cutoff = value;
    }

    bool SessionSettings::CloseRedundantConnections::get()
    {
        return this->_settings->close_redundant_connections;
    }

    void SessionSettings::CloseRedundantConnections::set(bool value)
    {
        this->_settings->close_redundant_connections = value;
    }

    int SessionSettings::AutoScrapeInterval::get()
    {
        return this->_settings->auto_scrape_interval;
    }

    void SessionSettings::AutoScrapeInterval::set(int value)
    {
        this->_settings->auto_scrape_interval = value;
    }

    int SessionSettings::AutoScrapeMinInterval::get()
    {
        return this->_settings->auto_scrape_min_interval;
    }

    void SessionSettings::AutoScrapeMinInterval::set(int value)
    {
        this->_settings->auto_scrape_min_interval = value;
    }

    int SessionSettings::MaxPeerlistSize::get()
    {
        return this->_settings->max_peerlist_size;
    }

    void SessionSettings::MaxPeerlistSize::set(int value)
    {
        this->_settings->max_peerlist_size = value;
    }

    int SessionSettings::MaxPausedPeerlistSize::get()
    {
        return this->_settings->max_paused_peerlist_size;
    }

    void SessionSettings::MaxPausedPeerlistSize::set(int value)
    {
        this->_settings->max_paused_peerlist_size = value;
    }

    int SessionSettings::MinAnnounceInterval::get()
    {
        return this->_settings->min_announce_interval;
    }

    void SessionSettings::MinAnnounceInterval::set(int value)
    {
        this->_settings->min_announce_interval = value;
    }

    bool SessionSettings::PrioritizePartialPieces::get()
    {
        return this->_settings->prioritize_partial_pieces;
    }

    void SessionSettings::PrioritizePartialPieces::set(bool value)
    {
        this->_settings->prioritize_partial_pieces = value;
    }

    int SessionSettings::AutoManageStartup::get()
    {
        return this->_settings->auto_manage_startup;
    }

    void SessionSettings::AutoManageStartup::set(int value)
    {
        this->_settings->auto_manage_startup = value;
    }

    bool SessionSettings::RateLimitIPOverhead::get()
    {
        return this->_settings->rate_limit_ip_overhead;
    }

    void SessionSettings::RateLimitIPOverhead::set(bool value)
    {
        this->_settings->rate_limit_ip_overhead = value;
    }

    bool SessionSettings::AnnounceToAllTrackers::get()
    {
        return this->_settings->announce_to_all_trackers;
    }

    void SessionSettings::AnnounceToAllTrackers::set(bool value)
    {
        this->_settings->announce_to_all_tiers = value;
    }

    bool SessionSettings::AnnounceToAllTiers::get()
    {
        return this->_settings->announce_to_all_tiers;
    }

    void SessionSettings::AnnounceToAllTiers::set(bool value)
    {
        this->_settings->announce_to_all_tiers = value;
    }

    bool SessionSettings::PreferUdpTrackers::get()
    {
        return this->_settings->prefer_udp_trackers;
    }

    void SessionSettings::PreferUdpTrackers::set(bool value)
    {
        this->_settings->prefer_udp_trackers = value;
    }

    bool SessionSettings::StrictSuperSeeding::get()
    {
        return this->_settings->strict_super_seeding;
    }

    void SessionSettings::StrictSuperSeeding::set(bool value)
    {
        this->_settings->strict_super_seeding = value;
    }

    int SessionSettings::SeedingPieceQuota::get()
    {
        return this->_settings->seeding_piece_quota;
    }

    void SessionSettings::SeedingPieceQuota::set(int value)
    {
        this->_settings->seeding_piece_quota = value;
    }

    int SessionSettings::MaxSparseRegions::get()
    {
        return this->_settings->max_sparse_regions;
    }

    void SessionSettings::MaxSparseRegions::set(int value)
    {
        this->_settings->max_sparse_regions = value;
    }

    bool SessionSettings::LockDiskCache::get()
    {
        return this->_settings->lock_disk_cache;
    }

    void SessionSettings::LockDiskCache::set(bool value)
    {
        this->_settings->lock_disk_cache = value;
    }

    int SessionSettings::MaxRejects::get()
    {
        return this->_settings->max_rejects;
    }

    void SessionSettings::MaxRejects::set(int value)
    {
        this->_settings->max_rejects = value;
    }

    int SessionSettings::ReceiveSocketBufferSize::get()
    {
        return this->_settings->recv_socket_buffer_size;
    }

    void SessionSettings::ReceiveSocketBufferSize::set(int value)
    {
        this->_settings->recv_socket_buffer_size = value;
    }

    int SessionSettings::SendSocketBufferSize::get()
    {
        return this->_settings->send_socket_buffer_size;
    }

    void SessionSettings::SendSocketBufferSize::set(int value)
    {
        this->_settings->send_socket_buffer_size = value;
    }

    bool SessionSettings::OptimizeHashingForSpeed::get()
    {
        return this->_settings->optimize_hashing_for_speed;
    }

    void SessionSettings::OptimizeHashingForSpeed::set(bool value)
    {
        this->_settings->optimize_hashing_for_speed = value;
    }

    int SessionSettings::FileChecksDelayPerBlock::get()
    {
        return this->_settings->file_checks_delay_per_block;
    }

    void SessionSettings::FileChecksDelayPerBlock::set(int value)
    {
        this->_settings->file_checks_delay_per_block = value;
    }

    int SessionSettings::ReadCacheLineSize::get()
    {
        return this->_settings->read_cache_line_size;
    }

    void SessionSettings::ReadCacheLineSize::set(int value)
    {
        this->_settings->read_cache_line_size = value;
    }

    int SessionSettings::WriteCacheLineSize::get()
    {
        return this->_settings->write_cache_line_size;
    }

    void SessionSettings::WriteCacheLineSize::set(int value)
    {
        this->_settings->write_cache_line_size = value;
    }

    int SessionSettings::OptimisticDiskRetry::get()
    {
        return this->_settings->optimistic_disk_retry;
    }

    void SessionSettings::OptimisticDiskRetry::set(int value)
    {
        this->_settings->optimistic_disk_retry = value;
    }

    bool SessionSettings::DisableHashChecks::get()
    {
        return this->_settings->disable_hash_checks;
    }

    void SessionSettings::DisableHashChecks::set(bool value)
    {
        this->_settings->disable_hash_checks = value;
    }

    bool SessionSettings::AllowReorderedDiskOperations::get()
    {
        return this->_settings->allow_reordered_disk_operations;
    }

    void SessionSettings::AllowReorderedDiskOperations::set(bool value)
    {
        this->_settings->allow_reordered_disk_operations = value;
    }

    bool SessionSettings::AllowI2PMixed::get()
    {
        return this->_settings->allow_i2p_mixed;
    }

    void SessionSettings::AllowI2PMixed::set(bool value)
    {
        this->_settings->allow_i2p_mixed = value;
    }

    int SessionSettings::MaxSuggestPieces::get()
    {
        return this->_settings->max_suggest_pieces;
    }

    void SessionSettings::MaxSuggestPieces::set(int value)
    {
        this->_settings->max_suggest_pieces = value;
    }

    bool SessionSettings::DropSkippedRequests::get()
    {
        return this->_settings->drop_skipped_requests;
    }

    void SessionSettings::DropSkippedRequests::set(bool value)
    {
        this->_settings->drop_skipped_requests = value;
    }

    bool SessionSettings::LowPrioDisk::get()
    {
        return this->_settings->low_prio_disk;
    }

    void SessionSettings::LowPrioDisk::set(bool value)
    {
        this->_settings->low_prio_disk = value;
    }

    int SessionSettings::LocalServiceAnnounceInterval::get()
    {
        return this->_settings->local_service_announce_interval;
    }

    void SessionSettings::LocalServiceAnnounceInterval::set(int value)
    {
        this->_settings->local_service_announce_interval = value;
    }

    int SessionSettings::DhtAnnounceInterval::get()
    {
        return this->_settings->dht_announce_interval;
    }

    void SessionSettings::DhtAnnounceInterval::set(int value)
    {
        this->_settings->dht_announce_interval = value;
    }

    int SessionSettings::UdpTrackerTokenExpiry::get()
    {
        return this->_settings->udp_tracker_token_expiry;
    }

    void SessionSettings::UdpTrackerTokenExpiry::set(int value)
    {
        this->_settings->udp_tracker_token_expiry = value;
    }

    bool SessionSettings::VolatileReadCache::get()
    {
        return this->_settings->volatile_read_cache;
    }

    void SessionSettings::VolatileReadCache::set(bool value)
    {
        this->_settings->volatile_read_cache = value;
    }

    bool SessionSettings::GuidedReadCache::get()
    {
        return this->_settings->guided_read_cache;
    }

    void SessionSettings::GuidedReadCache::set(bool value)
    {
        this->_settings->guided_read_cache = value;
    }

    int SessionSettings::DefaultCacheMinAge::get()
    {
        return this->_settings->default_cache_min_age;
    }

    void SessionSettings::DefaultCacheMinAge::set(int value)
    {
        this->_settings->default_cache_min_age = value;
    }

    int SessionSettings::NumOptimisticUnchokeSlots::get()
    {
        return this->_settings->num_optimistic_unchoke_slots;
    }

    void SessionSettings::NumOptimisticUnchokeSlots::set(int value)
    {
        this->_settings->num_optimistic_unchoke_slots = value;
    }

    int SessionSettings::DefaultEstReciprocationRate::get()
    {
        return this->_settings->default_est_reciprocation_rate;
    }

    void SessionSettings::DefaultEstReciprocationRate::set(int value)
    {
        this->_settings->default_est_reciprocation_rate = value;
    }

    int SessionSettings::IncreaseEstReciprocationRate::get()
    {
        return this->_settings->increase_est_reciprocation_rate;
    }

    void SessionSettings::IncreaseEstReciprocationRate::set(int value)
    {
        this->_settings->increase_est_reciprocation_rate = value;
    }

    int SessionSettings::DecreaseEstReciprocationRate::get()
    {
        return this->_settings->decrease_est_reciprocation_rate;
    }

    void SessionSettings::DecreaseEstReciprocationRate::set(int value)
    {
        this->_settings->decrease_est_reciprocation_rate = value;
    }

    bool SessionSettings::IncomingStartsQueuedTorrents::get()
    {
        return this->_settings->incoming_starts_queued_torrents;
    }

    void SessionSettings::IncomingStartsQueuedTorrents::set(bool value)
    {
        this->_settings->incoming_starts_queued_torrents = value;
    }

    bool SessionSettings::ReportTrueDownloaded::get()
    {
        return this->_settings->report_true_downloaded;
    }

    void SessionSettings::ReportTrueDownloaded::set(bool value)
    {
        this->_settings->report_true_downloaded = value;
    }

    bool SessionSettings::StrictEndGameMode::get()
    {
        return this->_settings->strict_end_game_mode;
    }

    void SessionSettings::StrictEndGameMode::set(bool value)
    {
        this->_settings->strict_end_game_mode = value;
    }

    bool SessionSettings::BroadcastLsd::get()
    {
        return this->_settings->broadcast_lsd;
    }

    void SessionSettings::BroadcastLsd::set(bool value)
    {
        this->_settings->broadcast_lsd = value;
    }

    bool SessionSettings::EnableOutgoingUtp::get()
    {
        return this->_settings->enable_outgoing_utp;
    }

    void SessionSettings::EnableOutgoingUtp::set(bool value)
    {
        this->_settings->enable_outgoing_utp = value;
    }

    bool SessionSettings::EnableIncomingUtp::get()
    {
        return this->_settings->enable_incoming_utp;
    }

    void SessionSettings::EnableIncomingUtp::set(bool value)
    {
        this->_settings->enable_incoming_utp = value;
    }

    bool SessionSettings::EnableOutgoingTcp::get()
    {
        return this->_settings->enable_outgoing_tcp;
    }

    void SessionSettings::EnableOutgoingTcp::set(bool value)
    {
        this->_settings->enable_outgoing_tcp = value;
    }

    bool SessionSettings::EnableIncomingTcp::get()
    {
        return this->_settings->enable_incoming_tcp;
    }

    void SessionSettings::EnableIncomingTcp::set(bool value)
    {
        this->_settings->enable_incoming_tcp = value;
    }

    int SessionSettings::MaxPeerExchangePeers::get()
    {
        return this->_settings->max_pex_peers;
    }

    void SessionSettings::MaxPeerExchangePeers::set(int value)
    {
        this->_settings->max_pex_peers = value;
    }

    bool SessionSettings::IgnoreResumeTimestamps::get()
    {
        return this->_settings->ignore_resume_timestamps;
    }

    void SessionSettings::IgnoreResumeTimestamps::set(bool value)
    {
        this->_settings->ignore_resume_timestamps = value;
    }

    bool SessionSettings::NoRecheckIncompleteResume::get()
    {
        return this->_settings->no_recheck_incomplete_resume;
    }

    void SessionSettings::NoRecheckIncompleteResume::set(bool value)
    {
        this->_settings->no_recheck_incomplete_resume = value;
    }

    bool SessionSettings::AnonymousMode::get()
    {
        return this->_settings->anonymous_mode;
    }

    void SessionSettings::AnonymousMode::set(bool value)
    {
        this->_settings->anonymous_mode = value;
    }

    bool SessionSettings::ForceProxy::get()
    {
        return this->_settings->force_proxy;
    }

    void SessionSettings::ForceProxy::set(bool value)
    {
        this->_settings->force_proxy = value;
    }

    int SessionSettings::TickInterval::get()
    {
        return this->_settings->tick_interval;
    }

    void SessionSettings::TickInterval::set(int value)
    {
        this->_settings->tick_interval = value;
    }

    bool SessionSettings::ReportWebSeedDownloads::get()
    {
        return this->_settings->report_web_seed_downloads;
    }

    void SessionSettings::ReportWebSeedDownloads::set(bool value)
    {
        this->_settings->report_web_seed_downloads = value;
    }

    int SessionSettings::ShareModeTarget::get()
    {
        return this->_settings->share_mode_target;
    }

    void SessionSettings::ShareModeTarget::set(int value)
    {
        this->_settings->share_mode_target = value;
    }

    int SessionSettings::UploadRateLimit::get()
    {
        return this->_settings->upload_rate_limit;
    }

    void SessionSettings::UploadRateLimit::set(int value)
    {
        this->_settings->upload_rate_limit = value;
    }

    int SessionSettings::DownloadRateLimit::get()
    {
        return this->_settings->download_rate_limit;
    }

    void SessionSettings::DownloadRateLimit::set(int value)
    {
        this->_settings->download_rate_limit = value;
    }

    int SessionSettings::LocalUploadRateLimit::get()
    {
        return this->_settings->local_upload_rate_limit;
    }

    void SessionSettings::LocalUploadRateLimit::set(int value)
    {
        this->_settings->local_upload_rate_limit = value;
    }

    int SessionSettings::LocalDownloadRateLimit::get()
    {
        return this->_settings->local_download_rate_limit;
    }

    void SessionSettings::LocalDownloadRateLimit::set(int value)
    {
        this->_settings->local_download_rate_limit = value;
    }

    int SessionSettings::DhtUploadRateLimit::get()
    {
        return this->_settings->dht_upload_rate_limit;
    }

    void SessionSettings::DhtUploadRateLimit::set(int value)
    {
        this->_settings->dht_upload_rate_limit = value;
    }

    int SessionSettings::UnchokeSlotsLimit::get()
    {
        return this->_settings->unchoke_slots_limit;
    }

    void SessionSettings::UnchokeSlotsLimit::set(int value)
    {
        this->_settings->unchoke_slots_limit = value;
    }

    int SessionSettings::HalfOpenLimit::get()
    {
        return this->_settings->half_open_limit;
    }

    void SessionSettings::HalfOpenLimit::set(int value)
    {
        this->_settings->half_open_limit = value;
    }

    int SessionSettings::ConnectionsLimit::get()
    {
        return this->_settings->connections_limit;
    }

    void SessionSettings::ConnectionsLimit::set(int value)
    {
        this->_settings->connections_limit = value;
    }

    int SessionSettings::ConnectionsSlack::get()
    {
        return this->_settings->connections_slack;
    }

    void SessionSettings::ConnectionsSlack::set(int value)
    {
        this->_settings->connections_slack = value;
    }

    int SessionSettings::UtpTargetDelay::get()
    {
        return this->_settings->utp_target_delay;
    }

    void SessionSettings::UtpTargetDelay::set(int value)
    {
        this->_settings->utp_target_delay = value;
    }

    int SessionSettings::UtpGainFactor::get()
    {
        return this->_settings->utp_gain_factor;
    }

    void SessionSettings::UtpGainFactor::set(int value)
    {
        this->_settings->utp_gain_factor = value;
    }

    int SessionSettings::UtpMinTimeout::get()
    {
        return this->_settings->utp_min_timeout;
    }

    void SessionSettings::UtpMinTimeout::set(int value)
    {
        this->_settings->utp_min_timeout = value;
    }

    int SessionSettings::UtpSynResends::get()
    {
        return this->_settings->utp_syn_resends;
    }

    void SessionSettings::UtpSynResends::set(int value)
    {
        this->_settings->utp_syn_resends = value;
    }

    int SessionSettings::UtpFinResends::get()
    {
        return this->_settings->utp_fin_resends;
    }

    void SessionSettings::UtpFinResends::set(int value)
    {
        this->_settings->utp_fin_resends = value;
    }

    int SessionSettings::UtpNumResends::get()
    {
        return this->_settings->utp_num_resends;
    }

    void SessionSettings::UtpNumResends::set(int value)
    {
        this->_settings->utp_num_resends = value;
    }

    int SessionSettings::UtpConnectTimeout::get()
    {
        return this->_settings->utp_connect_timeout;
    }

    void SessionSettings::UtpConnectTimeout::set(int value)
    {
        this->_settings->utp_connect_timeout = value;
    }

    bool SessionSettings::UtpDynamicSocketBuffer::get()
    {
        return this->_settings->utp_dynamic_sock_buf;
    }

    void SessionSettings::UtpDynamicSocketBuffer::set(bool value)
    {
        this->_settings->utp_dynamic_sock_buf = value;
    }

    int SessionSettings::UtpLossMultiplier::get()
    {
        return this->_settings->utp_loss_multiplier;
    }

    void SessionSettings::UtpLossMultiplier::set(int value)
    {
        this->_settings->utp_loss_multiplier = value;
    }

    int SessionSettings::MixedModeAlgorithm::get()
    {
        return this->_settings->mixed_mode_algorithm;
    }

    void SessionSettings::MixedModeAlgorithm::set(int value)
    {
        this->_settings->mixed_mode_algorithm = value;
    }

    bool SessionSettings::RateLimitUtp::get()
    {
        return this->_settings->rate_limit_utp;
    }

    void SessionSettings::RateLimitUtp::set(bool value)
    {
        this->_settings->rate_limit_utp = value;
    }

    int SessionSettings::ListenQueueSize::get()
    {
        return this->_settings->listen_queue_size;
    }

    void SessionSettings::ListenQueueSize::set(int value)
    {
        this->_settings->listen_queue_size = value;
    }

    bool SessionSettings::AnnounceDoubleNat::get()
    {
        return this->_settings->announce_double_nat;
    }

    void SessionSettings::AnnounceDoubleNat::set(bool value)
    {
        this->_settings->announce_double_nat = value;
    }

    int SessionSettings::TorrentConnectBoost::get()
    {
        return this->_settings->torrent_connect_boost;
    }

    void SessionSettings::TorrentConnectBoost::set(int value)
    {
        this->_settings->torrent_connect_boost = value;
    }

    bool SessionSettings::SeedingOutgoingConnections::get()
    {
        return this->_settings->seeding_outgoing_connections;
    }

    void SessionSettings::SeedingOutgoingConnections::set(bool value)
    {
        this->_settings->seeding_outgoing_connections = value;
    }

    bool SessionSettings::NoConnectPrivilegedPorts::get()
    {
        return this->_settings->no_connect_privileged_ports;
    }

    void SessionSettings::NoConnectPrivilegedPorts::set(bool value)
    {
        this->_settings->no_connect_privileged_ports = value;
    }

    int SessionSettings::AlertQueueSize::get()
    {
        return this->_settings->alert_queue_size;
    }

    void SessionSettings::AlertQueueSize::set(int value)
    {
        this->_settings->alert_queue_size = value;
    }

    int SessionSettings::MaxMetadataSize::get()
    {
        return this->_settings->max_metadata_size;
    }

    void SessionSettings::MaxMetadataSize::set(int value)
    {
        this->_settings->max_metadata_size = value;
    }

    bool SessionSettings::SmoothConnects::get()
    {
        return this->_settings->smooth_connects;
    }

    void SessionSettings::SmoothConnects::set(bool value)
    {
        this->_settings->smooth_connects = value;
    }

    bool SessionSettings::AlwaysSendUserAgent::get()
    {
        return this->_settings->always_send_user_agent;
    }

    void SessionSettings::AlwaysSendUserAgent::set(bool value)
    {
        this->_settings->always_send_user_agent = value;
    }

    bool SessionSettings::ApplyIPFilterToTrackers::get()
    {
        return this->_settings->apply_ip_filter_to_trackers;
    }

    void SessionSettings::ApplyIPFilterToTrackers::set(bool value)
    {
        this->_settings->apply_ip_filter_to_trackers = value;
    }

    int SessionSettings::ReadJobEvery::get()
    {
        return this->_settings->read_job_every;
    }

    void SessionSettings::ReadJobEvery::set(int value)
    {
        this->_settings->read_job_every = value;
    }

    bool SessionSettings::UseDiskReadAhead::get()
    {
        return this->_settings->use_disk_read_ahead;
    }

    void SessionSettings::UseDiskReadAhead::set(bool value)
    {
        this->_settings->use_disk_read_ahead = value;
    }

    bool SessionSettings::LockFiles::get()
    {
        return this->_settings->lock_files;
    }

    void SessionSettings::LockFiles::set(bool value)
    {
        this->_settings->lock_files = value;
    }

    int SessionSettings::SslListen::get()
    {
        return this->_settings->ssl_listen;
    }

    void SessionSettings::SslListen::set(int value)
    {
        this->_settings->ssl_listen = value;
    }

    int SessionSettings::TrackerBackoff::get()
    {
        return this->_settings->tracker_backoff;
    }

    void SessionSettings::TrackerBackoff::set(int value)
    {
        this->_settings->tracker_backoff = value;
    }

    bool SessionSettings::BanWebSeeds::get()
    {
        return this->_settings->ban_web_seeds;
    }

    void SessionSettings::BanWebSeeds::set(bool value)
    {
        this->_settings->ban_web_seeds = value;
    }

    int SessionSettings::MaxHttpReceiveBufferSize::get()
    {
        return this->_settings->max_http_recv_buffer_size;
    }

    void SessionSettings::MaxHttpReceiveBufferSize::set(int value)
    {
        this->_settings->max_http_recv_buffer_size = value;
    }

    bool SessionSettings::SupportShareMode::get()
    {
        return this->_settings->support_share_mode;
    }

    void SessionSettings::SupportShareMode::set(bool value)
    {
        this->_settings->support_share_mode = value;
    }

    bool SessionSettings::SupportMerkleTorrents::get()
    {
        return this->_settings->support_merkle_torrents;
    }

    void SessionSettings::SupportMerkleTorrents::set(bool value)
    {
        this->_settings->support_merkle_torrents = value;
    }

    bool SessionSettings::ReportReduntantBytes::get()
    {
        return this->_settings->report_redundant_bytes;
    }

    void SessionSettings::ReportReduntantBytes::set(bool value)
    {
        this->_settings->report_redundant_bytes = value;
    }

    String^ SessionSettings::HandshakeClientVersion::get()
    {
        return gcnew String(this->_settings->handshake_client_version.c_str());
    }

    void SessionSettings::HandshakeClientVersion::set(String^ value)
    {
        this->_settings->handshake_client_version = msclr::interop::marshal_as<std::string>(value);
    }

    bool SessionSettings::UseDiskCachePool::get()
    {
        return this->_settings->use_disk_cache_pool;
    }

    void SessionSettings::UseDiskCachePool::set(bool value)
    {
        this->_settings->use_disk_cache_pool = value;
    }

    int SessionSettings::InactiveDownRate::get()
    {
        return this->_settings->inactive_down_rate;
    }

    void SessionSettings::InactiveDownRate::set(int value)
    {
        this->_settings->inactive_down_rate = value;
    }

    int SessionSettings::InactiveUpRate::get()
    {
        return this->_settings->inactive_up_rate;
    }

    void SessionSettings::InactiveUpRate::set(int value)
    {
        this->_settings->inactive_up_rate = value;
    }
}