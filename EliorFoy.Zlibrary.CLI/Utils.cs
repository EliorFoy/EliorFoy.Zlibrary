using Flurl;
using Flurl.Http;
using LiteDB;
using System.Formats.Asn1;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Net;
using System.Runtime.CompilerServices;

namespace EliorFoy.Zlibrary.CLI
{


    public static class AccountPool
    {
        private static string[] _urls = { "https://www.thinkdoc.vip/yibooklist.txt" };
        private static string proxy {  get; set; }

        static AccountPool()
        {
            proxy =  GetProxyAsync().Result;
        }
        public static async Task<string> GetProxyAsync()
        {
            string proxy = null;
            int maxRetries = 5; // 设置最大重试次数
            int retryDelayMilliseconds = 1000; // 设置重试间隔（毫秒）

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    var response = await "https://webproxy.lumiproxy.com/request?area=US&u=https://zh.1lib.sk/"
                        .WithTimeout(TimeSpan.FromSeconds(10)) // 设置超时时间
                        .GetAsync();

                    if (response.StatusCode == 200)
                    {
                        foreach (var cookie in response.Cookies)
                        {
                            if (cookie.Name == "__cryproxy")
                            {
                                proxy = cookie.Value;
                                break;
                            }
                        }

                        if (!string.IsNullOrEmpty(proxy))
                        {
                            return proxy; // 如果找到了代理，返回代理字符串
                        }
                    }
                    else if (response.StatusCode == 404)
                    {
                        // 如果是404，等待一段时间后重试
                        await Task.Delay(retryDelayMilliseconds);
                    }
                    else
                    {
                        // 如果是其他状态码，抛出异常或处理错误
                        throw new Exception($"Request failed with status code {response.StatusCode}");
                    }
                }
                catch (FlurlHttpTimeoutException)
                {
                    // 处理超时异常
                    await Task.Delay(retryDelayMilliseconds);
                }
                catch (Exception ex)
                {
                    // 处理其他异常
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    throw; // 根据需要重新抛出异常或进行其他处理
                }
            }

            throw new Exception("Max retries reached. Proxy not found.");
        }

        public static string GetProxyAsync(this Downloader downloader)
        {
            return GetProxyAsync().Result;
        }
        static async public Task AccountPoolInit()
        {
            using (var db = new LiteDatabase(@"AccountPool.db"))
            {
                var usersCollection = db.GetCollection<UserAccount>("users");
                foreach (var url in _urls)
                {
                    var result = await url.GetStringAsync();
                    foreach (var account in result.Split('\n'))
                    {
                        var match = Regex.Match(account, @"remix_userid=(?<userid>\d+)&remix_userkey=(?<userkey>[a-f0-9]+)");
                        if (match.Success)
                        {
                            string userid = match.Groups["userid"].Value;
                            string userkey = match.Groups["userkey"].Value;
                            var userAccount = new UserAccount(userid,userkey);
                            usersCollection.Insert(userAccount);
                        }
                    }
                }
            } 
        }

        public static async Task<string> CheckTheRestDownloadNum(string userId, string userKey)
        {
            var respon = await "https://webproxy.lumiproxy.com/s"
                   .WithHeader("cookie", $"remix_userid|1lib.sk={userId}; remix_userkey|1lib.sk={userKey}; __cryproxy={proxy}")
                   .GetStringAsync();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(respon);
            var node = htmlDoc.DocumentNode
                .SelectSingleNode("//div[@class='caret-scroll']")
                .SelectSingleNode(".//div[@class='caret-scroll__title']");
            return node.InnerText.Trim();
        }

        public static async Task<bool> CheckAvaliability(string userId, string userKey)
        {
            var numString = await CheckTheRestDownloadNum(userId, userKey);
            var num = int.Parse(numString.Split("/")[0]) / int.Parse(numString.Split("/")[1]);
            if (num < 1) return true;
            else return false;
        }

    }
    public class UserAccount
    {
        public string Userid { get; set; }
        public string UserKey { get; set; }

        public UserAccount(string userid, string userkey)
        {
            this.Userid = userid;
            this.UserKey = userkey;
        }
    }

    public class Downloader
    {
        private string _url = "https://webproxy.lumiproxy.com/request?area=US&u=https://zh.1lib.sk/";
        public string proxy {  get; set; }
        
        public Downloader()
        {
            this.proxy = this.GetProxyAsync();
            Console.WriteLine("代理转换成功！");
        }

        public async Task Download(Book[] books)
        {
            Console.WriteLine(books.Length);
            List<Task> taskList = new List<Task>();
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(new Cookie("__cryproxy",this.proxy) { Domain = "webproxy.lumiproxy.com" });
            cookieContainer.Add(new Cookie("remix_userkey|1lib.sk", "805952563b5da47b2e477aad04be3c9a") { Domain = "webproxy.lumiproxy.com" });
            cookieContainer.Add(new Cookie("remix_userid|1lib.sk", "35246529") { Domain = "webproxy.lumiproxy.com" });
            foreach(var book in books)
            {
                Console.WriteLine(book.DownloadUrl);
                var task = book.DownloadUrl
                .WithHeader("cookie", $"remix_userid|1lib.sk=35246529; remix_userkey|1lib.sk=805952563b5da47b2e477aad04be3c9a; __cryproxy={this.proxy}")
                .WithTimeout(TimeSpan.FromHours(1))
                .DownloadFileAsync(@"C:\Users\DELL123\Desktop\test", $"{book.Title}.pdf");
                taskList.Append(task);
                await Console.Out.WriteLineAsync($"{book.Title}下载完成！");
            }
            Task.WaitAll(taskList.ToArray());
            await books[0].DownloadUrl
                .WithHeader("cookie", $"remix_userid|1lib.sk=35246529; remix_userkey|1lib.sk=805952563b5da47b2e477aad04be3c9a; __cryproxy={this.proxy}")
                .WithTimeout(TimeSpan.FromHours(1))
                .DownloadFileAsync(@"C:\Users\DELL123\Desktop\test", $"{books[0].Title}.pdf");
            Console.WriteLine("全部书籍下载完成！");
        }

        public async Task Search(string bookName, int searchPage)
        {
            
            var respon = await "https://webproxy.lumiproxy.com/s".AppendPathSegment(bookName)
                    .SetQueryParams(new { page = searchPage }) // 直接传递匿名对象
                    //.WithCookies(this.Cookies)
                    .WithHeader("cookie", "remix_userid|1lib.sk=35246529; remix_userkey|1lib.sk=805952563b5da47b2e477aad04be3c9a; __cryproxy=eyJVcmwiOiJodHRwczovL3poLjFsaWIuc2svIiwiQXJlYSI6IlVTIiwiS2V5IjoiUk5UR05LTU9sendJV3duT1o0UDhFIn0%3D")
                    .GetAsync();
            Console.WriteLine(respon.StatusCode);
            var result =await respon.GetStringAsync();

            
            if (result.Contains("登录"))
            {
                Console.WriteLine("登陆失败！");
            }
            else
            {
                Console.WriteLine("登录成功！");
            }
            //var cookies = res.Cookies;
            //foreach(var cookie in cookies)
            //{
            //    Console.WriteLine(cookie.Name);
            //    Console.WriteLine(cookie.Value);
            //}
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(result);
            var nodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='resItemBox resItemBoxBooks exactMatch']"); 
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var author = node.SelectSingleNode(".//z-cover")?.GetAttributeValue("author", string.Empty);
                    var isbn = node.SelectSingleNode(".//z-cover")?.GetAttributeValue("isbn", "");
                    var title = node.SelectSingleNode(".//z-cover")?.GetAttributeValue("title", "");
                    var bookImageUrl = node.SelectSingleNode(".//img")?.GetAttributeValue("data-src", "");
                    var yearNode = node.SelectSingleNode(".//div[@class='bookProperty property_year']/div[@class='property_value ']");
                    var year = yearNode?.InnerText;
                    var languageNode = node.SelectSingleNode(".//div[@class='bookProperty property_language']/div[@class='property_value text-capitalize']");
                    var language = languageNode?.InnerText;
                    var fileNode = node.SelectSingleNode(".//div[@class='bookProperty property__file']/div[@class='property_value ']");
                    var file = fileNode?.InnerText;
                    var pulisher = node.SelectSingleNode(".//a[@title='Publisher']")?.InnerText;
                    var refUrl = node.SelectSingleNode(".//z-cover").SelectSingleNode(".//a")?.GetAttributeValue("href", "");
                    var detailHtml = refUrl
                        .WithHeader("cookie", "remix_userid|1lib.sk=35246529; remix_userkey|1lib.sk=805952563b5da47b2e477aad04be3c9a; __cryproxy=eyJVcmwiOiJodHRwczovL3poLjFsaWIuc2svIiwiQXJlYSI6IlVTIiwiS2V5IjoiUk5UR05LTU9sendJV3duT1o0UDhFIn0%3D")
                        .WithTimeout(TimeSpan.FromHours(1))
                        .GetStringAsync().Result;
                    var doc = new HtmlDocument();
                    doc.LoadHtml(detailHtml);
                    var downloadUrl = doc.DocumentNode.SelectSingleNode("//a[@class='addDownloadedBook premiumBtn']").GetAttributeValue("href", "");
                    Console.WriteLine(downloadUrl);
                    //Console.WriteLine($"Title: {title}");
                    //Console.WriteLine($"Author: {author}");
                    //Console.WriteLine($"ISBN: {isbn}");
                    //Console.WriteLine($"Publisher:{pulisher}");
                    //Console.WriteLine($"Year: {year}");
                    //Console.WriteLine($"Language: {language}");
                    //Console.WriteLine($"File: {file}");
                    //Console.WriteLine($"Book Image URL: {bookImageUrl}");
                    Console.WriteLine("===========================================================================================================");
                    var newBook = new Book(title, author, isbn, year, language, file, bookImageUrl,downloadUrl);
                    Download(new Book[] { newBook}).Wait();
                    return;
                }
            }  
        }
    }

    public class Book
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public string Year { get; set; }
        public string Language { get; set; }
        public string File { get; set; }
        public string BookImage { get; set; }
        public string DownloadUrl { get; set; }

        public Book(string title, string author, string isbn, string year, string language, string file, string bookImage, string downloadUrl)
        {
            Title = title;
            Author = author;
            ISBN = isbn;
            Year = year;
            Language = language;
            File = file;
            BookImage = bookImage;
            DownloadUrl = downloadUrl;
        }
    }
 }