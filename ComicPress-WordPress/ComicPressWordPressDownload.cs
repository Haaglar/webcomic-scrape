using HtmlAgilityPack;
using System;
using System.IO;
using System.Net;

namespace ComicPress_WordPress
{
    class ComicPressWordPressDownload
    {
        enum ExitCodes { Success, InvalidArguments, InvalidSite, LostConnection };
        enum FileNameScheme { Original, Incremental, Title };
        static int Main(string[] args)
        {
            int comicCount = 0;
            FileNameScheme naming = FileNameScheme.Original;
            if (args.Length < 1 || args.Length > 2)
            {
                Console.WriteLine("Usage: ComicPress-WordPress.exe [-option] URL\n");
                Console.WriteLine(" -i     Rename files to incremental value");
                Console.WriteLine(" -t     Rename files to page title ");
                Console.WriteLine("\nIf no value is supplied, files aren't renamed.");
                return (int)ExitCodes.InvalidArguments;
            }
            //Setup naming scheme
            if (args.Length == 2)
            {
                if(args[0].StartsWith("-i"))
                {
                    naming = FileNameScheme.Incremental;
                }
                else if (args[0].StartsWith("-t"))
                {
                    naming = FileNameScheme.Title;
                }
                else
                {
                    Console.WriteLine("Invalid option specified");
                    Console.WriteLine("Usage: ComicPress-WordPress.exe [-option] URL\n");
                    Console.WriteLine("-i     Rename files to incremental value");
                    Console.WriteLine("-t     Rename file to page title ");
                    Console.WriteLine("If no value is supplied, files aren't renamed.");
                }
            }
            //Appened http:// for bad input

            string url = args[args.Length-1].StartsWith("http") ? args[args.Length - 1] : "http://" + args[args.Length - 1];
           
            //Comics are found under the comic id, however they may be nested under an a tag
            //For example the image when clicked goes to the next page
            string xpathComic = "//*[@id=\"comic\"]//img";

            //Looking at ComicPress webcomics (5) i see two different versions on how the pages navigate
            //ones that use comic-nav-base as a base and others that use navi navi as a base
            //And some dont have a first in the noraml place
            string xpathFirst = "//*[@class=\"navi navi-first\" or @class=\"comic-nav-base comic-nav-first\" or @class=\"first\"]//@href";
            //There is also a next at the top of the page, however that might redirect to "blog" information and not a comic and break the crawl
            string xpathNext = "//*[@class=\"navi navi-next\" or @class=\"comic-nav-base comic-nav-next\" or @class=\"next\"]//@href";
            string xpathTitle = "//*[@class=\"post-title\"]";
            string nextURL;

            //Find the first page
            HtmlDocument webPagestart = new HtmlWeb().Load(url);
            HtmlNodeCollection nodes = webPagestart.DocumentNode.SelectNodes(xpathFirst);
            if(nodes == null)
            {
                //The user may already suppilied the first page so that was the reason no nodes were found
                string xpathAlreadyFirst = "//*[@class=\"navi navi-first navi-void\"]";
                nodes = webPagestart.DocumentNode.SelectNodes(xpathAlreadyFirst);
                if (nodes == null)//Cant find it? not ComicPress then, or an unsopperted comicpress which i need to implement
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
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                folderDir = folderDir.Replace(c, '-');
            }
            Directory.CreateDirectory(folderDir);

            //Main loop
            while (!String.IsNullOrEmpty(nextURL))
            {
                HtmlDocument currentDocument = new HtmlWeb().Load(nextURL);
                HtmlNodeCollection imgCol = currentDocument.DocumentNode.SelectNodes(xpathComic);
                if(imgCol != null) //Maybe its pointing to a wrong location
                {
                    string comicImageLocation = imgCol[0].Attributes["src"].Value;
                    string fileNameToSave;
                    switch (naming)
                    {
                        case FileNameScheme.Title:
                            HtmlNodeCollection titleNode = currentDocument.DocumentNode.SelectNodes(xpathTitle);
                            if(titleNode == null)
                            {
                                goto default;
                            }
                            fileNameToSave = titleNode[0].InnerText;
                            if(String.IsNullOrEmpty(fileNameToSave))
                            {
                                goto default;
                            }
                            foreach (var c in Path.GetInvalidFileNameChars())
                            {
                                fileNameToSave = fileNameToSave.Replace(c, '-');
                            }
                            if(File.Exists(folderDir + "/" + fileNameToSave + Path.GetExtension(comicImageLocation)))
                            {
                                fileNameToSave += (++comicCount).ToString("0000");
                            }
                            fileNameToSave += Path.GetExtension(comicImageLocation);
                            break;
                        case FileNameScheme.Incremental:
                            fileNameToSave = (++comicCount).ToString("0000") + Path.GetExtension(comicImageLocation);
                            break;
                        default:
                            fileNameToSave = WebUtility.UrlDecode(Path.GetFileName(comicImageLocation));
                            foreach (var c in Path.GetInvalidFileNameChars())
                            {
                                fileNameToSave = fileNameToSave.Replace(c, '-');
                            }
                            break;
                    }
                    //Download the image
                    using (WebClient wc = new WebClient())
                    {
                        wc.DownloadFile(comicImageLocation, folderDir + "/" + fileNameToSave);
                        Console.WriteLine("Downloaded: " + comicImageLocation);
                    }
                }
                else //Something went wrong, but dont quit as the image might just be offline
                {
                    Console.WriteLine("Could not download image at" + nextURL);
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
            Console.WriteLine("Complete");
            return (int)ExitCodes.Success;
        }
    }
}
