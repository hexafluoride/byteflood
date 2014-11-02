#pragma once

#include <boost/asio/ip/address.hpp>
#include <boost/date_time/posix_time/posix_time_duration.hpp>
#include <boost/date_time/posix_time/conversion.hpp>
#include <libtorrent\bencode.hpp>
#include <libtorrent\peer_id.hpp>
#include <libtorrent\time.hpp>
#include <cliext\vector>
#include <vector>

using namespace System;
using namespace System::Net;

namespace Ragnar
{
    public ref class Utils
    {
    internal:
        static cli::array<unsigned char>^ GetByteArrayFromLibtorrentEntry(const libtorrent::entry &entry)
        {
            std::vector<unsigned char> buffer;
            libtorrent::bencode(std::back_inserter(buffer), entry);

            cli::array<unsigned char>^ result = gcnew cli::array<unsigned char>(buffer.size());

            for (int i = 0; i < result->Length; i++)
            {
                result[i] = buffer[i];
            }

            return result;
        }

        static TimeSpan^ GetTimeSpanFromPosixTimeDuration(boost::posix_time::time_duration const &t)
        {
            std::tm x = to_tm(t);
            return gcnew TimeSpan(x.tm_hour, x.tm_min, x.tm_sec);
        }

        static libtorrent::sha1_hash GetSha1HashFromString(String^ str)
        {
            libtorrent::sha1_hash hash;
            const char* ptr = (const char*)(System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(str)).ToPointer();
            libtorrent::from_hex(ptr, str->Length, (char*)&hash[0]);

            return hash;
        }

        static std::string GetStdStringFromManagedString(String^ str)
        {
            if (str->Length == 0)
                return std::string();

            cli::array<System::Byte> ^arr = System::Text::Encoding::Convert(
                System::Text::Encoding::Unicode,
                System::Text::Encoding::UTF8,
                System::Text::Encoding::Unicode->GetBytes(str)
                );

            cli::pin_ptr<unsigned char> ptr = &arr[0];
            const char *data = (const char*)(const unsigned char*)ptr;

            return std::string(data);
        }

        static System::Net::IPAddress^ GetIPAddress(boost::asio::ip::address const &address)
        {
            return IPAddress::Parse(gcnew String(address.to_string().c_str()));
        }

        static DateTime GetDateTimeFromTimeT(time_t time)
        {
            DateTime dt = DateTime(1970, 1, 1, 0, 0, 0, 0, System::DateTimeKind::Utc);
            dt = dt.AddSeconds(time);
            dt = dt.ToLocalTime();

            return dt;
        }
    };
}