using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RasterPaint.Utilities
{
    public static class IdHelper
    {
        public static uint Identifier { get; set; } = 1;

        public static uint NewId()
        {
            return Identifier++;
        }
    }
}
