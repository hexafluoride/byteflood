#include "stdafx.h"
#include "AddTorrentParams.h"
#include "TorrentInfo.h"
#include "Utils.h"
#include "Collections\Vector.h"
#include "Interop\StringValueConverter.h"

#include <libtorrent\add_torrent_params.hpp>
#include <libtorrent\file_pool.hpp>
#include <libtorrent\magnet_uri.hpp>
#include <msclr\marshal_cppstd.h>

namespace Ragnar
{
    using namespace Ragnar::Collections;
    using namespace Ragnar::Interop;

    AddTorrentParams::AddTorrentParams()
    {
        this->_params = new libtorrent::add_torrent_params();
        this->_trackers = gcnew Vector<std::string, System::String^>(this->_params->trackers, gcnew StringValueConverter());
        this->_urlSeeds = gcnew Vector<std::string, System::String^>(this->_params->url_seeds, gcnew StringValueConverter());
    }

    AddTorrentParams::AddTorrentParams(const libtorrent::add_torrent_params &params)
    {
        this->_params = new libtorrent::add_torrent_params(params);
        this->_trackers = gcnew Vector<std::string, System::String^>(this->_params->trackers, gcnew StringValueConverter());
        this->_urlSeeds = gcnew Vector<std::string, System::String^>(this->_params->url_seeds, gcnew StringValueConverter());
    }

    AddTorrentParams::~AddTorrentParams()
    {
        delete this->_params;
    }

    IList<System::String^>^ AddTorrentParams::Trackers::get()
    {
        return this->_trackers;
    }

    IList<System::String^>^ AddTorrentParams::UrlSeeds::get()
    {
        return this->_urlSeeds;
    }

    System::String^ AddTorrentParams::Name::get()
    {
        return gcnew System::String(this->_params->name.c_str());
    }

    void AddTorrentParams::Name::set(System::String^ value)
    {
        this->_params->name = msclr::interop::marshal_as<std::string>(value);
    }

    System::String^ AddTorrentParams::SavePath::get()
    {
        return gcnew System::String(this->_params->save_path.c_str());
    }

    void AddTorrentParams::SavePath::set(System::String^ value)
    {
        this->_params->save_path = msclr::interop::marshal_as<std::string>(value);
    }

    System::String^ AddTorrentParams::Url::get()
    {
        return gcnew System::String(this->_params->url.c_str());
    }

    void AddTorrentParams::Url::set(System::String^ value)
    {
        this->_params->url = msclr::interop::marshal_as<std::string>(value);
    }

    TorrentInfo^ AddTorrentParams::TorrentInfo::get()
    {
        return this->_info;
    }

    void AddTorrentParams::TorrentInfo::set(Ragnar::TorrentInfo^ value)
    {
        this->_info = value;
        this->_params->ti = value->get_ptr();
    }

    cli::array<byte>^ AddTorrentParams::ResumeData::get()
    {
        return this->_resumeData;
    }

    void AddTorrentParams::ResumeData::set(cli::array<byte>^ value)
    {
        this->_resumeData = value;
        this->_params->resume_data.resize(value->Length);

        for (int i = 0; i < value->Length; i++)
        {
            this->_params->resume_data[i] = value[i];
        }
    }

    int AddTorrentParams::MaxUploads::get()
    {
        return this->_params->max_uploads;
    }

    void AddTorrentParams::MaxUploads::set(int value)
    {
        this->_params->max_uploads = value;
    }

    int AddTorrentParams::MaxConnections::get()
    {
        return this->_params->max_connections;
    }

    void AddTorrentParams::MaxConnections::set(int value)
    {
        this->_params->max_connections = value;
    }

    int AddTorrentParams::UploadLimit::get()
    {
        return this->_params->upload_limit;
    }

    void AddTorrentParams::UploadLimit::set(int value)
    {
        this->_params->upload_limit = value;
    }

    int AddTorrentParams::DownloadLimit::get()
    {
        return this->_params->download_limit;
    }

    void AddTorrentParams::DownloadLimit::set(int value)
    {
        this->_params->download_limit = value;
    }

    bool AddTorrentParams::SeedMode::get()
    {
        return this->_params->seed_mode;
    }

    void AddTorrentParams::SeedMode::set(bool value)
    {
        this->_params->seed_mode = value;
    }

    AddTorrentParams^ Ragnar::AddTorrentParams::FromMagnetUri(System::String^ uri)
    {
        libtorrent::add_torrent_params params;
        libtorrent::error_code ec;
        
        libtorrent::parse_magnet_uri(msclr::interop::marshal_as<std::string>(uri), params, ec);

        if (ec)
        {
            throw gcnew System::Exception(gcnew System::String(ec.message().c_str()));
        }

        return gcnew AddTorrentParams(params);
    }
}