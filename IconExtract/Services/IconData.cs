using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconExtract.Services
{
    public class IconData
    {
        public byte Width { get; set; }
        public byte Height { get; set; }
        public byte ColorCount { get; set; }
        public ushort Planes { get; set; }
        public ushort BitCount { get; set; }
        public byte[] Data { get; set; } = [];
    }
}
