using HtmlAgilityPack;
using System;
using System.IO;
using System.Net;

namespace ComicPress_WordPress
{
    class ComicPressWordPressDownload
    {
        enum ExitCodes { Success, InvalidArguments, InvalidSite, LostConnection };
        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                return (int)ExitCodes.InvalidArguments;
            }
            string url = args[0].StartsWith("http") ? args[0] : "http://" + args[0];
            //Console.WriteLine(asd.GetLeftPart(UriPartial.Authority));

            //Looking at ComicPress webcomics (5) i see two different versions on how the pages navigate
            //ones that use comic-nav-base as a base and others that use navi navi as a base
            //And some dont have a first in the noraml place
           
            //Comics are found under the comic id, however they may be nested under an a tag
            string xpathComic = "//*[@id=\"comic\"]//img";

            //Need to find first, 
            string xpathFirst = "//*[@class=\"navi navi-first\" or @class=\"comic-nav-base comic-nav-first\"]";
            string xpathNext = "//*[@rel=\"next\"]";
            string nextURL;

            //Find the first page
            HtmlDocument webPagestart = new HtmlWeb().Load(url);
            HtmlNodeCollection nodes = webPagestart.DocumentNode.SelectNodes(xpathFirst);
            if(nodes == null)
            {
                //The user may already suppilied the first page so that was the reason no nodes were found
                string xpathAlreadyFirst = "//*[@class=\"navi navi-first navi-void\"]";
                nodes = webPagestart.DocumentNode.SelectNodes(xpathAlreadyFirst);
                if (nodes == null)//Cant find it? not ComicPress Then
                {
                    Console.WriteLine("Unsupported site");
                    return (int)ExitCodes.InvalidSite;
                }
                nextURL = url;
            }
            else
            {
                nextURL = nodes[0].Attributes["href"].Value;
            }

            //Now we know its good, create folder save location
            string folderDir = url;
            foreach (var c in System.IO.Path.GetInvalidFileNameChars())
            {
                folderDir = folderDir.Replace(c, '-');
            }
            Directory.CreateDirectory(folderDir);

            //Main loop
            while (!String.IsNullOrEmpty(nextURL))
            {
                HtmlDocument currentDocument = new HtmlWeb().Load(nextURL);
                string comicImageLocation = currentDocument.DocumentNode.SelectNodes(xpathComic)[0].Attributes["src"].Value;
                if(!String.IsNullOrEmpty(comicImageLocation)) //Maybe its offline
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.DownloadFile(comicImageLocation, folderDir + "/" + Path.GetFileName(comicImageLocation));
                        Console.WriteLine("Downloaded: " + comicImageLocation);
                    }
                }
                else //Something went wrong, but dont about as the image might just be offline
                {
                    Console.WriteLine("Could not download image at nextURL");
                }
                HtmlNodeCollection next = currentDocument.DocumentNode.SelectNodes(xpathNext);
                if(next != null)
                {
                    nextURL = next[0].Attributes["href"].Value;
                }
                else
                {
                    nextURL = "";
                }
            }
            return (int)ExitCodes.Success;
        }
    }
}
