#pragma once

namespace libtorrent
{
    struct session_settings;
}

namespace Ragnar
{
    using namespace System;

    public ref class SessionSettings
    {
    private:
        libtorrent::session_settings* _settings;

    internal:
        SessionSettings(const libtorrent::session_settings &settings);
        libtorrent::session_settings* get_ptr() { return this->_settings; }

    public:
        ~SessionSettings();

        property String^ UserAgent { String^ get(); void set(String^ value); }

        /// <summary>
        /// Gets or sets the number of seconds the tracker connection will wait from when it
        /// sent the request until it considers the tracker to have timed-out.
        /// Default value is 60 seconds.
        /// </summary>
        property int TrackerCompletionTimeout { int get(); void set(int value); }

        property int TrackerReceiveTimeout { int get(); void set(int value); }

        property int StopTrackerTimeout { int get(); void set(int value); }

        property int TrackerMaximumResponseLength { int get(); void set(int value); }

        property int PieceTimeout { int get(); void set(int value); }

        property int RequestTimeout { int get(); void set(int value); }

        property int RequestQueueTime { int get(); void set(int value); }

        property int MaxAllowedInRequestQueue { int get(); void set(int value); }

        property int MaxOutRequestQueue { int get(); void set(int value); }

        property int WholePiecesThreshold { int get(); void set(int value); }

        property int PeerTimeout { int get(); void set(int value); }

        property int UrlSeedTimeout { int get(); void set(int value); }

        property int UrlSeedPipelineSize { int get(); void set(int value); }

        property int UrlSeedWaitRetry { int get(); void set(int value); }

        property int FilePoolSize { int get(); void set(int value); }

        property bool AllowMultipleConnectionsPerIP { bool get(); void set(bool value); }

        property int MaxFailCount { int get(); void set(int value); }

        property int MinReconnectTime { int get(); void set(int value); }

        property int PeerConnectTimeout { int get(); void set(int value); }

        property bool IgnoreLimitsOnLocalNetwork { bool get(); void set(bool value); }

        property int ConnectionSpeed { int get(); void set(int value); }

        property bool SendRedundantHave { bool get(); void set(bool value); }

        property bool LazyBitfields { bool get(); void set(bool value); }

        property int InactivityTimeout { int get(); void set(int value); }

        property int UnchokeInterval { int get(); void set(int value); }

        property int OptimisticUnchokeInterval { int get(); void set(int value); }

        property String^ AnnounceIP { String^ get(); void set(String^ value); }

        property int NumWant { int get(); void set(int value); }

        property int InitialPickerThreshold { int get(); void set(int value); }

        property int AllowedFastSetSize { int get(); void set(int value); }

        property int SuggestMode { int get(); void set(int value); }

        property int MaxQueuedDiskBytes { int get(); void set(int value); }

        property int MaxQueuedDiskBytesLowWatermark { int get(); void set(int value); }

        property int HandshakeTimeout { int get(); void set(int value); }

        property bool UseDhtAsFallback { bool get(); void set(bool value); }

        property bool FreeTorrentHashes { bool get(); void set(bool value); }

        property bool UpnpIgnoreNonRouters { bool get(); void set(bool value); }

        property int SendBufferLowWatermark { int get(); void set(int value); }

        property int SendBufferWatermark { int get(); void set(int value); }

        property int SendBufferWatermarkFactor { int get(); void set(int value); }

        property int ChokingAlgorithm { int get(); void set(int value); }

        property int SeedChokingAlgorithm { int get(); void set(int value); }

        property bool UseParoleMode { bool get(); void set(bool value); }

        property int CacheSize { int get(); void set(int value); }

        property int CacheBufferChunkSize { int get(); void set(int value); }

        property int CacheExpiry { int get(); void set(int value); }

        property bool UseReadCache { bool get(); void set(bool value); }

        property bool ExplicitReadCache { bool get(); void set(bool value); }

        property int ExplicitCacheInterval { int get(); void set(int value); }

        property int DiskIOWriteMode { int get(); void set(int value); }

        property int DiskIOReadMode { int get(); void set(int value); }

        property bool CoalesceReads { bool get(); void set(bool value); }

        property bool CoalesceWrites { bool get(); void set(bool value); }

        // TODO: std::pair<int, int> outgoing_ports

        // TODO: char peer_tos

        property int ActiveDownloads { int get(); void set(int value); }

        property int ActiveSeeds { int get(); void set(int value); }

        property int ActiveDhtLimit { int get(); void set(int value); }

        property int ActiveTrackerLimit { int get(); void set(int value); }

        property int ActiveLsdLimit { int get(); void set(int value); }

        property int ActiveLimit { int get(); void set(int value); }

        property bool AutoManagePreferSeeds { bool get(); void set(bool value); }

        property bool DontCountSlowTorrents { bool get(); void set(bool value); }

        property int AutoManageInterval { int get(); void set(int value); }

        property float ShareRatioLimit { float get(); void set(float value); }

        property float SeedTimeRatioLimit { float get(); void set(float value); }

        property int SeedTimeLimit { int get(); void set(int value); }

        property int PeerTurnoverInterval { int get(); void set(int value); }

        property float PeerTurnover { float get(); void set(float value); }

        property float PeerTurnoverCutoff { float get(); void set(float value); }

        property bool CloseRedundantConnections { bool get(); void set(bool value); }

        property int AutoScrapeInterval { int get(); void set(int value); }

        property int AutoScrapeMinInterval { int get(); void set(int value); }

        property int MaxPeerlistSize { int get(); void set(int value); }

        property int MaxPausedPeerlistSize { int get(); void set(int value); }

        property int MinAnnounceInterval { int get(); void set(int value); }

        property bool PrioritizePartialPieces { bool get(); void set(bool value); }

        property int AutoManageStartup { int get(); void set(int value); }

        property bool RateLimitIPOverhead { bool get(); void set(bool value); }

        property bool AnnounceToAllTrackers { bool get(); void set(bool value); }

        property bool AnnounceToAllTiers { bool get(); void set(bool value); }

        property bool PreferUdpTrackers { bool get(); void set(bool value); }

        property bool StrictSuperSeeding { bool get(); void set(bool value); }

        property int SeedingPieceQuota { int get(); void set(int value); }

        property int MaxSparseRegions { int get(); void set(int value); }

        property bool LockDiskCache { bool get(); void set(bool value); }

        property int MaxRejects { int get(); void set(int value); }

        property int ReceiveSocketBufferSize { int get(); void set(int value); }

        property int SendSocketBufferSize { int get(); void set(int value); }

        property bool OptimizeHashingForSpeed { bool get(); void set(bool value); }

        property int FileChecksDelayPerBlock { int get(); void set(int value); }

        // TODO: disk_cache_algo_t disk_cache_algorithm

        property int ReadCacheLineSize { int get(); void set(int value); }

        property int WriteCacheLineSize { int get(); void set(int value); }

        property int OptimisticDiskRetry { int get(); void set(int value); }

        property bool DisableHashChecks { bool get(); void set(bool value); }

        property bool AllowReorderedDiskOperations { bool get(); void set(bool value); }

        property bool AllowI2PMixed { bool get(); void set(bool value); }

        property int MaxSuggestPieces { int get(); void set(int value); }

        property bool DropSkippedRequests { bool get(); void set(bool value); }

        property bool LowPrioDisk { bool get(); void set(bool value); }

        property int LocalServiceAnnounceInterval { int get(); void set(int value); }

        property int DhtAnnounceInterval { int get(); void set(int value); }

        property int UdpTrackerTokenExpiry { int get(); void set(int value); }

        property bool VolatileReadCache { bool get(); void set(bool value); }

        property bool GuidedReadCache { bool get(); void set(bool value); }

        property int DefaultCacheMinAge { int get(); void set(int value); }

        property int NumOptimisticUnchokeSlots { int get(); void set(int value); }

        property int DefaultEstReciprocationRate { int get(); void set(int value); }

        property int IncreaseEstReciprocationRate { int get(); void set(int value); }

        property int DecreaseEstReciprocationRate { int get(); void set(int value); }

        property bool IncomingStartsQueuedTorrents { bool get(); void set(bool value); }

        property bool ReportTrueDownloaded { bool get(); void set(bool value); }

        property bool StrictEndGameMode { bool get(); void set(bool value); }

        property bool BroadcastLsd { bool get(); void set(bool value); }

        property bool EnableOutgoingUtp { bool get(); void set(bool value); }

        property bool EnableIncomingUtp { bool get(); void set(bool value); }

        property bool EnableOutgoingTcp { bool get(); void set(bool value); }

        property bool EnableIncomingTcp { bool get(); void set(bool value); }

        property int MaxPeerExchangePeers { int get(); void set(int value); }

        property bool IgnoreResumeTimestamps { bool get(); void set(bool value); }

        property bool NoRecheckIncompleteResume { bool get(); void set(bool value); }

        property bool AnonymousMode { bool get(); void set(bool value); }

        property bool ForceProxy { bool get(); void set(bool value); }

        property int TickInterval { int get(); void set(int value); }

        property bool ReportWebSeedDownloads { bool get(); void set(bool value); }

        property int ShareModeTarget { int get(); void set(int value); }

        property int UploadRateLimit { int get(); void set(int value); }

        property int DownloadRateLimit { int get(); void set(int value); }

        property int LocalUploadRateLimit { int get(); void set(int value); }

        property int LocalDownloadRateLimit { int get(); void set(int value); }

        property int DhtUploadRateLimit { int get(); void set(int value); }

        property int UnchokeSlotsLimit { int get(); void set(int value); }

        property int HalfOpenLimit { int get(); void set(int value); }

        property int ConnectionsLimit { int get(); void set(int value); }

        property int ConnectionsSlack { int get(); void set(int value); }

        property int UtpTargetDelay { int get(); void set(int value); }

        property int UtpGainFactor { int get(); void set(int value); }

        property int UtpMinTimeout { int get(); void set(int value); }

        property int UtpSynResends { int get(); void set(int value); }

        property int UtpFinResends { int get(); void set(int value); }

        property int UtpNumResends { int get(); void set(int value); }

        property int UtpConnectTimeout { int get(); void set(int value); }

        property bool UtpDynamicSocketBuffer { bool get(); void set(bool value); }

        property int UtpLossMultiplier { int get(); void set(int value); }

        property int MixedModeAlgorithm { int get(); void set(int value); }

        property bool RateLimitUtp { bool get(); void set(bool value); }

        property int ListenQueueSize { int get(); void set(int value); }

        property bool AnnounceDoubleNat { bool get(); void set(bool value); }

        property int TorrentConnectBoost { int get(); void set(int value); }

        property bool SeedingOutgoingConnections { bool get(); void set(bool value); }

        property bool NoConnectPrivilegedPorts { bool get(); void set(bool value); }

        property int AlertQueueSize { int get(); void set(int value); }

        property int MaxMetadataSize { int get(); void set(int value); }

        property bool SmoothConnects { bool get(); void set(bool value); }

        property bool AlwaysSendUserAgent { bool get(); void set(bool value); }

        property bool ApplyIPFilterToTrackers { bool get(); void set(bool value); }

        property int ReadJobEvery { int get(); void set(int value); }

        property bool UseDiskReadAhead { bool get(); void set(bool value); }

        property bool LockFiles { bool get(); void set(bool value); }

        property int SslListen { int get(); void set(int value); }

        property int TrackerBackoff { int get(); void set(int value); }

        property bool BanWebSeeds { bool get(); void set(bool value); }

        property int MaxHttpReceiveBufferSize { int get(); void set(int value); }

        property bool SupportShareMode { bool get(); void set(bool value); }

        property bool SupportMerkleTorrents { bool get(); void set(bool value); }

        property bool ReportReduntantBytes { bool get(); void set(bool value); }

        property String^ HandshakeClientVersion { String^ get(); void set(String^ value); }

        property bool UseDiskCachePool { bool get(); void set(bool value); }

        property int InactiveDownRate { int get(); void set(int value); }

        property int InactiveUpRate { int get(); void set(int value); }
    };
}
