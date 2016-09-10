using HtmlAgilityPack;
using System;
using System.IO;
using System.Net;

namespace Webcomic_WordPress
{
    class WebcomicWordPressDownload
    {
        enum ExitCodes { Success, InvalidArguments, InvalidSite, LostConnection, PageError };
        static int Main(string[] args)
        {
            bool indexing = false; //Name based on comic number upload
            if (args.Length <1 || args.Length > 2)
            {
                Console.WriteLine("Usage: Webcomic-wordpress.exe [-i] URL");
                return (int)ExitCodes.InvalidArguments;
            }
            if (args.Length == 2 && args[0].Equals("-i"))
            {
                indexing = true;   
            }
            HtmlDocument webPagestart;
            string address;
            try
            {
                address = args[args.Length - 1].StartsWith("http") ? args[args.Length - 1] : "http://" + args[args.Length - 1];
                webPagestart = new HtmlWeb().Load(address);
            }
            catch
            {
                Console.WriteLine("Failed to download from URL");
                Console.WriteLine("Usage: Webcomic-wordpress.exe [-i] URL");
                return (int)ExitCodes.InvalidArguments;
            }

            //Need to create a folder
            string folderDir = address  ;
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                folderDir = folderDir.Replace(c, '-');
            }
            Directory.CreateDirectory(folderDir);
            //XPATH
            //path to image
            string imageXPath = "//span/img";
            //path tofirst page
            string firstXPath = "//a[@rel=\"first\"]";
            //path to next page
            string nextXPath = "//*[@rel=\"next\"]";
            //Path to final, as once we reach here we make no more requests
            string lastXPath = "//*[@rel=\"last\"]";

            //Now do the hard work

            //Initial page crawl as we have to do a couple additional things, like finding the first page
            HtmlNodeCollection nodesFirst = webPagestart.DocumentNode.SelectNodes(firstXPath);
            HtmlNodeCollection nodesLast = webPagestart.DocumentNode.SelectNodes(lastXPath);
            if (nodesFirst == null || nodesLast == null)
            {
                Console.WriteLine("Site doesn't use Webcomic");
                Console.WriteLine("Usage: Webcomic-wordpress.exe [-i] URL");
                return (int)ExitCodes.InvalidSite;
            }

            string currentPage = nodesFirst[0].Attributes["href"].Value;
            string finalurl = nodesLast[0].Attributes["href"].Value;
            int pageNo = 0;

            //Start crawl loop 
            do
            {

                HtmlDocument currentDocument = new HtmlWeb().Load(currentPage);
                HtmlNodeCollection imgLoc = currentDocument.DocumentNode.SelectNodes(imageXPath);
                if (imgLoc != null) //May not have an image
                {
                    string currentPageImageURL = imgLoc[0].Attributes["src"].Value;
                    using (WebClient wc = new WebClient())
                    {
                        //Filename based on user option
                        string fileName = indexing ? folderDir + "/" + (++pageNo).ToString("0000") + Path.GetExtension(currentPageImageURL) : folderDir + "/" + Path.GetFileName(currentPageImageURL);
                        wc.DownloadFile(currentPageImageURL, fileName);
                        Console.WriteLine("Downloaded: " + Path.GetFileName(fileName));
                    }
                }
                else
                {
                    Console.WriteLine("Image not found at " + currentPage);
                }

                HtmlNodeCollection nextLoc = currentDocument.DocumentNode.SelectNodes(nextXPath);
                if (nextLoc == null)
                {
                    Console.WriteLine("Could not find next page link at " + currentPage);
                    return (int)ExitCodes.PageError;
                }
                currentPage = nextLoc[0].Attributes["href"].Value;
            } while (!currentPage.Equals(finalurl)); //We use do while to handle the case of 1 page 
            Console.WriteLine("Complete");
            return (int)ExitCodes.Success;
        }
    }
}
