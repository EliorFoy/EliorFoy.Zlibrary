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

            AccountPool.CreateAvaliableAccountPoolForOnce().Wait();
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