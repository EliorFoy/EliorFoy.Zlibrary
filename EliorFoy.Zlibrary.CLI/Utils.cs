using Flurl;
using Flurl.Http;
using LiteDB;
using System.Formats.Asn1;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Net;

namespace EliorFoy.Zlibrary.CLI
{
    public static class AccountPool
    {
        private static string[] _urls = { "https://www.thinkdoc.vip/yibooklist.txt" };
        static async public Task Refresh()
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

        static public async Task CheckLiveAccount()
        {

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
        public IReadOnlyList<FlurlCookie>  Cookies { get; set; }
        public Downloader()
        {
            var response = _url.GetAsync().Result;
            while(response.StatusCode != 200)
            {
                response = _url.GetAsync().Result;
            }
            this.Cookies = response.Cookies;
            Console.WriteLine("代理转换成功！");
            
        }
        public async Task Search(string bookName, int searchPage)
        {
            Console.WriteLine("???");
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(new Cookie("remix_userkey|1lib.sk", "805952563b5da47b2e477aad04be3c9a"));
            cookieContainer.Add(new Cookie("remix_userid|1lib.sk", "35246529"));
            //var respon = await "https://webproxy.lumiproxy.com/s".AppendPathSegment(bookName)
            //        .SetQueryParams(new { page = searchPage }) // 直接传递匿名对象
            //        .WithCookies(this.Cookies).WithCookies(cookieContainer)
            //        .GetAsync();
            //Console.WriteLine(respon.StatusCode);
            //var result = respon.ToString();

            //Console.WriteLine(result);
            var result = "https://www.baidu.com"
                    //.WithCookies(this.Cookies)
                    .GetStringAsync();
            Console.WriteLine(result.Result.ToString());
            Console.WriteLine(result);
            if (result.Result.Contains("1/10"))
            {
                Console.WriteLine("登陆成功！");
            }
            else
            {
                Console.WriteLine("登录失败！");
            }
            //var cookies = res.Cookies;
            //foreach(var cookie in cookies)
            //{
            //    Console.WriteLine(cookie.Name);
            //    Console.WriteLine(cookie.Value);
            //}
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(result.Result);
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
                    Console.WriteLine($"Title: {title}");
                    Console.WriteLine($"Author: {author}");
                    Console.WriteLine($"ISBN: {isbn}");
                    Console.WriteLine($"Publisher:{pulisher}");
                    Console.WriteLine($"Year: {year}");
                    Console.WriteLine($"Language: {language}");
                    Console.WriteLine($"File: {file}");
                    Console.WriteLine($"Book Image URL: {bookImageUrl}");
                    Console.WriteLine("===========================================================================================================");
                    var newBook = new Book(title, author, isbn, year, language, file, bookImageUrl);
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
        public string File {  get; set; }
        public string BookImage { get; set; }
        public Book(string title, string author, string iSBN, string year, string language, string file, string bookImage)
        {
            Title = title;
            Author = author;
            ISBN = iSBN;
            Year = year;
            Language = language;
            File = file;
            BookImage = bookImage;
        }
    }
}