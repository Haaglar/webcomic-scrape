using System;

using System.Text;


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
            Uri asd = new Uri(args[0]);
            //Console.WriteLine(asd.GetLeftPart(UriPartial.Authority));

            //Looking at ComicPress webcomics (5) i see two different versions on how the pages navigate
            //ones that use comic-nav-base as a base and others that use navi navi as a base
            //TODO: handle them both :)  
            return (int)ExitCodes.Success;
        }
    }
}
