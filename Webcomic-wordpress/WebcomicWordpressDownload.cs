using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Webcomic_wordpress
{
    class WebcomicWordPressDownload
    {
        enum ExitCodes { Success, InvalidArguments, InvalidSite, LostConnection };
        //Webcomic works by having a div with the class "webcomic full" that contains the current webcomic and controls
        //Unfortunatly the image is stored in a dynamically named class so regex it is :^)

        static int Main(string[] args)
        {
            bool indexing = false; //Name based on comic number upload
            string indexingTempFileLocation = "";
            if (args.Length <1 || args.Length > 2)
            {
                Console.WriteLine("Usage: Webcomic-wordpress.exe [-i] URL");
                return (int)ExitCodes.InvalidArguments;
            }
            if (args.Length == 2 && args[0].Equals("-i"))
            {
                indexing = true;   
            }
            Uri address;
            try
            {
                address = new Uri(args[args.Length-1].StartsWith("http") ? args[args.Length - 1] : "http://" + args[args.Length - 1]);
            }
            catch
            {
                Console.WriteLine("Invalid URL");
                Console.WriteLine("Usage: Webcomic-wordpress.exe [-i] URL");
                return (int)ExitCodes.InvalidArguments;
            }
            Console.WriteLine("Trying to download from " + address.AbsoluteUri);
            String initialPage;
            using (WebClient initialRequest = new WebClient())
            {
                try
                {
                    initialPage = initialRequest.DownloadString(address);
                }
                catch (WebException)
                {
                    Console.WriteLine("Invalid URL / Couldn't reach host");
                    Console.WriteLine("Usage: Webcomic-wordpress.exe [-i] URL");
                    return (int)ExitCodes.InvalidArguments;
                }
            }

            //Need to create a folder
            string folderDir = address.AbsoluteUri;
            foreach (var c in System.IO.Path.GetInvalidFileNameChars())
            {
                folderDir = folderDir.Replace(c, '-');
            }
            Directory.CreateDirectory(folderDir);
            //Finds the webcomic image
            Regex imageFind = new Regex(@"<img\s*?src=""([^""]*)""\s*?width=""\d*""\s*?height=""\d*""");
            //Gets the link to the first page
            Regex findFirstPage = new Regex(@"<a\s*?href=""([^""]*)""\s*?rel=""first""");
            //Finds the last page
            Regex findLastPage = new Regex(@"href=""([^""]*)""\s*?rel=""last""\s*?class=""last-webcomic-link last-webcomic-link");
            //Finds the next page
            Regex findNextPage = new Regex(@"href=""([^""]*)""\s*?rel=""next""\s*?class=""next-webcomic-link next-webcomic-link");
            //Now do the hard work

            //Initial page crawl as we have to do a couple additional things
            Match m1 = imageFind.Match(initialPage);
            Match lastPageUrl = findLastPage.Match(initialPage);
            if (m1.Length == 0 || lastPageUrl.Length==0)
            {
                Console.WriteLine("Site doesn't use Webcomic");
                Console.WriteLine("Usage: Webcomic-wordpress.exe [-i] URL");
                return (int)ExitCodes.InvalidSite;
            }
            string url = m1.Groups[1].Value;
            string finalurl = lastPageUrl.Groups[1].Value;
            int pageNo = 1;
            using (WebClient wc = new WebClient())
            {
                string fileName;
                if (indexing)
                {
                    fileName = folderDir + "/" + "temp" + Path.GetExtension(url);
                    indexingTempFileLocation = fileName;
                }
                else
                {
                    fileName = folderDir + "/" + Path.GetFileName(url);
                }
                wc.DownloadFile(url, fileName);
                Console.WriteLine("Downloaded: " + Path.GetFileName(fileName));
            }
            string currentPage = findFirstPage.Match(initialPage).Groups[1].Value;
            //Start crawl loop 
            while (!currentPage.Equals(finalurl))
            {
                string thisPage;
                using (WebClient wc = new WebClient())
                {
                    try
                    {
                        thisPage = wc.DownloadString(currentPage);
                    }
                    catch (WebException)
                    {
                        Console.WriteLine("Couldn't reach host");
                        return (int)ExitCodes.InvalidArguments;
                    }
                }
                Match currentPageImage = imageFind.Match(thisPage);
                Match nextPageUrl = findNextPage.Match(thisPage);
                string currentPageImageURL = currentPageImage.Groups[1].Value;

                using (WebClient wc = new WebClient())
                {
                    string fileName = indexing ? folderDir + "/" + pageNo.ToString("0000") + Path.GetExtension(currentPageImageURL) : folderDir + "/" + Path.GetFileName(currentPageImageURL);
                    pageNo++;
                    wc.DownloadFile(currentPageImageURL, fileName);
                    Console.WriteLine("Downloaded: " + Path.GetFileName(fileName));
                }
                if (nextPageUrl.Length == 0)// At the end or something  
                {
                    break;
                }
                currentPage = nextPageUrl.Groups[1].Value;
            }
            if (indexing)
            {
                Console.WriteLine("Renaming temp file");
                File.Move(indexingTempFileLocation, folderDir + "/" + pageNo.ToString("0000") + Path.GetExtension(indexingTempFileLocation));
            }
            Console.WriteLine("Complete");
            return (int)ExitCodes.Success;
        }
    }
}
