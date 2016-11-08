#ifndef _RT_ENCRYPTION_H_
#define _RT_ENCRYPTION_H_
#include "enums.h"

namespace RTNet
{
    class RTEncryption
    {
    public:
        static rt_byte* Encrypt(rt_byte* input, int length, int* outLength);
        static rt_byte* Decrypt(rt_byte* input, int length, int* outLength);
    };
}
#endif
