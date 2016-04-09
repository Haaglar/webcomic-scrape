using HtmlAgilityPack;
using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

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
            //TODO: handle them both :)  
            string xpathComic = "//*[@id=\"comic\"]/img";

            //Need to find first
            string xpathFirst = "//*[@class=\"navi navi-first\" or @class=\"comic-nav-base comic-nav-first\"]";
            HtmlDocument webPagestart = new HtmlWeb().Load(url);
            var nodes = webPagestart.DocumentNode.SelectNodes(xpathFirst);
            if(nodes == null)
            {
                return (int)ExitCodes.InvalidSite;
            }
            Console.WriteLine(nodes[0].Attributes["href"].Value);
            return (int)ExitCodes.Success;
        }
    }
}
