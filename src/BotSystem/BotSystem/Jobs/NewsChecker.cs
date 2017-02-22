using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;
using Quartz;

namespace BotSystem.Jobs {
    public class NewsChecker :IJob{
        public void Execute(IJobExecutionContext context) {
//            HtmlDocument html = new HtmlDocument();
//            using (WebClient client = new WebClient()) {
//                html.LoadHtml(client.DownloadString(UrlConstants.GlobalNew));
//            }
//            var firstStory = html.QuerySelector("div.stories .story");
//            var lastPostId =firstStory.Attributes.AttributesWithName("data-story-id").FirstOrDefault();
//            var storyName = firstStory.QuerySelector(".story__title-link").InnerText;
//            Console.WriteLine($"#{lastPostId.Value} {storyName}");
//            //            IList<HtmlNode> nodes = html.QuerySelectorAll("div .my-class[data-attr=123] > ul li");
//            //            HtmlNode node = nodes[0].QuerySelector("p.with-this-class span[data-myattr]");
//            //            html.DocumentElement.SelectNodes("//a[@href")
//
////            throw new NotImplementedException();
        }
    }

    public static class UrlConstants {
        public static string GlobalNew = "http://pikabu.ru/new";
        public static string SpecifiedPost = "http://pikabu.ru/story/_";
    }
}
