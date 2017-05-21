using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace BK.Util
{
    public class FastCompressException : Exception
    {
        public FastCompressException(): base()
        {
        }

        public FastCompressException(string message): base(message)
        {
        }

        public FastCompressException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}