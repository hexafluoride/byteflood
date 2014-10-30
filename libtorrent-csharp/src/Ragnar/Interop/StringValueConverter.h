#pragma once

#include "ValueConverter.h"

#include <string>

namespace Ragnar
{
    namespace Interop
    {
        template ref class ValueConverter<std::string, System::String^>;

        public ref class StringValueConverter : ValueConverter<std::string, System::String^>
        {
        public:
            virtual std::string To(System::String^ value) override;

            virtual System::String^ From(const std::string &value) override;
        };
    }
}

