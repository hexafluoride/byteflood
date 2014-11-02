#include "stdafx.h"
#include "StringValueConverter.h"

#include <msclr\marshal_cppstd.h>
#include <string>

namespace Ragnar
{
    namespace Interop
    {
        std::string StringValueConverter::To(System::String^ value)
        {
            return msclr::interop::marshal_as<std::string>(value);
        }

        System::String^ StringValueConverter::From(const std::string &value)
        {
            return gcnew System::String(value.c_str());
        }
    }
}
