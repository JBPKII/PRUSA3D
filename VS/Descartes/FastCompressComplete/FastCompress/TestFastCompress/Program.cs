using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BK.Util;

namespace TestFastCompress
{
    class Program
    {
        static void Main(string[] args)
        {
            FastCompress.doNotUseTPL = true;
            FastCompress.compressStrictSeqential = false;
            System.Console.WriteLine("Time taken for Seq compression = {0}", FastCompress.CompressFast(@"Z:\Projects\Test\File1.test.seq", @"Z:\Projects\Test\File1.test", true));
            System.Console.WriteLine("Time taken for Seq Un compression = {0}", FastCompress.UncompressFast(@"Z:\Projects\Test\File1.test.orgSeq", @"Z:\Projects\Test\File1.test.seq", true));


            FastCompress.doNotUseTPL = false;
            FastCompress.compressStrictSeqential = false;
            System.Console.WriteLine("Time taken for Parallel compression = {0}", FastCompress.CompressFast(@"Z:\Projects\Test\File1.test.pll", @"Z:\Projects\Test\File1.test", true));
            System.Console.WriteLine("Time taken for Parallel Un compression = {0}", FastCompress.UncompressFast(@"Z:\Projects\Test\File1.test.orgpll", @"Z:\Projects\Test\File1.test.pll", true));

        }
    }
}
