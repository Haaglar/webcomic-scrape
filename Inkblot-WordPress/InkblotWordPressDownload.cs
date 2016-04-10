using HtmlAgilityPack;
using System;
using System.IO;
using System.Net;

namespace Inkblot_WordPress
{
    class InkblotWordPressDownload
    {
        enum ExitCodes { Success, InvalidArguments, InvalidSite };
        static int Main(string[] args)
        {
            int comicNo = 0;
            if(args.Length!= 1)
            {
                Console.WriteLine("Usage: Inkblot-WordPress URL");
                return (int)ExitCodes.InvalidArguments;
            }
            string url = args[0].StartsWith("http") ? args[0] : "http://" + args[0];
            string xPathFirst = "//*[contains(@class, \"first-webcomic-link\")]//@href";
            string xPathNext = "//*[contains(@class, \"next-webcomic-link\")]//@href";
            string xPathImage = "//*[@class=\"webcomic-image\"]/img";
            HtmlWeb requester = new HtmlWeb();
            HtmlDocument findFirst = requester.Load(url);
            //Find first url
            HtmlNodeCollection nodes = findFirst.DocumentNode.SelectNodes(xPathFirst);
            if(nodes == null)
            {
                Console.WriteLine("Could not find first page link");
                return (int)ExitCodes.InvalidSite;
            }
            string currentUrl = nodes[0].Attributes["href"].Value;
            //Create folder
            string folderDir = currentUrl;
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                currentUrl = folderDir.Replace(c, '-');
            }
            Directory.CreateDirectory(folderDir);
            //Main loop
            HtmlDocument currentPage = new HtmlDocument();
            bool running = true;
            while(running)
            {
                currentPage = requester.Load(currentUrl);
                HtmlNodeCollection image = currentPage.DocumentNode.SelectNodes(xPathImage);
                if(image != null)
                {
                    String toDownload = image[0].Attributes["src"].Value;
                    using (WebClient wc = new WebClient())
                    {
                        wc.DownloadFile(toDownload, folderDir +"/" + (++comicNo).ToString("0000") + Path.GetExtension(toDownload));
                        Console.WriteLine("Downloaded: " + toDownload);
                    }
                }
                else
                {
                    Console.WriteLine("Failed to find image on page " + currentUrl);
                }
                HtmlNodeCollection next = currentPage.DocumentNode.SelectNodes(xPathNext);
                if(next == null)
                {
                    running = false;
                }
                else
                {
                    string nextPage = next[0].Attributes["href"].Value;
                    if(nextPage.Equals(currentUrl))
                    {
                        running = false;
                    }
                    else
                    {
                        currentUrl = nextPage;
                    }
                }
            }

            Console.WriteLine("Complete");
            return (int)ExitCodes.Success;
        }
    }
}
