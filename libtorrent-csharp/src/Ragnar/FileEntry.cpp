#include "stdafx.h"
#include "FileEntry.h"

#include <libtorrent\file_storage.hpp>

namespace Ragnar
{
    FileEntry::FileEntry(const libtorrent::file_entry &entry)
    {
        this->_entry = new libtorrent::file_entry(entry);
    }

    FileEntry::~FileEntry()
    {
        delete this->_entry;
    }

    System::String^ FileEntry::Path::get()
    {
        return gcnew System::String(this->_entry->path.c_str());
    }

    long long FileEntry::Offset::get()
    {
        return this->_entry->offset;
    }

    long long FileEntry::Size::get()
    {
        return this->_entry->size;
    }

    long long FileEntry::FileBase::get()
    {
        return this->_entry->file_base;
    }
}