#pragma once

#include "ISession.h"

namespace libtorrent
{
    class session;
}

namespace Ragnar
{
    ref class Fingerprint;

    public ref class Session : ISession
    {
    private:
        bool _disposed;
        libtorrent::session* _session;
        IAlertFactory^ _alertFactory;

    public:
        Session();
        Session(Fingerprint^ fingerprint);

        ~Session();
        !Session();

        virtual void LoadState(cli::array<byte>^ buffer);

        virtual cli::array<byte>^ SaveState();

        // TODO: void refresh_torrent_status (std::vector<torrent_status>* ret, boost::uint32_t flags = 0) const;
        // TODO: void get_torrent_status(std::vector<torrent_status>* ret, boost::function<bool(torrent_status const&)> const& pred, boost::uint32_t flags = 0) const;

        virtual void PostTorrentUpdates();

        virtual TorrentHandle^ FindTorrent(System::String^ infoHash);

        virtual System::Collections::Generic::IEnumerable<TorrentHandle^>^ GetTorrents();

        virtual TorrentHandle^ AddTorrent(AddTorrentParams^ params);

        virtual void AsyncAddTorrent(AddTorrentParams^ params);

        // TODO: session_proxy abort ();

        virtual void Pause();

        virtual void Resume();

        virtual property bool IsPaused { bool get(); }

        virtual SessionStatus^ QueryStatus();

        // TODO: cache_status get_cache_status () const;

        // TODO: void get_cache_info (sha1_hash const& ih, std::vector<cached_piece_info>& ret) const;

        // TODO: feed_handle add_feed (feed_settings const& feed);
        // TODO: void remove_feed (feed_handle h);
        // TODO: void get_feeds (std::vector<feed_handle>& f) const;

        virtual property bool IsDhtRunning { bool get(); }

        virtual void StartDht();

        virtual void StopDht();

        // TODO: void set_dht_settings (dht_settings const& settings);

        // TODO: void add_dht_router (std::pair<std::string, int> const& node);
        // TODO: void add_dht_node(std::pair<std::string, int> const& node);

        // TODO: void dht_get_item (sha1_hash const& target);

        // TODO: void dht_get_item (boost::array<char, 32> key, std::string salt = std::string());

        // TODO: sha1_hash dht_put_item (entry data);

        // TODO: void dht_put_item (boost::array<char, 32> key, boost::function<void(entry&, boost::array<char, 64>&, boost::uint64_t&, std::string const&)> cb, std::string salt = std::string());

        // TODO: void add_extension (boost::function<boost::shared_ptr<torrent_plugin>(torrent*, void*)> ext);
        // TODO: void add_extension(boost::shared_ptr<plugin> ext);

        // TODO: void LoadCountryDatabase(System::String^ file);

        // TODO: void LoadAsNumDatabase(System::String^ file);

        // TODO: int as_for_ip (address const& addr);

        // TODO: ip_filter get_ip_filter () const;
        // TODO: void set_ip_filter(ip_filter const& f);

        // TODO: void set_port_filter (port_filter const& f);

        // TODO: peer_id id () const;
        // TODO: void set_peer_id(peer_id const& pid);

        virtual void SetKey(int key);

        virtual void ListenOn(int lower, int upper); // TODO: implement interface selector
        
        virtual property bool IsListening { bool get(); }

        virtual property int ListenPort { int get(); }

        virtual property int SslListenPort { int get(); }

        virtual void RemoveTorrent(TorrentHandle^ handle);

        virtual void RemoveTorrent(TorrentHandle^ handle, bool removeData);

        virtual SessionSettings^ QuerySettings();

        virtual void SetSettings(SessionSettings^ settings);

        // TODO: pe_settings get_pe_settings() const;
        // TODO: void set_pe_settings(pe_settings const& settings);

        // TODO: void set_proxy (proxy_settings const& s);
        // TODO: proxy_settings proxy() const;

        // TODO: proxy_settings i2p_proxy () const;
        // TODO: void set_i2p_proxy(proxy_settings const& s);

        virtual property IAlertFactory^ Alerts { IAlertFactory^ get(); }

        virtual void SetAlertMask(SessionAlertCategory mask);

        virtual void StopLsd();

        virtual void StartLsd();

        virtual void StopUpnp();

        virtual void StartUpnp();

        // TODO: void delete_port_mapping (int handle);
        // TODO: int add_port_mapping(protocol_type t, int external_port, int local_port);

        virtual void StopNatPmp();

        virtual void StartNatPmp();
    };
}

