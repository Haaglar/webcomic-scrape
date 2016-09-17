using HtmlAgilityPack;
using System;
using System.IO;
using System.Net;

namespace SmackJeeves
{
    class SmackJeevesDownload
    {
        enum ExitCodes { Success, InvalidArguments, InvalidSite };
        static int Main(string[] args)
        {
            int comicNo = 0;
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: SmackJeeves.exe URL");
                return (int)ExitCodes.InvalidArguments;
            }
            string url = args[0].StartsWith("http") ? args[0] : "http://" + args[0];
            string xPathNavigation = "//*[@class=\"navbar\"]//a";//Could be 
            string xPathImage = "//*[@id=\"comic_image\"]";

            HtmlWeb requester = new HtmlWeb();
            HtmlDocument webPage;
            try
            {
                webPage = requester.Load(url);
            }
            catch
            {
                Console.WriteLine("Invalid URL");
                return (int)ExitCodes.InvalidSite;
            }
            HtmlNodeCollection naviagtionSelection = webPage.DocumentNode.SelectNodes(xPathNavigation);
            if(naviagtionSelection == null)
            {
                Console.WriteLine("Is not a smackjeeves comic");
                return (int)ExitCodes.InvalidSite;
            }
            string folderDir = url;
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                folderDir = folderDir.Replace(c, '-');
            }
            Directory.CreateDirectory(folderDir);
            string currentLocation = naviagtionSelection[0].Attributes["href"].Value;
            if(currentLocation.Equals("#"))// Already at first
            {
                HtmlNodeCollection image = webPage.DocumentNode.SelectNodes(xPathImage);
                string imageAddress = image[0].Attributes["src"].Value;
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadFile(imageAddress, folderDir + "/" + (++comicNo).ToString("0000") + Path.GetExtension(imageAddress));
                    Console.WriteLine("Downloaded " + imageAddress);
                }
                currentLocation = naviagtionSelection[2].Attributes["href"].Value;
            }
            while(!currentLocation.Equals("#"))
            {
                webPage = requester.Load(currentLocation);
                HtmlNodeCollection image = webPage.DocumentNode.SelectNodes(xPathImage);
                string imageAddress = image[0].Attributes["src"].Value;
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadFile(imageAddress, folderDir + "/" + (++comicNo).ToString("0000") + Path.GetExtension(imageAddress));
                    Console.WriteLine("Downloaded " + imageAddress);
                }
                naviagtionSelection = webPage.DocumentNode.SelectNodes(xPathNavigation);
                currentLocation = naviagtionSelection[2].Attributes["href"].Value;
            }
            Console.WriteLine("Complete");
            return (int)ExitCodes.Success;
        }
    }
}
