using System;
using System.Text.RegularExpressions;

namespace CMSDiscover
{
    class ComicDiscover
    {
        enum ExitCodes { Success, InvalidArguments, InvalidSite, LostConnection };
        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: CMSDiscover.exe URL");
                return (int)ExitCodes.InvalidArguments;
            }
            if(!args[0].StartsWith("http"))
            {
                Console.WriteLine("URL must start with http(s)://");
                return (int)ExitCodes.InvalidArguments;
            }
            //Tests -- URL based
            //Tapastic
            Regex tapasticReg = new Regex(@"https?://(?:www\.)tapastic\.com/(?:series|episode)");
            Match isTap = tapasticReg.Match(args[0]);
            if(isTap.Success)
            {
                Console.WriteLine("Use Tapastic.exe");
                return (int)ExitCodes.Success;
            }
            //The duckwe
            Regex theDuckReg = new Regex(@"https?://(?:www\.)theduckwebcomics\.com/..."); //Min 3 letter comic name
            Match isDuck = theDuckReg.Match(args[0]);
            if(isDuck.Success)
            {
                Console.WriteLine("Use TheDuckWebcomics.exe");
                return (int)ExitCodes.Success;
            }
            //Comic fury
            Regex comicFuryReg = new Regex(@"https?://([^\.]+\.)(thecomicseries\.com|the-comic\.org|thecomicstrip\.org|webcomic\.ws|cfw\.me)");
            Match isCF = comicFuryReg.Match(args[0]);
            if (isCF.Success)
            {
                Console.WriteLine("Use ComicFury.exe");
                return (int)ExitCodes.Success;
            }
            //Smack Jeeves
            Regex smackJvReg = new Regex(@"https?://([^\.]+\.)(thewebcomic\.com|smackjeeves\.com)");
            Match isSJ = smackJvReg.Match(args[0]);
            if (isSJ.Success)
            {
                Console.WriteLine("Use SmackJeeves.exe");
                return (int)ExitCodes.Success;
            }
            return (int)ExitCodes.InvalidSite;
        }
    }
}
