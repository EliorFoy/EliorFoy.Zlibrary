using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliorFoy.Zlibrary.CLI
{
    internal class Program
    {
        static void Main()
        {
            var downloader = new Downloader();
            //var s = AccountPool.CheckTheRestDownloadNum("35246529", "805952563b5da47b2e477aad04be3c9a").Result;
            Console.WriteLine(AccountPool.CheckAvaliability("35246529", "805952563b5da47b2e477aad04be3c9a").Result);
            Console.ReadLine();
        }
    }
}
