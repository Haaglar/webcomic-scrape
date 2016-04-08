using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Webcomic_wordpress
{
    class Program
    {
        enum ExitCodes { Success, InvalidArguments, InvalidSite };
        static int Main(string[] args)
        {
            if(args.Length != 1)
            {
                Console.WriteLine("Usage: Webcomic-wordpress.exe URL");
                return (int)ExitCodes.InvalidArguments;
            }
            Uri address;
            try
            {
                address = new Uri(args[0].StartsWith("http") ? args[0] : "http://" + args[0]);
            }
            catch
            {
                Console.WriteLine("Invalid URL");
                Console.WriteLine("Usage: Webcomic-wordpress.exe URL");
                return (int)ExitCodes.InvalidArguments;
            }
            Console.WriteLine(address.AbsoluteUri);
            String initialPage;
            using (WebClient initialRequest = new WebClient())
            {
                initialPage = initialRequest.DownloadString(address);
            }
            //Webcomic works by having a div in the class "webcomic full" that contains the current webcomic and controls
            //Unfortunatly the image is stored in a dynamically named class so regex it is :^)
            //Finds the webcomic image
            Regex imageFind = new Regex(@"<img\s*?src=""([^""]*)""\s*?width=""\d*""\s*?height=""\d*""");
            //Gets the link to the first page
            Regex findFirstPage = new Regex(@"<a\s*?href=""([^""]*)""\s*?rel=""first""");

            string folderDir = address.AbsoluteUri;
            foreach (var c in System.IO.Path.GetInvalidFileNameChars())
            {
                folderDir = folderDir.Replace(c, '-');
            }
            Directory.CreateDirectory(folderDir);
            return (int)ExitCodes.Success;
        }
    }
}
