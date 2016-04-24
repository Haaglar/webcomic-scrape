using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Tapastic
{
    class TapasticDownload
    {
        enum ExitCodes { Success, InvalidArguments, InvalidSite };
        static int Main(string[] args)
        {
            int comicNo = 0;
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: Tapastic.exe URL");
                return (int)ExitCodes.InvalidArguments;
            }
            
            string url = args[0].StartsWith("http") ? args[0] : "http://" + args[0];
            if (!url.Contains("tapastic"))
            {
                Console.WriteLine("Not a tapastic url");
                return (int)ExitCodes.InvalidArguments;
            }

            string folderDir = url;
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                folderDir = folderDir.Replace(c, '-');
            }
            Directory.CreateDirectory(folderDir);

            string xPathComic = "//img[@class=\"art-image\"]";
            string xPathList = "/html/body/script[5]";

            //Initial request, we need to get the "Episode" list
            HtmlWeb requester = new HtmlWeb();
            HtmlDocument listDocument = requester.Load(url);
            HtmlNode scriptNode = listDocument.DocumentNode.SelectSingleNode(xPathList);
            string episodesString = scriptNode.InnerText;
            int cutStart = episodesString.IndexOf("{");
            int cutEnd = episodesString.IndexOf(";");
            string jsonCut = episodesString.Substring(cutStart, cutEnd-cutStart );
            _Data list = JsonConvert.DeserializeObject<_Data>(jsonCut);

            //Now download every comic
            foreach (EpisodeList episode in list.episodeList)
            {
                HtmlDocument page = requester.Load("https://tapastic.com/episode/" + episode.id);
                HtmlNodeCollection comicImage = page.DocumentNode.SelectNodes(xPathComic);
                foreach (var node in comicImage)
                {
                    string address = node.Attributes["src"].Value;
                    using (WebClient wc = new WebClient())
                    {
                        wc.DownloadFile(address, folderDir + "/" + (++comicNo).ToString("0000") + Path.GetExtension(address));
                        Console.WriteLine("Downloaded image at " + "https://tapastic.com/episode/" + episode.id);
                    }
                }
            }
            Console.WriteLine(list.ToString());
            return (int)ExitCodes.Success;
        }

        //Miscstuff
        public class EpisodeList
        {
            public int id { get; set; }
            public string title { get; set; }
            public string thumbUrl { get; set; }
            public object publishDate { get; set; }
            public int scene { get; set; }
            public int orgScene { get; set; }
            public string nsfwKind { get; set; }
            public int popularCnt { get; set; }
            public int thumbsupCnt { get; set; }
            public int shareCnt { get; set; }
            public int commentCnt { get; set; }
            public string publishedDate { get; set; }
            public bool read { get; set; }
            public bool isNew { get; set; }
            public int? bgmId { get; set; }
        }

        public class _Data
        {
            public int seriesId { get; set; }
            public int episodeId { get; set; }
            public List<EpisodeList> episodeList { get; set; }
            public bool isSeriesView { get; set; }
            public string seriesTitle { get; set; }
            public bool isPreLoad { get; set; }
            public bool isNoAD { get; set; }
            public bool isAutoSoundPlay { get; set; }
            public string subscribers { get; set; }
            public int subscribersCount { get; set; }
            public string barCookieName { get; set; }
            public string commentId { get; set; }
            public string suffix { get; set; }
            public bool isHero { get; set; }
            public bool isBlockedEpisode { get; set; }
            public bool isAutoSubscription { get; set; }
        }
    }
}
