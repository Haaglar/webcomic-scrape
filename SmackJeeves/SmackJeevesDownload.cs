using System;;


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

            string xPathNavigation = "//*[@class=\"navbar\"]//a";
            string xPathImage = "//*[@id=\"comic_image\"]";
            return (int)ExitCodes.Success;
        }
    }
}
