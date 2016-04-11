using HtmlAgilityPack;
using System;
using System.IO;
using System.Net;

namespace ComicControl
{
    //Downloader for webcomics that use ComicControl
    //Can be found if /comiccontrol/ dirctory exists
    //Often used by HiveWorks comics
    class ComicControlDownload
    {
        enum ExitCodes { Success, InvalidArguments, InvalidSite };
        static int Main(string[] args)
        {
            int comicNo = 0;
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: ComicControl.exe URL");
                return (int)ExitCodes.InvalidArguments;
            }
            string url = args[0].StartsWith("http") ? args[0] : "http://" + args[0];

            string xPathComic = "//*[@id=\"cc-comic\"]";
            string xPathNext = "//*[contains(@class,\"next\")]";
            string xPathFirst = "//*[contains(@class, \"first\")]";

            HtmlWeb fetcher = new HtmlWeb();
            HtmlDocument currentPage = fetcher.Load(url);

            HtmlNodeCollection nodes = currentPage.DocumentNode.SelectNodes(xPathFirst);
            if(nodes == null) //Cant find it
            {                
                Console.WriteLine("Website does not use ComicControl");
                return (int)ExitCodes.InvalidSite;
            }

            string folderDir = url;
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                folderDir = folderDir.Replace(c, '-');
            }
            Directory.CreateDirectory(folderDir);

            string currentUrl = nodes[0].Attributes["href"].Value ?? url;
            if(currentUrl.Equals(url)) //Already at first so download it
            {
                String imageUrl = currentPage.DocumentNode.SelectNodes(xPathComic)[0].Attributes["src"].Value;
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadFile(imageUrl, folderDir + "/" + (++comicNo).ToString("0000") + Path.GetExtension(imageUrl));
                    Console.WriteLine("Downloaded: " + imageUrl);
                }
                currentUrl = currentPage.DocumentNode.SelectNodes(xPathNext)[0].Attributes["href"].Value ?? "";
            }
            //Main Loop
            while(!String.IsNullOrEmpty(currentUrl))
            {
                currentPage = fetcher.Load(currentUrl);
                String imageUrl = currentPage.DocumentNode.SelectNodes(xPathComic)[0].Attributes["src"].Value;
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadFile(imageUrl, folderDir + "/" + (++comicNo).ToString("0000") + Path.GetExtension(imageUrl));
                    Console.WriteLine("Downloaded: " + imageUrl);
                }
                //Find next
                HtmlNodeCollection findNext = currentPage.DocumentNode.SelectNodes(xPathNext);
                if(findNext == null)
                {
                    currentUrl = "";
                }
                else
                {
                    currentUrl = findNext[0].Attributes["href"].Value ?? "";
                }
            }
            Console.WriteLine("Complete");
            return (int)ExitCodes.Success;
        }
    }
}
