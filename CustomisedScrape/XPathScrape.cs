using HtmlAgilityPack;
using System;
using System.IO;

using System.Net;
using System.Xml.XPath;

namespace CustomisedScrape
{
    class XPathScrape
    {
        enum ExitCodes { Success, InvalidArguments, InvalidSite };
        static int Main(string[] args)
        {
            int comicNo = 0;
            if (args.Length == 0 || args.Length % 2 != 1)
            {
                Console.WriteLine("Usage: XPathScrape.exe URL");
                return (int)ExitCodes.InvalidArguments;
            }

            string xPathComic = "//img[@class=\"comic\" or @id=\"comic\"]";
            string xPathNext = "//a[@class=\"next\" or @id=\"next\"]";
            string url;
            //Read arguments
            for (int i = 0; i < (args.Length-1); i++)
            {
                if (args[i].StartsWith("-n"))
                {
                    xPathNext = args[++i];
                }
                else if (args[i].StartsWith("-c"))
                {
                    xPathComic = args[++i];
                }
                else
                {
                    Console.WriteLine("Usage: CustomisedScrape.exe [OPTIONS] URL");
                    return (int)ExitCodes.InvalidArguments;
                }
            }

            url = args[args.Length-1].StartsWith("http") ? args[args.Length - 1] : "http://" + args[args.Length - 1];
            
            string folderDir = url;
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                folderDir = folderDir.Replace(c, '-');
            }
            Directory.CreateDirectory(folderDir);
            string folderDirSlash = folderDir + "/";

            HtmlWeb requester = new HtmlWeb();
            bool running = true;
            while(running)
            {
                Console.WriteLine(xPathNext);
                Console.WriteLine("Readgin page " + url);
                HtmlDocument document = requester.Load(url);
                HtmlNode comicImage;
                try
                {
                    comicImage = document.DocumentNode.SelectSingleNode(xPathComic);
                }
                catch(XPathException ex)
                {
                    Console.WriteLine("Bad input id supplied, failed to parse xPath for image");
                    return (int)ExitCodes.InvalidArguments;
                }
                if(comicImage == null)
                {
                    Console.WriteLine("Bad image id supplied, couldn't find comic");
                    return (int)ExitCodes.InvalidArguments;
                }
                using (WebClient wc = new WebClient())
                {
                    Uri construct = new Uri(new Uri(url), comicImage.Attributes["src"].Value);
                    wc.DownloadFile(construct, folderDirSlash + (++comicNo).ToString("0000") + Path.GetExtension(comicImage.Attributes["src"].Value));
                    Console.WriteLine("Downloaded image on page " + url);
                }
                HtmlNode next;
                try
                {
                    next = document.DocumentNode.SelectSingleNode(xPathNext);
                }
                catch (XPathException ex)
                {
                    Console.WriteLine("Bad input id supplied, failed to parse xPath for next page");
                    return (int)ExitCodes.InvalidArguments;
                }
                if (next == null && comicNo == 1)
                {
                    Console.WriteLine("Couldn't find next page");
                    return (int)ExitCodes.InvalidArguments;
                }
                string nextUrlTmp = next.Attributes["href"].Value;
                string nextUrl;
                if (String.IsNullOrEmpty(nextUrlTmp) || nextUrlTmp.EndsWith("#"))
                {
                    running = false;
                }
                else
                {
                    if(nextUrlTmp.StartsWith("http"))
                    {
                        nextUrl = nextUrlTmp;
                    }
                    else
                    {
                        nextUrl = new Uri(new Uri(url), nextUrlTmp).ToString();
                    }

                    if (nextUrl.Equals(url))
                    {
                        running = false;
                    }
                    else
                    {
                        url = nextUrl;
                    }
                }
            }
            return (int)ExitCodes.Success;
        }
    }
}
