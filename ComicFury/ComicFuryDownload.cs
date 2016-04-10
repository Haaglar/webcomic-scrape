using System;
using HtmlAgilityPack;
using System.Net;
using System.IO;

namespace ComicFury
{
    class ComicFuryDownload
    {
        enum ExitCodes { Success, InvalidArguments, InvalidSite};
        static int Main(string[] args)
        {
            if(args.Length != 1)
            {
                Console.WriteLine("Usage: ComicFury.exe URL\n");
                return (int)ExitCodes.InvalidArguments;
            }
            string url = args[0].StartsWith("http") ? args[0] : "http://" + args[0];
            //We need to ensure that it is the main page since we can go straight to the first
            Uri address = new Uri(url);
            string urlBase = address.GetLeftPart(UriPartial.Authority); 
            string urlNext = urlBase + "/comics/first";
            string xPathImage = "//*[@id=\"comicimage\"]";
            string xPathNext = "//*[@rel=\"next\"]";
            //There is no point in saving the original file name as it is a hashed value or something
            int comicNumber = 0;

            string folderDir = urlBase;
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                folderDir = folderDir.Replace(c, '-');
            }
            Directory.CreateDirectory(folderDir);

            while (!urlNext.Equals(urlBase + "/comics/"))
            {
                HtmlDocument currentPage = new HtmlWeb().Load(urlNext);
                HtmlNodeCollection nodes = currentPage.DocumentNode.SelectNodes(xPathImage);
                if (nodes.Count != 0)
                {
                    using (WebClient wc = new WebClient())
                    {
                        string imgLoc = nodes[0].Attributes["src"].Value;
                        wc.DownloadFile(imgLoc, folderDir + "/" + (++comicNumber).ToString("0000") + Path.GetExtension(imgLoc));
                        Console.WriteLine("Downloaded comic number: " + (comicNumber).ToString("0000"));
                    }
                }
                else
                {
                    Console.WriteLine("Are you sure this is a comicfury comic? Exiting.");
                    return (int)ExitCodes.InvalidSite;
                }
                urlNext = urlBase + currentPage.DocumentNode.SelectNodes(xPathNext)[0].Attributes["href"].Value;
            }
            Console.WriteLine("Complete"); 
            return (int)ExitCodes.Success;
        }
    }
}
