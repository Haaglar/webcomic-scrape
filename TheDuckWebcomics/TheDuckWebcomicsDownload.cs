using HtmlAgilityPack;
using System;
using System.IO;
using System.Net;

namespace TheDuckWebcomics
{
    class TheDuckWebcomicsDownload
    {
        enum ExitCodes { Success, InvalidArguments, InvalidSite };
        static int Main(string[] args)
        {
            int comicNo = 0;
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: TheDuckWebcomics.exe URL");
                return (int)ExitCodes.InvalidArguments;
            }

            string url = args[0].StartsWith("http") ? args[0] : "http://" + args[0];
            if(!url.Contains("theduckwebcomics.com"))
            {
                Console.WriteLine("This isn't hosted on theduckwebcomics.com");
                return (int)ExitCodes.InvalidSite;
            }
            string tdwc = "http://theduckwebcomics.com";
            string folderDir = url;
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                folderDir = folderDir.Replace(c, '-');
            }
            Directory.CreateDirectory(folderDir);

            
            HtmlWeb requester = new HtmlWeb();
            HtmlDocument webPage = requester.Load(url);

            string xPathNavigationFirst = "//*[@id=\"bottom_arrows\"]/div/ul/li/a[img/@class=\"arrow_first\"]";
            string xPathNavigationNext = "//*[@id=\"bottom_arrows\"]/div/ul/li/a[img/@class=\"arrow_next\"]";
            string xPathComic = "//*[@id=\"comic\"]/img";
            string currentUrl;
            HtmlNodeCollection webPageNavigationFirst = webPage.DocumentNode.SelectNodes(xPathNavigationFirst);
            if (webPageNavigationFirst.Count == 2)//At start or end
            {
                HtmlNodeCollection next = webPage.DocumentNode.SelectNodes(xPathNavigationNext);
                if (next != null) //Must be at end
                {
                    currentUrl = tdwc + webPageNavigationFirst[0].Attributes["href"].Value;
                }
                else//Must be at start so download it
                {
                    HtmlNode comic = webPage.DocumentNode.SelectSingleNode(xPathComic);
                    using (WebClient wc = new WebClient())
                    {
                        string fileName = comic.Attributes["src"].Value.Split(new[] { '?' })[0];
                        wc.DownloadFile(fileName, folderDir + "/" + (++comicNo).ToString("0000") + Path.GetExtension(fileName.Split(new[] { '?' })[0]));
                        Console.WriteLine("Downloaded: " + fileName);
                    }
                    currentUrl = tdwc + next[0].Attributes["href"].Value;
                }
            }
            else
            {
                currentUrl = tdwc+ webPageNavigationFirst[0].Attributes["href"].Value;
            }
            //Main loop
            while(true)
            {
                webPage = requester.Load(currentUrl);

                HtmlNode comic = webPage.DocumentNode.SelectSingleNode(xPathComic);
                using (WebClient wc = new WebClient())
                {
                    string fileName = comic.Attributes["src"].Value.Split(new[] { '?' })[0];
                    wc.DownloadFile(fileName, folderDir + "/" + (++comicNo).ToString("0000") + Path.GetExtension(fileName));
                    Console.WriteLine("Downloaded: " + fileName);
                }
                HtmlNodeCollection next = webPage.DocumentNode.SelectNodes(xPathNavigationNext);
                if (next == null)
                {
                    break;
                }
                currentUrl = tdwc + next[0].Attributes["href"].Value;
            }

            return (int)ExitCodes.Success;
        }
    }
}
