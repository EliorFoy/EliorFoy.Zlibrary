using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using Figgle;

namespace EliorFoy.Zlibrary.CLI
{
    internal class Program
    {
        static void Main()
        {
            Console.WriteLine(FiggleFonts.Slant.Render("EliorFoy Zlibrary Tool"));
    //        var detailHtml = "https://webproxy.lumiproxy.com/sencure/kescFDT9ECQp37fTiDI3Y"
    //.WithHeader("cookie", $"remix_userkey|1lib.sk=805952563b5da47b2e477aad04be3c9a; remix_userid|1lib.sk=35246529; selectedSiteMode|1lib.sk=books; __cryproxy=eyJVcmwiOiJodHRwczovL3poLjFsaWIuc2svYm9vay8xMDQ3OTcyLzFjZGVjZC8lRTclOUYlQTklRTklOTglQjUlRTUlODglODYlRTYlOUUlOTAuaHRtbD9kc291cmNlPXJlY29tbWVuZCIsIkFyZWEiOiJVUyIsIktleSI6IktDeFFRWWN4OHJRaU9ERWNLUGxlTCJ9")
    //.WithTimeout(TimeSpan.FromMinutes(5))
    //.GetStringAsync().Result;
    //        Console.WriteLine(detailHtml);
    //        if (detailHtml.Contains("登录")) { Console.WriteLine("登录不成功！"); } else { Console.WriteLine("成功"); }
    //        Console.ReadLine();
            //AccountPool.CreateAvaliableAccountPoolForOnce().Wait();
            var account = AccountPool.GetUserAccount().Result;
            var downloader = new Downloader(account.Userid,account.UserKey);
            Console.WriteLine("创建一次性账号池成功");
            while (true)
            {
                Console.Write("请输入需要下载的书籍：");
                var book = Console.ReadLine();

                while (true)
                {
                    Console.Write("翻页：");
                    string pageInput = Console.ReadLine();
                    List<Book> books = new List<Book>();

                    if (pageInput == "exit") { break; }

                    if (!string.IsNullOrEmpty(pageInput))
                    {
                        try
                        {
                            var page = int.Parse(pageInput);
                            books = downloader.Search(book, page).Result;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"错误: {ex.Message}");
                            continue;
                        }
                    }

                    while (true)
                    {
                        Console.WriteLine("请输入要下载的书本号 (或输入 exit 退出):");
                        var bookIndexString = Console.ReadLine();
                        if (bookIndexString == "exit") { break; }
                        if (string.IsNullOrEmpty(bookIndexString)) { continue; }

                        try
                        {
                            var bookIndex = int.Parse(bookIndexString);
                            if (bookIndex < 1 || bookIndex > books.Count)
                            {
                                Console.WriteLine("书本号超出范围，请重新输入。");
                                continue;
                            }

                            Task.Run(async () =>
                            {
                                await downloader.Download(new Book[] { books[bookIndex - 1] });
                            });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"错误: {ex.Message}");
                        }
                    }
                }
            }
        }
    }
}