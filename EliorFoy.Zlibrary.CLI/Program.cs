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
            downloader.Search("花书",1);
            Console.ReadLine();
        }
    }
}
