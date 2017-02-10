using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DataContract;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Quartz;

namespace BotSystem.Jobs {
    public class PostCommentsGrabber : IJob{
        public void Execute(IJobExecutionContext context) {
            HtmlDocument html = new HtmlDocument();
            int postId = 4801861;
            using (WebClient client = new WebClient()) {
                html.LoadHtml(client.DownloadString(UrlConstants.SpecifiedPost+ postId));
            }
            var comments = html.QuerySelectorAll(".b-comments_type_main .b-comment");
            var commentInfo = new List<CommentInfo>();

            foreach (var comment in comments) {
                commentInfo.Add(new CommentInfo {
                    Id = int.Parse(comment.Attributes.AttributesWithName("data-id").FirstOrDefault().Value),
                    UserName = comment.QuerySelector(".b-comment__user a span").InnerText,
                    Content = comment.QuerySelector(".b-comment__content").InnerHtml,
                    DateTime =
                        Helper.UnixTimeStampToDateTime(
                            double.Parse(
                                comment.QuerySelector(".b-comment__user time.b-comment__time").Attributes.AttributesWithName("datetime").FirstOrDefault().Value)),
                });
            }
            using (var db = new DataContext()) {
                db.Posts.Add(new Post {
                    Id = postId,
                    Title = "",
                    Content = "",
                    Created = DateTime.Now,

                    CommentsCount = 0,
                    Rating = 0,
                    UserName = "",
                });
                PostProcessingManager.ProcessingComment(db, postId, commentInfo);
                db.SaveChanges();
            }
            for (int i = 0; i < 10; i++)
                Console.WriteLine(JsonConvert.SerializeObject(commentInfo[i]));
            //            IList<HtmlNode> nodes = html.QuerySelectorAll("div .my-class[data-attr=123] > ul li");
            //            HtmlNode node = nodes[0].QuerySelector("p.with-this-class span[data-myattr]");
            //            html.DocumentElement.SelectNodes("//a[@href")

//            throw new NotImplementedException();
        }
    }
}
