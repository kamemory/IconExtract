using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Klib
{
    public static class ByteUtil
    {
        public static long ToLongBE(byte[] data)
        {
            long result = 0;
            foreach (byte b in data)
            {
                result = (result << 8) + b;
            }
            return result;
        }

        public static long ToLongLE(byte[] data)
        {
            long result = 0;
            foreach (byte b in data.Reverse())
            {
                result = (result << 8) + b;
            }
            return result;
        }
    }
}
