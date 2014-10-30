#include "stdafx.h"
#include "Fingerprint.h"

#include <libtorrent\fingerprint.hpp>

namespace Ragnar
{
    Fingerprint::Fingerprint(System::String^ id, int major, int minor, int revision, int tag)
    {
        auto ptr = System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(id);
        const char* id_string = static_cast<char*>(ptr.ToPointer());

        this->_fingerprint = new libtorrent::fingerprint(id_string, major, minor, revision, tag);

        System::Runtime::InteropServices::Marshal::FreeHGlobal(ptr);
    }

    System::String^ Fingerprint::Id::get()
    {
        return gcnew System::String(this->_fingerprint->name);
    }

    int Fingerprint::Major::get()
    {
        return this->_fingerprint->major_version;
    }

    int Fingerprint::Minor::get()
    {
        return this->_fingerprint->minor_version;
    }

    int Fingerprint::Revision::get()
    {
        return this->_fingerprint->revision_version;
    }

    int Fingerprint::Tag::get()
    {
        return this->_fingerprint->tag_version;
    }
}