#include "stdafx.h"
#include "AddTorrentParams.h"
#include "AlertFactory.h"
#include "Fingerprint.h"
#include "Session.h"
#include "SessionAlertCategory.h"
#include "SessionSettings.h"
#include "SessionStatus.h"
#include "TorrentHandle.h"
#include "Utils.h"

#include <libtorrent\bencode.hpp>
#include <libtorrent\lazy_entry.hpp>
#include <libtorrent\session.hpp>

namespace Ragnar
{
    Session::Session()
    {
        this->_session = new libtorrent::session();
        this->_alertFactory = gcnew AlertFactory(*this->_session);
    }

    Session::Session(Fingerprint^ fingerprint)
    {
        this->_session = new libtorrent::session(*fingerprint->get_ptr());
        this->_alertFactory = gcnew AlertFactory(*this->_session);
    }

    Session::~Session()
    {
        if (this->_disposed)
        {
            return;
        }

        this->!Session();

        this->_disposed = true;
    }

    Session::!Session()
    {
        delete this->_session;
    }

    void Session::LoadState(cli::array<byte>^ buffer)
    {
        pin_ptr<unsigned char> ptr = &buffer[0];
        const char *pbegin = (const char*)(const unsigned char*)ptr;
        const char *pend = pbegin + buffer->Length;

        libtorrent::lazy_entry entry;
        libtorrent::lazy_bdecode(pbegin, pend, entry);

        this->_session->load_state(entry);
    }

    cli::array<byte>^ Session::SaveState()
    {
        libtorrent::entry entry;
        this->_session->save_state(entry);

        return Utils::GetByteArrayFromLibtorrentEntry(entry);
    }

    void Session::PostTorrentUpdates()
    {
        this->_session->post_torrent_updates();
    }

    TorrentHandle^ Session::FindTorrent(System::String^ infoHash)
    {
        auto sha1 = Utils::GetSha1HashFromString(infoHash);
        auto handle = this->_session->find_torrent(sha1);

        if (!handle.is_valid())
        {
            return nullptr;
        }

        return gcnew TorrentHandle(handle);
    }

    System::Collections::Generic::IEnumerable<TorrentHandle^>^ Session::GetTorrents()
    {
        auto iterator = this->_session->get_torrents();
        auto result = gcnew System::Collections::Generic::List<TorrentHandle^>(iterator.size());

        for (int i = 0; i < iterator.size(); i++)
        {
            result->Add(gcnew TorrentHandle(iterator[i]));
        }

        return result;
    }

    TorrentHandle^ Session::AddTorrent(AddTorrentParams^ params)
    {
        libtorrent::torrent_handle handle = this->_session->add_torrent(params->get_ptr());
        return gcnew TorrentHandle(handle);
    }

    void Session::AsyncAddTorrent(AddTorrentParams^ params)
    {
        this->_session->async_add_torrent(params->get_ptr());
    }

    void Session::Pause()
    {
        this->_session->pause();
    }

    void Session::Resume()
    {
        this->_session->resume();
    }

    bool Session::IsPaused::get()
    {
        return this->_session->is_paused();
    }

    SessionStatus^ Session::QueryStatus()
    {
        return gcnew SessionStatus(this->_session->status());
    }

    bool Session::IsDhtRunning::get()
    {
        return this->_session->is_dht_running();
    }

    void Session::StartDht()
    {
        this->_session->start_dht();
    }

    void Session::StopDht()
    {
        this->_session->stop_dht();
    }

    void Session::SetKey(int key)
    {
        this->_session->set_key(key);
    }

    void Session::ListenOn(int lower, int upper)
    {
        this->_session->listen_on(std::make_pair(lower, upper));
    }

    bool Session::IsListening::get()
    {
        return this->_session->is_listening();
    }

    int Session::ListenPort::get()
    {
        return this->_session->listen_port();
    }

    int Session::SslListenPort::get()
    {
        return this->_session->ssl_listen_port();
    }

    void Session::RemoveTorrent(TorrentHandle^ handle)
    {
        this->RemoveTorrent(handle, false);
    }

    void Session::RemoveTorrent(TorrentHandle^ handle, bool removeData)
    {
        this->_session->remove_torrent(*handle->get_ptr(), removeData ? libtorrent::session::delete_files : 0);
    }

    SessionSettings^ Session::QuerySettings()
    {
        return gcnew SessionSettings(this->_session->settings());
    }

    void Session::SetSettings(SessionSettings^ settings)
    {
        this->_session->set_settings(*settings->get_ptr());
    }

    IAlertFactory^ Session::Alerts::get()
    {
        return this->_alertFactory;
    }
    
    void Session::SetAlertMask(SessionAlertCategory mask)
    {
        this->_session->set_alert_mask((unsigned int) mask);
    }

    void Session::StopLsd()
    {
        this->_session->stop_lsd();
    }

    void Session::StartLsd()
    {
        this->_session->start_lsd();
    }

    void Session::StopUpnp()
    {
        this->_session->stop_upnp();
    }

    void Session::StartUpnp()
    {
        this->_session->start_upnp();
    }

    void Session::StopNatPmp()
    {
        this->_session->stop_natpmp();
    }

    void Session::StartNatPmp()
    {
        this->_session->start_natpmp();
    }
}
