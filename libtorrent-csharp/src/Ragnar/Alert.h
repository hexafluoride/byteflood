#pragma once

namespace libtorrent
{
    class alert;
}

namespace Ragnar
{
    using namespace System;

    public ref class Alert abstract
    {
    private:
        String^ _message;

    internal:
        Alert(libtorrent::alert* alert);

    public:
        property String^ Message { String^ get(); }
    };
}