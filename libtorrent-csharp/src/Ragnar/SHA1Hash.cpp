#include "stdafx.h"
#include "SHA1Hash.h"

#include <libtorrent\sha1_hash.hpp>

namespace Ragnar
{
    SHA1Hash::SHA1Hash(const libtorrent::sha1_hash &hash)
    {
        this->_hash = new libtorrent::sha1_hash(hash);
    }

    bool SHA1Hash::IsZero::get()
    {
        return this->_hash->is_all_zeros();
    }

    void SHA1Hash::Clear()
    {
        this->_hash->clear();
    }

    System::String^ SHA1Hash::ToHex()
    {
        return gcnew System::String(libtorrent::to_hex(this->_hash->to_string()).c_str());
    }

    System::String^ SHA1Hash::ToString()
    {
        return gcnew System::String(this->_hash->to_string().c_str());
    }
}