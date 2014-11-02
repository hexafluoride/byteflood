#include "stdafx.h"
#include "Alert.h"

#include <libtorrent\alert.hpp>

namespace Ragnar
{
    using namespace System;

    Alert::Alert(libtorrent::alert* alert)
    {
        this->_message = gcnew String(alert->message().c_str());
    }

    String^ Alert::Message::get()
    {
        return this->_message;
    }
}