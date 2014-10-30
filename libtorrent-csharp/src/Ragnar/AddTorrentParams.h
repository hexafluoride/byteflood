#pragma once

#include <string>

namespace libtorrent
{
    struct add_torrent_params;
}

namespace Ragnar
{
    ref class TorrentInfo;

    using namespace System;
    using namespace System::Collections::Generic;

    public ref class AddTorrentParams
    {
    private:
        libtorrent::add_torrent_params* _params;
        cli::array<byte>^ _resumeData;
        TorrentInfo^ _info;
        IList<String^>^ _trackers;
        IList<String^>^ _urlSeeds;

    internal:
        libtorrent::add_torrent_params& get_ptr() { return *_params; }
        AddTorrentParams(const libtorrent::add_torrent_params &params);

    public:
        AddTorrentParams();
        ~AddTorrentParams();

        property IList<String^>^ Trackers { IList<String^>^ get(); }

        property IList<String^>^ UrlSeeds { IList<String^>^ get(); }

        property String^ Name
        {
            String^ get();
            void set(String^ value);
        }

        property String^ SavePath
        {
            void set(String^ value);
            String^ get();
        }

        property String^ Url
        {
            void set(String^ value);
            String^ get();
        }

        property TorrentInfo^ TorrentInfo
        {
            void set(Ragnar::TorrentInfo^ value);
            Ragnar::TorrentInfo^ get();
        }

        property cli::array<byte>^ ResumeData
        {
            cli::array<byte>^ get();
            void set(cli::array<byte>^ value);
        }

        property int MaxUploads { int get(); void set(int value); }

        property int MaxConnections { int get(); void set(int value); }

        property int UploadLimit { int get(); void set(int value); }

        property int DownloadLimit { int get(); void set(int value); }

        property bool SeedMode 
        {
            void set(bool value);
            bool get();
        }

        static AddTorrentParams^ FromMagnetUri(System::String^ uri);
    };
}
